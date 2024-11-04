using RimVore2;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using static RimWorld.PsychicRitualRoleDef;
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
                Log.Warning("RV-2R: Something went wrong when " + pawn.LabelShort + " tried to play-pred: " + e);
                return null;
            }
            finally
            {
                ActivePawn = null;
                Prey = null;
                VorePathDef = null;
            }
        }

        private protected Job _TryGiveJob()
        {
            if (GenAI.InDangerousCombat(ActivePawn)) return null;
            if (GenAI.EnemyIsNear(ActivePawn, radius)) return null;
            if (!ActivePawn.CanParticipateInVore(out string reason)) return null;


            PickPrey();
            if (Prey == null) return null;
            PickVorePathDef();
            if(VorePathDef == null) return null;
            RV2Log.Message($"{ActivePawn.Label} play-predding {Prey.Label} via {VorePathDef.label}", "Jobs");

            return MakeJob();
        }

        public Job MakeJob()
        {
            VoreProposal_TwoWay voreProposal_TwoWay = new VoreProposal_TwoWay(ActivePawn, Prey, ActivePawn, Prey, VorePathDef);
            var initDef = Prey.IsColonistPlayerControlled ? VoreJobDefOf.RV2_ProposeVore : VoreJobDefOf.RV2_VoreInitAsPredator;
            VoreJob Job = VoreJobMaker.MakeJob(initDef, ActivePawn);
            Job.targetA = Prey;
            Job.Proposal = voreProposal_TwoWay;
            Job.VorePath = VorePathDef;
            Job.count = 1;
            return Job;
        }
        public void PickVorePathDef()
        {
            var request = new VoreInteractionRequest(ActivePawn, Prey, VoreRole.Predator, true, goalWhitelist: ValidGoals().ToList());
            var Interaction = new VoreInteraction(request);
            VorePathDef = Interaction.PreferredPath ?? Interaction.ValidPaths.RandomElement();
        }
        public IEnumerable<VoreGoalDef> ValidGoals()
        {
            foreach(var def in DefDatabase<VoreGoalDef>.AllDefsListForReading)
            {
                if (def.IsLethal && !RV2_Rut_Settings.rutsStuff.FatalPlayVore) continue;
                yield return def;
            }
        }
        private void PickPrey()
        {
            PickVornyBond();
            if (Prey == null) return;
            PickIndescriminate();
        }

        private void PickIndescriminate()
        {
            Prey = GenClosest.ClosestThingReachable(
                ActivePawn.Position,
                ActivePawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.Pawn),
                PathEndMode.OnCell,
                TraverseParms.For(ActivePawn),
                this.radius,
                IsValidForIndescriminate
            ) as Pawn;
        }
        private bool IsValidForIndescriminate(Thing t)
        {
            if(!(t is Pawn prey)) return false;
            if(prey.Faction?.IsPlayer == true)
            {
                if (!RV2_Rut_Settings.rutsStuff.PlayVoreIndescriminate) 
                    return false;
            }

            if (GenAI.InDangerousCombat(prey)) return false;
            if (ActivePawn.CanReserve(prey)) return false;
            if (prey.IsForbidden(ActivePawn)) return false;

            if (RV2R_Utilities.IsBusy(ActivePawn, prey)) return false;
            if (RV2R_Utilities.IsFreeHostileToColony(ActivePawn, prey)) return false;
            if (!ActivePawn.CanVore(prey, out _)) return false;

            if (!RV2R_Utilities.IsSapient(prey)) return true;
            if (Rand.Chance(RV2_Rut_Settings.rutsStuff.PlayVoreColonistBias)) return true;

            return false;
        }

        private void PickVornyBond()
        {
            if (!RV2_Rut_Settings.rutsStuff.VornyBonds) return;
            if (!Rand.Chance(.2f)) return;
            var validBonds = GetValidBonds();
            if(validBonds.EnumerableNullOrEmpty()) return;
            Prey = validBonds.RandomElement();
        }
        public IEnumerable<Pawn> GetValidBonds()
        {
            return ActivePawn.relations.DirectRelations.Where(r => BondRelationsDefs().Contains(r.def) && IsValidForVornyBond(r)).Select(r=>r.otherPawn);
        }

        private bool IsValidForVornyBond(DirectPawnRelation relation)
        {
            var prey = relation.otherPawn;
            if (ActivePawn.CanReserve(prey)) return false;
            if (prey.Position.DistanceTo(ActivePawn.Position) > radius) return false;
            if (ActivePawn.CanVore(prey, out _)) return false;

            return true;
        }

        public IEnumerable<PawnRelationDef> BondRelationsDefs()
        {
            yield return PawnRelationDefOf.Bond;
        }
    }
}
