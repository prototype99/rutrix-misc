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
        static public bool IsBusy(Pawn pawn, Pawn target, bool respect = false)
        {

            if (pawn.GetLord()?.LordJob is LordJob_FormAndSendCaravan || target.GetLord()?.LordJob is LordJob_FormAndSendCaravan)
                return true;
            if (pawn.CarriedBy != null || target.CarriedBy != null)
                return true;
            if (!pawn.Spawned || !target.Spawned)
                return true;
            if (pawn.Drafted || target.Drafted)
                return true;
            if (pawn.IsBurning() || target.IsBurning())
                return true;
            if (pawn.ShouldBeSlaughtered() || target.ShouldBeSlaughtered())
                return true;

            if (pawn.Faction == null || pawn.IsHumanoid() || !pawn.Faction.IsPlayer)
                return false;
            return respect && pawn.playerSettings?.RespectedMaster != null && ((pawn.playerSettings.followDrafted && pawn.playerSettings.RespectedMaster.Drafted) || (pawn.playerSettings.followFieldwork && pawn.playerSettings.RespectedMaster.mindState.lastJobTag == JobTag.Fieldwork));
        }

        static public bool IsColonyHostile(Pawn pawn, Pawn target)
        {
            if (pawn.Faction == null
                || !pawn.Faction.IsPlayer
                || target.Faction == null
                || !target.Faction.HostileTo(Faction.OfPlayer)) return false;
            return !target.IsPrisonerOfColony;
        }

        static public bool IsSapient(Pawn pawn)
        {
            // "cogito ergo sum"
            if (pawn == null)
                return false;
            // Humanoid implies sapient
            if (pawn.IsHumanoid())
                return true;
            // Mechanoids are not sapient
            if (pawn.IsMechanoid())
                return false;
            if (pawn.IsColonistPlayerControlled) // Sentiant Animals
                return true;
            // Some frameworks pawnmorpher or kyulen add mood needs to otherwise non-humanlike pawns -> treat as sapient if present
            return pawn.needs?.mood != null;
        }

        static public float GetFodderWeight(Pawn pawn, Pawn target, bool check)
        {
            try
            {
                float weight = 0f;

                if (check && target.IsColonist && Rand.Chance(0.5f)) // 50% chance for any player faction pawn to be spared
                    return 0f;

                if (target.IsPrisonerInPrisonCell())
                    return 6f;

                if (target.IsAnimal())
                {
                    if (check)
                    {
                        List<Pawn> colonySpecies = target.Map.mapPawns.SpawnedColonyAnimals.FindAll((Pawn p) => p.def == target.def && p.gender == target.gender && !target.IsReserved());
                        if (colonySpecies.Count > 2)
                            weight = 1f;
                        else
                            return 0f;
                    }
                    else
                    {
                        int learnedTrainables = Enumerable.Count(DefDatabase<TrainableDef>.AllDefsListForReading, trainable => target.training.HasLearned(trainable));

                        float idealness = Mathf.Abs(target.BodySize * 2f / pawn.BodySize);
                        if (idealness > 1f)
                            idealness = Mathf.Pow(idealness, -1f); // Gives highest weight to prey that are half a predator's size; makes foxxo want to nom chickm and bunbun

                        weight = (2f * idealness) / learnedTrainables;
                    }
                }

                if (!target.IsHumanoid()) return weight;
                if (pawn.IsAnimal())
                    weight = 1f / (target.IsColonist ? 2f : 1f);
                else
                    weight = (100f - pawn.relations.OpinionOf(target)) / 50 / (target.IsColonist ? 1.5f : 1f); // Don't eat people you like

                return weight;
            }
            catch (Exception e)
            {
                Log.Warning("RV-2R: Something went wrong when trying to get " + target.LabelShort + "'s fodder weight: " + e);
                return 0;
            }
        }

        static public bool ShouldFriendlyTarget(Pawn pawn, Pawn target)
        {
            return pawn.Faction != null
                   && target.Faction != null && pawn.Faction == target.Faction
                   || ((target.Faction != null && target.Faction.AllyOrNeutralTo(pawn.Faction))
                       || (target.IsHumanoid() && target.GuestStatus != null)
                       || (!target.IsHumanoid() && pawn.Map.designationManager.DesignationOn(target, DesignationDefOf.Tame) != null));
        }

        private static VoreTracker GetVoreTracker(Pawn pawn)
        {
            if (!pawn.IsActivePredator()) return null;
            PawnData pawnData = pawn.PawnData(false);
            return pawnData?.VoreTracker;
        }
        static public bool IsInTargetMidsection(Pawn pawn, Pawn target, bool isLethal)
        {
            VoreTracker voreTracker = GetVoreTracker(target);
            return voreTracker != null && Enumerable.Any(voreTracker.VoreTrackerRecords, voreTrackerRecord => voreTrackerRecord.Prey == pawn && voreTrackerRecord.VoreGoal.IsLethal == isLethal && (voreTrackerRecord.CurrentVoreStage.def.partName.ToLower() == "stomach" || voreTrackerRecord.CurrentVoreStage.def.displayPartName.ToLower() == "intestines" || voreTrackerRecord.CurrentVoreStage.def.displayPartName.ToLower() == "womb"));
        }

        static public bool HasPreyIn(Pawn pawn, string organ)
        {
            VoreTracker voreTracker = GetVoreTracker(pawn);
            return voreTracker != null && Enumerable.Any(voreTracker.VoreTrackerRecords, voreTrackerRecord => string.Equals(voreTrackerRecord.CurrentVoreStage.def.partName, organ, StringComparison.CurrentCultureIgnoreCase));
        }

        private static bool PreyCheck(Pawn prey)
        {
            return prey.Dead || !prey.IsActivePredator() && prey.PawnData(false) != null;
        }
        static public int GetPreyCount(Pawn pawn)
        {
            int count = 0;
            VoreTracker voreTracker = GetVoreTracker(pawn);
            if (voreTracker == null) return count;
            foreach (VoreTrackerRecord voreTrackerRecord in voreTracker.VoreTrackerRecords)
            {
                count += 1;
                if (PreyCheck(pawn))
                    count += GetPreyCount(voreTrackerRecord.Prey);
            }
            return count;
        }
        static public float GetPreySize(Pawn pawn)
        {
            float weight = 0;
            VoreTracker voreTracker = GetVoreTracker(pawn);
            if (voreTracker == null) return weight;
            foreach (VoreTrackerRecord voreTrackerRecord in voreTracker.VoreTrackerRecords)
            {
                weight += voreTrackerRecord.Prey.BodySize;
                if (PreyCheck(pawn))
                    weight += GetPreySize(voreTrackerRecord.Prey);
            }
            return weight;
        }

        static public int GetHighestPreySkillLevel(Pawn pawn, SkillDef skill)
        {
            int level = 0;
            VoreTracker voreTracker = GetVoreTracker(pawn);
            if (voreTracker != null) level = (from voreTrackerRecord in voreTracker.VoreTrackerRecords where voreTrackerRecord.Prey.skills?.GetSkill(skill) != null select voreTrackerRecord.Prey.skills.GetSkill(skill).levelInt).Prepend(level).Max();
            return level;
        }

        static public bool IsAttracted(Pawn pawnA, Pawn pawnB)
        {
            if (pawnA.story == null || pawnB.story == null) return pawnA.GetLoveCluster().Contains(pawnB);
            bool straight = pawnA.gender != pawnB.gender;
            bool aGay = pawnA.story.traits.HasTrait(TraitDefOf.Gay);
            bool bGay = pawnB.story.traits.HasTrait(TraitDefOf.Gay);
            bool aBi = pawnA.story.traits.HasTrait(TraitDefOf.Bisexual);
            bool bBi = pawnB.story.traits.HasTrait(TraitDefOf.Bisexual);
            if (straight)
            {
                if ((aBi || !aGay) && (bBi || !bGay))
                    return true;
            }
            else
            if ((aBi || aGay) && (bBi || bGay))
                return true;

            return pawnA.GetLoveCluster().Contains(pawnB);

        }

        static public bool ShouldBandaid(Pawn pred, Pawn prey) // I'm working on it
        {
            if (pred.IsHumanoid() && pred.genes?.Xenotype != null)
            {
                if (pred.genes.xenotypeName == "basic android" || pred.genes.xenotypeName == "awakened android")
                    return true;
            }

            if (!prey.IsHumanoid() || prey.genes == null || prey.genes.Xenotype == null) return false;
            return prey.genes.xenotypeName == "basic android" || prey.genes.xenotypeName == "awakened android";
        }
    }
}
