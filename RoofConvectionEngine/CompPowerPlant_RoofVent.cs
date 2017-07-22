using RimWorld;
using Verse;

namespace RoofConvectionEngine {
    public class CompPowerPlant_RoofVent : CompPowerPlant{
        
        public new CompProperties_RoofVent Props {
            get {
                return (CompProperties_RoofVent)this.props;
            }
        }

        /*protected override float DesiredPowerOutput {
            get {
                this.Props.
                float holder = 0; ;
                float ambient = this.parent.AmbientTemperature;
                float outside = this.parent.Map.mapTemperature.OutdoorTemp;
                if (ambient < TargetTemp && TargetTemp < outside) {
                    holder =  (outside - ambient) / 10f * TenDegreeDeltaPowerGeneration;
                } else if(outside < TargetTemp && TargetTemp < ambient){
                    holder = (ambient - outside) / 10f * TenDegreeDeltaPowerGeneration;
                }
                if(holder > MinPowerThreshold) {
                    return holder;
                }
                return IdlePower;
                
            }
        }*/
    }
}
