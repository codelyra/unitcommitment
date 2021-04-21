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
            currDemand = payload.Demand;
            surplus = 0;

            CommitAllWindPowerplants();
            CommitOtherPlowerplants();

            if (surplus > 0)
            {
                AdjustCommitmentSurplus();
            }

            foreach (MeritedPlant meritedPlant in meritedPlants)
            {
                commitments.Add(new Commitment(meritedPlant.Name, meritedPlant.CommittedCapacity));
            }

            return commitments;
        }

        private void AdjustCommitmentSurplus()
        {
            List<MeritedPlant> activePowerplants = meritedPlants.FindAll(mp => mp.CommittedCapacity > 0);

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
            if(currDemand == 0)
                return;
            
            meritedPlants = EvaluatePowerplantsByMerit();
            int meritedPlantsIndex = 1;
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
                    else
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
