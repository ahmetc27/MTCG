namespace MTCG.Models
{
    public class Trade
    {
        public string Id { get; set; }
        public string Owner { get; set; }
        public Card OfferedCard { get; set; }
        public string RequiredType { get; set; }  // z. B. "Monster", "Spell"
        public double MinimumDamage { get; set; }

        public Trade(string id, string owner, Card offeredCard, string requiredType, double minimumDamage)
        {
            Id = id;
            Owner = owner;
            OfferedCard = offeredCard;
            RequiredType = requiredType;
            MinimumDamage = minimumDamage;
        }
    }
}