using System.Text.Json.Serialization;

namespace UnitCommitment.Models
{
    public class Commitment
    {
        public string Name { get; set; }

        [JsonPropertyName("p")]
        public double Power { get; set; }

        public Commitment(string name, double power)
        {
            Name = name;
            Power = power;
        }
    }
}
