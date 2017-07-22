using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
//using RimWorld;

namespace RoofConvectionEngine {
    public class ShieldNetManager {
        private enum DelayedActionType {
            RegisterTransmitter,
            DeregisterTransmitter,
            RegisterConnector,
            DeregisterConnector
        }

        private struct DelayedAction {
            public ShieldNetManager.DelayedActionType type;

            public CompShield compShield;

            public DelayedAction(ShieldNetManager.DelayedActionType type, CompShield compShield) {
                this.type = type;
                this.compShield = compShield;
            }
        }

        public Map map;

        private List<ShieldNet> allNets = new List<ShieldNet>();

        private List<ShieldNetManager.DelayedAction> delayedActions = new List<ShieldNetManager.DelayedAction>();

        public List<ShieldNet> AllNetsListForReading {
            get {
                return this.allNets;
            }
        }

        public ShieldNetManager(Map map) {
            this.map = map;
        }

        public void Notify_TransmitterSpawned(CompShield newTransmitter) {
            this.delayedActions.Add(new ShieldNetManager.DelayedAction(ShieldNetManager.DelayedActionType.RegisterTransmitter, newTransmitter));
            this.NotifyDrawersForWireUpdate(newTransmitter.parent.Position);
        }

        public void Notify_TransmitterDespawned(CompShield oldTransmitter) {
            this.delayedActions.Add(new ShieldNetManager.DelayedAction(ShieldNetManager.DelayedActionType.DeregisterTransmitter, oldTransmitter));
            this.NotifyDrawersForWireUpdate(oldTransmitter.parent.Position);
        }

        public void Notfiy_TransmitterTransmitsPowerNowChanged(CompShield transmitter) {
            if (!transmitter.parent.Spawned) {
                return;
            }
            this.delayedActions.Add(new ShieldNetManager.DelayedAction(ShieldNetManager.DelayedActionType.DeregisterTransmitter, transmitter));
            this.delayedActions.Add(new ShieldNetManager.DelayedAction(ShieldNetManager.DelayedActionType.RegisterTransmitter, transmitter));
            this.NotifyDrawersForWireUpdate(transmitter.parent.Position);
        }

        public void Notify_ConnectorWantsConnect(CompShield wantingCon) {
            if (Scribe.mode == LoadSaveMode.Inactive && !this.HasRegisterConnectorDuplicate(wantingCon)) {
                this.delayedActions.Add(new ShieldNetManager.DelayedAction(ShieldNetManager.DelayedActionType.RegisterConnector, wantingCon));
            }
            this.NotifyDrawersForWireUpdate(wantingCon.parent.Position);
        }

        public void Notify_ConnectorDespawned(CompShield oldCon) {
            this.delayedActions.Add(new ShieldNetManager.DelayedAction(ShieldNetManager.DelayedActionType.DeregisterConnector, oldCon));
            this.NotifyDrawersForWireUpdate(oldCon.parent.Position);
        }

        public void NotifyDrawersForWireUpdate(IntVec3 root) {
            this.map.mapDrawer.MapMeshDirty(root, MapMeshFlag.Things, true, false);
            this.map.mapDrawer.MapMeshDirty(root, MapMeshFlag.PowerGrid, true, false);
        }

        public void RegisterShieldNet(ShieldNet newNet) {
            this.allNets.Add(newNet);
            newNet.shieldNetManager = this;
            this.map.shieldNetGrid.Notify_ShieldNetCreated(newNet);
            ShieldNetMaker.UpdateVisualLinkagesFor(newNet);
        }

        public void DeletePowerNet(ShieldNet oldNet) {
            this.allNets.Remove(oldNet);
            this.map.shieldNetGrid.Notify_ShieldNetDeleted(oldNet);
        }

        public void PowerNetsTick() {
            for (int i = 0; i < this.allNets.Count; i++) {
                this.allNets[i].PowerNetTick();
            }
        }

