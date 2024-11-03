using RimVore2;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using static RV2R_RutsStuff.Patch_RV2R_Settings;

namespace RV2R_RutsStuff
{
    public class JobDriver_GutLovin : JobDriver
    {
        public const string PreyLibidoModifierName = "Prey_Libido";
        public const float FallbackLibidoValue = .66f;
        public const float BaseTicksLength = 2000f;
        public const float RandTicksLengthRandMin = .5f;
        public const float RandTicksLengthRandMax = 1.1f;
        public const float PredJoyBase = 0.002f;
        public const float PreyJobBase = .015f;

        private readonly TargetIndex BedInd = TargetIndex.A;

        private static readonly SimpleCurve LovinIntervalHoursFromAgeCurve = new SimpleCurve
        {
            {
                new CurvePoint(1f, 1.5f),
                true
            },
            {
                new CurvePoint(1.375f, 1.5f),
                true
            },
            {
                new CurvePoint(1.875f, 4f),
                true
            },
            {
                new CurvePoint(3.125f, 12f),
                true
            },
            {
                new CurvePoint(4.675f, 36f),
                true
            }
        };

        private Building_Bed Bed
        {
            get
            {
                return (Building_Bed)(Thing)this.job.GetTarget(this.BedInd);
            }
        }
        private bool HasBed => Bed != null;

        private int ticksLeft;

        public List<Pawn> WillingPrey;
        public List<Pawn> UnwillingPrey;

        public float PredMod = 0f;

        

