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

        public int x_size;
        public int z_size;

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

        //checks all positions 1 cell north, east, west, and south for single cell
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

        //checks all positions 1 cell north, east, west, and south for multi-cell
        public List<int> checkAdjacentNets(int x, int z, int[,] searchArray, int width, int height) {
            List<int> IDs = new List<int>();
            int ID;
            int top, bottom, left, right;
            //still part of the building
            top = z + height / 2;
            bottom = z - (height - 1) / 2;
            left = x - (width - 1) / 2;
            right = x + width / 2;
            //adjacent to the building
            int above, below, toTheLeft, toTheRight;
            above = z + height / 2 + 1;
            below = z - (height - 1) / 2 - 1;
            toTheLeft = x - (width - 1) / 2 - 1;
            toTheRight = x + width / 2 + 1;
            
            //top edge
            if(above < z_size) {
                for(int i = left; i <= right; i++) {
                    ID = searchArray[i, above];
                    if (ID != 0 && !IDs.Contains(ID)) {
                        IDs.Add(ID);
                    }
                }
            }
            //bottom edge
            if(below >= 0) {
                for (int i = left; i <= right; i++) {
                    ID = searchArray[i, below];
                    if (ID != 0 && !IDs.Contains(ID)) {
                        IDs.Add(ID);
                    }
                }
            }
            //right edge
            if(toTheRight < x_size) {
                for(int i = bottom; i <= top; i++) {
                    ID = searchArray[toTheRight, i];
                    if (ID != 0 && !IDs.Contains(ID)) {
                        IDs.Add(ID);
                    }
                }
            }
            //left edge
            if (toTheLeft >= 0) {
                for (int i = bottom; i <= top; i++) {
                    ID = searchArray[toTheLeft, i];
                    if (ID != 0 && !IDs.Contains(ID)) {
                        IDs.Add(ID);
                    }
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
        
        public List<IntVec2> checkAdjacentCellsMatch(int x, int z, int netIDFind, int width, int height) {
            List<IntVec2> cells = new List<IntVec2>();
            int top, bottom, left, right;
            //still part of the building
            top = z + height / 2;
            bottom = z - (height - 1) / 2;
            left = x - (width - 1) / 2;
            right = x + width / 2;
            //adjacent to the building
            int above, below, toTheLeft, toTheRight;
            above = z + height / 2 + 1;
            below = z - (height - 1) / 2 - 1;
            toTheLeft = x - (width - 1) / 2 - 1;
            toTheRight = x + width / 2 + 1;

            //top edge
            if (above < z_size) {
                for (int i = left; i <= right; i++) {
                    if (conduitArray[i, above] == netIDFind) {
                        cells.Add(new IntVec2(i, above));
                    }
                }
            }
            //bottom edge
            if (below >= 0) {
                for (int i = left; i <= right; i++) {
                    if (conduitArray[i, below] == netIDFind) {
                        cells.Add(new IntVec2(i, below));
                    }
                }
            }
            //right edge
            if (toTheRight < x_size) {
                for (int i = bottom; i <= top; i++) {
                    if (conduitArray[toTheRight, i] == netIDFind) {
                        cells.Add(new IntVec2(toTheRight, i));
                    }
                }
            }
            //left edge
            if (toTheLeft >= 0) {
                for (int i = bottom; i <= top; i++) {
                    if (conduitArray[toTheLeft, i] == netIDFind) {
                        cells.Add(new IntVec2(toTheLeft, i));
                    }
                }
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
        //only gets called when parent Building is despawned
        public void deregister(CompFieldConduit conduit) {
            int x = conduit.parent.Position.x;
            int z = conduit.parent.Position.z;

            int index = ConduitArray[x, z];

            List<IntVec2> adjacentCells;

            if (conduit.parent.RotatedSize.Equals(new IntVec2(1, 1))) {
                adjacentCells = checkAdjacentCellsMatch(x, z, index);
            } else {
                adjacentCells = checkAdjacentCellsMatch(x, z, index, conduit.parent.RotatedSize.x, conduit.parent.RotatedSize.z);
            }

            //handles case one adjacent conduit, needed for all cases
            fieldNets[index].deregister(conduit);
            var property = conduit.parent.GetType().GetProperty("compFieldCapacitor");
            if(property != null) {
                fieldNets[index].deregister(conduit.parent.GetComp<CompFieldCapacitor>());
            }
            property = conduit.parent.GetType().GetProperty("compFieldGenerator");
            if (property != null) {
                fieldNets[index].deregister(conduit.parent.GetComp<CompFieldGenerator>());
            }

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
                    if(i.parent.RotatedSize.x != 1 || i.parent.RotatedSize.z != 1) {
                        i.cleanupNetworkID();
                    }
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

            List<int> adjacentNets;
            if (conduit.parent.RotatedSize.Equals(new IntVec2(1, 1))) {
                adjacentNets = checkAdjacentNets(x, z, ConduitArray);
            } else {
                adjacentNets = checkAdjacentNets(x, z, ConduitArray, conduit.parent.RotatedSize.x, conduit.parent.RotatedSize.z);
            }
            int index = searchNewIndex(0,0,0);

            if (fieldNets.Count == 0 || adjacentNets.NullOrEmpty()) {
                //ConduitArray[x, z] = index;
                usedIndices.Add(index);
                fieldNets.Add(index, new FieldNet(index, new List<CompFieldConduit>()));
                Log.Message("fieldNets.Count == 0 || adjacentNets.NullOrEmpty()");
                fieldNets[index].register(conduit);
                return;
            } else if (adjacentNets.Count == 1) {
                index = adjacentNets.First();
                //ConduitArray[x, z] = index;
                Log.Message("adjacentNets.Count == 1");
                Log.Message(index.ToString());
                Log.Message(conduit.parent.Label);
                Log.Message(conduit.parent.Position.ToString());
                fieldNets[index].register(conduit);
            } else {
                Log.Message("else");
                FieldNet hold = fieldNets[adjacentNets[0]];
                FieldNet temp;
                for(int i = 1; i < adjacentNets.Count; i++) {
                    index = adjacentNets[i];
                    Log.Message("assign :" + index.ToString());
                    temp = fieldNets[index];
                    Log.Message("remove: :" + index.ToString());
                    fieldNets.Remove(index);
                    usedIndices.Remove(index);

                    //replaceNonZeroIndices(index, adjacentNets[0], ConduitArray);
                    hold.register(temp.Conduits);
                    hold.register(temp.Capacitors);
                    hold.register(temp.Generators);
                }
                //ConduitArray[x, z] = adjacentNets[0];
                hold.register(conduit);
                hold.flushDamageGeneration();
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
                //Messages.Message("FieldNet destroyed", MessageSound.Standard);
            }
        }

        public void register(CompFieldGenerator generator) {
            fieldNets[generator.NetworkID].register(generator);
        }

        public void deregister(CompFieldGenerator generator) {
            try {
                fieldNets[generator.NetworkID].deregister(generator);
            } catch (KeyNotFoundException e) {
                //Messages.Message("FieldNet destroyed", MessageSound.Standard);
            }
        }
    }
}
