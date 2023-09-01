using RimVore2;
using UnityEngine;
using Verse;

namespace RV2R_RutsStuff
{
    internal class SettingsContainer_RutsStuff : SettingsContainer
    {

        private BoolSmartSetting sizedEncumberance;
        private FloatSmartSetting encumberanceModifier;
        private FloatSmartSetting endoSicknessStrenght;
        private FloatSmartSetting regressionStrenght;
        private BoolSmartSetting endoCapture;
        private BoolSmartSetting insectoidCapture;
        private FloatSmartSetting playVoreChance;
        private BoolSmartSetting fatalPlayVore;
        private FloatSmartSetting endoBondChance;
        private BoolSmartSetting preyOnlyEndoBond;

        public bool SizedEncumberance => this.sizedEncumberance.value;
        public float EncumberanceModifier => this.encumberanceModifier.value / 100f;
        public float EndoSicknessStrength => this.endoSicknessStrenght.value / 100f;
        public float RegressionStrength => this.regressionStrenght.value / 100f;
        public bool EndoCapture => this.endoCapture.value;
        public bool InsectoidCapture => this.insectoidCapture.value;
        public float PlayVoreChance => this.playVoreChance.value;
        public bool FatalPlayVore => this.fatalPlayVore.value;
        public float EndoBondChance => this.endoBondChance.value;
        public bool PreyOnlyEndoBond => this.preyOnlyEndoBond.value;

        public override void EnsureSmartSettingDefinition()
        {
            /*    if (this.sizedEncumberance == null || this.sizedEncumberance.IsInvalid())
                    this.sizedEncumberance = new BoolSmartSetting("RV2R_Settings_SizedEncumberance", true, true, "RV2R_Settings_SizedEncumberance_Tip");
                if (this.encumberanceModifier == null || this.encumberanceModifier.IsInvalid())
                    this.encumberanceModifier = new FloatSmartSetting("RV2R_Settings_EncumberanceModifier", 100f, 100f, 0f, 400f, "RV2R_Settings_EncumberanceModifier_Tip", "0", "%");

                if (this.endoSicknessStrenght == null || this.endoSicknessStrenght.IsInvalid())
                    this.endoSicknessStrenght = new FloatSmartSetting("RV2R_Settings_EndoSickness", 100f, 100f, 0f, 400f, "RV2R_Settings_EndoSickness_Tip", "0", "%");
            */
            if (this.regressionStrenght == null || this.regressionStrenght.IsInvalid())
                    this.regressionStrenght = new FloatSmartSetting("RV2R_Settings_AgeRegression", 5f, 5f, 0f, 30f, "RV2R_Settings_AgeRegression_Tip", "0.0");
            
            if (this.endoCapture == null || this.endoCapture.IsInvalid())
                this.endoCapture = new BoolSmartSetting("RV2R_Settings_EndoCapture", true, true, "RV2R_Settings_EndoCapture_Tip");
            if (this.insectoidCapture == null || this.insectoidCapture.IsInvalid())
                this.insectoidCapture = new BoolSmartSetting("RV2R_Settings_EndoCapture_Insectoids", false, false, "RV2R_Settings_EndoCapture_Insectoids_Tip");

            if (this.playVoreChance == null || this.playVoreChance.IsInvalid())
                this.playVoreChance = new FloatSmartSetting("RV2R_Settings_AnimalRandomNomChance", 24f, 24f, 6f, 96f, "RV2R_Settings_AnimalRandomNomChance_Tip", "0.0");
            if (this.fatalPlayVore == null || this.fatalPlayVore.IsInvalid())
                this.fatalPlayVore = new BoolSmartSetting("RV2R_Settings_AnimalRandomNomFatal", false, false, "RV2R_Settings_AnimalRandomNomFatal_Tip");

            if (this.endoBondChance == null || this.endoBondChance.IsInvalid())
                this.endoBondChance = new FloatSmartSetting("RV2_Settings_EndoBondChance", 0.0001f, 0.0001f, 0.0000f, 0.0010f, "RV2_Settings_EndoBondChance_Tip", "0.0000", "%");
            if (this.preyOnlyEndoBond == null || this.preyOnlyEndoBond.IsInvalid())
                this.preyOnlyEndoBond = new BoolSmartSetting("RV2_Settings_EndoBondPredOnly", false, false, "RV2_Settings_EndoBondPredOnly_Tip");

        }

        private bool heightStale = true;

        private float height;

        private Vector2 scrollPosition;

        public override void Reset()
        {
            this.sizedEncumberance = null;
            this.encumberanceModifier = null;
            this.endoSicknessStrenght = null;
            this.regressionStrenght = null;
            this.endoCapture = null;
            this.insectoidCapture = null;
            this.playVoreChance = null;
            this.fatalPlayVore = null;
            this.endoBondChance = null;
            this.preyOnlyEndoBond = null;

            EnsureSmartSettingDefinition();
        }
        public void FillRect(Rect inRect)
        {
            Listing_Standard listing_Standard;
            UIUtility.MakeAndBeginScrollView(inRect, this.height, ref this.scrollPosition, out listing_Standard);
            if (listing_Standard.ButtonText("RV2_Settings_Reset".Translate(), null))
                this.Reset();
            
            listing_Standard.Gap(12f);
            /*
            this.sizedEncumberance.DoSetting(listing_Standard);
            this.encumberanceModifier.DoSetting(listing_Standard);
            listing_Standard.Gap(12f);
            this.endoSicknessStrenght.DoSetting(listing_Standard);
            */
            this.regressionStrenght.DoSetting(listing_Standard);
            listing_Standard.Gap(12f);
            this.endoCapture.DoSetting(listing_Standard);
            this.insectoidCapture.DoSetting(listing_Standard);
            listing_Standard.Gap(12f);
            this.playVoreChance.DoSetting(listing_Standard);
            this.fatalPlayVore.DoSetting(listing_Standard);
            listing_Standard.Gap(12f);
            this.endoBondChance.DoSetting(listing_Standard);
            this.preyOnlyEndoBond.DoSetting(listing_Standard);
            listing_Standard.EndScrollView(ref this.height, ref this.heightStale);
        }
        public override void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving || Scribe.mode == LoadSaveMode.LoadingVars)
                this.EnsureSmartSettingDefinition();
            /*
            Scribe_Deep.Look<BoolSmartSetting>(ref this.sizedEncumberance, "sizedEncumberance", new object[0]);
            Scribe_Deep.Look<FloatSmartSetting>(ref this.encumberanceModifier, "encumberanceModifier", new object[0]);
            Scribe_Deep.Look<FloatSmartSetting>(ref this.endoSicknessStrenght, "endoSicknessStrenght", new object[0]);*/
            Scribe_Deep.Look<FloatSmartSetting>(ref this.regressionStrenght, "regressionStrenght", new object[0]);
            Scribe_Deep.Look<BoolSmartSetting>(ref this.endoCapture, "endoCapture", new object[0]);
            Scribe_Deep.Look<BoolSmartSetting>(ref this.insectoidCapture, "insectoidCapture", new object[0]);
            Scribe_Deep.Look<FloatSmartSetting>(ref this.playVoreChance, "playVoreChance", new object[0]);
            Scribe_Deep.Look<BoolSmartSetting>(ref this.fatalPlayVore, "fatalPlayVore", new object[0]);
            Scribe_Deep.Look<FloatSmartSetting>(ref this.endoBondChance, "endoBondChance", new object[0]);
            Scribe_Deep.Look<BoolSmartSetting>(ref this.preyOnlyEndoBond, "preyOnlyEndoBond", new object[0]);
            this.PostExposeData();
        }
    }
}
