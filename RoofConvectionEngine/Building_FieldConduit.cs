using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using RimWorld;

namespace StructuralFieldsPlusTesting {
    class Building_FieldConduit : Building{
        public CompFieldConduit compFieldConduit;
        public CompFieldCapacitor compFieldCapacitor;
        
        public Building_FieldConduit() {
            compFieldConduit = new CompFieldConduit();
            compFieldCapacitor = new CompFieldCapacitor();
        }

        #region base functions
        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            this.compFieldConduit = base.GetComp<CompFieldConduit>();
            this.compFieldCapacitor = base.GetComp<CompFieldCapacitor>();
            
        }
        


        public override void ExposeData() {
            base.ExposeData();
            this.compFieldConduit = base.GetComp<CompFieldConduit>();
            this.compFieldCapacitor = base.GetComp<CompFieldCapacitor>();
        }

        public override string GetInspectString() {
            return "NetID: " + compFieldConduit.NetworkID.ToString() 
                + "\nCapacity: " + compFieldCapacitor.StoredFieldMax.ToString() 
                + "\nStored: " + compFieldCapacitor.CurrentField.ToString();
        }
        #endregion

    }
}
