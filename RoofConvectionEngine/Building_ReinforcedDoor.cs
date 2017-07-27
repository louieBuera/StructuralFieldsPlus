using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace StructuralFieldsPlusTesting{
    class Building_ReinforcedDoor : Building_Door{
        public CompFieldConduit compFieldConduit;

        public int NetworkID { get => base.Map.GetComponent<FieldMap>().ConduitArray[Position.x, Position.z]; }
        public FieldNet ConnectedFieldNet { get => base.Map.GetComponent<FieldMap>().fieldNets[NetworkID]; }

        public override void ExposeData() {
            base.ExposeData();
            this.compFieldConduit = base.GetComp<CompFieldConduit>();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            this.compFieldConduit = base.GetComp<CompFieldConduit>();
        }

        public override string GetInspectString() {
            StringBuilder stringBuilder = new StringBuilder();

            return "NetID: " + NetworkID.ToString() +
                "\nAvailable Field: " + string.Format("{0:N8}", ConnectedFieldNet.AvailableField) + "\n" +
                base.GetInspectString();
            // return the complete string
        }

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
    }
}
