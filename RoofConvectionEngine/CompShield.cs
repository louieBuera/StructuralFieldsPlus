using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
//using RimWorld;

namespace RoofConvectionEngine {
    public abstract class CompShield : ThingComp {
        public ShieldNet transNet;

        public CompShield connectParent;

        public List<CompShield> connectChildren;

        private static List<ShieldNet> recentlyConnectedNets = new List<ShieldNet>();

        private static CompShield lastManualReconnector = null;

        public static readonly float WattsToWattDaysPerTick = 1.66666669E-05f;

        public bool TransmitsPowerNow {
            get {
                return ((Building)this.parent).TransmitsPowerNow;
            }
        }

        public ShieldNet ShieldNet {
            get {
                if (this.transNet != null) {
                    return this.transNet;
                }
                if (this.connectParent != null) {
                    return this.connectParent.transNet;
                }
                return null;
            }
        }

        public CompProperties_Shield Props {
            get {
                return (CompProperties_Shield)this.props;
            }
        }

        public virtual void ResetPowerVars() {
            this.transNet = null;
            this.connectParent = null;
            this.connectChildren = null;
            CompShield.recentlyConnectedNets.Clear();
            CompShield.lastManualReconnector = null;
        }

        public virtual void SetUpPowerVars() {
        }

        public override void PostExposeData() {
            Thing thing = null;
            if (Scribe.mode == LoadSaveMode.Saving && this.connectParent != null) {
                thing = this.connectParent.parent;
            }
            Scribe_References.Look<Thing>(ref thing, "parentThing", false);
            if (thing != null) {
                this.connectParent = ((ThingWithComps)thing).GetComp<CompShield>();
            }
            if (Scribe.mode == LoadSaveMode.PostLoadInit && this.connectParent != null) {
                this.ConnectToTransmitter(this.connectParent, true);
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad) {
            base.PostSpawnSetup(respawningAfterLoad);
            if (this.Props.transmitsShield || this.parent.def.ConnectToPower) {
                this.parent.Map.mapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlag.PowerGrid, true, false);
                if (this.Props.transmitsShield) {
                    this.parent.Map.powerNetManager.Notify_TransmitterSpawned(this);
                }
                if (this.parent.def.ConnectToPower) {
                    this.parent.Map.powerNetManager.Notify_ConnectorWantsConnect(this);
                }
                this.SetUpPowerVars();
            }
        }

        public override void PostDeSpawn(Map map) {
            base.PostDeSpawn(map);
            if (this.Props.transmitsShield || this.parent.def.ConnectToPower) {
                if (this.Props.transmitsShield) {
                    if (this.connectChildren != null) {
                        for (int i = 0; i < this.connectChildren.Count; i++) {
                            this.connectChildren[i].LostConnectParent();
                        }
                    }
                    map.shieldNetManager.Notify_TransmitterDespawned(this);
                }
                if (this.parent.def.ConnectToPower) {
                    map.shieldNetManager.Notify_ConnectorDespawned(this);
                }
                map.mapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlag.PowerGrid, true, false);
            }
        }

        public virtual void LostConnectParent() {
            this.connectParent = null;
            if (this.parent.Spawned) {
                this.parent.Map.shieldNetManager.Notify_ConnectorWantsConnect(this);
            }
        }

        public override void PostPrintOnto(SectionLayer layer) {
            base.PostPrintOnto(layer);
            if (this.connectParent != null) {
                ShieldNetGraphics.PrintWirePieceConnecting(layer, this.parent, this.connectParent.parent, false);
            }
        }

        public override void CompPrintForPowerGrid(SectionLayer layer) {
            if (this.TransmitsPowerNow) {
                ShieldOverlayMats.LinkedOverlayGraphic.Print(layer, this.parent);
            }
            if (this.parent.def.ConnectToPower) {
                ShieldNetGraphics.PrintOverlayConnectorBaseFor(layer, this.parent);
            }
            if (this.connectParent != null) {
                ShieldNetGraphics.PrintWirePieceConnecting(layer, this.parent, this.connectParent.parent, true);
            }
        }

        /*
        [DebuggerHidden]
        public override IEnumerable<Gizmo> CompGetGizmosExtra() {
            CompShield.< CompGetGizmosExtra > c__IteratorB2 < CompGetGizmosExtra > c__IteratorB = new CompShield.< CompGetGizmosExtra > c__IteratorB2();

            < CompGetGizmosExtra > c__IteratorB.<> f__this = this;
            CompShield.< CompGetGizmosExtra > c__IteratorB2 expr_0E = < CompGetGizmosExtra > c__IteratorB;
            expr_0E.$PC = -2;
            return expr_0E;
        }
        */

        private void TryManualReconnect() {
            if (CompShield.lastManualReconnector != this) {
                CompShield.recentlyConnectedNets.Clear();
                CompShield.lastManualReconnector = this;
            }
            if (this.ShieldNet != null) {
                CompShield.recentlyConnectedNets.Add(this.ShieldNet);
            }
            CompShield compPower = ShieldConnectionMaker.BestTransmitterForConnector(this.parent.Position, this.parent.Map, CompShield.recentlyConnectedNets);
            if (compShield == null) {
                CompShield.recentlyConnectedNets.Clear();
                compPower = ShieldConnectionMaker.BestTransmitterForConnector(this.parent.Position, this.parent.Map, null);
            }
            if (compPower != null) {
                ShieldConnectionMaker.DisconnectFromPowerNet(this);
                this.ConnectToTransmitter(compPower, false);
                for (int i = 0; i < 5; i++) {
                    MoteMaker.ThrowMetaPuff(compPower.parent.Position.ToVector3Shifted(), compPower.parent.Map);
                }
                this.parent.Map.mapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlag.PowerGrid);
                this.parent.Map.mapDrawer.MapMeshDirty(this.parent.Position, MapMeshFlag.Things);
            }
        }

        public void ConnectToTransmitter(CompShield transmitter, bool reconnectingAfterLoading = false) {
            if (this.connectParent != null && (!reconnectingAfterLoading || this.connectParent != transmitter)) {
                Log.Error(string.Concat(new object[]
                {
                    "Tried to connect ",
                    this,
                    " to transmitter ",
                    transmitter,
                    " but it's already connected to ",
                    this.connectParent,
                    "."
                }));
                return;
            }
            this.connectParent = transmitter;
            if (this.connectParent.connectChildren == null) {
                this.connectParent.connectChildren = new List<CompShield>();
            }
            transmitter.connectChildren.Add(this);
            ShieldNet powerNet = this.ShieldNet;
            if (powerNet != null) {
                powerNet.RegisterConnector(this);
            }
        }

        public override string CompInspectStringExtra() {
            if (this.ShieldNet == null) {
                return "PowerNotConnected".Translate();
            }
            string text = (this.ShieldNet.CurrentEnergyGainRate() / CompShield.WattsToWattDaysPerTick).ToString("F0");
            string text2 = this.ShieldNet.CurrentStoredEnergy().ToString("F0");
            return "PowerConnectedRateStored".Translate(new object[]
            {
                text,
                text2
            });
        }
    }
}
