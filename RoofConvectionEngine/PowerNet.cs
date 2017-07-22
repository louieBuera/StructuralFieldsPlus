using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;


namespace RoofConvectionEngine {
    public class ShieldNet {
        private const int MaxRestartTryInterval = 200;

        private const int MinRestartTryInterval = 30;

        private const int ShutdownInterval = 20;

        private const float MinStoredEnergyToTurnOn = 5f;

        public ShieldNetManager shieldNetManager;

        public bool hasPowerSource;

        public List<CompShield> connectors = new List<CompShield>();

        public List<CompShield> transmitters = new List<CompShield>();

        public List<CompShieldTrader> powerComps = new List<CompShieldTrader>();

        public List<CompShieldBattery> batteryComps = new List<CompShieldBattery>();

        private float debugLastCreatedShield;

        private float debugLastRawStoredShield;

        private float debugLastApparentStoredShield;

        private static List<CompShieldTrader> partsWantingPowerOn = new List<CompShieldTrader>();

        private static List<CompShieldTrader> potentialShutdownParts = new List<CompShieldTrader>();

        private List<CompShieldBattery> givingBats = new List<CompShieldBattery>();

        private static List<CompShieldBattery> batteriesShuffled = new List<CompShieldBattery>();

        public ShieldNet(IEnumerable<CompShield> newTransmitters) {
            foreach (CompShield current in newTransmitters) {
                this.transmitters.Add(current);
                current.transNet = this;
                this.RegisterAllComponentsOf(current.parent);
                if (current.connectChildren != null) {
                    List<CompShield> connectChildren = current.connectChildren;
                    for (int i = 0; i < connectChildren.Count; i++) {
                        this.RegisterConnector(connectChildren[i]);
                    }
                }
            }
            this.hasPowerSource = false;
            for (int j = 0; j < this.transmitters.Count; j++) {
                if (this.IsPowerSource(this.transmitters[j])) {
                    this.hasPowerSource = true;
                    break;
                }
            }
        }

        private bool IsPowerSource(CompShield cp) {
            return cp is CompShieldBattery || (cp is CompShieldTrader && cp.Props.basePowerConsumption < 0f);
        }

        public void RegisterConnector(CompShield b) {
            if (this.connectors.Contains(b)) {
                Log.Error("PowerNet registered connector it already had: " + b);
                return;
            }
            this.connectors.Add(b);
            this.RegisterAllComponentsOf(b.parent);
        }

        public void DeregisterConnector(CompShield b) {
            this.connectors.Remove(b);
            this.DeregisterAllComponentsOf(b.parent);
        }

        private void RegisterAllComponentsOf(ThingWithComps parentThing) {
            CompShieldTrader comp = parentThing.GetComp<CompShieldTrader>();
            if (comp != null) {
                if (this.powerComps.Contains(comp)) {
                    Log.Error("PowerNet adding powerComp " + comp + " which it already has.");
                } else {
                    this.powerComps.Add(comp);
                }
            }
            CompShieldBattery comp2 = parentThing.GetComp<CompShieldBattery>();
            if (comp2 != null) {
                if (this.batteryComps.Contains(comp2)) {
                    Log.Error("PowerNet adding batteryComp " + comp2 + " which it already has.");
                } else {
                    this.batteryComps.Add(comp2);
                }
            }
        }

        private void DeregisterAllComponentsOf(ThingWithComps parentThing) {
            CompShieldTrader comp = parentThing.GetComp<CompShieldTrader>();
            if (comp != null) {
                this.powerComps.Remove(comp);
            }
            CompShieldBattery comp2 = parentThing.GetComp<CompShieldBattery>();
            if (comp2 != null) {
                this.batteryComps.Remove(comp2);
            }
        }

        public float CurrentEnergyGainRate() {
            if (DebugSettings.unlimitedPower) {
                return 100000f;
            }
            float num = 0f;
            for (int i = 0; i < this.powerComps.Count; i++) {
                if (this.powerComps[i].PowerOn) {
                    num += this.powerComps[i].EnergyOutputPerTick;
                }
            }
            return num;
        }

        public float CurrentStoredEnergy() {
            float num = 0f;
            for (int i = 0; i < this.batteryComps.Count; i++) {
                num += this.batteryComps[i].StoredEnergy;
            }
            return num;
        }

        public void PowerNetTick() {
            float num = this.CurrentEnergyGainRate();
            float num2 = this.CurrentStoredEnergy();
            if (num2 + num >= -1E-07f && !this.shieldNetManager.map.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare)) {
                float num3;
                if (this.batteryComps.Count > 0 && num2 >= 0.1f) {
                    num3 = num2 - 5f;
                } else {
                    num3 = num2;
                }
                if (UnityData.isDebugBuild) {
                    this.debugLastApparentStoredEnergy = num3;
                    this.debugLastCreatedEnergy = num;
                    this.debugLastRawStoredEnergy = num2;
                }
                if (num3 + num >= 0f) {
                    ShieldNet.partsWantingPowerOn.Clear();
                    for (int i = 0; i < this.powerComps.Count; i++) {
                        if (!this.powerComps[i].PowerOn && FlickUtility.WantsToBeOn(this.powerComps[i].parent) && !this.powerComps[i].parent.IsBrokenDown()) {
                            ShieldNet.partsWantingPowerOn.Add(this.powerComps[i]);
                        }
                    }
                    if (ShieldNet.partsWantingPowerOn.Count > 0) {
                        int num4 = 200 / ShieldNet.partsWantingPowerOn.Count;
                        if (num4 < 30) {
                            num4 = 30;
                        }
                        if (Find.TickManager.TicksGame % num4 == 0) {
                            CompShieldTrader compPowerTrader = ShieldNet.partsWantingPowerOn.RandomElement<CompShieldTrader>();
                            if (num + num2 >= -(compPowerTrader.EnergyOutputPerTick + 1E-07f)) {
                                compPowerTrader.PowerOn = true;
                                num += compPowerTrader.EnergyOutputPerTick;
                            }
                        }
                    }
                }
                this.ChangeStoredEnergy(num);
            } else if (Find.TickManager.TicksGame % 20 == 0) {
                ShieldNet.potentialShutdownParts.Clear();
                for (int j = 0; j < this.powerComps.Count; j++) {
                    if (this.powerComps[j].PowerOn && this.powerComps[j].EnergyOutputPerTick < 0f) {
                        ShieldNet.potentialShutdownParts.Add(this.powerComps[j]);
                    }
                }
                if (ShieldNet.potentialShutdownParts.Count > 0) {
                    ShieldNet.potentialShutdownParts.RandomElement<CompShieldTrader>().PowerOn = false;
                }
            }
        }

