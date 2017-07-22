using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace StructuralFieldsPlusTesting {
    public class CompFieldConduit : ThingComp {
        //private int networkID;
        private IntVec3 position;
        private FieldMap fieldMap;

        public int NetworkID {
            get => fieldMap.ConduitArray[position.x, position.z];
            set => fieldMap.ConduitArray[position.x, position.z] = value;
        }

        /*
        public CompFieldConduit() {
            this.networkID = (new Random()).Next();
        }
        */

        /*public override void PostExposeData() {
            base.PostExposeData();

            Scribe_Values.Look<int>(ref this.networkID, "networkID", -1, false);
        }*/

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
            map.GetComponent<FieldMap>().deregister(this);
        }


    }
}
