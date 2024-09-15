using HarmonyLib;
using RimVore2;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using System.Linq;
using static RV2R_RutsStuff.Patch_RV2R_Settings;

namespace RV2R_RutsStuff
{
    [HarmonyPatch(typeof(JobDriver_Lovin), "MakeNewToils")]
    public class Patch_JobDriver_Lovin
    {
        [HarmonyPostfix]
        public static IEnumerable<Toil> GrindUpPrey(IEnumerable<Toil> __result, JobDriver_Lovin __instance)
        {
            foreach (Toil t in __result)
            {
                yield return t;
            }
            if (!RV2_Rut_Settings.rutsStuff.PreyLovin) yield break;
            var toil = ToilMaker.MakeToil("Patch_JobDriver_Lovin making toil");
            toil.AddFinishAction(() =>
            {
                PreformTheAction(__instance.pawn);
            });
            yield return toil;
            
        }
        public static void PreformTheAction(Pawn pawn)
        {
            try
            {
                var voreTrackerList = pawn?.PawnData(true)?.VoreTracker?.VoreTrackerRecords?.Where(r => RV2R_Utilities.IsInTargetMidsection(r.Prey, r.Predator) && !r.VoreGoal.IsLethal);
                if (voreTrackerList.EnumerableNullOrEmpty()) return;

                List<Pawn> WasLoved = new List<Pawn>();
                voreTrackerList.ForEach(r =>
                {
                    if (r.Prey?.needs?.mood == null) return; //skip if prey doesn't need anything
                    if (r.StruggleManager.shouldStruggle) return; // skip if prey is struggling
                    if (r.IsForced) return; //skip if its forced vore

                    if (!LovePartnerRelationUtility.LovePartnerRelationExists(r.Prey, r.Predator) && r.Predator?.relations?.FamilyByBlood?.Contains(r.Prey) == true)
                        return;// skip incest (unless their is a lover's relation)

                    Thought_Memory memory = GetLovinMemoryFor(r.Prey, r.Predator);
                    if (memory != null)
                    {
                        r.Prey?.needs?.mood?.thoughts?.memories?.TryGainMemory(memory, r.Predator);//Memory between Pred and Prey
                    }


                    IEnumerable<Pawn> lovers = LovePartnerRelationUtility.ExistingLovePartners(r.Prey, false)
                    .Select(relation => relation.otherPawn)
                    .Where(p => voreTrackerList.Any(r2 => r2.Prey == p))
                    .Where(p => !WasLoved.Contains(p));

                    lovers.ForEach(lover =>
                    {
                        WasLoved.Add(lover);
                        WasLoved.Add(r.Prey);
                        r.Prey?.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.GotSomeLovin, lover, null);
                        lover.needs?.mood?.thoughts?.memories?.TryGainMemory(ThoughtDefOf.GotSomeLovin, lover, null);
                    });
                });
            }
            catch (Exception e)
            {
                Log.Warning("RV-2R: Something went wrong when trying to make a predatory 3 way : " + e);
            }
        }
        public static Thought_Memory GetLovinMemoryFor(Pawn prey, Pawn pred)
        {
            var num = prey.QuirkManager(false)?.ModifyValue("Prey_Libido", 0) ?? 1f;
            var thoughtDef = RV2R_Common.PreyLovin_Normal;
            switch (num)
            {
                case 2f:
                    thoughtDef = RV2R_Common.PreyLovin_VeryGood;
                    break;
                case 1.5f:
                    thoughtDef = RV2R_Common.PreyLovin_Good;
                    break;
                case 0.66f:
                case 0.33f:
                    thoughtDef = RV2R_Common.PreyLovin_Meh;
                    break;
                case 0.00f:
                    thoughtDef = null;
                    break;

            }
            if (thoughtDef == null) return null;
            return ThoughtMaker.MakeThought(thoughtDef) as Thought_Memory;
        }
    }
}
