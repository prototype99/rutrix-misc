using RimVore2;
using RimWorld;
using System;
using Verse;
using static RV2R_RutsStuff.Patch_RV2R_Settings;

namespace RV2R_RutsStuff
{
    internal class ThoughtWorker_VoreRelation : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!RV2_Rut_Settings.rutsStuff.EndoThoughts)
                return false;

            if(TryPred(p, out var r)) return r;
            if(TryPrey(p, out  r)) return r;
            return false;
        }
        private bool TryPrey(Pawn p, out bool r)
        {
            r = false;
            var record = p.GetVoreRecord();
            if (record == null) return false;
            if (record.VoreGoal.IsLethal) return false;

            var pred = p.GetVoreRecord().Predator;
            if(pred == null) return false;//Just a safety check
            switch (this.def.defName)
            {
                case "RV2R_LoverPrey": return LovePrey();
                case "RV2R_PetPrey": return PetPrey();
                case "RV2R_BondPrey": return BondPrey();
                default: return false;
            }
            bool LovePrey()
            {
                if (pred != LovePartnerRelationUtility.ExistingMostLikedLovePartner(p, false)) return false;
                r = p.relations.OpinionOf(pred) > 0;
                return true;
            }
            bool PetPrey()
            {
                if (p.relations.GetDirectRelation(RV2R_Common.PetPrey, pred) == null) return false;
                r = p.relations.OpinionOf(pred) > 0;
                return true;
            }
            bool BondPrey()
            {
                if (p.relations.GetDirectRelation(PawnRelationDefOf.Bond, predator) == null) return false;
                r = true;
                return true;
            }
        }
        private bool TryPred(Pawn p, out bool r)
        {
            r = false;

            if (!p.IsActivePredator()) return false;

            switch (this.def.defName)
            {
                case "RV2R_LoverPred": return LoverPred();
                case "RV2R_PetPred": return PetPred();
                case "RV2R_BondPred": return BondPred();
                default: return false;
            }

            bool LoverPred()
            {
                var prey =  LovePartnerRelationUtility.ExistingMostLikedLovePartner(p, false); ;
                if (prey?.GetVoreRecord()?.Predator != p) return false;
                r = !prey.GetVoreRecord().VoreGoal.IsLethal && p.relations.OpinionOf(prey) > 0;
                return true;
            }
            bool PetPred()
            {
                if (p.relations?.DirectRelations == null) return false;
                foreach(var relation in p.relations.DirectRelations)
                {
                    if (relation.def != RV2R_Common.PetPred) continue;
                    var record = relation.otherPawn.GetVoreRecord();
                    if (record == null) continue; ;
                    if (record.Predator != p) continue;
                    if (record.VoreGoal.IsLethal) continue;
                    r = p.relations.OpinionOf(relation.otherPawn) > 0;
                    return true;
                }
                return false;
            }
            bool BondPred()
            {
                if (p.relations?.DirectRelations == null) return false;
                int strength = -1;
                foreach(var relation in p.relations.DirectRelations)
                {
                    if (relation.def != PawnRelationDefOf.Bond) continue;
                    var record = relation.otherPawn.GetVoreRecord();
                    if (record == null) continue; ;
                    if (record.Predator != p) continue;
                    if (record.VoreGoal.IsLethal) continue;
                    strength += 1;
                }
                r = ThoughtState.ActiveAtStage(Math.Min(1, strength));
                return true;
            }

        }
    }
}
