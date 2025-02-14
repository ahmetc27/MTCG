using System.Collections.Generic;
using System.Linq;
using MTCG.Models;

namespace MTCG.Services
{
    public class CardService
    {
        private readonly Dictionary<string, List<Card>> _userCards = new();

        public void AddCardsToUser(string username, List<Card> cards)
        {
            if (!_userCards.ContainsKey(username))
            {
                _userCards[username] = new List<Card>();
            }
            _userCards[username].AddRange(cards);
        }

        public List<Card> GetUserCards(string username)
        {
            return _userCards.ContainsKey(username) ? _userCards[username] : new List<Card>();
        }
        public void RemoveCardFromUser(string username, Card card)
        {
            if (_userCards.ContainsKey(username))
            {
                _userCards[username].RemoveAll(c => c.Id == card.Id);
            }
        }
        public void AddCardToUser(string username, Card card)
        {
            if (!_userCards.ContainsKey(username))
            {
                _userCards[username] = new List<Card>();
            }
            _userCards[username].Add(card);
        }
    }
}