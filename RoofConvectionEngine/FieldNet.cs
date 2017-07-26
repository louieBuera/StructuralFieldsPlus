using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using RimWorld;

namespace StructuralFieldsPlusTesting {
    class FieldNet {
        int netID;
        //deferred variables should not be used for currentField/unusedStorage computations
        float deferDamage = 0;
        float deferGenerate = 0;
        float genPerTick = 0;
        float currentField = 0;
        float maxField = 0;
        int ctr = 0;


        private List<CompFieldConduit> conduits;
        private List<CompFieldCapacitor> capacitors = new List<CompFieldCapacitor>();
        private List<CompFieldGenerator> generators = new List<CompFieldGenerator>();

        #region getters+setters
        public float UnusedStorage { get => maxField - currentField; }
        public float AvailableField { get => currentField + deferGenerate - deferDamage;  }
        public List<CompFieldConduit> Conduits { get => conduits; set => conduits = value; }
        public int NetID { get => netID; set => netID = value; }
        internal List<CompFieldCapacitor> Capacitors { get => capacitors; set => capacitors = value; }
        internal List<CompFieldGenerator> Generators { get => generators; set => generators = value; }
        public float GenPerTick { get => genPerTick; set => genPerTick = value; }

        //public float CurrentField { get => currentField; set => currentField = value; }
        //public float DeferDamage { get => deferDamage; set => deferDamage = value; }
        //public float DeferGenerate { get => deferGenerate; set => deferGenerate = value; }

        public float unusedStoragePercentage(CompFieldCapacitor capacitor) {
            return capacitor.UnusedStorage / this.UnusedStorage;
        }

        public float filledStoragePercentage(CompFieldCapacitor capacitor) {
            return capacitor.CurrentField / this.currentField;
        }

        #endregion

        //constructor
        public FieldNet(int ID, List<CompFieldConduit> conduits) {
            netID = ID;
            this.conduits = conduits;
        }

        #region register/deregister
        public void register(CompFieldConduit conduit) {
            conduit.NetworkID = netID;
            conduit.spreadNetworkID();
            this.conduits.Add(conduit);
        }

        public void register(List<CompFieldConduit> conduits) {
            for(int i = 0; i < conduits.Count; i++) {
                conduits[i].NetworkID = netID;
            }
            this.conduits.AddRange(conduits);
        }

        public void deregister(CompFieldConduit conduit) {
            conduit.NetworkID = 0;
            this.conduits.Remove(conduit);
        }

        public void register(CompFieldCapacitor capacitor) {
            this.currentField += capacitor.CurrentField;
            this.maxField += capacitor.StoredFieldMax;
            this.capacitors.Add(capacitor);
        }

        public void register(List<CompFieldCapacitor> capacitors) {
            foreach (CompFieldCapacitor capacitor in capacitors) {
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

        public void register(CompFieldGenerator generator) {
            this.genPerTick += generator.GenPerTick;
            this.generators.Add(generator);
        }

        public void register(List<CompFieldGenerator> generators) {
            foreach (CompFieldGenerator i in generators) {
                this.genPerTick += i.GenPerTick;
            }
            this.generators.AddRange(generators);
        }

        public void deregister(CompFieldGenerator generator) {
            flushDamageGeneration();
            this.genPerTick -= generator.GenPerTick;
            generator.IsGenerating = false;
            this.generators.Remove(generator);
        }

        #endregion

        public void preApplyDamage(DamageInfo dInfo, out bool absorbed) {
            if (dInfo.Amount < currentField + deferGenerate - deferDamage) {
                deferDamage += dInfo.Amount;
                absorbed = true;
            } else {
                int delta = (int)Math.Floor(currentField + deferGenerate - deferDamage);
                dInfo.SetAmount(dInfo.Amount - delta);
                deferDamage += delta;
                absorbed = false;
            }
        }

        public void adjustGeneratorOutput(float delta) {
            genPerTick += delta;
        }
        

        public void tick() {
            //check if full and undamaged
            if(!UnityEngine.Mathf.Approximately(currentField, maxField) || !UnityEngine.Mathf.Approximately(deferDamage, 0)) {
                deferGenerate += genPerTick;
                //do tickRare every 30 ticks == 0.5 seconds
                if (currentField + deferGenerate - deferDamage > maxField) {
                    flushDamageGeneration();
                    //deferGenerate = maxField - currentField + deferDamage;
                    ctr = 0;
                }
            }
            ctr++;
            if (ctr == 30) {
                flushDamageGeneration();
            }
        }



        public void flushDamageGeneration() {
            //generated since last
            //float temp = (float)ctr * genPerTick;
            //total change in field strength
            float delta = deferGenerate - deferDamage;
            //distribute charge/discharge => attemp to balance fill percentage
            if(delta + currentField >= maxField) {
                foreach (CompFieldCapacitor i in capacitors) {
                    i.CurrentField = i.StoredFieldMax;
                }
                this.currentField = this.maxField;
            } else if (delta > 0) {
                foreach(CompFieldCapacitor i in capacitors) {
                    i.CurrentField += unusedStoragePercentage(i) * delta;
                }
                this.currentField += delta;
            } else if (delta < 0) {
                foreach (CompFieldCapacitor i in capacitors) {
                    i.CurrentField += filledStoragePercentage(i) * delta;
                }
                this.currentField += delta;
            }
            deferDamage = 0;
            deferGenerate = 0f;
            ctr = 0;
            //refresh generation
            genPerTick = 0;
            foreach(CompFieldGenerator i in generators) {
                genPerTick += i.GenPerTick;
            }
        }



        /*List<CompFieldGenerator> generators;
        List<CompFieldCapacitor> capacitors;
        List<CompFieldPillars> pillars;
        List<CompFieldProtected> protectorates;*/


    }
}
