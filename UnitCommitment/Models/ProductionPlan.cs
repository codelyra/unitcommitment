using System;
using System.Text.Json.Serialization;

namespace UnitCommitment.Models
{
    public class ProductionPlan
    {
        public string Name { get; set; }

        [JsonPropertyName("p")]
        public int commitment { get; set; }
    }
}
