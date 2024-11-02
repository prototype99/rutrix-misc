using DefOfs;
using RimVore2;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RV2R_RutsStuff
{
    internal static class RV2R_Utilities
    {
        public static VoreTracker GetVoreTracker(this Pawn pawn, bool initalize = false)
        {
            return pawn?.PawnData(initalize)?.VoreTracker;
        }

        public static bool IsInControllableState(Pawn pawn)
        {
            if(pawn.Dead) return false;
            if(pawn.IsBurning()) return false;
            if(pawn.InMentalState) return false;
            if (pawn.Downed) return false;

            return true;
        }
        static public bool IsBusy(Pawn pawn, Pawn target, bool respectMasterJob = false)
        {
            if (IsPawnBusy(pawn)) return true;
            if (IsPawnBusy(target)) return true;

            if (pawn.Faction == null) return false;
            if (pawn.IsHumanoid()) return false;
            if (!pawn.Faction.IsPlayer) return false;

            if(!respectMasterJob) return false;
            if (pawn?.playerSettings?.RespectedMaster == null) return false;
            if (pawn.playerSettings.followDrafted && pawn.playerSettings.RespectedMaster.Drafted) return true;
            if (pawn.playerSettings.followFieldwork && pawn.playerSettings.RespectedMaster.mindState.lastJobTag == JobTag.Fieldwork) return true;

            return false;

            
        }
        public static bool IsPawnBusy(Pawn pawn)
        {
            if (pawn.CarriedBy is null) return true;

            if (pawn.Drafted) return true;

            if (pawn.GetLord() != null && BusyLordJobs().Contains(pawn.GetLord().LordJob.GetType())) return true;

            if (pawn.IsBurning()) return true;

            if (pawn.ShouldBeSlaughtered()) return true;

            return false;
        }
        private static IEnumerable<Type> BusyLordJobs()
        {
            yield return typeof(LordJob_FormAndSendCaravan);
        }

        static public bool IsFreeHostileToColony(Pawn pawn, Pawn target)
        {
            if (pawn?.Faction?.IsPlayer != true) return false;//if you don't have a faction or is part of players not hostile
            if (target?.Faction?.HostileTo(Faction.OfPlayer) != true) return false;//If target is not a part of a faction or the faction is not hostile
            if (target.IsPrisonerOfColony) return false;//If target is a prisoner then they aren't actively hostile

            return true;
        }

        static public bool IsSapient(Pawn pawn)
        {
            if (pawn.IsHumanoid())
                return true;
            if (pawn.IsMechanoid())
                return false;
            if (pawn.IsColonistPlayerControlled) // Sentiant Animals
                return true;
            if (pawn.needs.mood != null) // Pawnmorpher, Kyulen
                return true;
            return false;
        }

        private static float GetFodderWeightAnimal(Pawn pawn, Pawn target, bool DoCheck)
        {
            //"check" is just that; a pre-emptive check to see if a target is valid.
            //We do a "check" run first to populate a list, then another to get odds.
            //Would likly be improved via the method returning a dictionary or something.

            if (DoCheck)
            {
                var colonySpecies = target.Map.mapPawns.SpawnedColonyAnimals.FindAll(p => 
                    p.def == target.def 
                    && p.gender == target.gender
                    && !target.IsReserved()
                );
                if (colonySpecies.Count > 2) return 1;
                return 0;
            }

            if(target.training == null) return 0;

            int learnedTrainables = DefDatabase<TrainableDef>.AllDefsListForReading.Sum(d=>target.training.HasLearned(d) ? 1 : 0);

            float idealness = Mathf.Abs(target.BodySize * 2f / pawn.BodySize);
            if (idealness > 1f)
                idealness = Mathf.Pow(idealness, -1f); // Gives highest weight to prey that are half a predator's size; makes foxxo want to nom chickm and bunbun

            return (2f * idealness) / learnedTrainables;
        }
        private static float GetFodderWeightHumanoid(Pawn pawn, Pawn target, bool DoColonySafetyCheck)
        {
            return (100 - pawn.relations.OpinionOf(target)) / 50 / (target.IsColonist ? 1.5f : 1f); // Don't eat people you like
        }
        static public float GetFodderWeight(Pawn pawn, Pawn target, bool DoColonySafetyCheck)
        {
            if(pawn.Map == null) return 0;
            if(target.Map == null) return 0;

            if (DoColonySafetyCheck && target.IsColonist && Rand.Chance(.5f)) return 0;
            if (target.IsPrisonerInPrisonCell()) return 0;

            if (target.IsAnimal()) return GetFodderWeightAnimal(pawn, target, DoColonySafetyCheck);
            else if (target.IsHumanoid()) return GetFodderWeightHumanoid(pawn, target, DoColonySafetyCheck);
            else return 0;
        }

        static public bool IsOfFriendlyFaction(Pawn pawn, Pawn target)
        {
            if(pawn.Faction == null) return false;

            if(target.Faction == pawn.Faction) return true;
            if(target?.Faction?.AllyOrNeutralTo(pawn.Faction) == true) return true;
            if(target.IsHumanoid() && target.GuestStatus != null) return true;
            if(!target.IsHumanoid() && pawn.Map.designationManager.DesignationOn(target, DesignationDefOf.Tame) != null) return true;
            return false;
        }
        static public bool IsInTargetMidsection(Pawn prey, Pawn predator)// This is used for gutlovin'... maybe include tail? Damn that's kinky, huh
        {
            var voreTracker = prey.GetVoreTracker();
            if (voreTracker == null) return false;

            if(!predator.IsActivePredator()) return false;

            return voreTracker.VoreTrackerRecords.Any(r =>
            {
                if (r.Prey != prey) return false;
                return MidsectionVoreStageDefs().Contains(r.CurrentVoreStage.def);
            });
        }
        public static IEnumerable<VoreStageDef> MidsectionVoreStageDefs()
        {
            yield return VoreStageDefOfs.Stomach_Warmup;
            yield return VoreStageDefOfs.Stomach_Warmup_Fast;
            yield return VoreStageDefOfs.Stomach_Churn;
            yield return VoreStageDefOfs.Stomach_SoftChurn;
            yield return VoreStageDefOfs.Stomach_Drain;

            yield return VoreStageDefOfs.Womb_ConvertLube;
            yield return VoreStageDefOfs.Womb_DissolveLube;
            yield return VoreStageDefOfs.Womb_MushLube;

            yield return VoreStageDefOfs.Intestines_SoftProcess;
        }

        static public bool HasPreyIn(Pawn pawn, string organ)
        {
            var voreTracker = pawn.GetVoreTracker();
            if (voreTracker == null) return false;

            if (!pawn.IsActivePredator()) return false;

            return voreTracker.VoreTrackerRecords.Any(r => r.CurrentVoreStage.def.partName.ToLower() == organ.ToLower());
        }
        static public int GetLivePreyCount(Pawn pawn, int depth = 0)
        {
            if (depth == 100)
            {
                Log.Warning("Prey Count calculation reach max depth size");
                return 0;
            }
            var voreTracker = pawn.GetVoreTracker();
            if (voreTracker == null) return 0;

            if (!pawn.IsActivePredator()) return 0;

            return voreTracker.VoreTrackerRecords.Sum(r =>
            {
                return 1 + GetLivePreyCount(r.Prey, depth + 1);
            });
        }
        static public float GetPreySize(Pawn pawn, int depth = 0)
        {
            if (depth == 100){
                Log.Warning("Prey Size calculation reach max depth size");
                return 0;
            }
            var voreTracker = pawn.GetVoreTracker();
            if (voreTracker == null) return 0;
            if (pawn.IsActivePredator()) return 0;
            return voreTracker.VoreTrackerRecords.Sum(r => r.Prey.BodySize + GetPreySize(r.Prey, depth+1));
        }

        static public int GetHighestPreySkillLevel(Pawn pawn, SkillDef skill)
        {
            var voreTracker = pawn.GetVoreTracker();
            if (voreTracker == null) return 0;
            if (pawn.IsActivePredator()) return 0;

            return voreTracker.VoreTrackerRecords
                .Select(r => r.Prey?.skills?.GetSkill(skill))
                .Where(s => s != null)
                .Select(s => s.levelInt)
                .Max(l => l);

        }

        static public bool IsAttracted(Pawn pawnA, Pawn pawnB)
        {
            return 
                (
                    RelationsUtility.AttractedToGender(pawnA, pawnB.gender) && RelationsUtility.AttractedToGender(pawnB, pawnA.gender)
                )
                || pawnA.GetLoveCluster().Contains(pawnB);
        }

        static public bool ShouldBandaid(Pawn pred, Pawn prey) // I never figured it out. Best guess is the hediff transpilers VFE:A uses, but I don't know.
        {
            if(!string.IsNullOrWhiteSpace(pred?.genes?.xenotypeName))
            {
                if (XenotypeNamesForBandage().Contains(pred.genes.xenotypeName)) return true;
            }
            if (!string.IsNullOrWhiteSpace(prey?.genes?.xenotypeName))
            {
                if (XenotypeNamesForBandage().Contains(prey.genes.xenotypeName)) return true;
            }
            return false;
        }
        private static IEnumerable<string> XenotypeNamesForBandage()
        {
            yield return "basic android";
            yield return "awakened android";
        }
    }
}
