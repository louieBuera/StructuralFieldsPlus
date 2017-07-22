using Verse;
using RimWorld;

namespace RoofConvectionEngine {
    public class CompProperties_RoofVent : CompProperties {
        private const float TenDegreeDeltaPowerGeneration = 20f;
        private const float TargetTemp = 21f;
        private const float IdlePower = -5f;
        private const float MinPowerThreshold = 5f;

        public CompProperties_RoofVent() {
            this.compClass = typeof(CompPowerPlant_RoofVent);
        }

    }
}
