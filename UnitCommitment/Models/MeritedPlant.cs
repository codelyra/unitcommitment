using System.Text.Json.Serialization;

namespace UnitCommitment.Models
{
    public class MeritedPlant
    {
        public MeritedPlant(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public double UnitCost { get; set; }

        public double GreedyCapacity { get; set; }

        public double MinCapacity { get; set; }

        public double CommittedCapacity { get; set; }

        public double DecreaseCapacity()
        {
            return CommittedCapacity - MinCapacity;
        }

        public double PerformanceMerit()
        {
            return GreedyCapacity / UnitCost;
        }

    }
}
