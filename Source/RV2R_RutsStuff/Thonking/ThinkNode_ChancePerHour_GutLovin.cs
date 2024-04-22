using RimVore2;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using static RV2R_RutsStuff.Patch_RV2R_Settings;

namespace RV2R_RutsStuff
{
    internal class ThinkNode_ChancePerHour_GutLovin : ThinkNode_ChancePerHour
    {
        protected override float MtbHours(Pawn pawn)
        {
            if (RV2_Rut_Settings.rutsStuff.GutLovinChance <= 0f)
                return -1f;

            if (pawn.ageTracker.AgeBiologicalYearsFloat < pawn.ageTracker.AdultMinAge
             && pawn.ageTracker.AgeBiologicalYears < 18f)
                return -1f;

            if(!pawn.IsActivePredator()) return -1f;
            if(pawn.CurrentBed() == null) return -1f;

            float drive = 1f;
            drive = ApplyQuirkResults(drive, pawn);
            if (drive <= 0f) return -1;
            var result = (36f / RV2_Rut_Settings.rutsStuff.GutLovinChance) / drive / ThinkNode_ChancePerHour_GutLovin.LovinMtbSinglePawnFactor(pawn);
            RV2Log.Message($"Predator {pawn.LabelShort} gut loves ever {result}" +
                $" - {RV2_Rut_Settings.rutsStuff.GutLovinChance}" +
                $" - {drive}" +
                $" - {ThinkNode_ChancePerHour_GutLovin.LovinMtbSinglePawnFactor(pawn)}"
                , "Gutlovin");

            return result;
        }

        private float ApplyQuirkResults(float drive, Pawn pawn)
        {
            var quirkManager = pawn.QuirkManager(false);
            if (quirkManager == null) return drive;
            foreach (var modifier in QuirkValueModifers())
                drive = quirkManager.ModifyValue(modifier, drive);
            return drive;
        }
        private IEnumerable<string> QuirkValueModifers()
        {
            yield return "Predator_Libido";
        }

        private static float LovinMtbSinglePawnFactor(Pawn pawn)
        {
            float painMod = 1f / (1f - pawn.health.hediffSet.PainTotal);
            float conMod = Math.Min(pawn.health.capacities.GetLevel(PawnCapacityDefOf.Consciousness), 1.5f);
            if (conMod < 0.5f)
                painMod /= conMod * 2f;                                  // Some HAR races have silly adult ages; Racc, for example are formaly adults at <i><b>5000</i></b> years
            float adultAge = Math.Min(18f, pawn.ageTracker.AdultMinAge); // This basicaly means they never score, so I'm helping them here
            return painMod / Math.Max(0.01f, GenMath.FlatHill(0f, adultAge * 0.77f, adultAge * 0.88f, adultAge * 1.38f, adultAge * 4.44f, 0.2f, pawn.ageTracker.AgeBiologicalYearsFloat));
        }
    }
}
