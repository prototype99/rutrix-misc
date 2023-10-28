/*
using HarmonyLib;
using RimVore2;
using RimWorld;
using Verse;
using static RV2R_RutsStuff.Patch_RV2R_Settings;

namespace RV2R_RutsStuff
{
    [HarmonyPatch(typeof(CompDrug), "PostIngested")]
    internal class Patch_SharedHit
    {
        [HarmonyPostfix]
        private static void ShareHit(ref CompDrug drug, ref Pawn ingester)
        {
            if (ingester.IsActivePredator() && drug.parent.def != ThingDefOf.Luciferium)
            {
                PawnData pawnData = ingester.PawnData(false);
                if (pawnData != null)
                {
                    VoreTracker voreTracker = pawnData.VoreTracker;
                    if (voreTracker != null)
                    {
                        foreach (VoreTrackerRecord rec in voreTracker.VoreTrackerRecords)
                        {
                            if (drug.Props.Addictive && ingester.RaceProps.IsFlesh)
                            {
                                HediffDef addictionHediffDef = drug.Props.chemical.addictionHediff;
                                Hediff_Addiction hediff_Addiction = AddictionUtility.FindAddictionHediff(rec.Prey, drug.Props.chemical);
                                Hediff hediff = AddictionUtility.FindToleranceHediff(rec.Prey, drug.Props.chemical);
                                float num = ((hediff != null) ? hediff.Severity : 0f)/2f;
                                if (hediff_Addiction != null)
                                {
                                    hediff_Addiction.Severity += drug.Props.existingAddictionSeverityOffset;
                                }
                                else if (Rand.Value < DrugStatsUtility.GetAddictivenessAtTolerance(drug.parent.def, num) && (num/2f) >= drug.Props.minToleranceToAddict)
                                {
                                    rec.Prey.health.AddHediff(addictionHediffDef, null, null, null);
                                    if (PawnUtility.ShouldSendNotificationAbout(rec.Prey))
                                    {
                                        Find.LetterStack.ReceiveLetter("LetterLabelNewlyAddicted".Translate(drug.Props.chemical.label).CapitalizeFirst(), "LetterNewlyAddicted".Translate(rec.Prey.LabelShort, this.Props.chemical.label, rec.Prey.Named("PAWN")).AdjustedFor(rec.Prey, "PAWN", true).CapitalizeFirst(), LetterDefOf.NegativeEvent, rec.Prey, null, null, null, null);
                                    }
                                    AddictionUtility.CheckDrugAddictionTeachOpportunity(ingester);
                                }
                                if (addictionHediffDef.causesNeed != null)
                                {
                                    Need need = rec.Prey.needs.AllNeeds.Find((Need x) => x.def == addictionHediffDef.causesNeed);
                                    if (need != null)
                                    {
                                        float needLevelOffset = drug.Props.needLevelOffset;
                                        AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(rec.Prey, drug.Props.chemical, ref needLevelOffset);
                                        need.CurLevel += needLevelOffset;
                                    }
                                }
                            }
                        }
                    }
                }

            }
        }

    }
}
*/