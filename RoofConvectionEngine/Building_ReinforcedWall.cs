using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace StructuralFieldsPlusTesting {
    class Building_ReinforcedWall : Building{
        //public CompFlickable compFlickable = new CompFlickable();
        public CompFieldConduit compFieldConduit;// = new CompFieldConduit();

        public int NetworkID { get => base.Map.GetComponent<FieldMap>().ConduitArray[Position.x, Position.z]; }
        public FieldNet ConnectedFieldNet { get => base.Map.GetComponent<FieldMap>().fieldNets[NetworkID]; }

        public override void ExposeData() {
            base.ExposeData();
            this.compFieldConduit = base.GetComp<CompFieldConduit>();
            //this.compFlickable = base.GetComp<CompFlickable>();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            this.compFieldConduit = base.GetComp<CompFieldConduit>();
            //this.compFlickable = base.GetComp<CompFlickable>();
            //origFunctionPower = ((CompProperties_Power)this.powerComp.props).basePowerConsumption;
        }

        //redirect damage to field
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
