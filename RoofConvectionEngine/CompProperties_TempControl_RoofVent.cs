using RimWorld;

namespace RoofConvectionEngine {
    public class CompProperties_TempControl_RoofVent : CompProperties_TempControl {
        public float TenDegreeDeltaThermalEnergyPerSecond = 21f;
        public new float minTargetTemperature = -150f;
        public new float maxTargetTemperature = 150f;

        public CompProperties_TempControl_RoofVent() {
            this.compClass = typeof(CompTempControl_RoofVent);
        }
    }
}
