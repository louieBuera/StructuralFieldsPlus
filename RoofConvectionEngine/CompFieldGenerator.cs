using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace StructuralFieldsPlusTesting {
    class CompFieldGenerator : ThingComp {
        private IntVec3 position;
        private FieldMap fieldMap;
        private bool isGenerating = false;
        public float GenPerTick {
            get => isGenerating ? 
                ((CompProperties_FieldGenerator)props).loadedWattage / ((CompProperties_FieldGenerator)props).wattToFieldDivisor : 
                0f ;
        }
        

        public void changeOutput(float delta) {
            fieldMap.fieldNets[NetworkID].adjustGeneratorOutput(delta);
        }

        public int NetworkID {
            get => fieldMap.ConduitArray[position.x, position.z];
            set => fieldMap.ConduitArray[position.x, position.z] = value;
        }
        public IntVec3 Position { get => position; set => position = value; }
        
        public CompProperties_FieldGenerator Props {
            get {
                return (CompProperties_FieldGenerator)this.props;
            }
        }

        public bool IsGenerating { get => isGenerating; set => isGenerating = value; }

        public override void PostSpawnSetup(bool respawningAfterLoad) {
            base.PostSpawnSetup(respawningAfterLoad);
            position = parent.Position;
            fieldMap = parent.Map.GetComponent<FieldMap>();
            fieldMap.register(this);
        }

        public override void PostDeSpawn(Map map) {
            base.PostDeSpawn(map);
            fieldMap.deregister(this);
        }
    }
}
