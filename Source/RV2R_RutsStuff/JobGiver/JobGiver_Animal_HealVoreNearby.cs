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
    public class JobGiver_Animal_HealVoreNearby : ThinkNode_JobGiver
    {

        private const float SearchRadius = 30f;
        private const float PercentageCountsAsHealed = .9f;
        private const float PercentageSkipWhenInBed = 0.65f;
        private const float BloodLoseCriticalInTicks = 9000;
        public Pawn Pred {  get; private set; }
        public Pawn Prey { get; private set; }
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_Animal_HealVoreNearby jobGiver_Animal_HealVoreNearby = (JobGiver_Animal_HealVoreNearby)base.DeepCopy(resolve);
            return jobGiver_Animal_HealVoreNearby;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {  
            try
            {
                if (!RV2_Rut_Settings.rutsStuff.AnimalDoctors) return null;
                this.Prey = null;
                this.Pred = pawn;
                return _TryGiveJob();
            }
            catch (Exception e)
            {
                Log.Warning("RV-2R: Something went wrong when " + Pred.LabelShort + " tried to endo-rescue: " + e);
                return null;
            }
        }
        public Job _TryGiveJob()
        {
            if (!PredCanTakeJob()) return null;

            Prey = FindTarget();
            if (Prey == null) return null;

            IEnumerable<VorePathDef> validPaths = Validpaths(Pred, Prey);
            if (validPaths.EnumerableNullOrEmpty<VorePathDef>())
            {
                RV2Log.Message("Predator " + Pred.LabelShort + " can't endo-rescue " + Prey.LabelShort, "Jobs");
                return null;
            }
            VorePathDef vorePathDef = validPaths.RandomElement<VorePathDef>();
            RV2Log.Message(Prey.LabelShort + "'s getting endo rescued by " + Pred.LabelShort + " via " + vorePathDef.label, "Jobs");
            return CreateJob(vorePathDef, Pred, Prey);
        }

        private bool PredCanTakeJob()
        {

            if (!Pred.CanParticipateInVore(out string reason)) return false;
            if (GenAI.InDangerousCombat(Pred)) return false;
            if (GenAI.EnemyIsNear(Pred, SearchRadius)) return false;

            return true;
        }

        public Pawn FindTarget()
        {
            var traverseParms = TraverseParms.For(Pred);

            return GenClosest.ClosestThingReachable(
                Pred.Position, Pred.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn),
                PathEndMode.OnCell, traverseParms, maxDistance: SearchRadius, validator: ValidTarget
            ) as Pawn;
        }
        public bool ValidTarget(Thing thing)
        {
            if (!(thing is Pawn prey)) return false;
            if (prey.IsMechanoid()) return false;
            if (prey.health.summaryHealth.SummaryHealthPercent > PercentageCountsAsHealed) return false;

            if (prey.IsForbidden(Pred)) return false;
            if (!RV2R_Utilities.IsOfFriendlyFaction(Pred, prey)) return false;
            if (!IsEligibleForMedicalAttention(prey)) return false;

            if (!Pred.CanEndoVore(prey, out _, false)) return false;

            if (!Pred.CanReserve(prey)) return false;
            if (RV2R_Utilities.IsBusy(Pred, prey)) return false;

            return true;
        }
        public static VoreJob CreateJob(VorePathDef pathDef, Pawn pred, Pawn prey)
        {
            VoreJob voreJob = VoreJobMaker.MakeJob(VoreJobDefOf.RV2_VoreInitAsPredator, pred);
            voreJob.targetA = prey;
            voreJob.VorePath = pathDef;
            voreJob.count = 1;
            return voreJob;
        }
        public static IEnumerable<VorePathDef> Validpaths(Pawn pred, Pawn prey)
        {
            var request = new VoreInteractionRequest(pred, prey, VoreRole.Predator, isForAuto: true, goalWhitelist: ValidGoals().ToList());
            return VoreInteractionManager.Retrieve(request).ValidPaths;
        }
        public static IEnumerable<VoreGoalDef> ValidGoals()
        {
            yield return VoreGoalDefOf.Heal;
        }
        private static bool IsEligibleForMedicalAttention(Pawn pawn)
        {
            //This section is checking if someone is downed, or otherwise in rescuable state and not in bed
            if (pawn.health.InPainShock) return true;
            if (!pawn.Downed) return false;
            if (!pawn.InBed()) return true;

            //From here its even if they are in a bed recovering
            if (pawn.health.summaryHealth.SummaryHealthPercent > PercentageSkipWhenInBed) return false;
            
            //I'm guessing this is to filter mechnoids?
            if (pawn.IsAnimal()) return true;
            if (!pawn.IsHumanoid()) return false;

            //Do we have a reason to heal them check
            if (pawn.health.HasHediffsNeedingTendByPlayer()) return true;
            if (HealthUtility.TicksUntilDeathDueToBloodLoss(pawn) <= BloodLoseCriticalInTicks) return true;

            return false;
        }
    }
}
