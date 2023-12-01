using RimVore2;
using RimWorld;
using RimWorld.Planet;
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
                if (RV2_Rut_Settings.rutsStuff.EncumberanceModifier <= 0.0f)
                    return 0f;

                Caravan caravan = this.pawn.GetCaravan() ?? null;
                if (!this.pawn.Spawned || (caravan != null && !caravan.Spawned) || this.ageTicks % RV2Mod.Settings.debug.HediffLabelRefreshInterval != 0)
                {
                    return this.severityInt;
                }

                this.severityInt = 0f;
                float totalWeight = 0f;
                int totalPrey = 0;
                PawnData pawnData = this.pawn.PawnData(false) ?? null;
                PawnData preyData = null;
                float quirkMod = this.pawn.QuirkManager(false)?.CapModOffsetModifierFor(PawnCapacityDefOf.Moving, null) ?? 1f;
                if (pawnData != null && this.pawn.IsActivePredator())
                {
                    RV2Log.Message("Encumberance: checking predator " + this.pawn.ToString(), "Encumberance");
                    VoreTracker voreTracker = pawnData.VoreTracker;
                    if (voreTracker != null)
                        foreach (VoreTrackerRecord voreTrackerRecord in voreTracker.VoreTrackerRecords)
                        {
                            RV2Log.Message("Encumberance: found prey " + voreTrackerRecord.Prey.ToString(), "Encumberance");
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
                            RV2Log.Message("Encumberance: added " + (voreTrackerRecord.Prey.BodySize * preyMod).ToString(), "Encumberance");
                            if (voreTrackerRecord.Prey.IsActivePredator())
                            {
                                preyData = voreTrackerRecord.Prey.PawnData(false) ?? null;
                            }
                            if (preyData != null)
                            {
                                VoreTracker voreTracker2 = preyData.VoreTracker;
                                if (voreTracker2 != null)
                                {
                                    RV2Log.Message("Encumberance: prey " + (voreTrackerRecord.Prey.ToString()).ToString() + " has prey, checking", "Encumberance");
                                    foreach (VoreTrackerRecord voreTrackerRecord2 in voreTracker2.VoreTrackerRecords.Except(voreTrackerRecord))
                                    {
                                        RV2Log.Message("Encumberance: found prey " + voreTrackerRecord2.Prey.ToString() + " in prey " + voreTrackerRecord2.Predator.ToString(), "Encumberance");
                                        RV2Log.Message("Encumberance: added " + (voreTrackerRecord2.Prey.BodySize).ToString(), "Encumberance");
                                        totalWeight += voreTrackerRecord2.Prey.BodySize;
                                        totalPrey += 1;
                                    }
                                }
                            }
                        }

                    RV2Log.Message("Encumberance: found total weight " + totalWeight.ToString(), "Encumberance");
                    RV2Log.Message("Encumberance: found total prey " + totalPrey.ToString(), "Encumberance");
                }
                if (totalWeight > 0f && totalPrey > 0)
                {
                    if (RV2_Rut_Settings.rutsStuff.SizedEncumberance)
                    {
                        RV2Log.Message("Encumberance: doing fancy weight for " + this.pawn.ToString() + ":  (preyweight)" + totalWeight.ToString() + " / (predsize)" + this.pawn.BodySize + " / 4 * (settings,quirks)" + (RV2_Rut_Settings.rutsStuff.EncumberanceModifier * quirkMod).ToString(), "Encumberance");
                        this.severityInt = Math.Min(totalWeight / Math.Max(this.pawn.BodySize, 0.01f) / 4f * RV2_Rut_Settings.rutsStuff.EncumberanceModifier * quirkMod, 1f);
                    }
                    else
                    {
                        RV2Log.Message("Encumberance: doing simple weight for " + this.pawn.ToString() + ":  (totalprey)" + totalPrey.ToString() + "/ 4 * (settings,quirks)" + (RV2_Rut_Settings.rutsStuff.EncumberanceModifier * quirkMod).ToString(), "Encumberance");
                        this.severityInt = Math.Min(totalPrey / 4f * RV2_Rut_Settings.rutsStuff.EncumberanceModifier * quirkMod, 1f);
                    }

                    RV2Log.Message("Encumberance: severity set to " + this.severityInt.ToString(), "Encumberance");
                }
                return this.severityInt;
            }
        }
        public override void Tick()
        {
            base.Tick();
            if (this.Severity > 0.05f && this.ageTicks % (RV2Mod.Settings.debug.HediffLabelRefreshInterval * 4) == 0)
            {
                this.pawn.health.Notify_HediffChanged(this);
            }
        }

        public override bool Visible
        {
            get
            {
                if (this.Severity > 0.05f && RV2_Rut_Settings.rutsStuff.VisibleEncumberance)
                    return true;
                return false;
            }
        }

        public List<VoreTrackerRecord> ConnectedVoreRecords = new List<VoreTrackerRecord>();
    }
}
