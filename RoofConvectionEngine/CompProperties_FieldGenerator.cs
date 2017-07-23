using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace StructuralFieldsPlusTesting {
    class CompProperties_FieldGenerator : CompProperties {
        public float wattToFieldPerTick = 250f / 60f;
        public float loadedWattage = 1000f;
        public float standbyWattage = 50f;
        
        public CompProperties_FieldGenerator() {
            this.compClass = typeof(CompFieldGenerator);
        }
    }
}
