using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StructuralFieldsPlusTesting {
    class FieldNet {
        int netID;

        private List<CompFieldConduit> conduits;

        public FieldNet(int ID, List<CompFieldConduit> conduits) {
            netID = ID;
            this.conduits = conduits;
        }

        public List<CompFieldConduit> Conduits { get => conduits; set => conduits = value; }

        public void deregister(CompFieldConduit conduit) {
            this.conduits.Remove(conduit);
        }

        public void register(CompFieldConduit conduit) {
            conduit.networkID = netID;
            this.conduits.Add(conduit);
        }

        public void register(List<CompFieldConduit> conduits) {
            for(int i = 0; i < conduits.Count; i++) {
                conduits[i].networkID = netID;
            }
            this.conduits.AddRange(conduits);
        }
        /*List<CompFieldGenerator> generators;
        List<CompFieldCapacitor> capacitors;
        List<CompFieldPillars> pillars;
        List<CompFieldProtected> protectorates;*/


    }
}
