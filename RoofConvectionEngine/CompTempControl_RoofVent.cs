using Verse;
using RimWorld;

namespace RoofConvectionEngine {
    public class CompTempControl_RoofVent : CompTempControl {
        
        public new CompProperties_TempControl_RoofVent Props {
            get {
                return (CompProperties_TempControl_RoofVent)this.props;
            }
        }
    }
}
