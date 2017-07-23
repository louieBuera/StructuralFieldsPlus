using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace StructuralFieldsPlusTesting{
    class Building_ShieldedTurret : Building_TurretGun {
        /*private float currentShields = 0;
        private float maxShields = 150;
        private float regenPerSecond = 1.2f;*/

        public CompFieldConduit compFieldConduit = new CompFieldConduit();
        public CompFieldCapacitor compFieldCapacitor = new CompFieldCapacitor();
        public CompFieldGenerator compFieldGenerator = new CompFieldGenerator();

        public int NetworkID { get => compFieldConduit.NetworkID;  }
        public FieldNet ConnectedFieldNet { get => base.Map.GetComponent<FieldMap>().fieldNets[NetworkID]; }
        //public float AvailableField { get => ConnectedFieldNet.CurrentField - ConnectedFieldNet.DeferDamage; }

        

        public override void PreApplyDamage(DamageInfo dInfo, out bool absorbed) {
            base.PreApplyDamage(dInfo, out absorbed);
            if (absorbed) {
                return;
            }
            ConnectedFieldNet.preApplyDamage(dInfo, out absorbed);
            return;
        }

        

        public override void ExposeData() {
            base.ExposeData();
            this.compFieldConduit = base.GetComp<CompFieldConduit>();
            this.compFieldCapacitor = base.GetComp<CompFieldCapacitor>();
            this.compFieldGenerator = base.GetComp<CompFieldGenerator>();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            this.compFieldConduit = base.GetComp<CompFieldConduit>();
            this.compFieldCapacitor = base.GetComp<CompFieldCapacitor>();
            this.compFieldGenerator = base.GetComp<CompFieldGenerator>();
        }

        public override string GetInspectString() {
            StringBuilder stringBuilder = new StringBuilder();

            // Add the inspections string from the base
            string baseString = base.GetInspectString();
            if (!baseString.NullOrEmpty()) {
                stringBuilder.Append(baseString);
                stringBuilder.AppendLine();
            }
            stringBuilder.Append("Field: ");
            stringBuilder.Append(string.Format("{0:N8}", ConnectedFieldNet.AvailableField));
            // return the complete string
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override void Tick() {
            base.Tick();
            /*currentShields += regenPerSecond / 60;
            if(currentShields > maxShields) {
                currentShields = maxShields;
            }*/
        }
    }
}
