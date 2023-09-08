using RimVore2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using static RV2R_RutsStuff.Patch_RV2R_Settings;

namespace RV2R_RutsStuff
{
    internal class ThinkNode_AnimalVoreChancePerHour : ThinkNode_ChancePerHour_Constant
    {
        private readonly float mtbHours = RV2_Rut_Settings.rutsStuff.PlayVoreChance;
        protected override float MtbHours(Pawn Pawn)
        {
            float preyMod = 1f;

            if (Pawn.IsActivePredator())
            {
                PawnData pawnData2 = Pawn.PawnData(true);
                if (pawnData2 != null)
                {
                    VoreTracker voreTracker = pawnData2.VoreTracker;
                    if (voreTracker != null)
                        foreach (VoreTrackerRecord rec in voreTracker.VoreTrackerRecords)
                            preyMod += RV2_Rut_Settings.rutsStuff.PlayVoreModifier;
                }
            }

            return this.mtbHours * preyMod;
        }
    }
}
