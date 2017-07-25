using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using RimWorld;

namespace StructuralFieldsPlusTesting {
    class Building_FieldConduit : Building{
        public CompFieldConduit compFieldConduit;

        public int NetworkID { get => base.Map.GetComponent<FieldMap>().ConduitArray[Position.x, Position.z]; }
        public FieldNet ConnectedFieldNet { get => base.Map.GetComponent<FieldMap>().fieldNets[NetworkID]; }

        #region base functions
        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            this.compFieldConduit = base.GetComp<CompFieldConduit>();
        }

        public override void ExposeData() {
            base.ExposeData();
            this.compFieldConduit = base.GetComp<CompFieldConduit>();
        }

        //redirect damage to field
        public override void PreApplyDamage(DamageInfo dInfo, out bool absorbed) {
            base.PreApplyDamage(dInfo, out absorbed);
            if (absorbed) {
                return;
            }
            if (((CompPowerTrader)PowerComp).PowerOn) {
                ConnectedFieldNet.preApplyDamage(dInfo, out absorbed);
            }
            return;
        }

        public override string GetInspectString() {
            return "NetID: " + compFieldConduit.NetworkID.ToString()
                + "\nAvailable: " + ConnectedFieldNet.AvailableField.ToString();
        }
        #endregion

    }
}
