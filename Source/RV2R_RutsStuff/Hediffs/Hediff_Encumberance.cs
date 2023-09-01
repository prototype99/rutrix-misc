using RimVore2;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using static RV2R_RutsStuff.Patch_RV2R_Settings;

namespace RV2R_RutsStuff
{
    public class Hediff_VoreEncumberance : Hediff
    {
        public override float Severity
        {
            get
            {
                float totalWeight = 0f;
                int totalPrey = 0;
                PawnData pawnData = null;
                float quirkMod = this.pawn.QuirkManager(true).CapModOffsetModifierFor(PawnCapacityDefOf.Moving, null);
                if (this.pawn.IsActivePredator())
                {
                    PawnData pawnData2 = this.pawn.PawnData(true);
                    if (pawnData2 != null)
                    {
                        VoreTracker voreTracker = pawnData2.VoreTracker;
                        if (voreTracker != null)
                            foreach (VoreTrackerRecord voreTrackerRecord in voreTracker.VoreTrackerRecords)
                            {
                                float preyMod = 1f;
                                if (voreTrackerRecord.CurrentVoreStage.def.partName == "tail")
                                    preyMod = 0.55f;
                                if (voreTrackerRecord.CurrentVoreStage.def.partGoal == "Amalgamate" || voreTrackerRecord.CurrentVoreStage.def.partGoal == "Assimilate")
                                    preyMod = 0.45f;

                                totalWeight += voreTrackerRecord.Prey.BodySize * preyMod;
                                totalPrey += 1;

                                if (voreTrackerRecord.Prey.IsActivePredator())
                                {
                                    pawnData = voreTrackerRecord.Prey.PawnData(true);
                                }
                                if (pawnData != null)
                                {
                                    VoreTracker voreTracker2 = voreTrackerRecord.Prey.PawnData(true).VoreTracker;
                                    if (voreTracker2 != null)
                                        foreach (VoreTrackerRecord voreTrackerRecord2 in voreTracker2.VoreTrackerRecords)
                                        {
                                            totalWeight += voreTrackerRecord2.Prey.BodySize * preyMod;
                                            totalPrey += 1;
                                        }
                                }
                            }
                    }
                }
                if (RV2_Rut_Settings.rutsStuff.SizedEncumberance)
                    this.severityInt = Math.Min(totalWeight / this.pawn.BodySize / 3f * RV2_Rut_Settings.rutsStuff.EncumberanceModifier * quirkMod, 1f);
                else
                    this.severityInt = Math.Min(totalPrey / 5f * RV2_Rut_Settings.rutsStuff.EncumberanceModifier * quirkMod, 1f);

                return this.severityInt;
            }
        }

        public override bool Visible => false;

        public List<VoreTrackerRecord> ConnectedVoreRecords = new List<VoreTrackerRecord>();

        private string label;
    }
}
