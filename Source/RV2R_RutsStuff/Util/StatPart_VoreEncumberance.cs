using RimVore2;
using RimWorld;
using Verse;

namespace RV2R_RutsStuff
{
    public class StatPart_VoreMoving : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            string text = null;
            if (!(req.Thing is Pawn pawn))
                return text;
            if (pawn.PawnData(false) == null)
                return text;
            if (pawn.QuirkManager(false) == null)
                return text;
            return string.Format("{0}: x{1}", "RV2R_StatsReport_VoreMoving".Translate(), pawn.QuirkManager(false).CapModOffsetModifierFor(PawnCapacityDefOf.Moving, null).ToStringByStyle(ToStringStyle.PercentZero, ToStringNumberSense.Absolute));
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!(req.Thing is Pawn pawn))
                return;
            if (pawn.PawnData(false) == null)
                return;
            if (pawn.QuirkManager(false) == null)
                return;
            val *= pawn.QuirkManager(false).CapModOffsetModifierFor(PawnCapacityDefOf.Moving, null);
        }
    }
}
