using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace StructuralFieldsPlusTesting {
    class Building_FieldGenerator : Building{
        public CompFieldConduit compFieldConduit = new CompFieldConduit();
        public CompFieldGenerator compFieldGenerator = new CompFieldGenerator();

        public int NetworkID { get => base.Map.GetComponent<FieldMap>().ConduitArray[Position.x, Position.z]; }
        public FieldNet ConnectedFieldNet { get => base.Map.GetComponent<FieldMap>().fieldNets[NetworkID]; }

        public override void ExposeData() {
            base.ExposeData();
            this.compFieldConduit = base.GetComp<CompFieldConduit>();
            this.compFieldGenerator = base.GetComp<CompFieldGenerator>();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad) {
            base.SpawnSetup(map, respawningAfterLoad);
            this.compFieldConduit = base.GetComp<CompFieldConduit>();
            this.compFieldGenerator = base.GetComp<CompFieldGenerator>();
        }

        public override string GetInspectString() {
            StringBuilder stringBuilder = new StringBuilder();

            return "NetID: " + NetworkID.ToString() +
                "\nGen Per Tick Total: " + string.Format("{0:N8}", ConnectedFieldNet.GenPerTick) +
                "\nGen Per Tick Local: " + string.Format("{0:N8}", compFieldGenerator.GenPerTick) +
                PowerComp.CompInspectStringExtra();
            // return the complete string
        }

        public override void TickRare() {
            base.TickRare();
            compFieldGenerator.CompTickRareExtra();
        }
    }
}
