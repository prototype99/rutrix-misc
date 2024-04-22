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
        private bool TryPrey(Pawn p, out ThoughtState r)
        {
            r = false;
            var record = p.GetVoreRecord();
            if (record == null) return false;
            if (record.VoreGoal.IsLethal) return false;

            var pred = p.GetVoreRecord().Predator;
            if(pred == null) return false;//Just a safety check
            (bool, ThoughtState) result = (false, false); 
            switch (this.def.defName)
            {
                case "RV2R_LoverPrey": result = LovePrey(); break;
                case "RV2R_PetPrey": result = PetPrey(); break;
                case "RV2R_BondPrey": result = BondPrey(); break;
            }
            r = result.Item2;
            return result.Item1;

            (bool, ThoughtState) LovePrey()
            {
                if (pred != LovePartnerRelationUtility.ExistingMostLikedLovePartner(p, false)) return (false, false);
                return (true, p.relations.OpinionOf(pred) > 0);
            }
            (bool, ThoughtState) PetPrey()
            {
                if (p.relations.GetDirectRelation(RV2R_Common.PetPrey, pred) == null) return (false, false);
                return (true, p.relations.OpinionOf(pred) > 0);
            }
            (bool, ThoughtState) BondPrey()
            {
                if (p.relations.GetDirectRelation(PawnRelationDefOf.Bond, pred) == null) return (false, false);
                return (true, true);
            }
        }
        private bool TryPred(Pawn p, out ThoughtState r)
        {
            r = false;

            if (!p.IsActivePredator()) return false;
            (bool, ThoughtState) result = (false, false);

            switch (this.def.defName)
            {
                case "RV2R_LoverPred": result = LoverPred(); break;
                case "RV2R_PetPred": result = PetPred(); break;
                case "RV2R_BondPred": result = BondPred(); break;
            }
            r = result.Item2;
            return result.Item1;


            (bool, ThoughtState) LoverPred()
            {
                var prey =  LovePartnerRelationUtility.ExistingMostLikedLovePartner(p, false);
                if (prey?.GetVoreRecord()?.Predator != p) return (false, false);
                return (true, !prey.GetVoreRecord().VoreGoal.IsLethal && p.relations.OpinionOf(prey) > 0);
            }
            (bool, ThoughtState) PetPred()
            {
                if (p.relations?.DirectRelations == null) return (false, false);
                foreach(var relation in p.relations.DirectRelations)
                {
                    if (relation.def != RV2R_Common.PetPred) continue;
                    var record = relation.otherPawn.GetVoreRecord();
                    if (record == null) continue; ;
                    if (record.Predator != p) continue;
                    if (record.VoreGoal.IsLethal) continue;
                    return (true, p.relations.OpinionOf(relation.otherPawn) > 0);
                }
                return (false, false);
            }
            (bool, ThoughtState) BondPred()
            {
                if (p.relations?.DirectRelations == null) return (false, false);
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
                return (true, ThoughtState.ActiveAtStage(Math.Min(1, strength)) );
            }

        }
    }
}
