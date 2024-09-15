using HarmonyLib;
using RimVore2;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using static RV2R_RutsStuff.Patch_RV2R_Settings;

namespace RV2R_RutsStuff
{
    [HarmonyPatch(typeof(PreferenceUtility), "AutoAccepts")]
    public class Patch_BondAutoAccept
    {
        public static bool Prefix(Pawn target, Pawn initiator, ref bool __result)
        {
            try
            {
                if (!RV2_Rut_Settings.rutsStuff.VornyBonds) return true;
                if (target.relations == null) return true;

                if (AutoAcceptingRelation().Any(r => target.relations.DirectRelationExists(r, initiator)))
                {
                    RV2Log.Message($"Recipiant {target.LabelShort} auto accepting proposal due to relation", "Proposals");
                    __result = true;
                    return false;
                }
            } catch(Exception e)
            {
                RV2Log.Error($"Caught exception in Patch_BondAutoAccept {e}", "RV2 Stuff Patches");
            }

            return true;
        }

        public static IEnumerable<PawnRelationDef> AutoAcceptingRelation()
        {
            yield return PawnRelationDefOf.Bond;
            yield return RV2R_Common.PetPrey;
            yield return RV2R_Common.PetPred;
        }
    }
}