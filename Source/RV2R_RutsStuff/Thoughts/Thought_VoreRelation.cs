using RimWorld;
using UnityEngine;

namespace RV2R_RutsStuff
{
    internal class Thought_VoreRelation : Thought_Situational
    {
        protected override float BaseMoodOffset
        {
            get
            {
                return Mathf.Max(GetValue(), 1f);

                float GetValue()
                {
                    switch (this.def.defName)
                    {
                        case "RV2R_LoverPred":
                        case "RV2R_LoverPrey":
                            if (LovePartnerRelationUtility.ExistingMostLikedLovePartner(this.pawn, false) == null) return 1;
                            return 0.05f * (float)this.pawn.relations.OpinionOf(LovePartnerRelationUtility.ExistingMostLikedLovePartner(this.pawn, false));
                        case "RV2R_PetPred":
                            if (this.pawn?.relations?.GetFirstDirectRelationPawn(RV2R_Common.PetPred) == null) return 1;
                            return 0.05f * (float)this.pawn.relations.OpinionOf(this.pawn.relations.GetFirstDirectRelationPawn(RV2R_Common.PetPred));
                        case "RV2R_PetPrey":
                            if (this.pawn?.relations?.GetFirstDirectRelationPawn(RV2R_Common.PetPrey) != null) return 1;
                            return 0.1f * (float)this.pawn.relations.OpinionOf(this.pawn.relations.GetFirstDirectRelationPawn(RV2R_Common.PetPrey));
                        default:
                            return 1f;
                    }
                }
            }
        }
    }
}
