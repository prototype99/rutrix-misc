using RimVore2;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using static RV2R_RutsStuff.Patch_RV2R_Settings;

namespace RV2R_RutsStuff
{
    public class JobGiver_DoGutLovin : ThinkNode_JobGiver
    {
        public Pawn ActivePawn;
        public PawnData PawnData;
        public QuirkManager QuirkManager;
        public float PredLib;

        public bool loverPresent = false;
        public bool noHumanlikes = false;
        public bool CanLove = false;
        public bool noAttraction = false;
        public int bestOp = -100;
        public float BestPreyLib = -10f;

        protected override Job TryGiveJob(Pawn pawn)
        {
            try
            {
                PredLib = 1;
                BestPreyLib = 0;
                ActivePawn = pawn;
                loverPresent = false;
                return _TryGiveJob();
            }
            catch (Exception e)
            {
                Log.Warning("RV-2R: Something went wrong in JobGiver_DoGutLovin for " + pawn.LabelShort + ": " + e);
                return null;
            }
            finally
            {
                ActivePawn = null;
                PawnData = null;
                QuirkManager = null;
                loverPresent = false;
            }
        }

        private Job _TryGiveJob()
        {
            if (Find.TickManager.TicksGame < ActivePawn.mindState.canLovinTick)
                return null;
            if (!ActivePawn.health.capacities.CanBeAwake)
                return null;
            if (!ActivePawn.IsActivePredator())
                return null;

            if (BedPrevents()) return null;
            PawnData = ActivePawn.PawnData(false);
            if (PawnData == null) return null;
            QuirkManager = ActivePawn.QuirkManager(false);

            CalcPredLib();
            if (PredLib <= 0f) return null;

            if(!CheckRecords()) return null;

            if (noAttraction) PredLib *= .5f;
            var ignoresPartnerDesire = PredNonConAllowed();
            if (BestPreyLib < 0f && !ignoresPartnerDesire) return null;
            if(!(RV2_Rut_Settings.rutsStuff.GutLovinCheats || loverPresent || OneNightStandCheck())) return null;

            if (!Rand.Chance(ignoresPartnerDesire ? PredLib : (PredLib + BestPreyLib) * .5f)) return null;

            return JobMaker.MakeJob(RV2R_Common.RV2R_GutLovin, ActivePawn.CurrentBed());
        }

        private bool OneNightStandCheck()
        {
            if (!RV2_Rut_Settings.rutsStuff.GutLovinStands) return false;
            return LovePartnerRelationUtility.ExistingLovePartner(ActivePawn, false) == null;
        }

        private bool PredNonConAllowed()
        {
            if (!RV2_Rut_Settings.rutsStuff.GutLovinNonCon) return false;
            if (NonConBlacklistTraits().Any(t => ActivePawn?.story?.traits?.HasTrait(t) == true)) return false;
            if (bestOp <= -10) return true;
            return NonConSupportingTraits().Any(t=>ActivePawn?.story?.traits?.HasTrait(t) == true);
        }
        public IEnumerable<TraitDef> NonConSupportingTraits()
        {
            yield return TraitDefOf.Psychopath;
        }
        public IEnumerable<TraitDef> NonConBlacklistTraits()
        {
            yield return TraitDefOf.Kind;
        }

        private bool CheckRecords()
        {
            var records = PawnData.VoreTracker?.VoreTrackerRecords;
            if (records.NullOrEmpty()) return false;
            foreach(var record in records)
            {
                if (!CheckRecord(record)) return false;
            }
            return true;
        }

        private bool CheckRecord(VoreTrackerRecord record)
        {
            Pawn prey = record.Prey;
            if (ActivePawn.relations.FamilyByBlood.Contains(prey))
            {
                if (!LovePartnerRelationUtility.LovePartnerRelationExists(prey, ActivePawn)) return false;
            }

            if (!noHumanlikes && prey.IsSapient())
            {
                if(RV2_Rut_Settings.rutsStuff.GutLovinSapients || prey.IsHumanoid())
                    noHumanlikes = false;
            }

            float preyLib = 1f;
            var qm = prey.QuirkManager(false);
            if (qm?.HasValueModifier(JobDriver_GutLovin.PreyLibidoModifierName) == true) preyLib = qm.ModifyValue(JobDriver_GutLovin.PreyLibidoModifierName, preyLib);

            noAttraction = noAttraction || RV2R_Utilities.IsAttracted(ActivePawn, prey);

            if (preyLib < 1.5f && !RV2R_Utilities.IsAttracted(prey, ActivePawn))
                preyLib /= 2f;

            bestOp = Math.Max(bestOp, ActivePawn.relations.OpinionOf(prey));
            BestPreyLib = Math.Max(BestPreyLib, preyLib);
            loverPresent = loverPresent || LovePartnerRelationUtility.LovePartnerRelationExists(ActivePawn, prey);
            return true;
        }

        private void CalcPredLib()
        {

            if (QuirkManager?.HasValueModifier(JobDriver_GutLovin.PredatorLibidoModifierName) == true)
                PredLib = QuirkManager.ModifyValue(JobDriver_GutLovin.PredatorLibidoModifierName, PredLib);
        }
        private bool BedPrevents()
        {
            Building_Bed bed = ActivePawn.CurrentBed();
            if (bed == null)
            {
                //trys to awaken
                return RestUtility.Awake(ActivePawn);
            }
            if (bed.Medical) return true;
            return bed.CurOccupants.Any(p => p != ActivePawn);
        }
    }
}
