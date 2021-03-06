﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using RimWorld;

namespace StructuralFieldsPlusTesting {
    class Building_FieldCapacitor : Building{
        public CompFieldConduit compFieldConduit;
        public CompFieldCapacitor compFieldCapacitor;

        public int NetworkID { get => base.Map.GetComponent<FieldMap>().ConduitArray[Position.x, Position.z]; }
        public FieldNet ConnectedFieldNet { get => base.Map.GetComponent<FieldMap>().fieldNets[NetworkID]; }

        public override void ExposeData() {
            base.ExposeData();
            this.compFieldConduit = base.GetComp<CompFieldConduit>();
            this.compFieldCapacitor = base.GetComp<CompFieldCapacitor>();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            this.compFieldConduit = base.GetComp<CompFieldConduit>();
            this.compFieldCapacitor = base.GetComp<CompFieldCapacitor>();
        }

        public override string GetInspectString() {

            return "NetID: " + NetworkID.ToString() +
                "\nAvailable Field: " + string.Format("{0:N8}", ConnectedFieldNet.AvailableField) +
                "\nLocal Field: " + string.Format("{0:N8}", compFieldCapacitor.CurrentField) +
                "\nMax Total Field: " + string.Format("{0:N8}", ConnectedFieldNet.maxField) +
                "\nMax Local Field: " + string.Format("{0:N8}", compFieldCapacitor.StoredFieldMax);
            // return the complete string
        }
    }
}
