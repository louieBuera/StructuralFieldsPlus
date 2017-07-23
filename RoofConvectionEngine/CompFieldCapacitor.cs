using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace StructuralFieldsPlusTesting {
    class CompFieldCapacitor : ThingComp{
        private IntVec3 position;
        private FieldMap fieldMap;
        private float currentField;

        public int NetworkID {
            get => fieldMap.ConduitArray[position.x, position.z];
            set => fieldMap.ConduitArray[position.x, position.z] = value;
        }
        public IntVec3 Position { get => position; set => position = value; }

        public float StoredFieldMax { get => ((CompProperties_FieldCapacitor)this.props).storedFieldMax; }

        public float UnusedStorage { get => StoredFieldMax - currentField; }

        public CompProperties_FieldCapacitor Props {
            get {
                return (CompProperties_FieldCapacitor)this.props;
            }
        }

        public float CurrentField { get => currentField; set => currentField = value; }

        public override void PostExposeData() {
            base.PostExposeData();
  
            Scribe_Values.Look<float>(ref this.currentField, "currentField", 0, false);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad) {
            base.PostSpawnSetup(respawningAfterLoad);
            position = parent.Position;
            fieldMap = parent.Map.GetComponent<FieldMap>();
            fieldMap.register(this);
            if (!respawningAfterLoad) {
                currentField = 0;
            }
        }

        public override void PostDeSpawn(Map map) {
            base.PostDeSpawn(map);
            fieldMap.deregister(this);
        }
    }
}
