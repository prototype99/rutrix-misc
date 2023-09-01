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

        protected override Job TryGiveJob(Pawn pawn)
        {
            RV2Log.Message(" 1 ", "Jobs");
            if (GenAI.InDangerousCombat(pawn))
                return null;

            RV2Log.Message(" 2 ", "Jobs");
            Predicate<Thing> predicate = delegate (Thing t)
            {
                Pawn pawn3 = (Pawn)t;
                return pawn3.Downed
                    && !pawn3.IsPrisonerOfColony
                    && pawn.CanReserve(pawn3, 1, -1, null, false)
                    && !pawn3.IsForbidden(pawn)
                    && !pawn.ShouldBeSlaughtered()
                    //&& pawn.Map.designationManager.DesignationOn(pawn3) == null
                    && pawn3.Faction != pawn.Faction
                    && (pawn3.Faction == null || pawn3.Faction.PlayerRelationKind == FactionRelationKind.Hostile);
            };
            RV2Log.Message(" 3 ", "Jobs");
            Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false), this.radius, predicate, null, 0, -1, false, RegionType.Set_Passable, false);
            if (pawn2 == null)
                return null;

            RV2Log.Message(" 4 ", "Jobs");
            List<VoreGoalDef> list = DefDatabase<VoreGoalDef>.AllDefsListForReading.Where((VoreGoalDef goal) => goal.IsLethal).ToList();

            RV2Log.Message(" 5 ", "Jobs");
            IEnumerable<VorePathDef> interaction = VoreInteractionManager.Retrieve(new VoreInteractionRequest(pawn, pawn2, VoreRole.Predator, true, false, false, null, null, null, null, list, null, null, null)).ValidPaths;

            RV2Log.Message(" 6 ", "Jobs");
            if (!interaction.EnumerableNullOrEmpty<VorePathDef>())
            {
                VorePathDef vorePathDef = interaction.RandomElement<VorePathDef>();
                RV2Log.Message("Eating hostile " + vorePathDef.label + " via " + vorePathDef.ToString(), "Jobs");
                VoreJob voreJob = VoreJobMaker.MakeJob(VoreJobDefOf.RV2_VoreInitAsPredator, pawn2);
                voreJob.targetA = pawn2;
                voreJob.VorePath = vorePathDef;
                voreJob.Initiator = pawn;
                voreJob.count = 1;
                return voreJob;
            }
            RV2Log.Message("Predator " + pawn.LabelShort + " can't fatal vore enemy; checking for healing instead", "Jobs");
            list = new List<VoreGoalDef> { VoreGoalDefOf.Heal };
            interaction = VoreInteractionManager.Retrieve(new VoreInteractionRequest(pawn, pawn2, VoreRole.Predator, true, false, false, null, null, null, null, list, null, null, null)).ValidPaths;
            if (interaction.EnumerableNullOrEmpty<VorePathDef>())
            {
                RV2Log.Message("Predator " + pawn.LabelShort + " can't heal vore enemy, no predation", "Jobs");
                return null;
            }
            VorePathDef vorePathDef2 = interaction.RandomElement<VorePathDef>();
            RV2Log.Message("Capturing hostile " + vorePathDef2.label + " via " + vorePathDef2.ToString(), "Jobs");
            VoreJob voreJob2 = VoreJobMaker.MakeJob(VoreJobDefOf.RV2_VoreInitAsPredator, pawn2);
            voreJob2.targetA = pawn2;
            voreJob2.VorePath = vorePathDef2;
            voreJob2.Initiator = pawn;
            voreJob2.count = 1;
            return voreJob2;
        }
    }
}
