using HarmonyLib;
using RimVore2;

namespace RV2R_RutsStuff
{
    [HarmonyPatch(typeof(Patch_PawnCapacityUtility), "ModifyOffsetWithQuirks")]
    internal class Patch_Return1f
    {
        internal static bool Prefix(float value, ref float __result)
        {
            /* Okay, to explain -
               The only thing that uses this, as far as I could find, were RV2's "Containing prey" hediffs.
               Those have been superceeded by the encumberance hediff...
               ...and it started affecting -any- hediff (at least, in my game).
               So, this just checks to see if you have encumberance enabled, returns the input if it is, and assumes you're using the old hediffs and runs the original method if not.
               I am fully aware of how gross a patch to just have a method return it's input with nothing else happening is.
            */ // - rutrix
            bool flag = Patch_RV2R_Settings.RV2_Rut_Settings.rutsStuff.EncumberanceModifier > 0f;
            if (flag)
            {
                __result = value;
                return false;
            }

            return true;
        }
    }
}