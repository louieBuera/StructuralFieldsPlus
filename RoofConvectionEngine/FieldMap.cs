using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using RimWorld;

namespace StructuralFieldsPlusTesting {
    class FieldMap : MapComponent {
        private const int startIndex = 1;
        private const int stopIndex = 16000;

        int x_size;
        int z_size;

        //int[,] embrasureArray;
        //int[,] roofArray;
        int[,] conduitArray;

        public Dictionary<int, FieldNet> fieldNets;
        List<int> usedIndices;// = new List<int>();

        public int[,] ConduitArray { get => conduitArray; set => conduitArray = value; }

        public FieldMap(Map map) : base(map) {

            x_size = map.Size.x;
            z_size = map.Size.z;
            //init value = 0
            //embrasureArray = new int[x_size, z_size];
            //roofArray = new int[x_size, z_size];
            ConduitArray = new int[x_size, z_size];

            fieldNets = new Dictionary<int, FieldNet>();

            usedIndices = new List<int>();
        }

        public override void MapComponentTick() {
            foreach(int i in usedIndices) {
                fieldNets[i].tick();
            }
        }

        //checks positions 1 cell north, east, west, and south 
        public List<int> checkAdjacentNets(int x, int z, int[,] searchArray) {
            List<int> IDs = new List<int>();
            int ID;
            if (x + 1 < x_size) {
                ID = searchArray[x + 1, z];
                if (ID != 0) {
                    IDs.Add(ID);
                }
            }
            if (x - 1 >= 0) {
                ID = searchArray[x - 1, z];
                if (ID != 0 && !IDs.Contains(ID)) {
                    IDs.Add(ID);
                }
            }
            if (z - 1 >= 0) {
                ID = searchArray[x, z - 1];
                if (ID != 0 && !IDs.Contains(ID)) {
                    IDs.Add(ID);
                }
            }
            if (z + 1 < z_size) {
                ID = searchArray[x, z + 1];
                if (ID != 0 && !IDs.Contains(ID)) {
                    IDs.Add(ID);
                }
            }
            return IDs;
        }

        //check immadiate adjacent to [x, z]
        public List<IntVec2> checkAdjacentCellsMatch(int x, int z, int netIDFind) {
            List<IntVec2> cells = new List<IntVec2>();

            if (x + 1 < x_size && ConduitArray[x + 1, z] == netIDFind) {
                cells.Add(new IntVec2(x + 1, z));
            }
            if (x - 1 >= 0 && ConduitArray[x - 1, z] == netIDFind) {
                cells.Add(new IntVec2(x - 1, z));
            }
            if (z + 1 < z_size && ConduitArray[x, z + 1] == netIDFind) {
                cells.Add(new IntVec2(x, z + 1));
            }
            if (z - 1 >= 0 && ConduitArray[x, z - 1] == netIDFind) {
                cells.Add(new IntVec2(x, z - 1));
            }

            return cells;
        }
        
        //set avoid to 0 to use as default
        public int searchNewIndex(int avoid1, int avoid2, int avoid3 ) {
            for(int i= startIndex; i < stopIndex; i++) {
                if (i != avoid1 && i != avoid2 && i != avoid3 && !usedIndices.Contains(i)) {
                    return i;
                }
            }
            return -1;
        }
        
        //convenience function
        public T deque<T>(List<T> list) {
            T hold = list[0];
            list.RemoveAt(0);
            return hold;
        }
        
        //handles removal of empty FieldNets
        public void deregister(CompFieldConduit conduit) {
            int x = conduit.parent.Position.x;
            int z = conduit.parent.Position.z;

            int index = ConduitArray[x, z];

            List<IntVec2> adjacentCells = checkAdjacentCellsMatch(x, z, index);

            //handles case one adjacent conduit, needed for all cases
            fieldNets[index].deregister(conduit);
            fieldNets[index].deregister(conduit.parent.GetComp<CompFieldCapacitor>());
            fieldNets[index].deregister(conduit.parent.GetComp<CompFieldGenerator>());

            //case single element of FieldNet, remove FieldNet
            if (adjacentCells.Count == 0) {
                usedIndices.Remove(index);
                fieldNets.Remove(index);
            //case more than one adjacent conduit
            } else if (adjacentCells.Count > 1) {
                //rebuilt net instead
                List<CompFieldConduit> conduits = fieldNets[index].Conduits;
                List<CompFieldCapacitor> capacitors = fieldNets[index].Capacitors;
                List<CompFieldGenerator> generators = fieldNets[index].Generators;
                foreach (CompFieldConduit i in conduits) {
                    i.NetworkID = 0;
                }
                foreach (CompFieldGenerator i in generators) {
                    i.IsGenerating = false;
                }
                usedIndices.Remove(index);
                fieldNets.Remove(index);
                foreach (CompFieldConduit i in conduits) {
                    register(i);
                }
                //relies on conduit of parent to be already in place
                foreach (CompFieldCapacitor i in capacitors) {
                    register(i);
                }
                foreach (CompFieldGenerator i in generators) {
                    register(i);
                }
            }
        }
        
        //handles registering conduits to FieldNets
        //handles creation of new FieldNets
        public void register(CompFieldConduit conduit) {
            int x = conduit.parent.Position.x;
            int z = conduit.parent.Position.z;

            List<int> adjacentNets = checkAdjacentNets(x, z, ConduitArray);
            int index = searchNewIndex(0,0,0);

            if (fieldNets.Count == 0 || adjacentNets.NullOrEmpty()) {
                //ConduitArray[x, z] = index;
                usedIndices.Add(index);
                fieldNets.Add(index, new FieldNet(index, new List<CompFieldConduit>()));
                fieldNets[index].register(conduit);
                return;
            } else if (adjacentNets.Count == 1) {
                index = adjacentNets.First();
                //ConduitArray[x, z] = index;
                fieldNets[index].register(conduit);
            } else {
                FieldNet hold = fieldNets[adjacentNets[0]];
                FieldNet temp;
                for(int i = 1; i < adjacentNets.Count; i++) {
                    index = adjacentNets[i];

                    temp = fieldNets[index];
                    fieldNets.Remove(index);
                    usedIndices.Remove(index);

                    //replaceNonZeroIndices(index, adjacentNets[0], ConduitArray);
                    hold.register(temp.Conduits);
                    hold.register(temp.Capacitors);
                }
                //ConduitArray[x, z] = adjacentNets[0];
                hold.register(conduit);
            }

        }

        //all other registrations must be preceded by the registration of the associated conduit

        public void register(CompFieldCapacitor capacitor) {
            fieldNets[capacitor.NetworkID].register(capacitor);
        }

        public void deregister(CompFieldCapacitor capacitor) {
            try {
                fieldNets[capacitor.NetworkID].deregister(capacitor);
            } catch (KeyNotFoundException e) {
                Messages.Message("FieldNet destroyed", MessageSound.Standard);
            }
        }

        public void register(CompFieldGenerator generator) {
            fieldNets[generator.NetworkID].register(generator);
        }

        public void deregister(CompFieldGenerator generator) {
            try {
                fieldNets[generator.NetworkID].deregister(generator);
            } catch (KeyNotFoundException e) {
                Messages.Message("FieldNet destroyed", MessageSound.Standard);
            }
        }
    }
}
