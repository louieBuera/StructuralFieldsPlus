using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using RimWorld;

namespace StructuralFieldsPlusTesting {
    class Building_FieldConduit : Building{
        public CompFieldConduit compFieldConduit;
        
        public Building_FieldConduit() {
            compFieldConduit = new CompFieldConduit();
        }

        #region base functions
        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            //if (respawningAfterLoad) {
            this.compFieldConduit = base.GetComp<CompFieldConduit>();
            //} else {
            //    this.compFieldConduit = new CompFieldConduit();
            //}
            
        }
        
        public override void ExposeData() {
            base.ExposeData();
            this.compFieldConduit = base.GetComp<CompFieldConduit>();
        }

        public override string GetInspectString() {
            
            return "NetID: " + compFieldConduit.NetworkID.ToString();
        }
        #endregion

    }
}
