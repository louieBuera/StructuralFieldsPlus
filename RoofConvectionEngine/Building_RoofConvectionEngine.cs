using RimWorld;
using Verse;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoofConvectionEngine {
    class Building_RoofConvectionEngine : Building{
        public CompTempControl compTempControl;
        public CompPowerTrader compPowerTrader;

        //Custom Vars
        private float TenDegreeDeltaThermalEnergyPerSecond = 63f;
        public float minTargetTemperature = -150f;
        public float maxTargetTemperature = 150f;
        private float defaultTargetTemp = 21f;

        private float minHeatThreshold = 5f;
        private float IdlePower = -5f;
        private float TenDegreeDeltaPowerGeneration = 60f;

        private bool destroyedFlag = false;
        private float heat;

        public override void PreApplyDamage(DamageInfo dinfo, out bool absorbed) {
            base.PreApplyDamage(dinfo, out absorbed);
            if (absorbed) {
                return;
            }

        }

        #region base functions
        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            this.compTempControl = base.GetComp<CompTempControl>();
            this.compPowerTrader = base.GetComp<CompPowerTrader>();
            //this.compHeatPusher = base.GetComp<CompHeatPusher>();
        }

        public override void ExposeData() {
            base.ExposeData();
            this.compPowerTrader = base.GetComp<CompPowerTrader>();
            this.compTempControl = base.GetComp<CompTempControl>();
            //this.compHeatPusher = base.GetComp<CompHeatPusher>();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish) {
            base.Destroy(mode);
        }

        public override string GetInspectString() {
            StringBuilder stringBuilder = new StringBuilder();

            // Add the inspections string from the base
            string baseString = base.GetInspectString();
            if (!baseString.NullOrEmpty()) {
                stringBuilder.Append(baseString);
                stringBuilder.AppendLine();
            }

            stringBuilder.Append("Heat Output Per Second: ");
            stringBuilder.Append(string.Format("{0:N8}", TempDelta()/10f * TenDegreeDeltaThermalEnergyPerSecond));

            // return the complete string
            return stringBuilder.ToString().TrimEndNewlines();
        }

        #endregion

        #region tickers
        public override void TickRare() {
            if (destroyedFlag) {
                return;
            }
            base.TickRare();
            DoTickerWork(250);
        }

        public override void Tick() {
            if (destroyedFlag) {
                return;
            }
            base.Tick();
            DoTickerWork(1);
        }

        private void DoTickerWork(int tickAmount) {
            if(compTempControl.targetTemperature > maxTargetTemperature || compTempControl.targetTemperature < minTargetTemperature) {
                compTempControl.targetTemperature = defaultTargetTemp;
            }
            heat = TempDelta();
            compPowerTrader.PowerOutput = EnergyOutput(heat);
            
            GenTemperature.PushHeat(this, heat * tickAmount / 60 / 10f * TenDegreeDeltaThermalEnergyPerSecond);
        }

        #endregion tickers

        public float EnergyOutput(float heatOutput) {
            float heat = Math.Abs(heatOutput);
            if (heat < minHeatThreshold) {
                return IdlePower;
            }
            return heat * TenDegreeDeltaPowerGeneration / 10;
        }

        public float TempDelta() {
            float ambient = this.AmbientTemperature;
            float outside = this.Map.mapTemperature.OutdoorTemp;
            float target = compTempControl.targetTemperature;
            if (ambient < target && ambient < outside) {
                //case add heat
                return Math.Min(target - ambient, outside - ambient);
            } else if (ambient > target && ambient > outside) {
                //case remove heat
                return Math.Max(target - ambient, outside - ambient);
            }
            return 0f;
        }
        /*
        public float HeatOutput() {
            float ambient = this.AmbientTemperature;
            float outside = this.Map.mapTemperature.OutdoorTemp;
            float target = compTempControl.targetTemperature;
            if (ambient < target && ambient < outside) {
                //case add heat
                return Math.Min(target - ambient, outside - ambient) / 10f * TenDegreeDeltaThermalEnergyPerSecond;
            } else if (ambient > target && ambient > outside){
                //case remove heat
                return Math.Max(target - ambient, outside - ambient) / 10f * TenDegreeDeltaThermalEnergyPerSecond;
            }
            return 0f;
        }
        */
    }
}
