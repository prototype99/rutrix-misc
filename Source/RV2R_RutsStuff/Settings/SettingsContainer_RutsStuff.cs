using RimVore2;
using UnityEngine;
using Verse;

namespace RV2R_RutsStuff
{
    internal class SettingsContainer_RutsStuff : SettingsContainer
    {

        private BoolSmartSetting sizedEncumberance;
        private FloatSmartSetting encumberanceModifier;

        private FloatSmartSetting endoSicknessStrength;
        private FloatSmartSetting regressionStrength;
        private BoolSmartSetting chronicCure;

        private BoolSmartSetting endoCapture;
        private BoolSmartSetting insectoidCapture;
        private BoolSmartSetting scariaCapture;

        private FloatSmartSetting playVoreChance;
        private FloatSmartSetting playVoreModifier;
        private BoolSmartSetting playVoreIndescriminate;
        private BoolSmartSetting fatalPlayVore;

        private FloatSmartSetting endoBondChance;
        private BoolSmartSetting preyOnlyEndoBond;
        private BoolSmartSetting willingOnlyEndoBond;
        private BoolSmartSetting vornyBonds;

        private BoolSmartSetting noBleedOut;
        private BoolSmartSetting stopBleeding;
        private BoolSmartSetting noBadTemp;
        private BoolSmartSetting curagaVore;

        public bool SizedEncumberance => this.sizedEncumberance.value;
        public float EncumberanceModifier => this.encumberanceModifier.value / 100f;

        public float EndoSicknessStrength => this.endoSicknessStrength.value / 100f;
        public float RegressionStrength => this.regressionStrength.value / 100f;
        public bool ChronicCure => this.chronicCure.value;

        public bool EndoCapture => this.endoCapture.value;
        public bool InsectoidCapture => this.insectoidCapture.value;
        public bool ScariaCapture => this.scariaCapture.value;

        public float PlayVoreChance => this.playVoreChance.value;
        public float PlayVoreModifier => this.playVoreModifier.value;
        public bool PlayVoreIndescriminate => this.playVoreIndescriminate.value;
        public bool FatalPlayVore => this.fatalPlayVore.value;

        public float EndoBondChance => this.endoBondChance.value;
        public bool PreyOnlyEndoBond => this.preyOnlyEndoBond.value;
        public bool WillingOnlyEndoBond => this.willingOnlyEndoBond.value;
        public bool VornyBonds => this.vornyBonds.value;

        public bool NoBleedOut => this.noBleedOut.value;
        public bool StopBleeding => this.stopBleeding.value;
        public bool NoBadTemp => this.noBadTemp.value;
        public bool CuragaVore => this.curagaVore.value;

