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
    public class JobGiver_Animal_VoreNearby : ThinkNode_JobGiver
    {
        private float radius = 30f;
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_Animal_VoreNearby jobGiver_Animal_EndoNearby = (JobGiver_Animal_VoreNearby)base.DeepCopy(resolve);
            jobGiver_Animal_EndoNearby.radius = this.radius;
            return jobGiver_Animal_EndoNearby;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!pawn.CanParticipateInVore(out string reason))
                return null;

            Pawn target = null;

            if (RV2_Rut_Settings.rutsStuff.VornyBonds && pawn.relations.GetDirectRelationsCount(PawnRelationDefOf.Bond) > 0)
            {
                Pawn bond = pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond);
                if (bond != null
                 && (pawn.GetLord() == null && bond.GetLord() == null)
                 && !GenAI.EnemyIsNear(bond, 25f)
                 && pawn.Position.DistanceTo(bond.Position) < 25f
                 && pawn.CanReserve(bond, 1, -1, null, false)
                 && bond.CanParticipateInVore(out reason)
                 && pawn.CanEndoVore(bond, out reason, false)
                 && Rand.Chance(0.2f))
                    target = bond;
            }
            if (target == null)
            {
                Predicate<Thing> predicate = delegate (Thing t)
                {
                    Pawn pawn3 = (Pawn)t;
                    return pawn3 != pawn
                        && (pawn3.Faction == pawn.Faction || pawn3.Faction.PlayerRelationKind != FactionRelationKind.Hostile)
                        && pawn.CanReserve(pawn3, 1, -1, null, false)
                        && !pawn3.IsForbidden(pawn)
                        && !GenAI.EnemyIsNear(pawn3, 25f)
                        && !pawn.ShouldBeSlaughtered()
                        && !pawn3.ShouldBeSlaughtered()
                        && pawn3.CanParticipateInVore(out reason)
                        && pawn.CanEndoVore(pawn3, out reason, false);
                };
                Pawn pawn2 = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode.OnCell, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false), this.radius, predicate, null, 0, -1, false, RegionType.Set_Passable, false);
                if (pawn2 == null
                 || pawn.GetLord() != null
                 || pawn2.GetLord() != null)
                    return null;
                else
                    target = pawn2;
            }
            if (target == null)
                return null;
            List<VoreGoalDef> list = DefDatabase<VoreGoalDef>.AllDefsListForReading.Where((VoreGoalDef goal) => !goal.IsLethal && goal.defName != "Store").ToList();
            if (!pawn.Name.Numerical)
                list.Append(VoreGoalDefOf.Store);

            if (RV2_Rut_Settings.rutsStuff.FatalPlayVore)
                list = DefDatabase<VoreGoalDef>.AllDefsListForReading;
            VoreInteraction voreInteraction = VoreInteractionManager.Retrieve(new VoreInteractionRequest(pawn, target, VoreRole.Predator, true, false, false, null, null, null, null, list, null, null));
            if (voreInteraction.ValidPaths.EnumerableNullOrEmpty<VorePathDef>())
            {
                RV2Log.Message("Predator " + pawn.LabelShort + " can't play-pred their target " + target.LabelShort, "Jobs");
                return null;
            }
            VorePathDef vorePathDef = (voreInteraction.PreferredPath ?? voreInteraction.ValidPaths.RandomElement<VorePathDef>());
            RV2Log.Message(pawn.LabelShort + " play-predding " + target.LabelShort + " via " + vorePathDef.label, "Jobs");
            VoreProposal_TwoWay voreProposal_TwoWay = new VoreProposal_TwoWay(voreInteraction.Predator, voreInteraction.Prey, pawn, target, vorePathDef);
            VoreJob voreJob = VoreJobMaker.MakeJob(target.IsHumanoid() ? VoreJobDefOf.RV2_ProposeVore : VoreJobDefOf.RV2_VoreInitAsPredator, pawn);
            voreJob.targetA = target;
            voreJob.Proposal = voreProposal_TwoWay;
            voreJob.VorePath = vorePathDef;
            voreJob.Initiator = pawn;
            voreJob.count = 1;
            return voreJob;
        }

    }
}
