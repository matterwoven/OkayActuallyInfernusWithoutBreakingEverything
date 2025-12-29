using InfernusMod.Survivors.Infernus;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
using RoR2;
using HG;

namespace InfernusMod.Characters.Survivors.Infernus.Content
{
    public class FlameZoneController : NetworkBehaviour
    {
        public float lifetime = 4f;
        public float radius = 4f;
        public float damagePerSecond = InfernusStaticValues.dashDamageCoefficient;
        public float tickInterval = 0.5f;

        public GameObject owner;

        private float tickStopwatch;

        private static readonly Collider[] overlapResults = new Collider[64];

        private readonly HashSet<HurtBox> victims = new HashSet<HurtBox>();

        private TeamIndex ownerTeam = TeamIndex.None;
        private CharacterBody ownerBody;

        private void Start()
        {
            if (NetworkServer.active)
            {
                Destroy(gameObject, lifetime);
            }
            if (owner)
            {
                ownerBody = owner.GetComponent<CharacterBody>();
                if (ownerBody)
                {
                    ownerTeam = ownerBody.teamComponent.teamIndex;
                }
            }
        }

        private void FixedUpdate()
        {
            if (!NetworkServer.active) return;


            tickStopwatch += Time.fixedDeltaTime;
            if (tickStopwatch < tickInterval) return;

            tickStopwatch = 0f;

            int count = Physics.OverlapSphereNonAlloc(
                transform.position,
                radius,
                overlapResults,
                LayerIndex.entityPrecise.mask
            );

            for (int i = 0; i < count; i++)
            {
                Collider col = overlapResults[i];
                if (!col) continue;

                HurtBox hurtBox = col.GetComponent<HurtBox>();
                if (!hurtBox || !hurtBox.healthComponent) continue;

                if (owner == hurtBox.healthComponent) return;

                HealthComponent hc = hurtBox.healthComponent;
                if (!hc.body) continue;

                //Doesn't hurt self
                CharacterBody victimBody = hc.body;
                if (ownerBody && victimBody == ownerBody)
                    continue;

                //Doesn't hurt allies
                if (victimBody.teamComponent &&
                    victimBody.teamComponent.teamIndex == ownerTeam)
                    continue;

                DamageInfo damageInfo = new DamageInfo
                {
                    attacker = owner,
                    inflictor = gameObject,
                    damage = damagePerSecond,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Generic,
                    crit = ownerBody && ownerBody.RollCrit(),
                    position = hurtBox.transform.position,
                    force = Vector3.zero,
                    procCoefficient = InfernusStaticValues.dashDamageCoefficient
                };

                hc.TakeDamage(damageInfo);
            }

            //foreach (HurtBox hurtBox in victims)
            //{
                //if (!hurtBox) continue;

                //HealthComponent hc = hurtBox.healthComponent;
                //if (!hc || !hc.body) continue;

                //DotController.InflictDot(
                    //hc.gameObject,                     // victim
                    //owner,                             // attacker
                    //hurtBox,                           // hurtbox of victim
                    //InfernusDebuffs.afterburnDebuffIndex,
                    //1f,                                // refresh duration
                    //1f                                 // stack
                //);
            //}
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!NetworkServer.active) return;

            HurtBox hurtBox = other.GetComponent<HurtBox>();
            if (!hurtBox || !hurtBox.healthComponent) return;

            victims.Add(hurtBox);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!NetworkServer.active) return;

            HurtBox hurtBox = other.GetComponent<HurtBox>();
            if (!hurtBox) return;

            victims.Remove(hurtBox);
        }
    }
}