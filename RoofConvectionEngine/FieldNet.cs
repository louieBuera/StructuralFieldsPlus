using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StructuralFieldsPlusTesting {
    class FieldNet {
        int netID;
        float deferDamage;
        float genPerTick;
        float fieldStrngth;
        double totalStrength;

        private List<CompFieldConduit> conduits;

        public FieldNet(int ID, List<CompFieldConduit> conduits) {
            netID = ID;
            this.conduits = conduits;
        }

        public List<CompFieldConduit> Conduits { get => conduits; set => conduits = value; }
        public int NetID { get => netID; set => netID = value; }

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

        public void flushDamageGeneration() {

        }

        /*List<CompFieldGenerator> generators;
        List<CompFieldCapacitor> capacitors;
        List<CompFieldPillars> pillars;
        List<CompFieldProtected> protectorates;*/


    }
}
