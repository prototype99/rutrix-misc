using RimVore2;
using System.Linq;
using Verse;
using Verse.AI;
using static RV2R_RutsStuff.Patch_RV2R_Settings;

namespace RV2R_RutsStuff
{
    internal class ThinkNode_AnimalVoreChancePerHour : ThinkNode_ChancePerHour_Constant
    {
        private float mtbHours => RV2_Rut_Settings.rutsStuff.PlayVoreChance;
        protected override float MtbHours(Pawn Pawn)
        {
            if (Pawn.training != null && !Pawn.training.GetWanted(RV2R_Common.PlayVore))
                return -1f;

            float preyMod = 1f;
            VoreTracker voreTracker = Pawn.PawnData(false)?.VoreTracker;
            if (voreTracker != null && Pawn.IsActivePredator())
            {
                preyMod += voreTracker.VoreTrackerRecords.Sum(r => RV2_Rut_Settings.rutsStuff.PlayVoreModifier);
            }

            return this.mtbHours * preyMod;
        }
    }
}
