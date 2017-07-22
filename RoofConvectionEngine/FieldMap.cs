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

        Dictionary<int, FieldNet> fieldNets;
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

        public List<IntVec2> checkAdjacentCellsAvoid(int netIDAvoid, int x, int z) {
            List<IntVec2> cells = new List<IntVec2>();

            if (x + 1 < x_size && ConduitArray[x + 1, z] != 0 && ConduitArray[x + 1, z] != netIDAvoid) {
                cells.Add(new IntVec2(x + 1, z));
            }
            if (x - 1 >= 0 && ConduitArray[x - 1, z] != 0 && ConduitArray[x - 1, z] != netIDAvoid) {
                cells.Add(new IntVec2(x - 1, z));
            }
            if (z + 1 < z_size && ConduitArray[x, z + 1] != 0 && ConduitArray[x, z + 1] != netIDAvoid) {
                cells.Add(new IntVec2(x, z + 1));
            }
            if (z - 1 >= 0 && ConduitArray[x, z - 1] != 0 && ConduitArray[x, z - 1] != netIDAvoid) {
                cells.Add(new IntVec2(x, z - 1));
            }

            return cells;
        }

        //set avoid to 0 to use as 
        public int searchNewIndex(int avoid1, int avoid2, int avoid3 ) {
            for(int i= startIndex; i < stopIndex; i++) {
                if (i != avoid1 && i != avoid2 && i != avoid3 && !usedIndices.Contains(i)) {
                    return i;
                }
            }
            return -1;
        }

        //use for merge networks
        public int replaceNonZeroIndices(int old, int neo, int[,] searchArray) {
            int count = 0;
            for(int x = 0; x < x_size; x++) {
                for(int z = 0; z < z_size; z++) {
                    if(searchArray[x, z] != 0 && searchArray[x, z] == old) {
                        searchArray[x, z] = neo;
                        count++;
                    }
                }
            }
            return count;
        }

        //use for split networks
        public int replaceNonZeroIndices(int[,] old, int[,] neo) {
            int count = 0;
            for (int x = 0; x < x_size; x++) {
                for (int z = 0; z < z_size; z++) {
                    if (neo[x, z] != 0) {
                        old[x, z] = neo[x, z];
                        count++;
                    }
                }
            }
            return count;
        }

        //convenience function
        public T deque<T>(List<T> list) {
            T hold = list[0];
            list.RemoveAt(0);
            return hold;
        }
        
        //only to be used when immediateAdjacentCells.Count > 1
        public List<FieldNet> splitNet(int x, int z, int netID, List<IntVec2> immediateAdjacentCells) {
            int[,] cloneArray = (int[,])ConduitArray.Clone();
            List<FieldNet> newFieldNets = new List<FieldNet>();
            
            //List<IntVec2> immediateAdjacentCells = checkAdjacentCells(x, z, netID);

            int tempID = searchNewIndex(0,0,0);

            //counts number of cells switched to new network
            //int ctr = 0;
            FieldNet initializing;
            CompFieldConduit conduit;
            FieldNet source = fieldNets[netID];
            List<CompFieldConduit> conduitsClone = new List<CompFieldConduit>();
            List<CompFieldConduit> removeConduits = new List<CompFieldConduit>();
            /*foreach(CompFieldConduit i in source.Conduits) {
                conduitsClone.Add()
            }*/
            conduitsClone.AddRange(source.Conduits);
            int avoid1 = 0;
            int avoid2 = 0;
            int avoid3 = 0;

            //vars for checking if disjoint networks
            List<IntVec2> bfsQueue = new List<IntVec2>();
            List<IntVec2> intermediate;
            IntVec2 operand;
            IntVec2 operand2;

            int net = 0;
            Messages.Message("Net", MessageSound.Standard);
            while (immediateAdjacentCells.Count > 0) {
                operand = deque(immediateAdjacentCells);
                cloneArray[operand.x, operand.z] = tempID;
                intermediate = checkAdjacentCellsMatch(operand.x, operand.z, netID);
                foreach (IntVec2 i in intermediate) {
                    cloneArray[i.x, i.z] = tempID;
                }
                bfsQueue.AddRange(intermediate);
                while (bfsQueue.Count > 0 && immediateAdjacentCells.Count > 0) {
                    operand = deque(bfsQueue);
                    cloneArray[operand.x, operand.z] = tempID;
                    intermediate = checkAdjacentCellsMatch(operand.x, operand.z, netID);
                    Messages.Message("intermediate: " + intermediate.Count.ToString(), MessageSound.Standard);
                    foreach (IntVec2 i in intermediate) {
                        cloneArray[i.x, i.z] = tempID;
                    }
                    bfsQueue.AddRange(intermediate);
                    for (int i = 0; i < immediateAdjacentCells.Count; i++) {
                        operand2 = immediateAdjacentCells[i];
                        if (cloneArray[operand2.x, operand2.z] == tempID) {
                            //immediateAdjacentCells.Remove(operand2);
                            intermediate.Add(operand2);
                            //break;
                        }
                    }
                    foreach (IntVec2 i in intermediate) {
                        immediateAdjacentCells.Remove(i);
                    }

                }
                //check if network is intact
                if (net == 0 && immediateAdjacentCells.Count == 0) {
                    Messages.Message("no split", MessageSound.Standard);
                    return newFieldNets;
                }
                //initialize new network
                initializing = new FieldNet(tempID, new List<CompFieldConduit>());
                newFieldNets.Add(initializing);
                //source = fieldNets[netID];
                //transfer conduits to new network
                for (int i = 0; i < conduitsClone.Count; i++) {
                    conduit = conduitsClone[i];
                    if (cloneArray[conduit.parent.Position.x, conduit.parent.Position.z] == tempID) {
                        removeConduits.Add(conduit);
                        source.deregister(conduit);
                        initializing.register(conduit);
                    }
                }
                foreach (CompFieldConduit i in removeConduits) {
                    conduitsClone.Remove(i);
                }
                net++;
                Messages.Message("Net" + net.ToString(), MessageSound.Standard);
                //set new FieldNet's ID to be avoided in search for new tempID
                if (net == 1) {
                    avoid1 = tempID;
                } else if (net == 2) {
                    avoid2 = tempID;
                } else if (net == 3) {
                    avoid3 = tempID;
                } else if (net == 4) {
                    fieldNets.Remove(netID);
                    usedIndices.Remove(netID);
                }
                tempID = searchNewIndex(avoid1, avoid2, avoid3);
            }
            replaceNonZeroIndices(ConduitArray, cloneArray);
            return newFieldNets;
        }

        //handles removal of empty FieldNets
        public void deregister(CompFieldConduit conduit) {
            int x = conduit.parent.Position.x;
            int z = conduit.parent.Position.z;

            int index = ConduitArray[x, z];

            List<IntVec2> adjacentCells = checkAdjacentCellsMatch(x, z, index);

            //handles case one adjacent conduit, needed for all cases
            ConduitArray[x, z] = 0;
            fieldNets[index].deregister(conduit);

            //case single element of FieldNet, remove FieldNet
            if (adjacentCells.Count == 0) {
                usedIndices.Remove(index);
                fieldNets.Remove(index);
            //case more than one adjacent conduit
            } else if (adjacentCells.Count > 1) {
                /*Messages.Message("AdjacentCells: " + adjacentCells.Count.ToString(), MessageSound.Standard);
                List<FieldNet> temp = splitNet(x, z, index, adjacentCells);
                Messages.Message("Count: " + temp.Count.ToString(), MessageSound.Standard);
                foreach (FieldNet i in temp) {
                    fieldNets.Add(i.NetID,i);
                    usedIndices.Add(i.NetID);
                }*/

                //rebuilt net instead
                List<CompFieldConduit> conduits = fieldNets[index].Conduits;
                conduits.Remove(conduit);
                foreach (CompFieldConduit i in conduits) {
                    i.NetworkID = 0;
                }
                usedIndices.Remove(index);
                fieldNets.Remove(index);
                foreach (CompFieldConduit i in conduits) {
                    register(i);
                }
            }
        }

        public string toString(List<int> list) {
            string temp = "";

            foreach(int i in list){
                temp += i.ToString() + "";
            }

            return temp;
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
                }
                //ConduitArray[x, z] = adjacentNets[0];
                hold.register(conduit);
            }

        }

        /*public void register(CompFieldConduit conduit) {
            int index = searchNewIndex(0, 0, 0);
            conduit.networkID = index;
            usedIndices.Add(index);
        }*/

        /*public void deregister(CompFieldConduit conduit) {
            usedIndices.Remove(conduit.networkID);
        }*/
    }
}
