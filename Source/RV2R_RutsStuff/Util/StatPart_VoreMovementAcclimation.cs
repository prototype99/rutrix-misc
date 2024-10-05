using RimWorld;
using Verse;

namespace RV2R_RutsStuff
{
    public class StatPart_VoreMovementAclimation : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            string text = null;
            if (!(req.Thing is Pawn pawn))
                return text;
            if (pawn.health == null)
                return text;
            if (pawn.health.hediffSet == null)
                return text;
            if (!pawn.health.hediffSet.HasHediff(RV2R_Common.MovingAcclimation, false))
                return text;
            if (pawn.health.hediffSet.GetFirstHediffOfDef(RV2R_Common.MovingAcclimation, false) == null)
                return text;
            if (pawn.health.hediffSet.GetFirstHediffOfDef(RV2R_Common.MovingAcclimation, false).Severity > 1f)
                text = string.Format("{0}: ÷{1}", "RV2R_StatsReport_VoreCapacityAcclimation".Translate(), pawn.health.hediffSet.GetFirstHediffOfDef(RV2R_Common.MovingAcclimation, false).Severity.ToStringByStyle(ToStringStyle.FloatTwo, ToStringNumberSense.Absolute));
            return text;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!(req.Thing is Pawn pawn))
                return;
            if (pawn.health == null)
                return;
            if (pawn.health.hediffSet == null)
                return;
            if (!pawn.health.hediffSet.HasHediff(RV2R_Common.MovingAcclimation, false))
                return;
            Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(RV2R_Common.MovingAcclimation, false);
            if (firstHediffOfDef != null && firstHediffOfDef.Severity > 1f)
                val /= firstHediffOfDef.Severity;
        }
    }
}
