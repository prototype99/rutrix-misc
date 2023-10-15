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
                float quirkMod = this.pawn.QuirkManager(false)?.CapModOffsetModifierFor(PawnCapacityDefOf.Moving, null) ?? 1f;
                if (this.pawn.IsActivePredator())
                {
                    PawnData pawnData2 = this.pawn.PawnData(false);
                    if (pawnData2 != null)
                    {
                        VoreTracker voreTracker = pawnData2.VoreTracker;
                        if (voreTracker != null)
                            foreach (VoreTrackerRecord voreTrackerRecord in voreTracker.VoreTrackerRecords)
                            {
                                float preyMod = 1f;
                                if (voreTrackerRecord.CurrentVoreStage.def.partName == "tail"
                                 || voreTrackerRecord.VoreType.defName == "Cock"
                                 || voreTrackerRecord.VoreType.defName == "Udder"
                                 || voreTrackerRecord.VoreType.defName == "Membrane"
                                 || voreTrackerRecord.VoreGoal.defName == "Amalgamate"
                                 || voreTrackerRecord.VoreGoal.defName == "Assimilate")
                                    preyMod = 0.75f;

                                totalWeight += voreTrackerRecord.Prey.BodySize * preyMod;
                                totalPrey += 1;

                                if (voreTrackerRecord.Prey.IsActivePredator())
                                {
                                    pawnData = voreTrackerRecord.Prey.PawnData(false);
                                }
                                if (pawnData != null)
                                {
                                    VoreTracker voreTracker2 = voreTrackerRecord.Prey.PawnData(false).VoreTracker;
                                    if (voreTracker2 != null)
                                        foreach (VoreTrackerRecord voreTrackerRecord2 in voreTracker2.VoreTrackerRecords)
                                        {
                                            totalWeight += voreTrackerRecord2.Prey.BodySize;
                                            totalPrey += 1;
                                        }
                                }
                            }
                    }
                }
                this.severityInt = 0f;
                if (totalWeight > 0f && totalPrey > 0)
                {
                    if (RV2_Rut_Settings.rutsStuff.SizedEncumberance)
                        this.severityInt = Math.Min(totalWeight / Math.Max(this.pawn.BodySize,0.01f) / 4f * RV2_Rut_Settings.rutsStuff.EncumberanceModifier * quirkMod, 1f);
                    else
                        this.severityInt = Math.Min(totalPrey / 4f * RV2_Rut_Settings.rutsStuff.EncumberanceModifier * quirkMod, 1f);
                }
                return this.severityInt;
            }
        }
        public override void Tick()
        {
            base.Tick();
            if (this.Severity > 0.05f && this.ageTicks % (RV2Mod.Settings.debug.HediffLabelRefreshInterval * 2) == 0)
            {
                this.pawn.health.Notify_HediffChanged(this);
            }
        }

        public override bool Visible => false;

        public List<VoreTrackerRecord> ConnectedVoreRecords = new List<VoreTrackerRecord>();
    }
}
