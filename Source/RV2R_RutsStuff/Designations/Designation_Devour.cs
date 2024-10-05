using RimVore2;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

// I never figured out how to get the designation to cancel on invalid pawns. Likely needed a pawn.tickRare patch. -- Rutrix
namespace RV2R_RutsStuff
{
    public class Designator_Devour : Designator
    {
        public override int DraggableDimensions
        {
            get
            {
                return 2;
            }
        }

        protected override DesignationDef Designation
        {
            get
            {
                return RV2R_Common.Devour;
            }
        }

        public Designator_Devour()
        {
            this.defaultLabel = "RV2R_DesginateDevour".Translate();
            this.defaultDesc = "RV2R_DesginateDevourDesc".Translate();
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/Devour", true);
            this.soundDragSustain = SoundDefOf.Designate_DragStandard;
            this.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            this.useMouseIcon = true;
            this.soundSucceeded = SoundDefOf.Designate_Hunt;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(base.Map)) return false;
            if (this.EdiblesInCell(c).Any<Pawn>()) return true;
            return "RV2R_DesginateDevourErr".Translate();
        }

        public override void DesignateSingleCell(IntVec3 loc)
        {
            foreach (Pawn pawn in this.EdiblesInCell(loc))
            {
                this.DesignateThing(pawn);
            }
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            var Pawn = t as Pawn;
            if (Pawn == null) return false;
            if (Pawn.IsMechanoid()) return false;
            if (!IsInValidFaction(Pawn)) return false;
            if (Pawn.IsPrisonerInPrisonCell()) return false;

            SettingsContainer_RutsStuff settings = Patch_RV2R_Settings.RV2_Rut_Settings.rutsStuff;
            var AllowColonistDesignation = Pawn.Faction == Faction.OfPlayer && settings.DevourColonistsFull;
            if (!Pawn.Downed && !AllowColonistDesignation) return false;

            if (Pawn.CanBeFatalPrey(out string _)) return true;
            if (Pawn.Faction.HostileTo(Faction.OfPlayer) && (Pawn.IsHumanoid() || !settings.EndoCapture))
            {
                return "RV2R_DesginateDevourErrHuman".Translate();
            }
            if (Pawn.IsInsectoid() || !settings.InsectoidCapture)
            {
                return "RV2R_DesginateDevourErrScaria".Translate();
            }
            if (Pawn.health.hediffSet.HasHediff(HediffDefOf.Scaria) || !settings.ScariaCapture)
            {
                return "RV2R_DesginateDevourErrInsect".Translate();
            }
            return true;
        }
        public bool IsInValidFaction(Pawn pawn)
        {
            if (pawn == null) return true;
            if(pawn.Faction.HostileTo(Faction.OfPlayer)) return true;

            SettingsContainer_RutsStuff settings = Patch_RV2R_Settings.RV2_Rut_Settings.rutsStuff;
            if (settings.DevourFriendlies && pawn.Faction != Faction.OfPlayer) return true;
            if((settings.DevourColonists || settings.DevourColonistsFull) && pawn.Faction == Faction.OfPlayer) return true;
            return false;
        }

        public override void DesignateThing(Thing t)
        {
            base.Map.designationManager.RemoveAllDesignationsOn(t, false);
            base.Map.designationManager.AddDesignation(new Designation(t, this.Designation));
            this.justDesignated.Add((Pawn)t);
        }

        protected override void FinalizeDesignationSucceeded()
        {
            base.FinalizeDesignationSucceeded();
            this.justDesignated.Clear();
        }

        private IEnumerable<Pawn> EdiblesInCell(IntVec3 c)
        {
            if(c.Fogged(base.Map)) yield break;
            foreach(var pawns in c.GetThingList(base.Map)
                .Where(t=>CanDesignateThing(t))
                .Select(t=>t as Pawn).Where(p => p != null)
            ) {
                yield return pawns;
            }
        }

        protected private List<Pawn> justDesignated = new List<Pawn>();
    }
}
