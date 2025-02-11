using System.Collections.Generic;
using System.Linq;
using MTCG.Models;

namespace MTCG.Services
{
    public class DeckService
    {
        private readonly Dictionary<string, Deck> _userDecks = new();

        public bool SetUserDeck(string username, List<Card> cards)
        {
            if (cards.Count != 4)
                return false; // ❌ Muss genau 4 Karten sein

            _userDecks[username] = new Deck { Cards = cards };
            return true; // ✅ Erfolgreich gespeichert
        }

        public Deck? GetUserDeck(string username)
        {
            return _userDecks.ContainsKey(username) ? _userDecks[username] : null;
        }
    }
}