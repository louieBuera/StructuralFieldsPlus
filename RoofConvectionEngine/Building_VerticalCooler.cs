using RimWorld;
using Verse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoofConvectionEngine {
    class Building_VerticalCooler : Building_Cooler{

        public const float HeatOutputMultiplier = 1.25f;

        public const float EfficiencyLossPerDegreeDifference = 0.0076923077f;

        public const float HeatThreshold = 0.02f;

        public override void TickRare() {
            //base.TickRare();
            if (this.compPowerTrader.PowerOn) {
                bool flag = false;
                //heating
                float outsideTemp = this.Map.mapTemperature.OutdoorTemp;
                //cooling
                float ambientTemp = this.AmbientTemperature;
                //hotside - coolside
                float num = outsideTemp - ambientTemp;
                //hotside - 40 greater than hotside - coolside
                //coolside colder than -40
                if (outsideTemp - 40f > num) {
                    //num = hotside - 40
                    //min coldside operating temp == 40
                    num = outsideTemp - 40f;
                }
                //num2 = total efficiency
                float num2 = 1f - num * EfficiencyLossPerDegreeDifference;
                if (num2 < 0f) {
                    num2 = 0f;
                }
                //num3 = EPS stat * efficiency * const// total heat push
                float num3 = this.compTempControl.Props.energyPerSecond * num2 * 4.16666651f;
                /*RoomGroup roomGroup = this.Position.GetRoomGroup(base.Map);
                if (roomGroup == null) {
                    Messages.Message("null", MessageSound.Standard);
                } else if(roomGroup.UsesOutdoorTemperature){
                    Messages.Message("outdoor", MessageSound.Standard);
                } else {
                    Messages.Message("zilch", MessageSound.Standard);
                }
                //calibrating for room size etc., no changes yet
                Messages.Message("num3", MessageSound.Standard);
                Messages.Message(string.Format("{0:N8}", num3), MessageSound.Standard);
                float num4 = GenTemperature.ControlTemperatureTempChange(this.Position, base.Map, num3, this.compTempControl.targetTemperature);
                Messages.Message(this.Position.ToString(), MessageSound.Standard);
                Messages.Message("num4", MessageSound.Standard);
                Messages.Message(string.Format("{0:N8}", num4), MessageSound.Standard);*/
                //check if close to zero
                //flag = !UnityEngine.Mathf.Approximately(num4, 0f);
                flag = Math.Abs(num3 / (float)this.compTempControl.Props.energyPerSecond) > HeatThreshold && ambientTemp > this.compTempControl.targetTemperature;
                /*Messages.Message(string.Format("{0:N8}", num3 / (float)this.compTempControl.Props.energyPerSecond), MessageSound.Standard);
                Messages.Message(flag.ToString(), MessageSound.Standard);*/
                if (flag) {
                    //cool the room
                    //this.Position.GetRoomGroup(base.Map).Temperature += num4;
                    GenTemperature.PushHeat(this.Position, base.Map, num3);
                    //heat other side * efficiency factor
                    //GenTemperature.PushHeat(intVec2, base.Map, -num3 * HeatOutputMultiplier);
                    //removed due to exhausting to Outside
                }
                    
                CompProperties_Power props = this.compPowerTrader.Props;
                if (flag) {
                    this.compPowerTrader.PowerOutput = -props.basePowerConsumption;
                } else {
                    this.compPowerTrader.PowerOutput = -props.basePowerConsumption * this.compTempControl.Props.lowPowerConsumptionFactor;
                }
                this.compTempControl.operatingAtHighPower = flag;
            }
        }
    }
}