        public override void EnsureSmartSettingDefinition()
        {
            if (this.sizedEncumberance == null || this.sizedEncumberance.IsInvalid())
                this.sizedEncumberance = new BoolSmartSetting("RV2R_Settings_SizedEncumberance", true, true, "RV2R_Settings_SizedEncumberance_Tip");
            if (this.encumberanceModifier == null || this.encumberanceModifier.IsInvalid())
                this.encumberanceModifier = new FloatSmartSetting("RV2R_Settings_EncumberanceModifier", 100f, 100f, 0f, 400f, "RV2R_Settings_EncumberanceModifier_Tip", "0", "%");

            if (this.endoSicknessStrength == null || this.endoSicknessStrength.IsInvalid())
                this.endoSicknessStrength = new FloatSmartSetting("RV2R_Settings_EndoSickness", 100f, 100f, 0f, 400f, "RV2R_Settings_EndoSickness_Tip", "0", "%");
            if (this.regressionStrength == null || this.regressionStrength.IsInvalid())
                this.regressionStrength = new FloatSmartSetting("RV2R_Settings_AgeRegression", 5f, 5f, 0f, 60f, "RV2R_Settings_AgeRegression_Tip", "0.0");
            if (this.chronicCure == null || this.chronicCure.IsInvalid())
                this.chronicCure = new BoolSmartSetting("RV2R_Settings_AgeRegressionCure", true, true, "RV2R_Settings_AgeRegressionCure_Tip");

            if (this.endoCapture == null || this.endoCapture.IsInvalid())
                this.endoCapture = new BoolSmartSetting("RV2R_Settings_EndoCapture", true, true, "RV2R_Settings_EndoCapture_Tip");
            if (this.insectoidCapture == null || this.insectoidCapture.IsInvalid())
                this.insectoidCapture = new BoolSmartSetting("RV2R_Settings_EndoCapture_Insectoids", false, false, "RV2R_Settings_EndoCapture_Insectoids_Tip");
            if (this.scariaCapture == null || this.scariaCapture.IsInvalid())
                this.scariaCapture = new BoolSmartSetting("RV2R_Settings_EndoCapture_Scaria", false, false, "RV2R_Settings_EndoCapture_Scaria_Tip");

            if (this.playVoreChance == null || this.playVoreChance.IsInvalid())
                this.playVoreChance = new FloatSmartSetting("RV2R_Settings_AnimalRandomNomChance", 24f, 24f, 6f, 168f, "RV2R_Settings_AnimalRandomNomChance_Tip", "0.0");
            if (this.playVoreModifier == null || this.playVoreModifier.IsInvalid())
                this.playVoreModifier = new FloatSmartSetting("RV2R_Settings_AnimalRandomNomChanceModifier", 25f, 25f, 0f, 400f, "RV2R_Settings_AnimalRandomNomChanceModifier_Tip", "0", "%");
            if (this.playVoreIndescriminate == null || this.playVoreIndescriminate.IsInvalid())
                this.playVoreIndescriminate = new BoolSmartSetting("RV2R_Settings_AnimalRandomNomIndiscriminate", false, false, "RV2R_Settings_AnimalRandomNomIndiscriminate_Tip");
            if (this.fatalPlayVore == null || this.fatalPlayVore.IsInvalid())
                this.fatalPlayVore = new BoolSmartSetting("RV2R_Settings_AnimalRandomNomFatal", false, false, "RV2R_Settings_AnimalRandomNomFatal_Tip");

            if (this.endoBondChance == null || this.endoBondChance.IsInvalid())
                this.endoBondChance = new FloatSmartSetting("RV2R_Settings_EndoBondChance", 0.0001f, 0.0001f, 0.0000f, 0.0020f, "RV2R_Settings_EndoBondChance_Tip", "0.0000", "%");
            if (this.preyOnlyEndoBond == null || this.preyOnlyEndoBond.IsInvalid())
                this.preyOnlyEndoBond = new BoolSmartSetting("RV2R_Settings_EndoBondPredOnly", false, false, "RV2R_Settings_EndoBondPredOnly_Tip");
            if (this.willingOnlyEndoBond == null || this.willingOnlyEndoBond.IsInvalid())
                this.willingOnlyEndoBond = new BoolSmartSetting("RV2R_Settings_EndoBondUnwilling", false, false, "RV2R_Settings_EndoBondUnwilling_Tip");
            if (this.vornyBonds == null || this.vornyBonds.IsInvalid())
                this.vornyBonds = new BoolSmartSetting("RV2R_Settings_VornyBonds", false, false, "RV2R_Settings_VornyBonds_Tip");

            if (this.noBleedOut == null || this.noBleedOut.IsInvalid())
                this.noBleedOut = new BoolSmartSetting("RV2R_Cheats_NoBleedOut", false, false, "RV2R_Cheats_NoBleedOut_Tip");
            if (this.stopBleeding == null || this.stopBleeding.IsInvalid())
                this.stopBleeding = new BoolSmartSetting("RV2R_Cheats_StopBleeding", false, false, "RV2R_Cheats_StopBleeding_Tip");
            if (this.noBadTemp == null || this.noBadTemp.IsInvalid())
                this.noBadTemp = new BoolSmartSetting("RV2R_Cheats_NoExtremeTemp", false, false, "RV2R_Cheats_NoExtremeTemp_Tip");
            if (this.curagaVore == null || this.curagaVore.IsInvalid())
                this.curagaVore = new BoolSmartSetting("RV2R_Cheats_HyperHealing", false, false, "RV2R_Cheats_HyperHealing_Tip");

        }

        private bool heightStale = true;

        private float height;

        private Vector2 scrollPosition;

