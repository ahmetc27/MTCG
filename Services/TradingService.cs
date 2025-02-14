using System.Collections.Generic;
using MTCG.Models;

namespace MTCG.Services
{
    public class TradingService
    {
        private readonly List<Trade> _activeTrades = new();

        public bool AddTrade(Trade trade)
        {
            // Prüfen, ob die ID bereits existiert
            if (_activeTrades.Any(t => t.Id == trade.Id))
            {
                Console.WriteLine($"Trade mit ID {trade.Id} existiert bereits!");
                return false;
            }

            _activeTrades.Add(trade);
            return true;
        }
        public List<Trade> GetActiveTrades()
        {
            return _activeTrades;
        }
        public Trade? GetTradeById(string tradeId)
        {
            return _activeTrades.FirstOrDefault(t => t.Id == tradeId);
        }

        public void RemoveTrade(string tradeId)
        {
            _activeTrades.RemoveAll(t => t.Id == tradeId);
        }
    }
}