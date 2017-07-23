using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace StructuralFieldsPlusTesting {
    class CompProperties_FieldCapacitor : CompProperties{
        public float storedFieldMax = 5000f;
        public float efficiency = 1f;

        public CompProperties_FieldCapacitor() {
            this.compClass = typeof(CompFieldCapacitor);
        }
    }
}
