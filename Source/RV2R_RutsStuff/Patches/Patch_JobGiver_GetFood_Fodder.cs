using HarmonyLib;
using RimVore2;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using static RV2R_RutsStuff.Patch_RV2R_Settings;
using static UnityEngine.GraphicsBuffer;

namespace RV2R_RutsStuff {

    [HarmonyPatch(typeof(JobGiver_GetFood), "TryGiveJob")]
    public static class Patch_JobGiver_GetFodder
    {

        [HarmonyPostfix]
        public static void InterceptGetFodder(ref Job __result, Pawn pawn)
        {
            try
            {
                Job job = __result;
                if (ShouldPreemptiveSkip(pawn, job)) return;

                if (ActivePredProcessor(ref __result, pawn)) return;//Checks if is already preding that would satisfy current status

                if (!Rand.Chance(RV2_Rut_Settings.rutsStuff.FodderChance / (pawn.IsColonistPlayerControlled ? (pawn.RaceProps.predator ? 10f : 50f) : 1f))) return;

                if ( pawn.PreferenceFor(VoreRole.Predator, ModifierOperation.Add) < 0f ) return;//Doesnt want to be pred
                if (ValidGoals().Any(g => pawn.PreferenceFor(g, VoreRole.Predator, ModifierOperation.Add) > 0f)) return; //doesnt want any valid goal
                var map = pawn.Map;
                if (map == null) return;
                var nearPawns = map.mapPawns.AllPawnsSpawned
                    .Where(p => p != pawn)//Exclude self
                    .Where(p => baseCheck(p))//Base check
                    .Where(p =>
                        prisonCheck(p)
                        || (
                            p.IsColonistPlayerControlled || RV2_Rut_Settings.rutsStuff.FodderAnimalsFull
                            && (animalCheck(p) || humanoidCheck(p))
                        )
                     );
                if (!nearPawns.Any()) return;

                Dictionary<Pawn, float> fodderList = BuildFodderDesires(pawn, nearPawns);

                if (!fodderList.Any()) return;

                RV2Log.Message($"Predator {pawn.LabelShort} has {fodderList.Count.ToString()} nearby fodder", "Jobs");

                Pawn prey = fodderList.RandomElementByWeightWithDefault(fodder => fodder.Value, 0.01f).Key;

                if (prey == null) return;

                if (NonPrisonerCheck(job, prey)) return;

                RV2Log.Message($"Predator {pawn.LabelShort} picked {prey.LabelShort}", "Jobs");

                VoreInteraction voreInteraction = VoreInteractionManager.Retrieve(new VoreInteractionRequest(pawn, prey, VoreRole.Predator, true, false, false, null, null, null, null, ValidGoals().ToList()));

                VoreJob voreJob = MakeJob(pawn, prey, voreInteraction);
                if (voreJob == null) return;

                job = JobMaker.MakeJob(JobDefOf.Goto, prey);
                pawn.jobs.jobQueue.EnqueueFirst(voreJob, new JobTag?(JobTag.Idle));
                __result = job;
                return;
            }
            catch (Exception e)
            {
                Log.Warning("RV-2R: Something went wrong when trying to intercept get-food logic: " + e);
            }

            bool baseCheck(Pawn target)
            {
                if (target == null) return false;
                return pawn.CanVore(target, out var outText)
                    && pawn.Position.DistanceTo(target.Position) <= 40f
                    && pawn.Position.DistanceTo(target.Position) >= 2f // So they don't spam requests to the same snack over and over
                    && pawn.CanReach(target.Position, PathEndMode.Touch, Danger.None)
                    && pawn.CanReserve(target)
                    && !RV2R_Utilities.IsBusy(pawn, target, true)
                    && !target.IsForbidden(pawn)
                    && !VoreInteractionManager.Retrieve(new VoreInteractionRequest(pawn, target, VoreRole.Predator, true, false, false, null, null, null, null, ValidGoals().ToList())).ValidPaths.NullOrEmpty()
                ;
            };
            bool prisonCheck(Pawn target)
            {
                if (target == null) return false;
                return target.IsPrisonerInPrisonCell()
                    && ValidPrisonerInteractionModes().Contains(target.guest.ExclusiveInteractionMode);
            };
            bool animalCheck(Pawn target)
            {
                return pawn.Position.DistanceTo(target.Position) <= 15f
                     && target.IsAnimal() && target.Name != null && target.Name.Numerical
                     && ((RV2_Rut_Settings.rutsStuff.FodderPenAnimals && target.RaceProps.FenceBlocked)
                      || (RV2_Rut_Settings.rutsStuff.FodderAnimals && (!target.RaceProps.predator || target.BodySize <= pawn.BodySize * 0.65f))
                      || (RV2_Rut_Settings.rutsStuff.FodderPredators && target.RaceProps.predator)
                        )
                ;
            };
            bool humanoidCheck(Pawn target)
            {
                return pawn.Position.DistanceTo(target.Position) <= 15f
                && ((RV2_Rut_Settings.rutsStuff.FodderGuests && !target.IsColonist && !RV2R_Utilities.IsColonyHostile(pawn, target))
                      || (RV2_Rut_Settings.rutsStuff.FodderColonists && target.IsColonistPlayerControlled)
                        )
                ;
            };
        }

