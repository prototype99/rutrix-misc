using RimVore2;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

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

        private Pawn ActivePawn;
        private Pawn Prey;
        private VorePathDef VorePathDef;

        protected override Job TryGiveJob(Pawn pawn)
        {
            try
            {
                ActivePawn = pawn;
                return _TryGiveJob();
                
            }
            catch (Exception e)
            {
                Log.Warning("RV-2R: Something went wrong when " + pawn.LabelShort + " tried to nom downed enemy: " + e);
                return null;
            } finally { 
                ActivePawn = null;
                Prey = null;
                VorePathDef = null;
            }
        }

        private Job _TryGiveJob()
        {
            if (GenAI.InDangerousCombat(ActivePawn))
                return null;

            if (GenAI.EnemyIsNear(ActivePawn, radius))
                return null;

            if (!ActivePawn.CanParticipateInVore(out string _))
                return null;

            PickPrey();
            if (Prey == null) return null;

            PickPath();
            if (VorePathDef == null) return null;
         
            return MakeJob();
        }

        private void PickPath()
        {
            PickPath(ValidLethalGoals());
            if (VorePathDef != null) return;

            RV2Log.Message($"Predator {ActivePawn.LabelShort} can't lethal vore {Prey}; checking for healing instead", "Jobs");
            if (MayHeal())
                PickPath(ValidHealingGoals());
            if(VorePathDef != null) return;

            PickPath(ValidNonLethalGoals());
        }
        public void PickPath(IEnumerable<VoreGoalDef> ValidGoals)
        {
            var request = new VoreInteractionRequest(ActivePawn, Prey, VoreRole.Predator, true, goalWhitelist: ValidGoals.ToList());
            IEnumerable<VorePathDef> interaction = VoreInteractionManager.Retrieve(request).ValidPaths;
            VorePathDef = interaction.FirstOrDefault();
        }
        private bool MayHeal()
        {
            if (!Prey.Faction.IsPlayer) return false;
            if (!Prey.health.HasHediffsNeedingTend()) return false;

            return true;
        }
        public IEnumerable<VoreGoalDef> ValidHealingGoals()
        {
            yield return VoreGoalDefOf.Heal;
        }
        public Job MakeJob()
        {
            if (VorePathDef == null) return null;

            VoreJob voreJob = VoreJobMaker.MakeJob(VoreJobDefOf.RV2_VoreInitAsPredator, ActivePawn);
            voreJob.targetA = Prey;
            voreJob.IsForced = true;
            voreJob.count = 1;
            voreJob.IsForced = true;
            voreJob.VorePath = VorePathDef;

            RV2Log.Message($"{ActivePawn.LabelShort} eating hostile {Prey.LabelShort} via {VorePathDef}", "Jobs");

            return voreJob;
        }
        public IEnumerable<VoreGoalDef> ValidLethalGoals()
        {
            return DefDatabase<VoreGoalDef>.AllDefsListForReading.Where((VoreGoalDef goal) => goal.IsLethal);
        }
        public IEnumerable<VoreGoalDef> ValidNonLethalGoals()
        {
            return DefDatabase<VoreGoalDef>.AllDefsListForReading.Where((VoreGoalDef goal) => !goal.IsLethal);
        }
        public void PickPrey()
        {
            Prey = GenClosest.ClosestThingReachable(ActivePawn.Position, ActivePawn.Map
                , ThingRequest.ForGroup(ThingRequestGroup.Pawn)
                , PathEndMode.OnCell
                , TraverseParms.For(ActivePawn)
                , this.radius
                , IsValidTarget
                ) as Pawn;
        }
        public bool IsValidTarget(Thing t)
        {
            if (!(t is Pawn target)) return false;

            if (!target.Downed && !ActivePawn.Faction.HostileTo(target.Faction)) return false;
            if (target.IsMechanoid()) return false;
            if (target.IsForbidden(ActivePawn)) return false;
            if (target.Map.designationManager.DesignationOn(target, RV2R_Common.Devour) == null) return false;
            if (!ActivePawn.CanReserve(target)) return false;
            if (!ActivePawn.CanVore(target, out _)) return false;
            return true;
        }
    }
}
