using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace StructuralFieldsPlusTesting{
    class Building_ShieldedTurret : Building_TurretGun {
        public CompFieldConduit compFieldConduit = new CompFieldConduit();
        public CompFieldCapacitor compFieldCapacitor = new CompFieldCapacitor();
        public CompFieldGenerator compFieldGenerator = new CompFieldGenerator();
        //private bool isGenerating = false;
        //private float origFunctionPower;

        private int ticker = 0;

        public int NetworkID { get => base.Map.GetComponent<FieldMap>().ConduitArray[Position.x, Position.z]; }
        public FieldNet ConnectedFieldNet { get => base.Map.GetComponent<FieldMap>().fieldNets[NetworkID]; }

        //redirect damage to field
        public override void PreApplyDamage(DamageInfo dInfo, out bool absorbed) {
            base.PreApplyDamage(dInfo, out absorbed);
            if (absorbed) {
                return;
            }
            if (((CompPowerTrader)PowerComp).PowerOn) {
                ConnectedFieldNet.preApplyDamage(dInfo, out absorbed);
            }
            //ConnectedFieldNet.preApplyDamage(dInfo, out absorbed);
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
            stringBuilder.Append("Available Field: ");
            stringBuilder.Append(string.Format("{0:N8}", ConnectedFieldNet.AvailableField));
            stringBuilder.Append("\nLocal Field: ");
            stringBuilder.Append(string.Format("{0:N8}", compFieldCapacitor.CurrentField));
            // return the complete string
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override void Tick() {
            base.Tick();
            ticker++;
            if (ticker == 60) {
                compFieldGenerator.CompTickRareExtra();
                ticker = 0;
            }
            /*if(!compFieldGenerator.IsGenerating && powerComp.PowerOn && ConnectedFieldNet.UnusedStorage >= 1) {
                //IsGnerating has to be updated to update GenPerTick, before adjustingGeneratorOutput from connectedFieldNet
                compFieldGenerator.IsGenerating = true;
                ConnectedFieldNet.adjustGeneratorOutput(compFieldGenerator.GenPerTick);
                base.powerComp.PowerOutput = -((CompProperties_Power)this.powerComp.props).basePowerConsumption -
                    ((CompProperties_FieldGenerator)this.compFieldGenerator.props).loadedWattage;
            } else if (compFieldGenerator.IsGenerating && (!powerComp.PowerOn || ConnectedFieldNet.UnusedStorage < 1)) {
                //has to be applied before shutting down generator
                ConnectedFieldNet.adjustGeneratorOutput(-compFieldGenerator.GenPerTick);
                compFieldGenerator.IsGenerating = false;
                base.powerComp.PowerOutput = -((CompProperties_Power)this.powerComp.props).basePowerConsumption -
                    ((CompProperties_FieldGenerator)this.compFieldGenerator.props).standbyWattage;
            }*/
            /*currentShields += regenPerSecond / 60;
            if(currentShields > maxShields) {
                currentShields = maxShields;
            }*/
        }
    }
}