        public override void Reset()
        {
            this.sizedEncumberance = null;
            this.encumberanceModifier = null;
            this.endoSicknessStrength = null;
            this.regressionStrength = null;
            this.chronicCure = null;
            this.endoCapture = null;
            this.insectoidCapture = null;
            this.scariaCapture = null;
            this.playVoreChance = null;
            this.playVoreModifier = null;
            this.playVoreIndescriminate = null;
            this.fatalPlayVore = null;
            this.endoBondChance = null;
            this.preyOnlyEndoBond = null;
            this.noBleedOut = null;
            this.stopBleeding = null;
            this.noBadTemp = null;
            this.curagaVore = null;

            EnsureSmartSettingDefinition();
        }
        public void FillRect(Rect inRect)
        {
            UIUtility.MakeAndBeginScrollView(inRect, this.height, ref this.scrollPosition, out Listing_Standard listing_Standard);
            if (listing_Standard.ButtonText("RV2_Settings_Reset".Translate(), null))
                this.Reset();

            listing_Standard.Gap(12f);
            listing_Standard.Label("RV2R_Encumberance_Section".Translate());
            listing_Standard.Gap(12f);
            this.sizedEncumberance.DoSetting(listing_Standard);
            this.encumberanceModifier.DoSetting(listing_Standard);
            listing_Standard.Gap(18f);
            listing_Standard.Label("RV2R_Tick_Section".Translate());
            listing_Standard.Gap(12f);
            this.endoSicknessStrength.DoSetting(listing_Standard);
            this.regressionStrength.DoSetting(listing_Standard);
            this.chronicCure.DoSetting(listing_Standard);
            listing_Standard.Gap(18f);
            listing_Standard.Label("RV2R_Capture_Section".Translate());
            listing_Standard.Gap(12f);
            this.endoCapture.DoSetting(listing_Standard);
            this.insectoidCapture.DoSetting(listing_Standard);
            this.scariaCapture.DoSetting(listing_Standard);
            listing_Standard.Gap(18f);
            listing_Standard.Label("RV2R_AnimalNom_Section".Translate());
            listing_Standard.Gap(12f);
            this.playVoreChance.DoSetting(listing_Standard);
            this.playVoreModifier.DoSetting(listing_Standard);
            this.playVoreIndescriminate.DoSetting(listing_Standard);
            this.fatalPlayVore.DoSetting(listing_Standard);
            listing_Standard.Gap(18f);
            listing_Standard.Label("RV2R_Bonds_Section".Translate());
            listing_Standard.Gap(12f);
            this.endoBondChance.DoSetting(listing_Standard);
            this.preyOnlyEndoBond.DoSetting(listing_Standard);
            this.willingOnlyEndoBond.DoSetting(listing_Standard);
            this.vornyBonds.DoSetting(listing_Standard);
            listing_Standard.Gap(18f);
            listing_Standard.Label("RV2R_Cheats_Section".Translate());
            listing_Standard.Gap(12f);
            this.noBleedOut.DoSetting(listing_Standard);
            this.stopBleeding.DoSetting(listing_Standard);
            this.noBadTemp.DoSetting(listing_Standard);
            this.curagaVore.DoSetting(listing_Standard);
            listing_Standard.Gap(72f);
            listing_Standard.Label("RV2R_FuckUnity".Translate());
            listing_Standard.EndScrollView(ref this.height, ref this.heightStale);
        }
        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
                this.EnsureSmartSettingDefinition();

            Scribe_Deep.Look<BoolSmartSetting>(ref this.sizedEncumberance, "sizedEncumberance", new object[0]);
            Scribe_Deep.Look<FloatSmartSetting>(ref this.encumberanceModifier, "encumberanceModifier", new object[0]);
            Scribe_Deep.Look<FloatSmartSetting>(ref this.endoSicknessStrength, "endoSicknessStrength", new object[0]);
            Scribe_Deep.Look<FloatSmartSetting>(ref this.regressionStrength, "regressionStrength", new object[0]);
            Scribe_Deep.Look<BoolSmartSetting>(ref this.chronicCure, "chronicCure", new object[0]);
            Scribe_Deep.Look<BoolSmartSetting>(ref this.endoCapture, "endoCapture", new object[0]);
            Scribe_Deep.Look<BoolSmartSetting>(ref this.insectoidCapture, "insectoidCapture", new object[0]);
            Scribe_Deep.Look<BoolSmartSetting>(ref this.scariaCapture, "scariaCapture", new object[0]);
            Scribe_Deep.Look<FloatSmartSetting>(ref this.playVoreChance, "playVoreChance", new object[0]);
            Scribe_Deep.Look<BoolSmartSetting>(ref this.playVoreIndescriminate, "playVoreIndescriminate", new object[0]);
            Scribe_Deep.Look<BoolSmartSetting>(ref this.fatalPlayVore, "fatalPlayVore", new object[0]);
            Scribe_Deep.Look<FloatSmartSetting>(ref this.endoBondChance, "endoBondChance", new object[0]);
            Scribe_Deep.Look<BoolSmartSetting>(ref this.preyOnlyEndoBond, "preyOnlyEndoBond", new object[0]);
            Scribe_Deep.Look<BoolSmartSetting>(ref this.willingOnlyEndoBond, "willingOnlyEndoBond", new object[0]);
            Scribe_Deep.Look<BoolSmartSetting>(ref this.vornyBonds, "vornyBonds", new object[0]);
            Scribe_Deep.Look<BoolSmartSetting>(ref this.noBleedOut, "noBleedOut", new object[0]);
            Scribe_Deep.Look<BoolSmartSetting>(ref this.stopBleeding, "stopBleeding", new object[0]);
            Scribe_Deep.Look<BoolSmartSetting>(ref this.noBadTemp, "noBadTemp", new object[0]);
            Scribe_Deep.Look<BoolSmartSetting>(ref this.curagaVore, "curagaVore", new object[0]);
            this.PostExposeData();
        }
    }
}
