using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace RoofConvectionEngine{
    class Building_ShieldedTurret : Building_TurretGun {
        private float currentShields = 0;
        private float maxShields = 150;
        private float regenPerSecond = 1.2f;

        public override void PreApplyDamage(DamageInfo dInfo, out bool absorbed) {
            base.PreApplyDamage(dInfo, out absorbed);
            if (absorbed) {
                return;
            }

            if(dInfo.Amount < currentShields) {
                currentShields -= dInfo.Amount;
                absorbed = true;
            } else {
                dInfo.SetAmount(dInfo.Amount - (int)Math.Floor(currentShields));
                currentShields -= (float)Math.Floor(currentShields);
                absorbed = false;
            }
        }
        


        public override string GetInspectString() {
            StringBuilder stringBuilder = new StringBuilder();

            // Add the inspections string from the base
            string baseString = base.GetInspectString();
            if (!baseString.NullOrEmpty()) {
                stringBuilder.Append(baseString);
                stringBuilder.AppendLine();
            }
            stringBuilder.Append("Shield: ");
            stringBuilder.Append(string.Format("{0:N8}", currentShields));
            // return the complete string
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override void Tick() {
            base.Tick();
            currentShields += regenPerSecond / 60;
            if(currentShields > maxShields) {
                currentShields = maxShields;
            }
        }
    }
}
