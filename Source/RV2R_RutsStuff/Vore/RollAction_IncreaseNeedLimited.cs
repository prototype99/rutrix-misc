using RimVore2;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace RV2R_RutsStuff
{
    public class RollAction_IncreaseNeedLimited : RollAction
    {
        public override bool TryAction(VoreTrackerRecord record, float rollStrength)
        {
            base.TryAction(record, rollStrength);
            if (invert)
            {
                rollStrength *= -1f;
            }
            if (this.need == NeedDefOf.Food && record.Prey.needs?.food != null)
            {
                rollStrength *= TargetPawn.health.hediffSet.HungerRateFactor;
            }

            if (this.need == null) return RV2PawnUtility.TryIncreaseNeed(TargetPawn, this.need, rollStrength);
            Need need = TargetPawn?.needs?.TryGetNeed(this.need);
            if (need == null) return RV2PawnUtility.TryIncreaseNeed(TargetPawn, this.need, rollStrength);
            if (need.CurLevelPercentage > limit)
                rollStrength *= 0.15f;
            return RV2PawnUtility.TryIncreaseNeed(TargetPawn, this.need, rollStrength);
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
            {
                yield return error;
            }
            bool flag = target == VoreRole.Invalid;
            if (flag)
            {
                yield return "required field \"target\" is not set";
            }
            bool flag2 = need == null;
            if (flag2)
            {
                yield return "required field \"need\" is not set";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref need, "need");
        }

        public NeedDef need;

        public float limit;
    }
}
