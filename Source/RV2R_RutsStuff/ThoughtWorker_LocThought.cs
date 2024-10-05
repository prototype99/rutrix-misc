/*

    I wanted to have prey get location/goal-specific moods; shut-ins being happy about getting stored, anal-averse hating being in the intestines, you know.

    Just grab their p.PreferenceFor(), 4head. Right?
    For goals, this mostly works. Heal and other goals that change to pass are odd.
    Types, on the other hand... for ub/cv/bv, they're fine. Oral? Tail? Not so much. It's reasonable to assume a prey would like the belly but hate the guts, or hate the tail and be fine with the belly and love the guts. Fetishes are weird.

    Getting this to work like I want it would require getting a preference for each -stage-. That is to say, a disconected method that grabs where they are or what's happening to them, and spits out a some float I guess.

    It could work? Never got around to it. Would likly require copious thought.DefName.Contains(quirk.DefName).


using System;
using System.Collections.Generic;
using RimVore2;
using RimWorld;
using Verse;

namespace RV2R_RutsStuff
{
	internal partial class ThoughtWorker_LocThought : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.QuirkManager(false) == null)
			{
				return ThoughtState.Inactive;
			}
			VoreTrackerRecord voreRecord = p.GetVoreRecord();
			List<Quirk> activeQuirks = p.QuirkManager(false).ActiveQuirks;
			if (voreRecord == null)
			{
				return ThoughtState.Inactive;
			}
			if (activeQuirks.NullOrEmpty<Quirk>())
			{
				return ThoughtState.Inactive;
			}
			if (this.def != null)
			{
					if (this.def.defName.Contains("TypePreference") && (this.def.defName.Contains(voreRecord.VoreType)))
					{
						
						float pref = 1f * p.PreferenceFor(voreRecord.VoreType, VoreRole.Prey, ModifierOperation.Multiply)
						if (pref > 2f)
							return ThoughtState.ActiveAtStage(0);
						if (pref > 1.35f)
							return ThoughtState.ActiveAtStage(1);
						if (pref < -1.35f)
							return ThoughtState.ActiveAtStage(2);
						if (pref < -2f)
							return ThoughtState.ActiveAtStage(3);
					}
					else if (this.def.defName.Contains(voreRecord.VoreGoal)
					{
						float pref = 1f * p.PreferenceFor(voreRecord.VoreGoal, VoreRole.Prey, ModifierOperation.Multiply)
						if (pref > 2f)
							return ThoughtState.ActiveAtStage(0);
						if (pref > 1.35f)
							return ThoughtState.ActiveAtStage(1);
						if (pref < -1.35f)
							return ThoughtState.ActiveAtStage(2);
						if (pref < -2f)
							return ThoughtState.ActiveAtStage(3);
					}
				
			}
			return ThoughtState.Inactive;
		}
	}
}

*/