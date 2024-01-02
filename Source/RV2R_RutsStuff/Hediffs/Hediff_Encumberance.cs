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
                try
                {
                    if (RV2_Rut_Settings.rutsStuff.EncumberanceModifier <= 0.0f)
                        return 0f;

                    Caravan caravan = pawn.GetCaravan() ?? null;
                    if (hasUpdate || (!pawn.Spawned && (caravan != null && !caravan.Spawned)) || (ageTicks % (RV2Mod.Settings.debug.HediffLabelRefreshInterval * 4) != 0 && ageTicks > 10))
                    {
                        hasUpdate = ageTicks % (RV2Mod.Settings.debug.HediffLabelRefreshInterval * 4) != 0;
                        return severityInt;
                    }

                    hasUpdate = true;
                    severityInt = 0f;
                    float totalWeight = 0f;
                    int totalPrey = 0;
                    PawnData pawnData = pawn.PawnData(false) ?? null;
                    PawnData preyData = null;
                    float quirkMod = pawn.QuirkManager(false)?.CapModOffsetModifierFor(PawnCapacityDefOf.Moving, null) ?? 1f;
                    if (pawnData != null && pawn.IsActivePredator())
                    {
                        RV2Log.Message("Encumberance: checking predator " + pawn.ToString(), "Encumberance");
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
                                if (!voreTrackerRecord.Prey.Dead && voreTrackerRecord.Prey.IsActivePredator())
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
                            RV2Log.Message("Encumberance: doing fancy weight for " + pawn.ToString() + ":  (preyweight)" + totalWeight.ToString() + " / (predsize)" + pawn.BodySize + " / 4 * (settings,quirks)" + (RV2_Rut_Settings.rutsStuff.EncumberanceModifier * quirkMod).ToString(), "Encumberance");
                            severityInt = Math.Min(totalWeight / Math.Max(pawn.BodySize, 0.01f) / 4f * RV2_Rut_Settings.rutsStuff.EncumberanceModifier * quirkMod, 1f);
                        }
                        else
                        {
                            RV2Log.Message("Encumberance: doing simple weight for " + pawn.ToString() + ":  (totalprey)" + totalPrey.ToString() + "/ 4 * (settings,quirks)" + (RV2_Rut_Settings.rutsStuff.EncumberanceModifier * quirkMod).ToString(), "Encumberance");
                            severityInt = Math.Min(totalPrey / 4f * RV2_Rut_Settings.rutsStuff.EncumberanceModifier * quirkMod, 1f);
                        }

                        RV2Log.Message("Encumberance: severity set to " + severityInt.ToString(), "Encumberance");
                    }
                    return severityInt;
                }
                catch (Exception e)
                {
                    Log.Warning("RV-2R: Something went wrong when trying to handle encumberance: " + e);
                    return severityInt;
                }
            }
        }
        public override void Tick()
        {
            base.Tick();
            if (Severity > 0.05f && ageTicks % (RV2Mod.Settings.debug.HediffLabelRefreshInterval * 4) == 0)
            {
                pawn.health.Notify_HediffChanged(this);
            }
        }

        public override bool Visible
        {
            get
            {
                if (Severity > 0.05f && RV2_Rut_Settings.rutsStuff.VisibleEncumberance)
                    return true;
                return false;
            }
        }

        private bool hasUpdate = false;
        public List<VoreTrackerRecord> ConnectedVoreRecords = new List<VoreTrackerRecord>();
    }
}
