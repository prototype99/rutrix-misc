using HarmonyLib;
using RimVore2;
using RimWorld;
using System;
using System.Linq;
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

#if v1_3
            bool VaildForEffects = !__instance.VoreGoal.IsLethal;          
#else
            bool VaildForEffects = (!__instance.VoreGoal.IsLethal &&
                 !(__instance.Prey.genes.xenotypeName == "basic android" || __instance.Prey.genes.xenotypeName == "awakened android" || !(__instance.Predator.genes.xenotypeName == "basic android" || __instance.Predator.genes.xenotypeName == "awakened android")));
            // I appologize to your androids, but support for them's gonna be some work; just trying to make them not explode
#endif
            if (VaildForEffects)
            {
                SettingsContainer_RutsStuff settings = RV2_Rut_Settings.rutsStuff;
                if (settings.EndoSicknessStrength > 0f)
                {
                    Hediff firstHediffOfDef = __instance.Prey.health.hediffSet.GetFirstHediffOfDef(RV2R_Common.EndoHediff, false);
                    if (firstHediffOfDef != null)
                    {
                        firstHediffOfDef.Severity = Math.Min(firstHediffOfDef.Severity + (0.0135f * settings.EndoSicknessStrength), 1f);
                        if (settings.EndoPacify && firstHediffOfDef.Severity >= 0.95f)
                        {
                            if (__instance.StruggleManager.shouldStruggle) 
                                __instance.StruggleManager.shouldStruggle = false;
                            if (RV2R_Utilities.IsColonyHostile(__instance.Predator, __instance.Prey))
                                __instance.Prey.guest.SetGuestStatus(__instance.Predator.Faction, GuestStatus.Prisoner);
                        }
                    }
                    else
                        __instance.Prey.health.AddHediff(RV2R_Common.EndoHediff, null, null, null);
                }

                if (settings.NoBleedOut)
                {
                    Hediff bleedOut = __instance.Prey.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss, false);
                    if (bleedOut != null)
                        bleedOut.Severity = Math.Min(bleedOut.Severity, 0.9f);
                }
                if (settings.StopBleeding)
                {
                    foreach (Hediff hediff in __instance.Prey.health.hediffSet.hediffs.Where((Hediff diff) => diff.Bleeding))
                    {
                        hediff.def.injuryProps.bleedRate *= 0.75f;
                        __instance.Prey.health.Notify_HediffChanged(hediff);
                    }
                }
                if (settings.NoBadTemp)
                {
                    Hediff hypo = __instance.Prey.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Hypothermia, false);
                    if (hypo != null)
                        hypo.Severity = Math.Min(hypo.Severity, 0.29f);
                    Hediff hyper = __instance.Prey.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Heatstroke, false);
                    if (hyper != null)
                        hyper.Severity = Math.Min(hyper.Severity, 0.29f);
                }

                if (settings.RegressionStrength > 0f
                 && __instance.CurrentVoreStage.def.displayPartName == "womb"
                 && __instance.Prey.ageTracker.AgeBiologicalTicks > __instance.Prey.ageTracker.AdultMinAgeTicks)
                {
                    long num = Math.Min(__instance.Prey.ageTracker.AgeBiologicalTicks - __instance.Prey.ageTracker.AdultMinAgeTicks, (long)Math.Ceiling(GenDate.TicksPerDay * settings.RegressionStrength));
                    __instance.Prey.ageTracker.AgeBiologicalTicks -= num;
                    Message message = new Message();
                    if (settings.ChronicCure)
                    {
                        foreach (Hediff hediff in __instance.Prey.health.hediffSet.hediffs.Where((Hediff diff) => diff.def.chronic))
                        {
                            float ratio = __instance.Prey.ageTracker.AdultMinAgeTicks / __instance.Prey.ageTracker.AgeBiologicalTicks;
                            if (ratio > 0.5f)
                            {
                                if (hediff.TryGetComp<HediffComp_SeverityPerDay>() != null)
                                {
                                    hediff.Severity = Math.Max(hediff.Severity - 0.002f * (ratio / 2f), hediff.def.minSeverity > 0f ? hediff.def.minSeverity : 0f);
                                    if (hediff.Severity <= 0f)
                                    {
                                        message = new Message("HealingCureHediff".Translate(__instance.Prey, hediff.def.label), MessageTypeDefOf.PositiveEvent, new LookTargets(__instance.Prey));
                                        Messages.Message(message, true);
                                        __instance.Prey.health.RemoveHediff(__instance.Prey.health.hediffSet.GetFirstHediffOfDef(hediff.def, false));
                                    }
                                }
                                else if (Rand.Chance(0.0005f * (ratio / 2f)))
                                {
                                    message = new Message("HealingCureHediff".Translate(__instance.Prey, hediff.def.label), MessageTypeDefOf.PositiveEvent, new LookTargets(__instance.Prey));
                                    Messages.Message(message, true);
                                    __instance.Prey.health.RemoveHediff(__instance.Prey.health.hediffSet.GetFirstHediffOfDef(hediff.def, false));
                                }
                            }
                        }
                    }
                    PawnData pawnData = __instance.Prey.PawnData(true);
                    if (pawnData != null && pawnData.VoreTracker != null)
                    {
                        VoreTracker voreTracker = pawnData.VoreTracker;
                        foreach (VoreTrackerRecord voreTrackerRecord in voreTracker.VoreTrackerRecords)
                        {
                            num = Math.Min(voreTrackerRecord.Prey.ageTracker.AgeBiologicalTicks - voreTrackerRecord.Prey.ageTracker.AdultMinAgeTicks, (long)Math.Ceiling(GenDate.TicksPerDay * settings.RegressionStrength));
                            voreTrackerRecord.Prey.ageTracker.AgeBiologicalTicks -= num;
                            if (settings.ChronicCure)
                            {
                                float ratio = voreTrackerRecord.Prey.ageTracker.AdultMinAgeTicks / voreTrackerRecord.Prey.ageTracker.AgeBiologicalTicks;
                                foreach (Hediff hediff in voreTrackerRecord.Prey.health.hediffSet.hediffs.Where((Hediff diff) => diff.def.chronic))
                                {
                                    if (ratio > 0.5f)
                                    {
                                        if (hediff.TryGetComp<HediffComp_SeverityPerDay>() != null)
                                        {
                                            hediff.Severity = Math.Max(hediff.Severity - 0.002f * (ratio / 2f), hediff.def.minSeverity > 0f ? hediff.def.minSeverity : 0f);
                                            if (hediff.Severity <= 0f)
                                            {
                                                message = new Message("HealingCureHediff".Translate(voreTrackerRecord.Prey, hediff.def.label), MessageTypeDefOf.PositiveEvent, new LookTargets(voreTrackerRecord.Prey));
                                                Messages.Message(message, true);
                                                voreTrackerRecord.Prey.health.RemoveHediff(voreTrackerRecord.Prey.health.hediffSet.GetFirstHediffOfDef(hediff.def, false));
                                            }
                                        }
                                        else if (Rand.Chance(0.0005f * (ratio / 2f)))
                                        {
                                            message = new Message("HealingCureHediff".Translate(voreTrackerRecord.Prey, hediff.def.label), MessageTypeDefOf.PositiveEvent, new LookTargets(voreTrackerRecord.Prey));
                                            Messages.Message(message, true);
                                            voreTrackerRecord.Prey.health.RemoveHediff(voreTrackerRecord.Prey.health.hediffSet.GetFirstHediffOfDef(hediff.def, false));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (settings.EndoBondChance > 0.0000f)
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
                     && (validSetup || !settings.PreyOnlyEndoBond)
                     && (!__instance.IsForced || !settings.WillingOnlyEndoBond)
                     && animal.relations.GetDirectRelationsCount(PawnRelationDefOf.Bond, null) == 0)
                    {
                        if (Rand.Chance(settings.EndoBondChance))
                        {
                            if (animal.Faction == null)
                                InteractionWorker_RecruitAttempt.DoRecruit(humanoid, animal, false);

                            RelationsUtility.TryDevelopBondRelation(humanoid, animal, 100f);
                            animal.training.Train(TrainableDefOf.Tameness, humanoid, true);
                            animal.training.Train(TrainableDefOf.Obedience, humanoid, true);
                        }
                    }
                }
            }
        }
    }
}
