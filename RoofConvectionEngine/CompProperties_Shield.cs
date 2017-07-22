using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;

//using RimWorld;

namespace RoofConvectionEngine {
    public class CompProperties_Shield : CompProperties {
        public bool transmitsShield;

        public float baseShieldConsumption;

        public bool startElectricalFires;

        public bool shortCircuitInRain = true;

        public SoundDef soundPowerOn;

        public SoundDef soundPowerOff;

        public SoundDef soundAmbientPowered;

        /*
        [DebuggerHidden]
        public override IEnumerable<string> ConfigErrors(ThingDef parentDef) {
            CompProperties_Shield.< ConfigErrors > c__Iterator7A < ConfigErrors > c__Iterator7A = new CompProperties_Shield.< ConfigErrors > c__Iterator7A();

            < ConfigErrors > c__Iterator7A.parentDef = parentDef;

            < ConfigErrors > c__Iterator7A.<$> parentDef = parentDef;

            < ConfigErrors > c__Iterator7A.<> f__this = this;
            CompProperties_Shield.< ConfigErrors > c__Iterator7A expr_1C = < ConfigErrors > c__Iterator7A;
            expr_1C.$PC = -2;
            return expr_1C;
        }*/
    }
}
