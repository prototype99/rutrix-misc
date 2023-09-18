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
#if v1_3   
            bool doEffects = true;
#else
            bool doEffects = !(record.Predator.genes.xenotypeName == "basic android" || record.Predator.genes.xenotypeName == "awakened android" || record.Prey.genes.xenotypeName == "basic android" || record.Prey.genes.xenotypeName == "awakened android");
            // I appologize to your androids, but support for them's gonna be some work; just trying to make them not explode
#endif
            if (!doEffects) return;

            Hediff firstHediffOfDef = record.Predator.health.hediffSet.GetFirstHediffOfDef(RV2R_Common.Encumbrance, false) ?? null;
            if (firstHediffOfDef == null)
                record.Predator.health.AddHediff(RV2R_Common.Encumbrance, null, null, null);

            if (!record.VoreGoal.IsLethal)
            {
                if (RV2_Rut_Settings.rutsStuff.ScariaCapture)
                {
                    Hediff rabies = record.Prey.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Scaria, false) ?? null;
                    if (record.Prey.IsAnimal()
                     && record.Prey.Faction == null
                     && rabies != null)
                        record.Prey.health.RemoveHediff(record.Prey.health.hediffSet.GetFirstHediffOfDef(rabies.def, false));
                }

                if (RV2_Rut_Settings.rutsStuff.EndoCapture
                 && record.Prey.Downed
                 && record.Predator.Faction != null
                 && record.Predator.Faction.IsPlayer
                 && record.Prey.Faction != null
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
}
