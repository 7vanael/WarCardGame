namespace WarCardGame
{
    internal class Program
    {
        const string ROUND_WINS = "Round wins";
        const string WARS_WON = "Wars won";

        static void Main(string[] args)
        {
            Console.WriteLine("Enter seed or enter to auto-generate one:");
            var seedText = Console.ReadLine();

            var seed = 0;
            if (!int.TryParse(seedText, out seed))
            {
                seed = new Random().Next(0, 10000);
            }

            Console.WriteLine($"seed = {seed}");

            var rng = new Random(seed);

            var allPlayers = new List<Player>();

            Console.WriteLine("Enter the names of players and just Enter to finish:");

            var done = false;
            while (!done)
            {
                var name = Console.ReadLine();

                if (name != "")
                    allPlayers.Add(new Player() { Name = name });
                else
                    done = true;
            }

            if (!allPlayers.Any())
            {
                Console.WriteLine("No players! Peace out!");
                return;
            }

            foreach (var player in allPlayers)
            {
                player.Stats[ROUND_WINS] = 0;
                player.Stats[WARS_WON] = 0;
            }

            var activePlayers = new List<Player>(allPlayers);

            // Create the deck
            var deck = new List<Card>();

            foreach (var suit in Enum.GetValues<Suit>())
            {
                foreach (var rank in Enum.GetValues<Rank>())
                {
                    var card = new Card { Suit = suit, Rank = rank };
                    deck.Add(card);
                }
            }

            var drawPile = CardTools.Shuffle(deck, rng);

            //Deals the deck to each player's draw piles
            while (drawPile.Any())
            {
                foreach (var player in allPlayers)
                {
                    if (drawPile.Any())
                    {
                        CardTools.TransferCard(drawPile.First(), drawPile, player.DrawPile);
                    }
                }
            }

            var roundCounter = 0;
            var warCounter = 0;

            var isGameOver = false;

            while (!isGameOver)
            {
                var isRoundWar = activePlayers.Any(player => player.Hand.Any());
                if (!isRoundWar)
                {
                    roundCounter++;
                    Console.WriteLine($"---------ROUND {roundCounter}--------");
                }

                foreach (var player in new List<Player>(activePlayers))
                {
                    if (WarRules.HasPlayerLost(player))
                    {
                        activePlayers.Remove(player);
                        Console.WriteLine($"{player.Name} has been removed from the game.");
                    }
                }

                if (activePlayers.Count() == 1)
                {
                    isGameOver = true;
                    Console.WriteLine($"The game is over. {activePlayers.Single().Name} has won!!");
                    break;
                }

                var maxHandSize = activePlayers.Select(player => player.Hand.Count()).Max();

                var stillActiveInRoundPlayers = activePlayers.Where(player => player.Hand.Count() == maxHandSize).ToList();

                foreach (var player in stillActiveInRoundPlayers)
                {
                    WarRules.DrawACard(player, rng);
                }

                var roundWinners = WarRules.DetermineRoundWinners(stillActiveInRoundPlayers);

                if (roundWinners.Count() == 1)
                {
                    var winner = roundWinners.Single();

                    foreach (var player in allPlayers)
                    {
                        CardTools.TransferCardPile(player.Hand, winner.DiscardPile);
                        if (!activePlayers.Contains(player))
                        {
                            CardTools.TransferCardPile(player.DrawPile, winner.DiscardPile);
                            CardTools.TransferCardPile(player.DiscardPile, winner.DiscardPile);
                        }
                    }

                    Console.WriteLine(winner.Name + " wins the hand");
                    winner.Stats[ROUND_WINS]++;
                    if (isRoundWar)
                    {
                        winner.Stats[WARS_WON]++;
                    }
                }
                else // WAR!
                {
                    Console.WriteLine($"WAAAARRR! #{++warCounter}");
                    foreach (var player in new List<Player>(roundWinners))
                    {
                        if (!WarRules.CanPlayerWar(player))
                        {
                            activePlayers.Remove(player);
                            roundWinners.Remove(player);
                            Console.WriteLine($"{player.Name} has been removed from the game.");
                        }
                    }
                    if (activePlayers.Count() == 1)
                    {
                        continue;
                        //isGameOver = true;
                        //Console.WriteLine($"\n\n The game is over. {activePlayers.Single().Name} has won!!");
                        //break;
                    }
                    foreach (var player in roundWinners)
                    {
                        WarRules.DrawACard(player, rng);
                        WarRules.DrawACard(player, rng);
                    }
                }
                //throw new Exception("Yikes! War didn't end well. (Surpirse, surprise)");		
            }
            Console.WriteLine($"\n\n--------WAR SUMMARY------\n\n ");
            Console.WriteLine($"War Counter = {warCounter}");
            foreach (var player in allPlayers)
            {
                Console.WriteLine($"{player.Name} won {player.Stats[ROUND_WINS]} rounds");
                Console.WriteLine($"{player.Name} won {player.Stats[WARS_WON]} wars");
            }
            Console.WriteLine("\n\n\n***``~~--*SPARKLES*--~~``***");
        }
    }
}