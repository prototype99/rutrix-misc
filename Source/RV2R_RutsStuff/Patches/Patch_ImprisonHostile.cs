using HarmonyLib;
using RimVore2;
using RimWorld;
using Verse;
using static RV2R_RutsStuff.Patch_RV2R_Settings;

namespace RV2R_RutsStuff
{
    [HarmonyPatch(typeof(PreVoreUtility), "PopulateRecord")]
    internal class Patch_ImprisonHostile
    {
        [HarmonyPostfix]
        private static void EndoJail(ref VoreTrackerRecord record)
        {
            //Hediff firstHediffOfDef = record.Predator.health.hediffSet.GetFirstHediffOfDef(Common.Encumbrance, false);
            //if (firstHediffOfDef == null)
            //    record.Predator.health.AddHediff(Common.Encumbrance, null, null, null);

            if (RV2_Rut_Settings.rutsStuff.EndoCapture
             && record.Prey.Downed
             && !record.VoreGoal.IsLethal
             && record.Predator.Faction != null
             && record.Predator.Faction.IsPlayer
             && record.Prey.Faction.HostileTo(record.Predator.Faction)
             && !record.Prey.IsPrisonerOfColony)
            {
                if (record.Prey.IsHumanoid())
                    record.Prey.guest.SetGuestStatus(record.Predator.Faction, GuestStatus.Prisoner);
                if (RV2_Rut_Settings.rutsStuff.InsectoidCapture
                 && record.Prey.IsInsectoid()
                 && record.Prey.Faction.def == FactionDefOf.Insect)
                    record.Prey.SetFaction(null, null);
            }
        }
    }
}
