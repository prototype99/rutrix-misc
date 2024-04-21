using RimVore2;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using static RV2R_RutsStuff.Patch_RV2R_Settings;

namespace RV2R_RutsStuff
{
    public class JobGiver_DoGutLovin : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Building_Bed building_Bed = pawn.CurrentBed();
            if (building_Bed == null
             || building_Bed.Medical
             || !pawn.health.capacities.CanBeAwake)
                return null;

            if (!pawn.IsActivePredator())
                return null;

            if (Find.TickManager.TicksGame < pawn.mindState.canLovinTick)
                return null;

            if (building_Bed.SleepingSlotsCount > 1)
                using (IEnumerator<Pawn> enumerator = building_Bed.CurOccupants.GetEnumerator())
                    while (enumerator.MoveNext())
                        if (enumerator.Current != pawn)
                            return null;

            float predLib = 1f;
            float preyLib = 0f;
            int bestOp = -100;
            float tempPreyLib = 1f;

            if (pawn.QuirkManager(false).HasValueModifier("Predator_Libido"))
                predLib = pawn.QuirkManager(false).ModifyValue("Predator_Libido", predLib);

            if (predLib <= 0f)
                return null;

            bool loverPresnt = false;
            bool canLove = false;
            bool noHumanlikes = true;
            bool noAttraction = true;

            IEnumerable<Pawn> familyByBlood = pawn.relations.FamilyByBlood;
            if (pawn.IsActivePredator())
            {
                PawnData pawnData = pawn.PawnData(false);
                if (pawnData != null)
                {
                    VoreTracker voreTracker = pawnData.VoreTracker;
                    if (voreTracker != null)
                        foreach (VoreTrackerRecord voreTrackerRecord in voreTracker.VoreTrackerRecords)
                        {
                            Pawn preyPawn = voreTrackerRecord.Prey;
                            foreach (Pawn pawn2 in familyByBlood)
                                if (preyPawn == pawn2
                                 && !LovePartnerRelationUtility.LovePartnerRelationExists(voreTrackerRecord.Prey, voreTrackerRecord.Predator))
                                {
                                    loverPresnt = false;
                                    predLib = 0f;
                                    break;
                                }
                            
                            if (voreTrackerRecord.Prey.IsHumanoid())
                                noHumanlikes = false;

                            if (voreTrackerRecord.Prey.QuirkManager(false).HasValueModifier("Prey_Libido"))
                                tempPreyLib = voreTrackerRecord.Prey.QuirkManager(false).ModifyValue("Prey_Libido", tempPreyLib);

                            if (noAttraction
                             && (pawn.gender != preyPawn.gender || pawn.story.traits.HasTrait(TraitDefOf.Gay)) || pawn.story.traits.HasTrait(TraitDefOf.Bisexual)
                             || predLib >= 1.5f)
                                noAttraction = false;

                            if (tempPreyLib < 1.5f
                             && preyPawn.gender == pawn.gender && !preyPawn.story.traits.HasTrait(TraitDefOf.Gay)
                             && !preyPawn.story.traits.HasTrait(TraitDefOf.Bisexual))
                                tempPreyLib /= 2f;

                            bestOp = Math.Max(bestOp, pawn.relations.OpinionOf(voreTrackerRecord.Prey));

                            preyLib = Math.Max(preyLib, tempPreyLib);  // Use the vornyest prey's score

                            if (LovePartnerRelationUtility.LovePartnerRelationExists(voreTrackerRecord.Prey, voreTrackerRecord.Predator))
                                loverPresnt = true;

                            canLove = (preyLib > 0 || canLove);
                        }
                }
            }
            if (noAttraction)
                predLib /= 2f;

            bool pyscho = RV2_Rut_Settings.rutsStuff.GutLovinNonCon
                      && !pawn.story.traits.HasTrait(TraitDefOf.Kind)
                      && (pawn.story.traits.HasTrait(TraitDefOf.Psychopath)
                       || bestOp <= -10);

            if (noHumanlikes)
                return null;        // If we're not being a barn

            if (!pyscho && !canLove)
                return null;        // And -someone- in there wants it (or we don't care)

            if (!loverPresnt && !(LovePartnerRelationUtility.ExistingLovePartner(pawn, false) == null && RV2_Rut_Settings.rutsStuff.GutLovinStands))
                return null;        // And we're romanticly involved (or don't care)

            if (Rand.Chance(pyscho ? predLib : ((predLib + preyLib) / 2f)))  // And we're in the mood
                return JobMaker.MakeJob(RV2R_Common.RV2R_GutLovin, pawn.CurrentBed());                                  // Then the owo begins

            return null;
        }
    }
}
