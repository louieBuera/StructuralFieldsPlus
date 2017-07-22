using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace StructuralFieldsPlusTesting {
    public class CompFieldConduit : ThingComp {
        //private int networkID;

        public int NetworkID {
            get => parent.Map.GetComponent<FieldMap>().ConduitArray[parent.Position.x, parent.Position.z];
            set => parent.Map.GetComponent<FieldMap>().ConduitArray[parent.Position.x, parent.Position.z] = value;
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
            if (!respawningAfterLoad) {
                parent.Map.GetComponent<FieldMap>().register(this);
            }
        }

        public override void PostDeSpawn(Map map) {
            base.PostDeSpawn(map);
            Messages.Message("DeSpawn Triggered", MessageSound.Standard);
            map.GetComponent<FieldMap>().deregister(this);
        }


    }
}
