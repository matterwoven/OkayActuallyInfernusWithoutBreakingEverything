using EntityStates;
using InfernusMod.Survivors.Infernus;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using R2API;
using static UnityEngine.SendMouseEvents;

namespace InfernusMod.Survivors.Infernus.SkillStates
{
    public class Shoot : BaseSkillState
    {
        public static float damageCoefficient = InfernusStaticValues.gunDamageCoefficient;
        public static float procCoefficient = 0.6f;
        public static float baseDuration = 0.2f;
        //delay on firing is usually ass-feeling. only set this if you know what you're doing
        public static float firePercentTime = 0.0f;
        public static float force = 200f;
        public static float recoil = 0.5f;
        public static float range = 256f;
        public static GameObject tracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerGoldGat");

        private float duration;
        private float fireTime;
        private bool hasFired;
        private string muzzleString;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            fireTime = firePercentTime * duration;
            characterBody.SetAimTimer(2f);
            muzzleString = "Muzzle";

            PlayAnimation("LeftArm, Override", "ShootGun", "ShootGun.playbackRate", 1.8f);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= fireTime)
            {
                Fire();
            }

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        private void Fire()
        {
            if (!hasFired)
            {
                hasFired = true;

                characterBody.AddSpreadBloom(1.5f);
                EffectManager.SimpleMuzzleFlash(EntityStates.Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, muzzleString, false);
                Util.PlaySound("InfernusShootPistol", gameObject);

                if (isAuthority)
                {
                    Ray aimRay = GetAimRay();
                    AddRecoil(-1f * recoil, -2f * recoil, -0.5f * recoil, 0.5f * recoil);

                    new BulletAttack
                    {
                        bulletCount = 1,
                        aimVector = aimRay.direction,
                        origin = aimRay.origin,
                        damage = damageCoefficient * damageStat,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageTypeCombo.GenericPrimary,
                        falloffModel = BulletAttack.FalloffModel.None,
                        maxDistance = range,
                        force = force,
                        hitMask = LayerIndex.CommonMasks.bullet,
                        minSpread = 0f,
                        maxSpread = 0f,
                        isCrit = RollCrit(),
                        owner = gameObject,
                        muzzleName = muzzleString,
                        smartCollision = true,
                        procChainMask = default,
                        procCoefficient = procCoefficient,
                        radius = 0.75f,
                        sniper = false,
                        stopperMask = LayerIndex.CommonMasks.bullet,
                        weapon = null,
                        tracerEffectPrefab = tracerEffectPrefab,
                        spreadPitchScale = 1f,
                        spreadYawScale = 1f,
                        queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                        hitEffectPrefab = EntityStates.Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,

                        //Apply afterburn stack on hit
                        hitCallback = ApplyAfterburnOnHit()
                    }.Fire();
                    
                }
            }
        }

        private BulletAttack.HitCallback ApplyAfterburnOnHit()
        {
            return (BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo) =>
            {
                bool returnValue = BulletAttack.DefaultHitCallbackImplementation(bulletAttack, ref hitInfo);
                bool isCrit = bulletAttack.isCrit;

                //Victim data
                HealthComponent victimHealthComponent = hitInfo.hitHurtBox.healthComponent;
                CharacterBody victimCharacterBody = victimHealthComponent.body;
                GameObject victimGameObject = victimHealthComponent.gameObject;
                HurtBox victimHurtBox = hitInfo.hitHurtBox;

                //Victim dotController
                DotController dotController = DotController.FindDotController(victimGameObject);

                applyBuildup();

                int buildupCount = buildupCountStacks(dotController);


                if (buildupCount >= 5)
                {
                    clearBuildup(dotController, victimCharacterBody);
                    applyAfterburn();
                }

                void applyBuildup()
                {
                    //for (int i = 0; i < appliedStacks; i++)
                    //{
                        DotController.InflictDot(
                            victimGameObject,
                            gameObject,
                            victimHurtBox,
                            InfernusDebuffs.afterburnBuildupIndex,
                            10f,
                            1f
                        );
                    //}
                }

                void applyAfterburn()
                {
                    // Apply real afterburn
                    DotController.InflictDot(
                        victimGameObject,
                        gameObject, //Attacker (us) gameobject
                        victimHurtBox,
                        InfernusDebuffs.afterburnDebuffIndex,
                        10f,
                        1f
                    );
                }


                int buildupCountStacks(DotController controllerForTarget)
                {
                    int buildupsCounted = 0;
                    var dotStacks = controllerForTarget.dotStackList;

                    // 2️⃣ Count buildup stacks
                    for (int i = 0; i < dotStacks.Count; i++)
                    {
                        if (dotStacks[i].dotIndex == InfernusDebuffs.afterburnBuildupIndex)
                        {
                            buildupsCounted++;
                        }
                    }

                    return buildupsCounted;
                }

                void clearBuildup(DotController controller, CharacterBody body)
                {
                    // Remove buildup dot stacks manually
                    for (int i = controller.dotStackList.Count - 1; i >= 0; i--)
                    {
                        if (controller.dotStackList[i].dotIndex == InfernusDebuffs.afterburnBuildupIndex)
                        {
                            controller.dotStackList.RemoveAt(i);
                        }
                    }

                    // Clear associated buff visuals
                    body.ClearTimedBuffs(InfernusDebuffs.afterburnBuildup);
                }


                return returnValue;
            };
        }
            
            

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}