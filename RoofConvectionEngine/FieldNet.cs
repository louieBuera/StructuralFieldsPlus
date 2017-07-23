using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using RimWorld;

namespace StructuralFieldsPlusTesting {
    class FieldNet {
        int netID;
        float deferDamage = 0;
        float genPerTick = 5;
        float currentField = 0;
        float maxField = 0;
        int ctr = 0;

        private List<CompFieldConduit> conduits;
        private List<CompFieldCapacitor> capacitors = new List<CompFieldCapacitor>();
        public float UnusedStorage { get => maxField - currentField; }

        public FieldNet(int ID, List<CompFieldConduit> conduits) {
            netID = ID;
            this.conduits = conduits;
        }

        public List<CompFieldConduit> Conduits { get => conduits; set => conduits = value; }
        public int NetID { get => netID; set => netID = value; }
        internal List<CompFieldCapacitor> Capacitors { get => capacitors; set => capacitors = value; }
        public float CurrentField { get => currentField; set => currentField = value; }
        public float DeferDamage { get => deferDamage; set => deferDamage = value; }

        public void deregister(CompFieldConduit conduit) {
            conduit.NetworkID = 0;
            this.conduits.Remove(conduit);
        }

        public void register(CompFieldConduit conduit) {
            conduit.NetworkID = netID;
            this.conduits.Add(conduit);
        }

        public void register(List<CompFieldConduit> conduits) {
            for(int i = 0; i < conduits.Count; i++) {
                conduits[i].NetworkID = netID;
            }
            this.conduits.AddRange(conduits);
        }

        public void preApplyDamage(DamageInfo dInfo, out bool absorbed) {
            if (dInfo.Amount < currentField - deferDamage) {
                deferDamage += dInfo.Amount;
                absorbed = true;
            } else {
                int delta = (int)Math.Floor(currentField - deferDamage);
                dInfo.SetAmount(dInfo.Amount - delta);
                deferDamage += delta;
                absorbed = false;
            }
        }

        public void register(CompFieldCapacitor capacitor) {
            this.currentField += capacitor.CurrentField;
            this.maxField += capacitor.StoredFieldMax;
            this.capacitors.Add(capacitor);
        }

        public void register(List<CompFieldCapacitor> capacitors) {
            /*for (int i = 0; i < capacitors.Count; i++) {
                conduits[i].NetworkID = netID;
            }*/
            foreach(CompFieldCapacitor capacitor in capacitors) {
                this.currentField += capacitor.CurrentField;
                this.maxField += capacitor.StoredFieldMax;
            }
            this.capacitors.AddRange(capacitors);
        }

        public void deregister(CompFieldCapacitor capacitor) {
            flushDamageGeneration();
            this.currentField -= capacitor.CurrentField;
            this.maxField -= capacitor.StoredFieldMax;
            this.capacitors.Remove(capacitor);
        }

        public void tick() {
            ctr++;
            //do tickRare every 30 ticks == 0.5 seconds
            if(ctr == 30) {
                tickRare();
            }
        }

        public void tickRare() {
            flushDamageGeneration();
        }

        public float unusedStoragePercentage(CompFieldCapacitor capacitor) {
            return capacitor.UnusedStorage / this.UnusedStorage;
        }

        public float filledStoragePercentage(CompFieldCapacitor capacitor) {
            return capacitor.CurrentField / this.currentField;
        }

        public void flushDamageGeneration() {
            //generated since last
            float temp = (float)ctr * genPerTick;
            //total change in field strength
            temp -= deferDamage;
            //distribute charge/discharge => attemp to balance fill percentage
            if(temp + CurrentField >= maxField) {
                foreach (CompFieldCapacitor i in capacitors) {
                    i.CurrentField = i.StoredFieldMax;
                }
                this.currentField = this.maxField;
            } else if (temp > 0) {
                foreach(CompFieldCapacitor i in capacitors) {
                    i.CurrentField += unusedStoragePercentage(i) * temp;
                }
                this.currentField += temp;
            } else if (temp < 0) {
                foreach (CompFieldCapacitor i in capacitors) {
                    i.CurrentField += filledStoragePercentage(i) * temp;
                }
                this.currentField += temp;
            }
            ctr = 0;
        }



        /*List<CompFieldGenerator> generators;
        List<CompFieldCapacitor> capacitors;
        List<CompFieldPillars> pillars;
        List<CompFieldProtected> protectorates;*/


    }
}
