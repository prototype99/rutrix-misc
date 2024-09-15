using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimVore2;
using RimWorld;

namespace DefOfs
{
    [DefOf]
    public static class VoreStageDefOfs
    {
        //Only was getting the stomach ones from testing, manually added the others
        //Stomach_Warmup, Stomach_Digest, Stomach_Churn, Stomach_Pleasure, Stomach_Store, Stomach_Hold, Stomach_Heal, Stomach_Warmup_Fast, Stomach_Drain, Stomach_SoftChurn
        //Womb_Warmup Womb_DissolveLube Womb_MushLube
        //Intestines_SoftProcess 
        public static VoreStageDef Stomach_Warmup;
        public static VoreStageDef Stomach_Digest;
        public static VoreStageDef Stomach_Churn;
        public static VoreStageDef Stomach_Pleasure;
        public static VoreStageDef Stomach_Store;
        public static VoreStageDef Stomach_Hold;
        public static VoreStageDef Stomach_Heal;
        public static VoreStageDef Stomach_Warmup_Fast;
        public static VoreStageDef Stomach_Drain;
        public static VoreStageDef Stomach_SoftChurn;

        public static VoreStageDef Womb_Warmup;
        public static VoreStageDef Womb_DissolveLube;
        public static VoreStageDef Womb_MushLube;
        public static VoreStageDef Womb_Store;
        public static VoreStageDef Womb_Hold;
        public static VoreStageDef Womb_Pleasure;
        public static VoreStageDef Womb_ConvertLube;
        public static VoreStageDef Womb_Heal;
        
        public static VoreStageDef Intestines_SoftProcess;
        public static VoreStageDef Intestines_Pleasure;
        public static VoreStageDef Intestines_Process;
        public static VoreStageDef Intestines_Store;
        public static VoreStageDef Intestines_Hold;
    }
}
