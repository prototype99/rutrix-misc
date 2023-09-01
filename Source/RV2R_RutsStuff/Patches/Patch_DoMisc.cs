using HarmonyLib;
using RimVore2;
using RimWorld;
using System;
using Verse;
using static RV2R_RutsStuff.Patch_RV2R_Settings;

namespace RV2R_RutsStuff
{
    [HarmonyPatch(typeof(VoreTrackerRecord), "TickRare")]
    internal class Patch_DoMisc
    {
        [HarmonyPostfix]
        static void AdditionalEffects(VoreTrackerRecord __instance)
        {
            if (!__instance.VoreGoal.IsLethal)
            {
                /*
                if (RV2_Rut_Settings.rutsStuff.EndoSicknessStrength > 0f)
                {
                    Hediff firstHediffOfDef = __instance.Prey.health.hediffSet.GetFirstHediffOfDef(RV2R_Common.EndoHediff, false);
                    if (firstHediffOfDef != null)
                        firstHediffOfDef.Severity = Math.Min(firstHediffOfDef.Severity + (0.0135f * RV2_Rut_Settings.rutsStuff.EndoSicknessStrength), 1f);
                    else
                        __instance.Prey.health.AddHediff(RV2R_Common.EndoHediff, null, null, null);
                }
                */
                if (RV2_Rut_Settings.rutsStuff.RegressionStrength > 0f
                 && __instance.CurrentVoreStage.def.displayPartName == "womb"
                 && __instance.Prey.ageTracker.AgeBiologicalTicks > __instance.Prey.ageTracker.AdultMinAgeTicks)
                {
                    long num = Math.Min(__instance.Prey.ageTracker.AgeBiologicalTicks - __instance.Prey.ageTracker.AdultMinAgeTicks, (long)Math.Ceiling(GenDate.TicksPerDay * RV2_Rut_Settings.rutsStuff.RegressionStrength));
                    __instance.Prey.ageTracker.AgeBiologicalTicks -= num;
                    PawnData pawnData = __instance.Prey.PawnData(true);
                    if (pawnData != null && pawnData.VoreTracker != null)
                    {
                        VoreTracker voreTracker = pawnData.VoreTracker;
                        foreach (VoreTrackerRecord voreTrackerRecord in voreTracker.VoreTrackerRecords)
                        {
                            num = Math.Min(voreTrackerRecord.Prey.ageTracker.AgeBiologicalTicks - voreTrackerRecord.Prey.ageTracker.AdultMinAgeTicks, (long)Math.Ceiling(GenDate.TicksPerDay * RV2_Rut_Settings.rutsStuff.RegressionStrength));
                            voreTrackerRecord.Prey.ageTracker.AgeBiologicalTicks -= num;
                        }
                    }
                }
                if (RV2_Rut_Settings.rutsStuff.EndoBondChance > 0f)
                {
                    bool validSetup = false;
                    Pawn humanoid;
                    Pawn animal;
                    if (__instance.Predator.IsHumanoid())
                    {
                        humanoid = __instance.Predator;
                        animal = __instance.Prey;
                        validSetup = true;
                    }
                    else
                    {
                        humanoid = __instance.Prey;
                        animal = __instance.Predator;
                    }
                    if (humanoid.IsColonist
                     && (validSetup || !RV2_Rut_Settings.rutsStuff.PreyOnlyEndoBond)
                     && animal.relations.GetDirectRelationsCount(PawnRelationDefOf.Bond, null) == 0)
                    {
                        if (Rand.Chance(RV2_Rut_Settings.rutsStuff.EndoBondChance))
                        {
                            if (animal.Faction == null)
                                InteractionWorker_RecruitAttempt.DoRecruit(humanoid, animal, false);

                            RelationsUtility.TryDevelopBondRelation(humanoid, animal, 5f);
                            animal.training.Train(TrainableDefOf.Tameness, humanoid, true);
                            animal.training.Train(TrainableDefOf.Obedience, humanoid, true);
                        }
                    }
                }
            }
        }
    }
}