        public void UpdatePowerNetsAndConnections_First() {
            int count = this.delayedActions.Count;
            for (int i = 0; i < count; i++) {
                ShieldNetManager.DelayedAction delayedAction = this.delayedActions[i];
                ShieldNetManager.DelayedActionType type = this.delayedActions[i].type;
                if (type != ShieldNetManager.DelayedActionType.RegisterTransmitter) {
                    if (type == ShieldNetManager.DelayedActionType.DeregisterTransmitter) {
                        this.TryDestroyNetAt(delayedAction.compShield.parent.Position);
                        ShieldConnectionMaker.DisconnectAllFromTransmitterAndSetWantConnect(delayedAction.compShield, this.map);
                        delayedAction.compShield.ResetPowerVars();
                    }
                } else {
                    ThingWithComps parent = delayedAction.compShield.parent;
                    if (this.map.powerNetGrid.TransmittedPowerNetAt(parent.Position) != null) {
                        Log.Warning(string.Concat(new object[]
                        {
                            "Tried to register trasmitter ",
                            parent,
                            " at ",
                            parent.Position,
                            ", but there is already a power net here. There can't be two transmitters on the same cell."
                        }));
                    }
                    delayedAction.compShield.SetUpPowerVars();
                    foreach (IntVec3 current in GenAdj.CellsAdjacentCardinal(parent)) {
                        this.TryDestroyNetAt(current);
                    }
                }
            }
            for (int j = 0; j < count; j++) {
                ShieldNetManager.DelayedAction delayedAction2 = this.delayedActions[j];
                if (delayedAction2.type == ShieldNetManager.DelayedActionType.RegisterTransmitter || delayedAction2.type == ShieldNetManager.DelayedActionType.DeregisterTransmitter) {
                    this.TryCreateNetAt(delayedAction2.compShield.parent.Position);
                    foreach (IntVec3 current2 in GenAdj.CellsAdjacentCardinal(delayedAction2.compShield.parent)) {
                        this.TryCreateNetAt(current2);
                    }
                }
            }
            for (int k = 0; k < count; k++) {
                ShieldNetManager.DelayedAction delayedAction3 = this.delayedActions[k];
                ShieldNetManager.DelayedActionType type = this.delayedActions[k].type;
                if (type != ShieldNetManager.DelayedActionType.RegisterConnector) {
                    if (type == ShieldNetManager.DelayedActionType.DeregisterConnector) {
                        ShieldConnectionMaker.DisconnectFromPowerNet(delayedAction3.compShield);
                        delayedAction3.compShield.ResetPowerVars();
                    }
                } else {
                    delayedAction3.compShield.SetUpPowerVars();
                    ShieldConnectionMaker.TryConnectToAnyPowerNet(delayedAction3.compShield, null);
                }
            }
            this.delayedActions.RemoveRange(0, count);
            if (DebugViewSettings.drawPower) {
                this.DrawDebugPowerNets();
            }
        }

        private bool HasRegisterConnectorDuplicate(CompShield compPower) {
            for (int i = this.delayedActions.Count - 1; i >= 0; i--) {
                if (this.delayedActions[i].compShield == compPower) {
                    if (this.delayedActions[i].type == ShieldNetManager.DelayedActionType.DeregisterConnector) {
                        return false;
                    }
                    if (this.delayedActions[i].type == ShieldNetManager.DelayedActionType.RegisterConnector) {
                        return true;
                    }
                }
            }
            return false;
        }

        private void TryCreateNetAt(IntVec3 cell) {
            if (!cell.InBounds(this.map)) {
                return;
            }
            if (this.map.powerNetGrid.TransmittedPowerNetAt(cell) == null) {
                Building transmitter = cell.GetTransmitter(this.map);
                if (transmitter != null && transmitter.TransmitsPowerNow) {
                    ShieldNet powerNet = ShieldNetMaker.NewPowerNetStartingFrom(transmitter);
                    this.RegisterShieldNet(powerNet);
                    for (int i = 0; i < powerNet.transmitters.Count; i++) {
                        ShieldConnectionMaker.ConnectAllConnectorsToTransmitter(powerNet.transmitters[i]);
                    }
                }
            }
        }

        private void TryDestroyNetAt(IntVec3 cell) {
            if (!cell.InBounds(this.map)) {
                return;
            }
            ShieldNet powerNet = this.map.shieldNetGrid.TransmittedShieldNetAt(cell);
            if (powerNet != null) {
                this.DeletePowerNet(powerNet);
            }
        }

        private void DrawDebugPowerNets() {
            if (Current.ProgramState != ProgramState.Playing) {
                return;
            }
            int num = 0;
            foreach (ShieldNet current in this.allNets) {
                foreach (CompShield current2 in current.transmitters.Concat(current.connectors)) {
                    foreach (IntVec3 current3 in GenAdj.CellsOccupiedBy(current2.parent)) {
                        CellRenderer.RenderCell(current3, (float)num * 0.44f);
                    }
                }
                num++;
            }
        }
    }
}
