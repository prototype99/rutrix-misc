using HarmonyLib;
using RimVore2;
using RimVore2.Tabs;
using RimWorld;
using System;
using Verse;

namespace RV2R_RutsStuff
{
    [HarmonyPatch(typeof(VoreButton), "CanPlayerGiveCommand")]
    internal class Patch_CanGiveCommand
    {
        [HarmonyPostfix]
        public static void TamedCommands(ref bool __result, Pawn pawn)
        {
            try
            {
                if (!RV2R_Utilities.IsInControllableState(pawn)) return;
                if (!RV2R_Utilities.IsSapient(pawn))
                {
                    __result = true;
                    return;
                }
                if (!pawn.IsAnimal()) return;
                if (!pawn.IsActivePredator()) return;

                if (RV2R_Utilities.GetHighestPreySkillLevel(pawn, SkillDefOf.Animals) > pawn.GetStatValue(StatDefOf.MinimumHandlingSkill) * 2)
                {
                    __result = true;
                    return;
                }
                if (pawn.training == null) return;
                var master = pawn.playerSettings?.Master;
                if (master == null) return;

                if (master.IsPreyOf(pawn))//is inside animal
                {
                    __result = true;
                    return;
                }
                if (master.Position.DistanceTo(pawn.Position) <= 8f)//or close enough
                {
                    __result = true;
                    return;
                }
            }
            catch (Exception e)
            {
                RV2Log.Error($"Caught exception in TamedCommands {e}", "RV2 Stuff Patches");
            }
        }
    }
}
