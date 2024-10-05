
/*
Alright, to explain;
This mess, when a pawn is generated does, in order-
	-Checks if they have a faction and their pawnData and quirkManager are initalized, and does so if not
	-Checks if they're humanoid or a predator
	-Checks if they're capible of long endo
	-Checks if they like long endo (or would be potentaly forced)
	-Generates -another- pawn, of a pawnkind that can fit inside them
	-Randomly sets the voretype and if the prey is willing
	-Maxes out the prey's endosickness
	-Shoves the prey inside.

The end result; a raider and their belly prisoner, showing up waddling to your killbox.
Problems-
	-The interaction signaling the start of vore is still in both their social tabs, harming the illusion a bit
	-Preds can be incapacitated by encumberance, which is both kinda dumb and kinda funny
	-Preds are usualy noticably slower, making melee/close range pre-filleds much less of a threat(if they even engage before retreating)
	-This applies to -all- pawn generation, ->incuding your starters<-, meaning you could get n=colonistCount free prisoners by just hitting Randomize a bunch
	-Belly pets/prisoners, as fun as they are, by definition can't contribute much to anything
	-Any long-endo'd prey not in a colonist just ends up getting passed to the world because keeping 70 active vore records for preds that functionaly no longer exist is dumb even by my standards
	-It also lags out the game when generating large raids/trading caravans
	-In the event their prey is freed, they just wander off or are in the middle of heavy cross-fire
	-Most of the pre-fillers get GC'd, but some won't, potentaly harming already stressed late-game performance

It's -is- fun to mess with, at least.

using System;
using System.Linq;
using HarmonyLib;
using RimVore2;
using RimWorld;
using Verse;

namespace RV2R_RutsStuff
{
[HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new Type[] { typeof(PawnGenerationRequest) })]
internal static class RV2_Patch_GeneratePawn
{
	[HarmonyPostfix]
	private static void RV2_TrySpawnPreFilled(ref Pawn __result)
	{
		Pawn pawn = __result;
		if (pawn.IsHumanoid() && pawn.Faction == null)
		{
			pawn.PawnData(true);
			pawn.PawnData(false).QuirkManager(true);
			return;
		}
		if (!pawn.IsHumanoid() && !pawn.RaceProps.predator)
			return;
		if (!pawn.ageTracker.Adult)
			return;
		if (pawn.PawnData(true) == null)
			return;
		if (pawn.PawnData(false).QuirkManager(true) == null)
			return;
		if (!pawn.CanBePredator(out _))
			return;
		if (pawn.PawnData(false).QuirkManager(false).GetTotalSelectorModifier(VoreRole.Predator, ModifierOperation.Add) < 0f)
			return;
		if (!pawn.PawnData(false).QuirkManager(false).HasQuirk(QuirkDefOf.Enablers_Core_Goal_Longendo))
			return;
		if (!pawn.IsSlave && !pawn.IsPrisoner)
		{
			if (pawn.PawnData(false).QuirkManager(false).HasQuirk(DefDatabase<QuirkDef>.GetNamed("GoalPreference_Predator_Store_Dislikes", true)))
				return;
			if (pawn.PawnData(false).QuirkManager(false).HasQuirk(DefDatabase<QuirkDef>.GetNamed("GoalPreference_Predator_Store_Refuses", true)))
				return;
		}
		float num = 0.1f;
		if (pawn.PawnData(false).QuirkManager(false).HasQuirk(DefDatabase<QuirkDef>.GetNamed("GoalPreference_Predator_Store_Likes", true)))
			num += 0.1f;
		if (pawn.PawnData(false).QuirkManager(false).HasQuirk(DefDatabase<QuirkDef>.GetNamed("GoalPreference_Predator_Store_Obsessed", true)))
			num += 0.2f;
		if (pawn.Faction != null)
			if (pawn.Faction.def.naturalEnemy || pawn.Faction.def.permanentEnemy)
				num += 0.1f;
			if (pawn.Faction.leader == pawn)
				num += 0.5f;
		if (pawn.RaceProps.predator)
			if (pawn.RaceProps.maxPreyBodySize >= 2f)
				num += 0.4f;
			if (pawn.RaceProps.maxPreyBodySize >= 1.5f)
				num += 0.3f;
			if (pawn.RaceProps.maxPreyBodySize >= 1f)
				num += 0.2f;
			if (pawn.RaceProps.maxPreyBodySize >= 0.66f)
				num += 0.175f;
			if (pawn.RaceProps.maxPreyBodySize >= 0.5f)
				num += 0.15f;
			if (pawn.RaceProps.maxPreyBodySize < 0.33f)
				num += 0.125f;
		if (Rand.Chance(1f - num))
			return;
		try
		{
			RV2Log.Message("Attempting to prefill " + pawn.LabelShort, false, "Prefill", 0);
			float num2 = 1f;
			if (pawn.IsAnimal())
				num2 -= 0.25f;
			if (pawn.PawnData(false).QuirkManager(false).GetTotalSelectorModifier(RaceType.Humanoid, ModifierOperation.Add) < 0f)
				num2 -= 0.25f;
			if (pawn.PawnData(false).QuirkManager(false).GetTotalSelectorModifier(RaceType.Animal, ModifierOperation.Add) > 1f)
				num2 -= 0.25f;
			PawnKindDef pawnKindDef;
			if (Rand.Chance(num2))
				pawnKindDef = DefDatabase<PawnKindDef>.AllDefsListForReading.Where((PawnKindDef pk) => pk.RaceProps.Humanlike && pk.RaceProps.baseBodySize <= pawn.CalculateVoreCapacity()).RandomElement<PawnKindDef>();
			else
				pawnKindDef = DefDatabase<PawnKindDef>.AllDefsListForReading.Where((PawnKindDef pk) => pk.RaceProps.Animal && pk.RaceProps.baseBodySize <= pawn.CalculateVoreCapacity() && pk.RaceProps.baseBodySize >= pawn.BodySize / 5f && (!pawn.IsHumanoid() || pk.RaceProps.petness > 0f)).RandomElement<PawnKindDef>();
			if (pawnKindDef != null)
			{
				Pawn pawn2 = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKindDef, null, PawnGenerationContext.NonPlayer, -1, false, false, false, true, false, 1f, false, true, false, true, false, false, false, false, false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, false, false, false, false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null, false));
				if (pawn2 != null && pawn.CanEndoVore(pawn2, out text, false))
				{
					RV2Log.Message("Valid prefill prey for " + pawn.LabelShort + " generated", false, "Prefill", 0);
					VoreInteraction voreInteraction = VoreInteractionManager.Retrieve(new VoreInteractionRequest(pawn, pawn2, VoreRole.Predator, false, false, false, null, null, null, null, null, null, null, null, null, null));
					VorePathDef vorePathDef = null;
					if (voreInteraction.ValidGoals.Contains(VoreGoalDefOf.Store))
						vorePathDef = voreInteraction.ValidPaths.Where((VorePathDef p) => p.voreGoal == VoreGoalDefOf.Store).RandomElement<VorePathDef>();
					if (vorePathDef != null)
					{
						RV2Log.Message(pawn.LabelShort + " has valid goal and prey, prefilling with " + pawn2.LabelShort, false, "Prefill", 0);
						VoreStageDef voreStageDef = vorePathDef.stages.Where((VoreStageDef s) => s.partGoal == DefDatabase<PartGoalDef>.GetNamed("Store", true)).RandomElementWithFallback(null);
						float num3 = 0.33f;
						if (pawn.Faction.def.naturalEnemy || pawn.Faction.def.permanentEnemy)
							num3 += 0.33f;
						if (pawn.PawnData(false).QuirkManager(false).HasQuirk(DefDatabase<QuirkDef>.GetNamed("StrugglePreference_Hated", true)))
							num3 -= 0.33f;
						if (pawn2.RaceProps.petness > 0f)
							num3 -= 0.15f;
						VoreTrackerRecord voreTrackerRecord = new VoreTrackerRecord(pawn, pawn2, Rand.Chance(0.33f), pawn, new VorePath(vorePathDef), vorePathDef.stages.IndexOf(voreStageDef), false);
						PreVoreUtility.PopulateRecord(ref voreTrackerRecord, false);
						pawn.PawnData(true).VoreTracker.TrackVore(voreTrackerRecord);
						pawn2.health.AddHediff(RV2R_Common.EndoHediff, null, null, null).Severity = 1f;
						if (pawn2.apparel != null)
							pawn2.apparel.DestroyAll(DestroyMode.Vanish);
						if (pawn2.equipment != null)
							pawn2.equipment.DestroyAllEquipment(DestroyMode.Vanish);
						if (pawn2.inventory != null)
							pawn2.inventory.DestroyAll(DestroyMode.Vanish);
						if (!voreTrackerRecord.IsForced && pawn2.RaceProps.petness > 0f && pawn2.RaceProps.trainability != TrainabilityDefOf.None && pawn.IsHumanoid() && Rand.Chance(pawn2.RaceProps.petness))
						{
							PawnBioAndNameGenerator.GeneratePawnName(pawn2, NameStyle.Full, null, false, null);
							pawn.relations.AddDirectRelation(PawnRelationDefOf.Bond, pawn2);
						}
						if (pawn.Faction != null && pawn2.IsHumanoid() && Rand.Chance(voreTrackerRecord.IsForced ? 0.66f : 0.2f))
							pawn2.guest.SetGuestStatus(pawn.Faction, Rand.Chance(0.5f) ? GuestStatus.Prisoner : GuestStatus.Slave);
						if (pawn.IsHumanoid() && pawn2.IsHumanoid() && Rand.Chance(0.05f))
						{
							pawn.relations.AddDirectRelation(RV2R_Common.PetPred, pawn2);
							pawn2.relations.AddDirectRelation(RV2R_Common.PetPrey, pawn);
						}
						if (pawn.needs.mood != null && pawn.needs.mood.thoughts != null)
						{
							Thought thought = ThoughtMaker.MakeThought(RV2R_Common.ActivePred_Normal);
							if (pawn.QuirkManager(false) != null)
								if (pawn.QuirkManager(false).GetTotalSelectorModifier(VoreRole.Predator, ModifierOperation.Add) <= -2f)
									thought = ThoughtMaker.MakeThought(RV2R_Common.ActivePred_Reluctant);
								else if (pawn.QuirkManager(false).GetTotalSelectorModifier(VoreRole.Predator, ModifierOperation.Add) >= 2f)
									thought = ThoughtMaker.MakeThought(RV2R_Common.ActivePred_Vorny);
							for (int i = 0; i < 30; i++)
								pawn.needs.mood.thoughts.memories.TryGainMemory(thought.def, pawn2, null);
						}
						if (pawn2.needs.mood != null && pawn2.needs.mood.thoughts != null)
						{
							Thought thought2 = ThoughtMaker.MakeThought(RV2R_Common.ActivePrey_Normal);
							if (pawn2.QuirkManager(false) != null)
								if (pawn2.QuirkManager(false).GetTotalSelectorModifier(VoreRole.Prey, ModifierOperation.Add) <= -2f)
									thought2 = ThoughtMaker.MakeThought(RV2R_Common.ActivePrey_Reluctant);
								else if (pawn2.QuirkManager(false).GetTotalSelectorModifier(VoreRole.Prey, ModifierOperation.Add) >= 2f)
									thought2 = ThoughtMaker.MakeThought(RV2R_Common.ActivePrey_Vorny);
							for (int j = 0; j < 30; j++)
								pawn2.needs.mood.thoughts.memories.TryGainMemory(thought2.def, pawn, null);
						}
					}
					else
						Log.Message(__result.LabelShort + " no valid path, " + pawn2.LabelShort);
				}
			}
			else
				RV2Log.Message("No valid prefill prey for " + pawn.LabelShort, false, "Prefill", 0);
		}
		catch (Exception ex)
		{
			string text2 = "RimVore-2: Something went wrong when trying pre-fill a long endo pred, Error:\n";
			Exception ex2 = ex;
			Log.Warning(text2 + ((ex2 != null) ? ex2.ToString() : null));
		}
	}
}
}
*/