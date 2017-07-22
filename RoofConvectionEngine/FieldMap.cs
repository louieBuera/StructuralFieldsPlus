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

        int[,] embrasureArray;
        int[,] roofArray;
        int[,] conduitArray;

        Dictionary<int, FieldNet> fieldNets;
        List<int> usedIndices;// = new List<int>();
        
        public FieldMap(Map map) : base(map) {

            x_size = map.Size.x;
            z_size = map.Size.z;
            //init value = 0
            embrasureArray = new int[x_size, z_size];
            roofArray = new int[x_size, z_size];
            conduitArray = new int[x_size, z_size];

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
        public List<IntVec2> checkAdjacentCells(int x, int z, int netIDFind) {
            List<IntVec2> cells = new List<IntVec2>();

            if (x + 1 < x_size && conduitArray[x + 1, z] == netIDFind) {
                cells.Add(new IntVec2(x + 1, z));
            }
            if (x - 1 >= 0 && conduitArray[x - 1, z] == netIDFind) {
                cells.Add(new IntVec2(x - 1, z));
            }
            if (z + 1 < z_size && conduitArray[x, z + 1] == netIDFind) {
                cells.Add(new IntVec2(x, z + 1));
            }
            if (z - 1 >= 0 && conduitArray[x, z - 1] == netIDFind) {
                cells.Add(new IntVec2(x, z - 1));
            }

            return cells;
        }

        public List<IntVec2> checkAdjacentCellsAvoid(int netIDAvoid, int x, int z) {
            List<IntVec2> cells = new List<IntVec2>();

            if (x + 1 < x_size && conduitArray[x + 1, z] != netIDAvoid) {
                cells.Add(new IntVec2(x + 1, z));
            }
            if (x - 1 >= 0 && conduitArray[x - 1, z] != netIDAvoid) {
                cells.Add(new IntVec2(x - 1, z));
            }
            if (z + 1 < z_size && conduitArray[x, z + 1] != netIDAvoid) {
                cells.Add(new IntVec2(x, z + 1));
            }
            if (z - 1 >= 0 && conduitArray[x, z - 1] != netIDAvoid) {
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
        /*public int replaceNonZeroIndices(int[,] old, int[,] neo) {
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
        }*/
        
        //only to be used when immediateAdjacentCells.Count > 1
        
        /*public List<FieldNet> splitNet(int x, int z, short netID, List<IntVec2> immediateAdjacentCells) {
            short[,] cloneArray = (short[,])conduitArray.Clone();
            List<FieldNet> newFieldNets = new List<FieldNet>();
            
            //List<IntVec2> immediateAdjacentCells = checkAdjacentCells(x, z, netID);

            short tempID = searchNewIndex(0,0,0);

            //counts number of cells switched to new network
            //int ctr = 0;
            FieldNet initializing;
            CompFieldConduit conduit;
            FieldNet source = fieldNets[netID];
            List<CompFieldConduit> conduitsClone = new List<CompFieldConduit>();
            List<CompFieldConduit> removeConduits = new List<CompFieldConduit>();*/
            /*foreach(CompFieldConduit i in source.Conduits) {
                conduitsClone.Add()
            }*/
            /*conduitsClone.AddRange(source.Conduits);
            short avoid1 = 0;
            short avoid2 = 0;
            short avoid3 = 0;

            //vars for checking if disjoint networks
            List<IntVec2> bfsQueue = new List<IntVec2>();
            List<IntVec2> intermediate;
            IntVec2 operand;
            IntVec2 operand2;
            
            if (immediateAdjacentCells.Count == 2) {
                operand = immediateAdjacentCells[0];
                cloneArray[operand.x, operand.z] = tempID;
                intermediate = checkAdjacentCellsAvoid(tempID, operand.x, operand.z);
                foreach (IntVec2 i in intermediate) {
                    cloneArray[i.x, i.z] = tempID;
                }
                bfsQueue.AddRange(intermediate);
                while(bfsQueue.Count > 0 && cloneArray[immediateAdjacentCells[1].x, immediateAdjacentCells[1].z] != tempID) {
                    operand = deque(bfsQueue);
                    cloneArray[operand.x, operand.z] = tempID;
                    intermediate = checkAdjacentCellsAvoid(tempID, operand.x, operand.z);
                    foreach (IntVec2 i in intermediate) {
                        cloneArray[i.x, i.z] = tempID;
                    }
                    bfsQueue.AddRange(intermediate);
                }
                if(cloneArray[immediateAdjacentCells[1].x, immediateAdjacentCells[1].z] == tempID) {
                    return newFieldNets;
                } else {
                    //adjust conduitArray
                    replaceNonZeroIndices(conduitArray, cloneArray);
                    //initilize and add FieldNet
                    initializing = new FieldNet(tempID, new List<CompFieldConduit>());
                    newFieldNets.Add(initializing);
                    //source = fieldNets[netID];
                    for(int i = 0; i < conduitsClone.Count; i++) {
                        conduit = conduitsClone[i];
                        if(cloneArray[conduit.parent.Position.x, conduit.parent.Position.z] == tempID) {
                            removeConduits.Add(conduit);
                            source.deregister(conduit);
                            initializing.register(conduit);
                        }
                    }
                    foreach (CompFieldConduit i in removeConduits) {
                        conduitsClone.Remove(i);
                    }
                    return newFieldNets;
                }
            } else {
                int net = 0;
                while(immediateAdjacentCells.Count > 0) {
                    operand = deque(immediateAdjacentCells);
                    cloneArray[operand.x, operand.z] = tempID;
                    intermediate = checkAdjacentCellsAvoid(tempID, operand.x, operand.z);
                    foreach (IntVec2 i in intermediate) {
                        cloneArray[i.x, i.z] = tempID;
                    }
                    bfsQueue.AddRange(intermediate);
                    while (bfsQueue.Count > 0 && immediateAdjacentCells.Count > 0) {
                        operand = deque(bfsQueue);
                        cloneArray[operand.x, operand.z] = tempID;
                        intermediate = checkAdjacentCellsAvoid(tempID, operand.x, operand.z);
                        foreach (IntVec2 i in intermediate) {
                            cloneArray[i.x, i.z] = tempID;
                        }
                        bfsQueue.AddRange(intermediate);
                        for (int i = 0; i < immediateAdjacentCells.Count; i++) {
                            operand2 = immediateAdjacentCells[i];
                            if (cloneArray[operand2.x, operand2.z] == tempID) {
                                immediateAdjacentCells.Remove(operand2);
                                break;
                            }
                        }

                    }
                    if (net == 0 && immediateAdjacentCells.Count == 0) {
                        return newFieldNets;
                    }
                    initializing = new FieldNet(tempID, new List<CompFieldConduit>());
                    newFieldNets.Add(initializing);
                    //source = fieldNets[netID];
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
                    if(net == 1) {
                        avoid1 = tempID;
                    } else if(net == 2){
                        avoid2 = tempID;
                    } else if (net == 3) {
                        avoid3 = tempID;
                    }
                    tempID = searchNewIndex(avoid1, avoid2, avoid3);
                }
                replaceNonZeroIndices(conduitArray, cloneArray);
                return newFieldNets;
            }
        }*/

        //handles removal of empty FieldNets
        /*public void deregister(CompFieldConduit conduit) {
            int x = conduit.parent.Position.x;
            int z = conduit.parent.Position.z;

            short index = conduitArray[x, z];

            List<IntVec2> adjacentCells = checkAdjacentCells(x, z, index);

            conduitArray[x, z] = 0;
            fieldNets[index].deregister(conduit);

            //case single element of FieldNet, remove
            if (adjacentCells.Count == 0) {
                usedIndices.Remove(index);
                fieldNets.Remove(index);
            } else if (adjacentCells.Count > 1) {

            }
        }*/

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

            List<int> adjacentNets = checkAdjacentNets(x, z, conduitArray);
            Messages.Message(toString(adjacentNets), MessageSound.Standard);
            int index = searchNewIndex(0,0,0);

            if (fieldNets.Count == 0 || adjacentNets.NullOrEmpty()) {
                conduitArray[x, z] = index;
                usedIndices.Add(index);
                fieldNets.Add(index, new FieldNet(index, new List<CompFieldConduit>()));
                fieldNets[index].register(conduit);
                return;
            } else if (adjacentNets.Count == 1) {
                conduitArray[x, z] = index;
                index = adjacentNets.First();
                fieldNets[index].register(conduit);
            } else {
                FieldNet hold = fieldNets[adjacentNets[0]];
                FieldNet temp;
                for(int i = 1; i < adjacentNets.Count; i++) {
                    index = adjacentNets[i];

                    temp = fieldNets[index];
                    fieldNets.Remove(index);

                    usedIndices.Remove(index);

                    replaceNonZeroIndices(index, adjacentNets[0], conduitArray);
                    hold.register(temp.Conduits);
                }
                conduitArray[x, z] = adjacentNets[0];
                hold.register(conduit);
            }

        }

        /*public void register(CompFieldConduit conduit) {
            int index = searchNewIndex(0, 0, 0);
            conduit.networkID = index;
            usedIndices.Add(index);
        }*/

        public void deregister(CompFieldConduit conduit) {
            usedIndices.Remove(conduit.networkID);
        }
    }
}
