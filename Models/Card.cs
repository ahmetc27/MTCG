namespace MTCG.Models
{
    public class Card
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Damage { get; set; }
        public string Type { get; set; }

        public Card(string id, string name, double damage, string type)
        {
            Id = id;
            Name = name;
            Damage = damage;
            Type = type;
        }
    }
}