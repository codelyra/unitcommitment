using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NLog;
using UnitCommitment.Models;

namespace UnitCommitment.Services
{
    public class CommitmentService
    {
        private readonly Logger _log;

        public Payload payload { get; set; }
        public AppSettings appSettings { get; set; }
        private double currDemand;
        private double surplus;
        List<MeritedPlant> meritedPlants = new List<MeritedPlant>();
        private List<Commitment> commitments = new List<Commitment>();

        public CommitmentService(Payload payload, AppSettings appSettings)
        {
            _log = LogManager.GetLogger("Default");

            this.payload = payload;
            this.appSettings = appSettings;
        }

        public List<Commitment> CommitPoweplants()
        {
            // key variables 
            currDemand = payload.Demand;
            surplus = 0;

            if (currDemand <= 0) {
                _log.Error("There is no feasible power demand");
                return commitments;
            }

            // use all clean, cheap and efficient wind plants
            CommitAllWindPowerplants();

            // forward step: greedy fractional knapsack
            CommitOtherPlowerplants();

            // backprop step: surplus elimination by high-merit powerplants
            if (surplus > 0)
            {
                AdjustCommitmentSurplus();
            }

            // create and return poweplant commitments
            double totalGridPower = 0;
            foreach (MeritedPlant meritedPlant in meritedPlants)
            {
                commitments.Add(new Commitment(meritedPlant.Name, meritedPlant.CommittedCapacity));
                totalGridPower += meritedPlant.CommittedCapacity;
            }

            if(totalGridPower <= payload.Demand)
            {
                _log.Error("Grid cannot supply total demand");
            }
            return commitments;
        }

        private void AdjustCommitmentSurplus()
        {
            // reminder: meritedPlants contains only gasfired and turbojets
            // commited plants are at the top of the meritocratic rule
            List<MeritedPlant> activePowerplants = meritedPlants.FindAll(mp => mp.CommittedCapacity > 0);

            // backprop: surplus is created when we are forced to take min power instead of exact curr demand
            // the last select plant is therefore operating at minimal power, we spread undesired surplus backwards
            int activePowerplantsIndex = activePowerplants.Count - 1;
            while (surplus > 0 && activePowerplantsIndex >= 1)
            {
                if(activePowerplants[activePowerplantsIndex-1].DecreaseCapacity() >= surplus)
                {
                    activePowerplants[activePowerplantsIndex-1].CommittedCapacity -= surplus;
                    surplus = 0;
                }
                else
                {
                    activePowerplants[activePowerplantsIndex-1].CommittedCapacity = activePowerplants[activePowerplantsIndex-1].MinCapacity;
                    surplus -= activePowerplants[activePowerplantsIndex-1].CommittedCapacity;
                }
                activePowerplantsIndex--;
            }
        }

        private void CommitAllWindPowerplants()
        {
            // whatever the wind efficiency is, wind is still the best merit
            double efficiency = payload.Constraints.Wind / 100;
            IEnumerable<Powerplant> windPowerplants = payload.Suppliers.FindAll(s => s.Type.Equals("windturbine"));
            foreach (Powerplant supplier in windPowerplants)
            {
                double effectivePower = supplier.MaxCapacity * efficiency;
                if (currDemand > 0)
                {
                    double commitedPower;
                    if (currDemand >= effectivePower)
                    {
                        commitedPower = effectivePower;
                        currDemand -= commitedPower;
                    }
                    else
                    {
                        commitedPower = effectivePower - currDemand;
                        currDemand = 0;
                    }
                    commitments.Add(new Commitment(supplier.Name, Math.Round(commitedPower, 2)));
                }
            }
        }

        private void CommitOtherPlowerplants()
        {
            // demand above wind power total capacity
            if(currDemand == 0)
                return;

            // merit is defined as a ratio between unit cost and total capacity
            meritedPlants = EvaluatePowerplantsByMerit();
            int meritedPlantsIndex = 1;

            // greedy knapsack: keep picking top valued elements until sack (total demand) is full
            while(currDemand > 0 && meritedPlantsIndex <= meritedPlants.Count)
            {
                double committedValue = 0;
                MeritedPlant meritedPlant = meritedPlants[meritedPlantsIndex - 1];
                if (currDemand >= meritedPlant.GreedyCapacity)
                {
                    committedValue = meritedPlant.GreedyCapacity;
                }
                else
                {
                    if (currDemand >= meritedPlant.MinCapacity)
                    {
                        committedValue = currDemand;
                    }
                    else // set at minimum and calculated surplus (amount of generated power above demand)
                    {
                        committedValue = meritedPlant.MinCapacity;
                        surplus = meritedPlant.MinCapacity - currDemand;
                    }
                }
                meritedPlant.CommittedCapacity = committedValue;
                currDemand -= committedValue;
                meritedPlantsIndex++;
            }
        }

        private List<MeritedPlant> EvaluatePowerplantsByMerit()
        { 
            List<Powerplant> powerplants = payload.Suppliers.FindAll(s => s.Type != "windturbine");

            // first calculate effective cost taking into account plant efficiency, fuels cost and Co2 penalty
            foreach (Powerplant supplier in powerplants)
            {
                double efficiency = supplier.Efficiency / 100;
                double effectiveUnitCost = 0;
                if (supplier.Type == "gasfired")
                {
                    effectiveUnitCost = payload.Constraints.Gas + (1 / efficiency);
                    if (appSettings.ConsiderCo2)
                    {
                        _log.Info("Penalizing CO2 Emissions");
                        effectiveUnitCost += payload.Constraints.Co2 * appSettings.Co2Value;
                    }
                }
                else if (supplier.Type == "turbojet")
                {
                    effectiveUnitCost = payload.Constraints.Kerosine + (1 / efficiency);
                }
                MeritedPlant meritedPlant = new MeritedPlant(supplier.Name);
                meritedPlant.UnitCost = effectiveUnitCost;
                meritedPlant.GreedyCapacity = supplier.MaxCapacity;
                meritedPlant.MinCapacity = supplier.MinCapacity;
                meritedPlants.Add(meritedPlant);
            }

            // sort by performance merit: full capacity / unit cost
            meritedPlants.Sort(OrderBMerit);
            return meritedPlants;
        }

        private int OrderBMerit(MeritedPlant a, MeritedPlant b)
        {
            if (a.PerformanceMerit() < b.PerformanceMerit())
            {
                return 1;
            }
            else if (a.PerformanceMerit() > b.PerformanceMerit())
            {
                return -1;
            }
            else
                return 0;
        }

    }
}