        private static Dictionary<Pawn, float> BuildFodderDesires(Pawn pawn, IEnumerable<Pawn> nearPawns)
        {
            Dictionary<Pawn, float> fodderList = new Dictionary<Pawn, float>();
            foreach (Pawn p in nearPawns)
            {
                if (RV2R_Utilities.GetFodderWeight(pawn, p, true) < 0f) continue;

                fodderList.Add(p, RV2R_Utilities.GetFodderWeight(pawn, p, false) * pawn.PreferenceFor(p, ModifierOperation.Add));
            }
            return fodderList;
        }
        
        private static bool CanFodder(Pawn pawn)
        {
            if (pawn.IsColonistPlayerControlled)
                return true;
            if (!pawn.Name.Numerical && RV2_Rut_Settings.rutsStuff.FodderNamedAllowed)
                return true;
            if (pawn.IsAnimal())
            {
                if (pawn.RaceProps.predator && RV2_Rut_Settings.rutsStuff.FodderPredatorsAllowed)
                    return true;
                if (!pawn.RaceProps.predator && RV2_Rut_Settings.rutsStuff.FodderAnimalsAllowed)
                    return true;
            }
            return false;
        }
        private static bool ActivePredProcessor(ref Job __result, Pawn pawn)
        {
            if (!pawn.IsActivePredator() || pawn.needs.food.CurLevelPercentage < 0.10f) return false;

            PawnData pawnData = pawn.PawnData(false);
            VoreTracker voreTracker = pawnData?.VoreTracker;
            if (voreTracker == null) return false;
            foreach (VoreTrackerRecord record in voreTracker.VoreTrackerRecords)
            {
                if (ValidGoals().Contains(record.VoreGoal))
                {
                    __result = null;
                    return true;
                }

                if (ValidFeedingVoreType().Contains(record.VoreType) && record.HasReachedEntrance)
                {
                    __result = null;
                    return true;
                }
            }
            return false;
        }


        private static VoreJob MakeJob(Pawn pawn, Pawn prey, VoreInteraction voreInteraction)
        {
            VoreJob voreJob = VoreJobMaker.MakeJob(VoreJobDefOf.RV2_VoreInitAsPredator, pawn, prey);
            voreJob.VorePath = voreInteraction.PreferredPath ?? voreInteraction.ValidPaths.RandomElement();
            voreJob.targetA = prey;
            voreJob.IsForced = prey.PreferenceFor(voreJob.VorePath.voreGoal, VoreRole.Prey, ModifierOperation.Add) <= 3f;
            if (voreJob == null) return null;
            if (!voreJob.IsForced) return voreJob;
            if (prey.IsColonistPlayerControlled)
            {
                return RemakeAsProposal();
            }
            var quirkManager = pawn.QuirkManager(false);
            if (quirkManager == null) return voreJob;
            foreach (var quirk in PredProposalOnlyQuirks())
            {
                if (quirkManager.HasQuirk(quirk)) return RemakeAsProposal();
            }
            return voreJob;

            VoreJob RemakeAsProposal()
            {
                VoreProposal_TwoWay proposal = new VoreProposal_TwoWay(pawn, prey, pawn, prey, voreJob.VorePath);
                voreJob = VoreJobMaker.MakeJob(VoreJobDefOf.RV2_ProposeVore, pawn, prey);
                voreJob.targetA = prey;
                voreJob.Proposal = proposal;
                voreJob.VorePath = proposal.VorePath;
                return voreJob;
            }
        }

        private static bool ShouldPreemptiveSkip(Pawn pawn, Job job)
        {
            if (pawn.Faction == null || !pawn.Faction.IsPlayer)
                return true;

            if (!pawn.CanBePredator(out string outText))
                return true;

            if (pawn.RaceProps.FenceBlocked)
                return true;

            if (!CanFodder(pawn))
                return true;

            if (job != null
                && job.GetTarget(TargetIndex.A).Thing is Corpse
                && !ReplaceableJobs().Contains(job.def))
                return true;

            if (job != null
                 && pawn.needs.food.CurLevelPercentage < 0.10f
                 && Rand.Chance(0.5f)) // Give up if no-one's saying yes
                return true;

            return false;
        }
        private static bool NonPrisonerCheck(Job job, Pawn prey)
        {
            return job != null && !prey.IsPrisonerInPrisonCell() && !Rand.Chance(RV2_Rut_Settings.rutsStuff.MiscFodderChance);
        }

        private static IEnumerable<QuirkDef> PredProposalOnlyQuirks()
        {
            var struggleHated = DefDatabase<QuirkDef>.GetNamedSilentFail("StrugglePreference_Hated");
            if (struggleHated != null) yield return struggleHated;
        }
        private static IEnumerable<JobDef> ReplaceableJobs()
        {
            yield return JobDefOf.Ingest;
            yield return JobDefOf.TakeFromOtherInventory;
        }
        private static IEnumerable<VoreGoalDef> ValidGoals()
        {
            yield return VoreGoalDefOf.Digest;
            yield return RV2R_Common.Drain;
        }
        private static IEnumerable<PrisonerInteractionModeDef> ValidPrisonerInteractionModes()
        {
            yield return RV2R_Common.Fodder;
        }
        private static IEnumerable<VoreTypeDef> ValidFeedingVoreType()
        {
            yield return VoreTypeDefOf.Oral;
            yield return VoreTypeDefOf.Anal;
        }
        private static IEnumerable<string> FeedingDefNames()
        {
            yield return "Intestines_Enter";
        }
        private static IEnumerable<string> FeedingDefContainsNames()
        {
            yield return "warmup";
            yield return "digest";
            yield return "churn";
        }
    }

}