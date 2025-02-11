using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MTCG.Models;

namespace MTCG.Services
{
    public class BattleService
    {
        private readonly Queue<string> _battleQueue = new(); // Warteschlange für Spieler
        private readonly DeckService _deckService;
        private readonly Dictionary<string, string> _battleResults = new(); // Speichert Kampfresultate

        public BattleService(DeckService deckService)
        {
            _deckService = deckService;
        }

        public string JoinBattle(string username)
        {
            if (_battleQueue.Count > 0) // Es gibt schon einen wartenden Spieler
            {
                string opponent = _battleQueue.Dequeue();
                return StartBattle(username, opponent);
            }
            else
            {
                _battleQueue.Enqueue(username);
                return "Waiting for opponent...";
            }
        }

        private string StartBattle(string player1, string player2)
        {
            Deck? deck1 = _deckService.GetUserDeck(player1);
            Deck? deck2 = _deckService.GetUserDeck(player2);

            if (deck1 == null || deck2 == null || deck1.Cards.Count != 4 || deck2.Cards.Count != 4)
            {
                return "One or both players do not have a valid deck.";
            }

            StringBuilder battleLog = new();
            battleLog.AppendLine($"Battle between {player1} and {player2} begins!");

            int player1Wins = 0;
            int player2Wins = 0;

            for (int round = 0; round < 4; round++)
            {
                Card card1 = deck1.Cards[round];
                Card card2 = deck2.Cards[round];

                battleLog.AppendLine($"Round {round + 1}: {player1}'s {card1.Name} ({card1.Damage} DMG) vs {player2}'s {card2.Name} ({card2.Damage} DMG)");

                if (card1.Damage > card2.Damage)
                {
                    player1Wins++;
                    battleLog.AppendLine($"{player1} wins this round!");
                }
                else if (card2.Damage > card1.Damage)
                {
                    player2Wins++;
                    battleLog.AppendLine($"{player2} wins this round!");
                }
                else
                {
                    battleLog.AppendLine("It's a tie!");
                }
            }

            string winner;
            if (player1Wins > player2Wins)
            {
                winner = $"{player1} wins the battle!";
            }
            else if (player2Wins > player1Wins)
            {
                winner = $"{player2} wins the battle!";
            }
            else
            {
                winner = "The battle ends in a draw!";
            }

            battleLog.AppendLine(winner);
            _battleResults[player1] = _battleResults[player2] = battleLog.ToString();

            return battleLog.ToString();
        }

        public string GetLastBattleResult(string username)
        {
            return _battleResults.ContainsKey(username) ? _battleResults[username] : "No recent battles.";
        }
    }
}