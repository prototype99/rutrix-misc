using HarmonyLib;
using RimVore2;
using RimVore2.Tabs;
using RimWorld;
using Verse;

namespace RV2R_RutsStuff
{
    [HarmonyPatch(typeof(VoreButton), "CanPlayerGiveCommand")]
    internal class Patch_CanGiveCommand
    {
        [HarmonyPostfix]
        public static void TamedCommands(ref bool __result, Pawn pawn)
        {
            if(pawn.Dead) return;
            if(pawn.InMentalState) return;
            if(pawn.Downed) return;
            if (pawn.IsBurning()) return;

            if (pawn.IsColonistPlayerControlled)
            {
                __result = true;
                return;
            }
            if(!pawn.IsAnimal()) return;
            if (!pawn.IsActivePredator()) return;
            if(RV2R_Utilities.GetHighestPreySkillLevel(pawn, SkillDefOf.Animals) >= pawn.GetStatValue(StatDefOf.MinimumHandlingSkill) * 2)
            {
                __result = true;
                return;
            }
            if(pawn.training == null) return;
            Pawn_PlayerSettings _PlayerSettings = pawn.playerSettings;
            if (_PlayerSettings == null) return;
            if(_PlayerSettings.RespectedMaster == null) return;
            __result = (pawn.playerSettings.RespectedMaster.Position.DistanceTo(pawn.Position) <= 8f || pawn.playerSettings.RespectedMaster.IsPreyOf(pawn));
        }
    }
}
