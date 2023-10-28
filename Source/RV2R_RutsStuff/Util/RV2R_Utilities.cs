using RimVore2;
using RimWorld;
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

            if (pawn.IsBurning() || target.IsBurning())
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

        static public bool IsColonyHostile(Pawn pawn, Pawn target)
        {
            if (pawn.Faction != null && pawn.Faction.IsPlayer)
            {
                if (target.Faction != null && target.Faction.HostileTo(Faction.OfPlayer))
                {
                    if (!target.IsPrisonerOfColony)
                        return true;
                }
            }
            return false;
        }
        static public bool ShouldFriendlyTarget(Pawn pawn, Pawn target)
        {
            if (pawn.Faction != null
             && (target.Faction != null && pawn.Faction == target.Faction)
               || ((target.Faction != null && target.Faction.AllyOrNeutralTo(pawn.Faction))
               || (target.IsHumanoid() && target.GuestStatus != null)
               || (!target.IsHumanoid() && pawn.Map.designationManager.DesignationOn(target, DesignationDefOf.Tame) != null)))
                return true;

            return false;
        }

        static public bool ShouldBandaid(Pawn pred, Pawn prey) // I'm working on it
        {
#if v1_3
            return false;
#else
            if (pred.genes != null && pred.genes.Xenotype != null)
            {
                if (pred.genes.xenotypeName == "basic android" || pred.genes.xenotypeName == "awakened android")
                    return true;
            }
            if (prey.genes != null && prey.genes.Xenotype != null)
            {
                if (prey.genes.xenotypeName == "basic android" || prey.genes.xenotypeName == "awakened android")
                    return true;
            }
            return false;
#endif
        }
    }
}
