using RimWorld;
using Verse;

namespace RV2R_RutsStuff
{
    internal class RV2R_Common
    {
        public static readonly HediffDef EndoHediff = HediffDef.Named("RV2R_EndoRecovery");
        public static readonly HediffDef Encumbrance = HediffDef.Named("RV2R_VoreEncumbrence");

        public static readonly TrainableDef PlayVore = DefDatabase<TrainableDef>.GetNamedSilentFail("PlayVore");

        public static readonly PawnRelationDef PetPred = DefDatabase<PawnRelationDef>.GetNamedSilentFail("PetPred");
        public static readonly PawnRelationDef PetPrey = DefDatabase<PawnRelationDef>.GetNamedSilentFail("PetPrey");
    }
}
