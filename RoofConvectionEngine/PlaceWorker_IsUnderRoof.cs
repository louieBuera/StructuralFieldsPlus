using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace RoofConvectionEngine {
    public class PlaceWorker_IsUnderRoof : PlaceWorker {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Thing thingToIgnore = null) {
            //return base.AllowsPlacing(checkingDef, loc, rot, thingToIgnore);
            if (base.Map.roofGrid.Roofed(loc) && !base.Map.roofGrid.RoofAt(loc).isThickRoof) {
                return true;
            }
            return new AcceptanceReport("MustPlaceRoofed");
        }
    }
}