        #region override
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.ticksLeft, "ticksLeft", 0, false);
            Scribe_Collections.Look(ref this.WillingPrey, nameof(this.WillingPrey), LookMode.Reference);
            Scribe_Collections.Look(ref this.UnwillingPrey, nameof(this.UnwillingPrey), LookMode.Reference);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (this.Bed == null) return true;
            return this.pawn.Reserve(this.Bed, this.job, this.Bed.SleepingSlotsCount, 0, null, errorOnFailed);
        }

        public override bool CanBeginNowWhileLyingDown()
        {
            if (Bed == null) return true;
            if (JobInBedUtility.InBedOrRestSpotNow(this.pawn, this.job.GetTarget(this.BedInd))) return true;

            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            VoreTracker voreTracker = this.pawn.PawnData(false)?.VoreTracker;
            if (voreTracker == null)
            {
                RV2Log.Warning("Gutloving make new toils request when pawn lacked vore tracker", "Jobs");
                yield break;
            }
            float minPreyMod = 0f;

            QuirkManager quirkManager = this.pawn.QuirkManager(false);
            PredMod = quirkManager?.ModifyValue(PreyLibidoModifierName, PredMod) ?? FallbackLibidoValue;

            PrepareFromTrackers(voreTracker, ref minPreyMod);

            var ticksLength = (int)(BaseTicksLength
                * ((PredMod + minPreyMod) / 2f)
                * Rand.Range(RandTicksLengthRandMin, RandTicksLengthRandMax));
            this.ticksLeft = ticksLength;
            RV2Log.Message($"Predator {pawn} starting gut lovin' ({PredMod}, {minPreyMod}, {WillingPrey.Count}, {UnwillingPrey.Count}, {ticksLength})", "Gutlovin");

            if(Bed == null)
            {
                pawn.jobs.jobQueue.EnqueueFirst(JobMaker.MakeJob(JobDefOf.LayDown, pawn.Position));
            } else
            {
                this.FailOnDespawnedOrNull(BedInd);
                this.KeepLyingDown(BedInd);
                yield return Toils_Bed.ClaimBedIfNonMedical(BedInd);
                yield return Toils_Bed.GotoBed(BedInd);
            }

            Toil toil = Toils_LayDown.LayDown(BedInd, HasBed,
                lookForOtherJobs: false,
                canSleep: false,
                gainRestAndHealth: false,
                noBedLayingPosture: PawnPosture.LayingOnGroundNormal
            );
            toil.AddPreTickAction(() => PreTickAction());
            toil.AddFinishAction(() => OnFinish());
            toil.socialMode = RandomSocialMode.Off;
            yield return toil;
        }
        #endregion

        #region Prepare
        private void PrepareFromTrackers(VoreTracker voreTracker, ref float preyMod)
        {
            WillingPrey = new List<Pawn>();
            UnwillingPrey = new List<Pawn>();
            if (voreTracker.VoreTrackerRecords.NullOrEmpty()) return;

            foreach (var record in voreTracker.VoreTrackerRecords)
            {
                if (!IsValidPreyForLoving(record.Prey)) return;

                float desire = GetDesireFor(record);

                if (ShouldBeUnwilling(record, desire)) UnwillingPrey.Add(record.Prey);
                else WillingPrey.Add(record.Prey);

                preyMod = Mathf.Max(preyMod, desire);
            }
        }
        private float GetDesireFor(VoreTrackerRecord record)
        {
            float desire = 1f;
            var qm = record.Prey.QuirkManager(false);
            if (qm?.HasValueModifier(PreyLibidoModifierName) == true)
                desire = qm.ModifyValue(PreyLibidoModifierName, desire);

            return desire;
        }
        private bool ShouldBeUnwilling(VoreTrackerRecord record, float desire)
        {
            if (desire > .33f) return false;
            if (record.Prey.relations.OpinionOf(this.pawn) >= 0) return false;
            return true;
        }
        private bool IsValidPreyForLoving(Pawn prey)
        {
            if (prey.IsHumanoid()) return true;

            //Animal/pawnmorpher
            if (!RV2_Rut_Settings.rutsStuff.GutLovinSapients) return false;
            if (RV2R_Utilities.IsSapient(prey)) return true;

            return false;
        }
        #endregion

        #region PretickAction
        private void PreTickAction()
        {
            this.ticksLeft--;

            if (this.ticksLeft <= 0)
            {
                this.ReadyForNextToil();
                return;
            }
            if (!this.pawn.IsHashIntervalTick(100)) return;

            FleckMaker.ThrowMetaIcon(this.pawn.Position, this.pawn.Map, FleckDefOf.Heart);

            if (this.pawn?.needs?.joy != null) this.pawn.needs.joy.CurLevel += PredJoyBase * PredMod;
            var voreTracker = this.pawn.PawnData(false).VoreTracker;
            voreTracker.VoreTrackerRecords.ForEach(record =>
            {
                ApplyPreTickActionToRecord(record);
            });
        }

        private void ApplyPreTickActionToRecord(VoreTrackerRecord record)
        {
            if (record.Prey.needs?.joy == null) return;
            var qm = record.Prey.QuirkManager(false);
            float willingness = 1f;

            if (qm?.HasValueModifier(PreyLibidoModifierName) == true) willingness = qm.ModifyValue(PreyLibidoModifierName, willingness);
            if (willingness <= 0f) return;
            record.Prey.needs.joy.CurLevel += PreyJobBase * willingness;
        }
        #endregion
        #region OnFinished
        private void OnFinish()
        {
            ApplyFinishPredThoughts(out var predOpinion);

            UpdateLovingTickMindState(pawn);

            pawn.GetVoreTracker().VoreTrackerRecords.ForEach(record =>
            {
                ApplyOnFinishToRecord(record, predOpinion);
            });
        }

        private void ApplyFinishPredThoughts(out ThoughtDef predOpinion)
        {
            GetPredThoughts(out var predThought, out predOpinion);

            Thought_Memory thought_Memory = ThoughtMaker.MakeThought(predThought) as Thought_Memory;
            if (pawn.health?.hediffSet != null)
            {
                thought_Memory.moodOffset += (int)PredMod;
                thought_Memory.moodPowerFactor = PawnLoveEnhancerFactor(pawn);
            }
            TryApplyMemory(this.pawn, thought_Memory);
        }

        private void ApplyOnFinishToRecord(VoreTrackerRecord record, ThoughtDef thought_MemoryOpinion)
        {
            var preyLib = 1f;
            var thought = GetPreyThoughts(preyLib);

            if (WillingPrey.Contains(record.Prey)) ApplyOnFinishToRecordWillingPrey(record, ref preyLib, ref thought);
            else ApplyOnFinishedToRecordUnwillingPrey(record, ref preyLib, ref thought);

            TryApplyMemory(record.Prey, thought, this.pawn);
            TryApplyMemory(this.pawn, thought_MemoryOpinion, record.Prey);
        }
        private void ApplyOnFinishedToRecordUnwillingPrey(VoreTrackerRecord record, ref float preyLib, ref ThoughtDef thoughtDef)
        {
            //only applying this if there is NO willing prey, this is the rape condition I believe
            if (WillingPrey.Any()) return;
            if (!UnwillingPrey.Contains(record.Prey)) return;

            thoughtDef = RV2R_Common.PreyLovin_Bad;
        }
        private void ApplyOnFinishToRecordWillingPrey(VoreTrackerRecord record, ref float preyLib, ref ThoughtDef thoughtDef)
        {
            preyLib = record.Prey.QuirkManager(false)?.ModifyValue(PreyLibidoModifierName, preyLib) ?? FallbackLibidoValue;
            UpdateLovingTickMindState(record.Prey);

            WillingPrey.ForEach(other =>
            {
                if (!ShouldApplyGotSomeLovingTo(record, other)) return;
                TryApplyMemory(record.Prey, ThoughtDefOf.GotSomeLovin, other);
            });
        }

        private bool ShouldApplyGotSomeLovingTo(VoreTrackerRecord record, Pawn other)
        {
            if (record.Prey == other) return false;
            if (!LovePartnerRelationUtility.LovePartnerRelationExists(record.Prey, other)) return false;
            var otherVoreRecord = other.GetVoreRecord();
            if (otherVoreRecord.CurrentBodyPart != record.CurrentBodyPart) return false;
            return true;
        }
        private void UpdateLovingTickMindState(Pawn pawn)
        {
            if (pawn.mindState == null) return;
            pawn.mindState.canLovinTick = Find.TickManager.TicksGame + this.GenerateRandomMinTicksToNextLovin(pawn);
        }
        #endregion

        #region LoveEnhancer
        private float PawnLoveEnhancerFactor(Pawn pawn)
        {
            if (pawn?.health?.hediffSet?.hediffs?.Any(h => LoveEnhancerHediff().Contains(h.def)) == true) return 1.5f;

            return 1.0f;
        }
        private IEnumerable<HediffDef> LoveEnhancerHediff()
        {
            yield return HediffDefOf.LoveEnhancer;
        }
        #endregion

        #region Thought mappers
        private ThoughtDef GetPreyThoughts(float preyLib)
        {
            ThoughtDef preyThought = RV2R_Common.PreyLovin_Normal;

            if (preyLib > 1.5f)
                preyThought = RV2R_Common.PreyLovin_VeryGood;
            else if (preyLib >= 1f)
                preyThought = RV2R_Common.PreyLovin_Good;
            else if (preyLib <= 0.5f)
                preyThought = RV2R_Common.PreyLovin_Meh;
            else if (preyLib <= 0.0f)
                preyThought = null;

            return preyThought;
        }
        private void GetPredThoughts(out ThoughtDef predThought, out ThoughtDef predOpinion)
        {
            // Okay, so, to explain: The pred needs a mood boost, and an opinion boost for each prey.
            // Since getting +120 mood from belly humping 10 prey is maybe a little much,
            // we handle the mood boost and opinion boosts with seperate thoughts.
            predThought = RV2R_Common.PredLovin_Normal_Mood;
            predOpinion = RV2R_Common.PredLovin_Normal;
            if (PredMod > 1.5f)
            {
                predThought = RV2R_Common.PredLovin_VeryGood_Mood;
                predOpinion = RV2R_Common.PredLovin_VeryGood;
            }
            else if (PredMod >= 1f)
            {
                predThought = RV2R_Common.PredLovin_Good_Mood;
                predOpinion = RV2R_Common.PredLovin_Good;
            }
            else if (PredMod <= 0.5f)
            {
                predThought = RV2R_Common.PreyLovin_Meh; // Meh contains no mood boosts, so doubling up does nothing
                predOpinion = RV2R_Common.PreyLovin_Meh;
            }
        }
        #endregion

        #region util
        private int GenerateRandomMinTicksToNextLovin(Pawn pawn)
        {
            if (DebugSettings.alwaysDoLovin)
                return 300;

            float relAge = (pawn.ageTracker.AgeBiologicalYearsFloat / pawn.ageTracker.AdultMinAge);
            float num = JobDriver_GutLovin.LovinIntervalHoursFromAgeCurve.Evaluate(relAge);
            num = Rand.Gaussian(num, 0.3f);

            return (int)(num * 2500f);
        }
        private static void TryApplyMemory(Pawn Pawn, ThoughtDef def, Pawn otherPawn = null)
        {
            var memories = Pawn?.needs?.mood?.thoughts?.memories;
            if (memories == null) return;
            Thought_Memory memory = ThoughtMaker.MakeThought(def) as Thought_Memory;
            if (memories == null) return;
            memories.TryGainMemory(memory, otherPawn);
        }
        private static void TryApplyMemory(Pawn Pawn, Thought_Memory memory, Pawn otherPawn = null)
        {
            var memories = Pawn?.needs?.mood?.thoughts?.memories;
            if (memories == null) return;
            memories.TryGainMemory(memory, otherPawn);
        }
        #endregion
    }
}
