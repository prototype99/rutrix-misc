using RimVore2;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using static RV2R_RutsStuff.Patch_RV2R_Settings;

namespace RV2R_RutsStuff
{
    public class JobGiver_Animal_DevourNearby : ThinkNode_JobGiver
    {

        private float radius = 30f;
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_Animal_DevourNearby jobGiver_Animal_DevourNearby = (JobGiver_Animal_DevourNearby)base.DeepCopy(resolve);
            jobGiver_Animal_DevourNearby.radius = this.radius;
            return jobGiver_Animal_DevourNearby;
        }
        protected bool PreemptiveSkip(Pawn pawn)
        {
            if (!pawn.CanParticipateInVore(out string reason))
                return true;

            if (GenAI.InDangerousCombat(pawn))
                return true;

            if (GenAI.EnemyIsNear(pawn, radius))
                return true;

            return false;
        }
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (PreemptiveSkip(pawn)) return null;

            try
            {
                var prey = SelectPrey(pawn);
                if (prey == null) return null;

                VoreJob voreJob = MakeBaseJob(prey, pawn);
                if(TryLethalGoal(prey, pawn, ref voreJob)) return voreJob;
                RV2Log.Message($"Predator {pawn.LabelShort} can't fatal vore {prey.LabelShort}; checking for healing instead", true, "Jobs");
                if (TryCaptureGoal(prey, pawn, ref voreJob)) return voreJob;
            }
            catch (Exception e)
            {
                Log.Warning("RV-2R: Something went wrong when " + pawn.LabelShort + " tried to nom downed enemy: " + e);
                return null;
            }
            return null;
        }

        private Pawn SelectPrey(Pawn pawn)
        {
            bool predicate(Thing t)
            {
                Pawn target = t as Pawn;
                if (target == null) return false;
                return target.Downed
                    && pawn.CanVore(target, out var reason)
                    && pawn.CanReserve(target, 1, -1, null, false)
                    && !target.IsMechanoid()
                    && !target.IsForbidden(pawn)
                    && RV2R_Utilities.IsColonyHostile(pawn, target);
            }
            Pawn prey = GenClosest.ClosestThingReachable(
                pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn),
                PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly,
                TraverseMode.ByPawn, false, false, false), this.radius,
                predicate, null, 0, -1, false, RegionType.Set_Passable, false
                ) as Pawn;
            return prey;
        }

        private bool TryCaptureGoal(Pawn prey, Pawn pawn, ref VoreJob voreJob)
        {
            if (!RV2_Rut_Settings.rutsStuff.EndoCapture)
            {
                RV2Log.Message($"Predator {pawn.LabelShort} can't fatal or heal vore {prey.LabelShort}, no predation", true, "Jobs");
                return false;
            }
            if (prey.IsAnimal() && pawn.Map.designationManager.DesignationOn(prey, DesignationDefOf.Tame) == null)
            {
                RV2Log.Message($"Predator {pawn.LabelShort} can't fatal or heal vore {prey.LabelShort}, no predation; designate to tame to capture", true, "Jobs");
                return false;
            }
            if ((prey.IsInsectoid() && !prey.IsHumanoid()) // Needs to be set up like this because of Apini; they're made of insect meat
                && (!RV2_Rut_Settings.rutsStuff.InsectoidCapture || pawn.Map.designationManager.DesignationOn(prey, DesignationDefOf.Tame) == null))
            {
                return false;
            }

            var VoreGoals = ValidCaptureGoals();
            var interaction = VoreInteractionManager.Retrieve(new VoreInteractionRequest(pawn, prey, VoreRole.Predator, true, false, false, null, null, null, null, VoreGoals.ToList())).ValidPaths;
            if (interaction.EnumerableNullOrEmpty<VorePathDef>())
            {
                RV2Log.Message("Predator " + pawn.LabelShort + " can't heal vore " + prey.LabelShort + ", no predation", true, "Jobs");
                return false;
            }

            var vorePathDef = interaction.RandomElement();
            RV2Log.Message(pawn.LabelShort + " capturing hostile " + prey.LabelShort + " via " + vorePathDef.ToString(), "Jobs");
            voreJob.VorePath = vorePathDef;
            voreJob.IsForced = true;
            return true;
        }

        private IEnumerable<VoreGoalDef> ValidLethalGoal()
        {
            return DefDatabase<VoreGoalDef>.AllDefsListForReading.Where((VoreGoalDef goal) => goal.IsLethal);
        }
        private IEnumerable<VoreGoalDef> ValidCaptureGoals()
        {
            yield return VoreGoalDefOf.Heal;
        }

        private bool TryLethalGoal(Pawn prey, Pawn pawn, ref VoreJob voreJob)
        {

            var lethalGoals = ValidLethalGoal();

            IEnumerable<VorePathDef> interaction = VoreInteractionManager.Retrieve(new VoreInteractionRequest(pawn, prey, VoreRole.Predator, true, false, false, null, null, null, null, lethalGoals.ToList())).ValidPaths;
            
            if (interaction.EnumerableNullOrEmpty()) return false;

            var vorePathDef = interaction.RandomElement();
            RV2Log.Message($"{pawn.LabelShort} eating hostile {prey.LabelShort} via {vorePathDef}", "Jobs");
            voreJob.VorePath = vorePathDef;
            voreJob.IsForced = true;

            return true;
        }

        private VoreJob MakeBaseJob(Pawn prey, Pawn pawn)
        {
            var voreJob = VoreJobMaker.MakeJob(VoreJobDefOf.RV2_VoreInitAsPredator, prey);
            voreJob.targetA = prey;
            voreJob.Initiator = pawn;
            voreJob.IsForced = true;
            voreJob.count = 1;
            return voreJob;
        }
    }
}
