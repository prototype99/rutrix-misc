using DefOfs;
using RimWorld;
using UnityEngine;
using Verse;

namespace RV2R_RutsStuff
{
    internal class Thought_VoreRelation : Thought_Situational
    {
        protected override float BaseMoodOffset
        {
            get
            {
                return Mathf.Max(1f, GetValue(this.def, this.pawn));
            }
        }
        public static float GetValue(ThoughtDef def, Pawn pawn)
        {
            if(def == VoreThoughtsDefOfs.RV2R_LoverPrey) 
                return 0.05f * (float)pawn.relations.OpinionOf(LovePartnerRelationUtility.ExistingMostLikedLovePartner(pawn, false));
            if(def == VoreThoughtsDefOfs.RV2R_LoverPred)
                return 0.05f * (float)pawn.relations.OpinionOf(LovePartnerRelationUtility.ExistingMostLikedLovePartner(pawn, false));
            if(def == VoreThoughtsDefOfs.RV2R_PetPred)
                return 0.05f * (float)pawn.relations.OpinionOf(pawn.relations.GetFirstDirectRelationPawn(RV2R_Common.PetPred));
            if(def == VoreThoughtsDefOfs.RV2R_PetPrey)
                return 0.05f * (float)pawn.relations.OpinionOf(pawn.relations.GetFirstDirectRelationPawn(RV2R_Common.PetPrey));

            return 1;
        }
    }
}
