using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WarCardGame
{
    public static class WarRules
    {
        public static bool CanPlayerWar(Player player)
        {
            //returns true if player has 3 or more cards, otherwise false.
            return (player.DiscardPile.Count() + player.DrawPile.Count() >= 3);
        }
        public static void DrawACard(Player player, Random rng)
        {
            if (!player.DrawPile.Any() && player.DiscardPile.Any())
            {
                CardTools.TransferCardPile(player.DiscardPile, player.DrawPile);
                player.DrawPile = CardTools.Shuffle(player.DrawPile, rng);
                Console.WriteLine($"{player.Name} shuffled their deck. They have {player.DrawPile.Count()} cards.");
            }

            if (!player.DrawPile.Any())
            {
                throw new Exception($"{player.Name} tried to draw a card when they don't have any.");
            }
            else
            {
                player.DrawACard();
                Console.WriteLine(player.Name + " plays " + player.Hand.Last().ToString());
            }
        }

        public static bool HasPlayerLost(Player player)
        {
            if (!player.DrawPile.Any() && !player.DiscardPile.Any())
            {
                Console.WriteLine(player.Name + " Lost");
                return true;
            }
            return false;
        }

        public static List<Player> DetermineRoundWinners(List<Player> players)
        {
            //var winningRank = players.Select(player => player.Hand.Last().Rank).Max();

            var winningRank = Enum.GetValues<Rank>().Min();

            foreach (var player in players)
            {
                var playerRank = player.Hand.Last().Rank;
                if (playerRank > winningRank)
                    winningRank = playerRank;
            }

            var winningPlayers = new List<Player>();
            foreach (var player in players)
            {
                var playerRank = player.Hand.Last().Rank;
                if (playerRank == winningRank)
                {
                    winningPlayers.Add(player);
                }
            }

            return winningPlayers;
        }
    }
}