        private void ChangeStoredEnergy(float extra) {
            if (extra > 0f) {
                this.DistributeEnergyAmongBatteries(extra);
            } else {
                float num = -extra;
                this.givingBats.Clear();
                for (int i = 0; i < this.batteryComps.Count; i++) {
                    if (this.batteryComps[i].StoredEnergy > 1E-07f) {
                        this.givingBats.Add(this.batteryComps[i]);
                    }
                }
                float a = num / (float)this.givingBats.Count;
                int num2 = 0;
                while (num > 1E-07f) {
                    for (int j = 0; j < this.givingBats.Count; j++) {
                        float num3 = Mathf.Min(a, this.givingBats[j].StoredEnergy);
                        this.givingBats[j].DrawPower(num3);
                        num -= num3;
                        if (num < 1E-07f) {
                            return;
                        }
                    }
                    num2++;
                    if (num2 > 10) {
                        break;
                    }
                }
                if (num > 1E-07f) {
                    Log.Warning("Drew energy from a PowerNet that didn't have it.");
                }
            }
        }

        private void DistributeEnergyAmongBatteries(float energy) {
            if (energy <= 0f || !this.batteryComps.Any<CompShieldBattery>()) {
                return;
            }
            ShieldNet.batteriesShuffled.Clear();
            ShieldNet.batteriesShuffled.AddRange(this.batteryComps);
            ShieldNet.batteriesShuffled.Shuffle<CompShieldBattery>();
            int num = 0;
            while (true) {
                num++;
                if (num > 10000) {
                    break;
                }
                float num2 = 3.40282347E+38f;
                for (int i = 0; i < ShieldNet.batteriesShuffled.Count; i++) {
                    num2 = Mathf.Min(num2, ShieldNet.batteriesShuffled[i].AmountCanAccept);
                }
                if (energy < num2 * (float)ShieldNet.batteriesShuffled.Count) {
                    goto IL_12F;
                }
                for (int j = ShieldNet.batteriesShuffled.Count - 1; j >= 0; j--) {
                    float amountCanAccept = ShieldNet.batteriesShuffled[j].AmountCanAccept;
                    bool flag = amountCanAccept <= 0f || amountCanAccept == num2;
                    if (num2 > 0f) {
                        ShieldNet.batteriesShuffled[j].AddEnergy(num2);
                        energy -= num2;
                    }
                    if (flag) {
                        ShieldNet.batteriesShuffled.RemoveAt(j);
                    }
                }
                if (energy < 0.0005f || !ShieldNet.batteriesShuffled.Any<CompShieldBattery>()) {
                    goto IL_196;
                }
            }
            Log.Error("Too many iterations.");
            goto IL_1A0;
            IL_12F:
            float amount = energy / (float)ShieldNet.batteriesShuffled.Count;
            for (int k = 0; k < ShieldNet.batteriesShuffled.Count; k++) {
                ShieldNet.batteriesShuffled[k].AddEnergy(amount);
            }
            energy = 0f;
            IL_196:
            IL_1A0:
            ShieldNet.batteriesShuffled.Clear();
        }

        public string DebugString() {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("POWERNET:");
            stringBuilder.AppendLine("  Created energy: " + this.debugLastCreatedEnergy);
            stringBuilder.AppendLine("  Raw stored energy: " + this.debugLastRawStoredEnergy);
            stringBuilder.AppendLine("  Apparent stored energy: " + this.debugLastApparentStoredEnergy);
            stringBuilder.AppendLine("  hasPowerSource: " + this.hasPowerSource);
            stringBuilder.AppendLine("  Connectors: ");
            foreach (CompShield current in this.connectors) {
                stringBuilder.AppendLine("      " + current.parent);
            }
            stringBuilder.AppendLine("  Transmitters: ");
            foreach (CompShield current2 in this.transmitters) {
                stringBuilder.AppendLine("      " + current2.parent);
            }
            stringBuilder.AppendLine("  powerComps: ");
            foreach (CompShieldTrader current3 in this.powerComps) {
                stringBuilder.AppendLine("      " + current3.parent);
            }
            stringBuilder.AppendLine("  batteryComps: ");
            foreach (CompShieldBattery current4 in this.batteryComps) {
                stringBuilder.AppendLine("      " + current4.parent);
            }
            return stringBuilder.ToString();
        }
    }
}
