using RimVore2;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;
using static RV2R_RutsStuff.Patch_RV2R_Settings;

namespace RV2R_RutsStuff
{
    public class Hediff_VoreEncumberance : Hediff
    {
        public const int TickEverXTicks = 250;
        public override void Tick()
        {
            base.Tick();
            if (ageTicks % TickEverXTicks != 0) return;
            try
            {
                Severity = TickSeverity(pawn);
            }
            catch (Exception e)
            {
                Log.Warning("RV-2R: Something went wrong when getting " + pawn.LabelShort + "'s encumberance: " + e);
                return;
            }
            pawn.health.Notify_HediffChanged(this);
        }

        private float TickSeverity(Pawn pawn)
        {
            if (pawn.kindDef.defName.Contains("NCP_")) // Carnivorous Plants
                return 0f;

            if (RV2_Rut_Settings.rutsStuff.EncumberanceModifier <= 0.001f)
                return 0f;
            if (RV2_Rut_Settings.rutsStuff.EncumberanceCap < 0.05f)
                return 0f;
            if (!pawn.IsActivePredator())
                return 0f;
            if (pawn.PawnData(false) == null)
                return 0f;

            float quirkMod = pawn.QuirkManager(false)?.CapModOffsetModifierFor(PawnCapacityDefOf.Moving, null) ?? 1f;

            Hediff accHediff = pawn.health.hediffSet.GetFirstHediffOfDef(RV2R_Common.MovingAcclimation, false);
            float accMod = accHediff == null ? 1f : 1 + accHediff.Severity;
            float severity = 0f;

            if (RV2_Rut_Settings.rutsStuff.SizedEncumberance)
            {
                float totalWeight = RV2R_Utilities.GetPreySize(pawn);
                severity = Math.Min(totalWeight / Math.Max(pawn.BodySize, 0.01f) / 4f * RV2_Rut_Settings.rutsStuff.EncumberanceModifier * quirkMod / accMod, RV2_Rut_Settings.rutsStuff.EncumberanceCap);
            }
            else
            {
                int totalPrey = RV2R_Utilities.GetLivePreyCount(pawn);
                severity = Math.Min(totalPrey / 4f * RV2_Rut_Settings.rutsStuff.EncumberanceModifier * quirkMod / accMod, RV2_Rut_Settings.rutsStuff.EncumberanceCap);
            }

            if (RV2_Rut_Settings.rutsStuff.MovingCapacityAclimation > 0f && severity > RV2_Rut_Settings.rutsStuff.MovingCapacityAclimationLimit)
            {
                if (accHediff != null)
                {
                    float mod = Math.Min(1f, severity / RV2_Rut_Settings.rutsStuff.MovingCapacityAclimationLimit);
                    accHediff.Severity += 0.0005f * mod * RV2_Rut_Settings.rutsStuff.MovingCapacityAclimation;
                }
                else
                    pawn.health.AddHediff(RV2R_Common.MovingAcclimation, null, null, null);


            }
            return severity;
        }

        public override bool Visible
        {
            get
            {
                if (Severity > 0.05f && RV2_Rut_Settings.rutsStuff.VisibleEncumberance)
                    return true;
                return false;
            }
        }

    }
}
