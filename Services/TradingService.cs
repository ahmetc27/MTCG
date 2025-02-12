using System.Collections.Generic;
using MTCG.Models;

namespace MTCG.Services
{
    public class TradingService
    {
        private readonly List<Trade> _activeTrades = new();

        public void AddTrade(Trade trade)
        {
            _activeTrades.Add(trade);
        }

        public List<Trade> GetActiveTrades()
        {
            return _activeTrades;
        }
    }
}