using RimVore2;
using RimWorld;
using System.Collections.Generic;
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

            if (!pawn.IsHumanoid() && pawn.Faction != null && pawn.Faction.IsPlayer)
            {
                if (respect && pawn.playerSettings != null && pawn.playerSettings.RespectedMaster != null
                 && ((pawn.playerSettings.followDrafted && pawn.playerSettings.RespectedMaster.Drafted) || (pawn.playerSettings.followFieldwork && pawn.playerSettings.RespectedMaster.mindState.lastJobTag == JobTag.Fieldwork)))
                    return true;
                if (pawn.ShouldBeSlaughtered()
                 || target.ShouldBeSlaughtered())
                    return true;
            }

            return false;
        }

        static public bool ShouldFriendlyTarget(Pawn pawn, Pawn target)
        {
            if (pawn.Faction != null
             && (target.Faction != null && pawn.Faction == target.Faction)
               || (target.Faction != null && target.Faction.AllyOrNeutralTo(pawn.Faction)
               || (!target.IsHumanoid() && pawn.Map.designationManager.DesignationOn(target, DesignationDefOf.Tame) != null)))
                return true;

            return false;
        }
    }
}
