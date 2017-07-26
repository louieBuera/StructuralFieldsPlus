using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace StructuralFieldsPlusTesting {
    public class CompFieldConduit : ThingComp {
        //private int networkID;
        public IntVec3 position;
        private FieldMap fieldMap;

        public int NetworkID {
            get => fieldMap.ConduitArray[position.x, position.z];
            set => fieldMap.ConduitArray[position.x, position.z] = value;
        }

        public void spreadNetworkID() {
            if(parent.RotatedSize.Equals(new IntVec2(1, 1))) {
                return;
            }
            int height, width, x, z, top, bottom, left, right;
            height = parent.RotatedSize.z;
            width = parent.RotatedSize.x;
            x = parent.Position.x;
            z = parent.Position.z;
            top = z + height / 2;
            //if dimension is even, Position lakes lower-left cell from exact center
            //just round down to compensate
            bottom = z - (height - 1) / 2;
            left = x - (width - 1) / 2;
            right = x + width / 2;
            if(top > fieldMap.z_size) {
                top = fieldMap.z_size - 1;
            }
            if(bottom < 0) {
                bottom = 0;
            }
            if (right > fieldMap.x_size) {
                right = fieldMap.x_size - 1;
            }
            if(left < 0) {
                left = 0;
            }
            for (int i = left; i <= right; i++) {
                for(int j = bottom; j <= top; j++) {
                    if (i != x || j != z) {
                        fieldMap.ConduitArray[i, j] = NetworkID;
                    }
                }
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad) {
            base.PostSpawnSetup(respawningAfterLoad);
            position = parent.Position;
            fieldMap = parent.Map.GetComponent<FieldMap>();
            //if (!respawningAfterLoad) {
            fieldMap.register(this);
            //}
        }
        
        public override void PostDeSpawn(Map map) {
            base.PostDeSpawn(map);
            Messages.Message("DeSpawn Triggered", MessageSound.Standard);
            fieldMap.deregister(this);
        }


    }
}
