using RimVore2;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using static RV2R_RutsStuff.Patch_RV2R_Settings;

namespace RV2R_RutsStuff
{
    public class JobGiver_DoGutLovin : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            if(PreemptiveSkip(pawn)) return null;


            float predLib = 1f;

            if (pawn.QuirkManager(false).HasValueModifier("Predator_Libido"))
                predLib = pawn.QuirkManager(false).ModifyValue("Predator_Libido", predLib);
            if (predLib <= 0f) return null;

            float preyLib = 0f;
            int bestOp = -100;
            float tempPreyLib = 1f;

            bool loverPresnt = false;
            bool canLove = false;
            bool noHumanlikes = true;
            bool noAttraction = true;

            IEnumerable<Pawn> familyByBlood = pawn.relations.FamilyByBlood;
            ProcessActivePred(pawn, familyByBlood, ref predLib, ref preyLib, ref bestOp, ref tempPreyLib, ref loverPresnt, ref canLove, ref noHumanlikes, ref noAttraction);
            if (noAttraction)
                predLib /= 2f;

            bool NonCon = RV2_Rut_Settings.rutsStuff.GutLovinNonCon
                      && !pawn.story.traits.HasTrait(TraitDefOf.Kind)
                      && (pawn.story.traits.HasTrait(TraitDefOf.Psychopath)
                       || bestOp <= -10);

            if (noHumanlikes)
                return null;        // If we're not being a barn

            if (!NonCon && !canLove)
                return null;        // And -someone- in there wants it (or we don't care)

            if (!loverPresnt && !(LovePartnerRelationUtility.ExistingLovePartner(pawn, false) == null && RV2_Rut_Settings.rutsStuff.GutLovinStands))
                return null;        // And we're romanticly involved (or don't care)

            if (Rand.Chance(NonCon ? predLib : ((predLib + preyLib) / 2f)))  // And we're in the mood
                return JobMaker.MakeJob(RV2R_Common.RV2R_GutLovin, pawn.CurrentBed());                                  // Then the owo begins

            return null;
        }

        private void ProcessActivePred(Pawn pawn, IEnumerable<Pawn> familyByBlood, ref float predLib, ref float preyLib, ref int bestOp, ref float tempPreyLib, ref bool loverPresnt, ref bool canLove, ref bool noHumanlikes, ref bool noAttraction)
        {
            if (pawn.IsActivePredator()) return;
            PawnData pawnData = pawn.PawnData(false);
            if (pawnData == null) return;
            VoreTracker voreTracker = pawnData.VoreTracker;
            if (voreTracker == null) return;
            foreach(var r in voreTracker.VoreTrackerRecords)
            {
                var prey = r.Prey;
                if (familyByBlood.Any(p => p == prey && !LovePartnerRelationUtility.LovePartnerRelationExists(prey, r.Predator)))
                {
                    loverPresnt = false;
                    preyLib = 0f;
                }

                if (prey.IsHumanoid()) noHumanlikes = false;
                if (prey.QuirkManager(false)?.HasValueModifier("Prey_Libido") == true) tempPreyLib = prey.QuirkManager(true).ModifyValue("Prey_Libido", tempPreyLib);

                if (RV2R_Utilities.IsAttractedTo(pawn, prey) || predLib > 1.5) noAttraction = false;

                if (!RV2R_Utilities.IsAttractedTo(prey, pawn) && tempPreyLib < 1.5f) tempPreyLib /= 2f;

                bestOp = Math.Max(bestOp, pawn.relations.OpinionOf(prey));
                preyLib = Math.Max(preyLib, tempPreyLib);

                if (LovePartnerRelationUtility.LovePartnerRelationExists(prey, r.Predator)) loverPresnt = true;

                canLove = (canLove || preyLib > 0);
            }
        }

        private bool PreemptiveSkip(Pawn pawn)
        {
            if (pawn.mindState == null) return true;
            if (pawn.health?.capacities?.CanBeAwake == false) return true;


            if (Find.TickManager.TicksGame < pawn.mindState.canLovinTick) return true;
            if (!pawn.IsActivePredator()) return true;

            Building_Bed building_Bed = pawn.CurrentBed();
            if (building_Bed == null) return true;
            if (building_Bed.Medical) return true;
            if (building_Bed.SleepingSlotsCount > 1)//If bed has multiple slots
            {
                if (!building_Bed.CurOccupants.Any(p => p == pawn)) return true;
            }
            return false;
        }
    }
}
