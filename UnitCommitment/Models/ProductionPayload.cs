using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UnitCommitment.Models
{
    public class ProductionPayload
    {
        [JsonPropertyName("load")]
        public int Demand { get; set; }

        [JsonPropertyName("fuels")]
        public Fuels Market { get; set; }

        [JsonPropertyName("powerplants")]
        public IEnumerable<Powerplants> Grid { get; set; }
    }

    public class Fuels
    {
        [JsonPropertyName("gas(euro/MWh)")]
        public double Gas { get; set; }

        [JsonPropertyName("kerosine(euro/MWh)")]
        public double Kerosine { get; set; }

        [JsonPropertyName("co2(euro/ton)")]
        public int CO2 { get; set; }

        [JsonPropertyName("wind(%)")]
        public int Wind { get; set; }
    }

    public class Powerplants 
    {
        public double Name { get; set; }

        public double Type { get; set; }

        public int Efficiency { get; set; }

        [JsonPropertyName("pmin")]
        public int MinCapacity { get; set; }

        [JsonPropertyName("pmax")]
        public int MaxCapacity { get; set; }
    }
}
