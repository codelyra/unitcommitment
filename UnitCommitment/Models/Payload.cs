using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UnitCommitment.Models
{
    public class Payload
    {
        [JsonPropertyName("load")]
        public int Demand { get; set; }

        [JsonPropertyName("fuels")]
        public Fuels Constraints { get; set; }

        [JsonPropertyName("powerplants")]
        public List<Powerplant> Suppliers { get; set; }
    }
}
