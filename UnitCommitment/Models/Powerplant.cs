using System.Text.Json.Serialization;

namespace UnitCommitment.Models
{
    public class Powerplant
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public double Efficiency { get; set; }

        [JsonPropertyName("pmin")]
        public int MinCapacity { get; set; }

        [JsonPropertyName("pmax")]
        public int MaxCapacity { get; set; }

        public int EffectiveMinCapacity { get; set; }

        public int EffectiveMaxCapacity { get; set; }

        public double OperationalCost { get; set; }

        public int CommittedPower { get; set; }
    }
}
