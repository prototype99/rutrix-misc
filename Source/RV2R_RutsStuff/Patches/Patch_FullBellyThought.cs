using HarmonyLib;
using RimVore2;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RV2R_RutsStuff
{
    [HarmonyPatch(typeof(ThoughtWorker_NeedFood), "CurrentStateInternal")]
    internal class Patch_FullBellyThought
    {
        [HarmonyPostfix]
        public static void PreyKeepsBellyFull(Pawn p, ref ThoughtState __result)
        {
            try
            {
                if (p.needs?.food == null) return;
                if (!ValidOrgans().Any(organ => RV2R_Utilities.HasPreyIn(p, organ))) return;
                switch (p.needs.food.CurCategory)
                {
                    case HungerCategory.Fed:
                        __result = ThoughtState.Inactive;
                        break;
                    case HungerCategory.Hungry:
                        __result = ThoughtState.Inactive;
                        break;
                    case HungerCategory.UrgentlyHungry:
                        __result = ThoughtState.ActiveAtStage(0);
                        break;
                    case HungerCategory.Starving:
                        Hediff firstHediffOfDef = p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition, false);
                        int num = ((firstHediffOfDef == null) ? 0 : firstHediffOfDef.CurStageIndex);
                        __result = ThoughtState.ActiveAtStage(1 + num);
                        break;
                }
            }
            catch (Exception e) {
                RV2Log.Error($"Caught exception in Patch_FullBellyThought {e}", "RV2 Stuff Patches");
            }
        }
        public static IEnumerable<string> ValidOrgans()
        {
            yield return "stomach";
        }
    }
}
