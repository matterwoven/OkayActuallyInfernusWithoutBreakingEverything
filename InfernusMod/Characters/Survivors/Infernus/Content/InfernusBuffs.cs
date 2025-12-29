using RoR2;
using UnityEngine;
using R2API;

namespace InfernusMod.Survivors.Infernus
{


    public static class InfernusBuffs
    {
        // armor buff gained during roll
        public static BuffDef speedBuff;

        public static void Init(AssetBundle assetBundle)
        {
            speedBuff = Modules.Content.CreateAndAddBuff("InfernusSpeedBuff",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/WhipBoost").iconSprite,
                Color.white,
                false,
                false);

        }
    }

    public static class InfernusDebuffs
    {
        public static BuffDef afterburnDebuff;
        public static BuffDef afterburnBuildup;
        public static BuffDef napalmDebuff;

        public static DotController.DotDef afterburnBuildupDotDef;
        public static DotController.DotDef afterburnDotDef;
        public static DotController.DotIndex afterburnDebuffIndex;
        public static DotController.DotIndex afterburnBuildupIndex;

        public static void Init(AssetBundle assetBundle)
        {

            afterburnDebuff = Modules.Content.CreateAndAddBuff(
                "InfernusAfterburn",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/OnFire").iconSprite,
                Color.red,
                false,
                true
            );

            afterburnBuildup = Modules.Content.CreateAndAddBuff(
                "InfernusBuildup",
                LegacyResourcesAPI.Load<BuffDef>("BuffDefs/OnFire").iconSprite,
                Color.white,
                true,
                true
            );

            napalmDebuff = Modules.Content.CreateAndAddBuff(
                "NapalmDebuff",
                assetBundle.LoadAsset<Sprite>("texNapalmColored"),
                Color.yellow,
                true,
                true
            );

            DotController.DotDef afterburnDot = new DotController.DotDef
            {
                associatedBuff = afterburnDebuff,
                damageCoefficient = InfernusStaticValues.afterburnDamageCoefficient,
                interval = 0.5f,
                damageColorIndex = DamageColorIndex.Void,
                resetTimerOnAdd = true
            };

            DotController.DotDef emptyDmg = new DotController.DotDef
            {
                associatedBuff = afterburnBuildup,
                damageCoefficient = 0f,
                interval = 0.1f,
                damageColorIndex = DamageColorIndex.Default,
                resetTimerOnAdd = true,
            };

            //Way to add afterburn using these definitions
            afterburnDebuffIndex = DotAPI.RegisterDotDef(afterburnDot);
            afterburnBuildupIndex = DotAPI.RegisterDotDef(emptyDmg);

            afterburnBuildupDotDef = emptyDmg;
            afterburnDotDef = afterburnDot;

            DotController.DotStack afterburnBuildupDot = new DotController.DotStack
            {
                dotIndex = afterburnBuildupIndex,
                totalDuration = 8f
            };

        }
    }
}
