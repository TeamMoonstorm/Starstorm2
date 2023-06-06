using EntityStates;
using HG;
using Moonstorm.Components;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Buffs
{
    [DisabledContent]
    public sealed class AffixKinetic : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdEliteKinetic", SS2Bundle.Indev);

        public sealed class Behavior : BaseBuffBodyBehavior
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => SS2Content.Buffs.bdEliteKinetic;
            public static GameObject pulsePrefab = SS2Assets.LoadAsset<GameObject>("VoidElitePulse", SS2Bundle.Indev);
            public static GameObject auraPrefab = SS2Assets.LoadAsset<GameObject>("KineticAOEIndicator", SS2Bundle.Indev);
            GameObject tempAuraPrefab;
            //Prefab info:
            //Final radius: 10, Duration 1.0
            //Destroy On Timer: 3 Seconds
            //Has Shaker Emiter
            private bool alreadyIn = false;
            private bool hasMadeAura = false;
            private float timer = 0;
            private SphereSearch sphereSearch;

            private void FixedUpdate()
            {
                if (!NetworkServer.active)
                {
                    return;
                }
                if (!body.outOfCombat && timer > 4f)
                {
                    PullEnemies();
                    timer = 0;
                    //GeneratePulse();
                    //alreadyIn = true;
                }
                //else if (body.outOfCombat && alreadyIn)
                //{
                //    alreadyIn = false;
                //}
                //if (!hasMadeAura)
                //{
                //    tempAuraPrefab = UnityEngine.Object.Instantiate<GameObject>(auraPrefab, body.corePosition, Quaternion.identity);
                //    tempAuraPrefab.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(body.gameObject, null);
                //    hasMadeAura = true;
                //}else if(body.equipmentSlot.equipmentIndex != SS2Content.Equipments.EliteKineticEquipment.equipmentIndex)
                //{
                //    Destroy(tempAuraPrefab);
                //}
                timer += Time.fixedDeltaTime;
            }

            private void OnEnable()
            {
                SS2Log.Info("ahhh");
                tempAuraPrefab = UnityEngine.Object.Instantiate<GameObject>(auraPrefab, body.corePosition, Quaternion.identity);
                tempAuraPrefab.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(body.gameObject, null);
                //hasMadeAura = true;
            }

            private void OnDisable()
            {
                SS2Log.Info("oooohohohohohohh");
                if (tempAuraPrefab)
                {
                    SS2Log.Info("yay");
                    Destroy(tempAuraPrefab);
                }
            }

            private void PullEnemies()
            {
                float aoeRadius = 20;// + (aoeRangeStacking.Value * (float)(cryoCount - 1));
                float bodyRadius = body.radius;
                float effectiveRadius = aoeRadius;// + (bodyRadius * .5f);
                //float AOEDamageMult = debuffDamage;// + (stackingDamageAOE.Value * (float)(cryoCount - 1));
                //float AOEDamage = damageReport.attackerBody.damage * AOEDamageMult;
                //
                //float duration = debuffDuration; // + (slowDurationStacking.Value * (cryoCount - 1));

                var bodyTeam = body.teamComponent.teamIndex;

                //float num = 8f + 4f * (float)cryoCount;
                //float radius = victimBody.radius;
                //float num2 = num + radius;
                //float num3 = 1.5f;
                //float baseDamage = obj.attackerBody.damage * num3;
                //float value = (float)(1 + cryoCount) * 0.75f * obj.attackerBody.damage;

                Vector3 corePosition = body.corePosition;

                SphereSearch kineticAOESphereSearch = new SphereSearch();
                List<HurtBox> kineticAOEHurtBoxBuffer = new List<HurtBox>();

                kineticAOESphereSearch.origin = corePosition;
                kineticAOESphereSearch.mask = LayerIndex.entityPrecise.mask;
                kineticAOESphereSearch.radius = effectiveRadius;
                kineticAOESphereSearch.RefreshCandidates();
                kineticAOESphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(bodyTeam));
                kineticAOESphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
                kineticAOESphereSearch.OrderCandidatesByDistance();
                kineticAOESphereSearch.GetHurtBoxes(kineticAOEHurtBoxBuffer);
                kineticAOESphereSearch.ClearCandidates();

                for (int i = 0; i < kineticAOEHurtBoxBuffer.Count; i++)
                {
                    HurtBox hurtBox = kineticAOEHurtBoxBuffer[i];
                    //Debug.Log("hurtbox " + hurtBox);
                    if (hurtBox.healthComponent)
                    {
                        var victim = hurtBox.healthComponent.body;
                        if (victim)
                        {

                            float dashVelocity = 21f;
                            float shorthopVelocity = .5f;

                            //Quaternion quat = Quaternion.Euler(dir.x, dir.y, dir.z);
                            //float num = body.acceleration * body.characterMotor.airControl;
                            //float num2 = Mathf.Sqrt(dashVelocity / num);
                            //float num3 = body.moveSpeed / num;
                            //float horizontalBonus = (num2 + num3) / num3;
                            //Vector3 direction = new Vector3( )
                            Vector3 v1 = body.transform.position;
                            Vector3 v2 = victim.transform.position;

                            //GenericCharacterMain.ApplyJumpVelocity(body.characterMotor, body, horizontalBonus, shorthopVelocity, false);

                            Vector3 vector = v1 - v2;
                            Vector3.Normalize(vector);

                            string effectName = "RoR2/DLC1/MoveSpeedOnKill/MoveSpeedOnKillActivate.prefab";
                            GameObject effectPrefab = Addressables.LoadAssetAsync<GameObject>(effectName).WaitForCompletion();
                            EffectManager.SimpleImpactEffect(effectPrefab, v2, vector, true);
                            //if (vault)
                            //{
                            //    characterMotor.velocity = vector;
                            //}
                            //else
                            //{
                            //vector.y = 0f;
                            //float magnitude = vector.magnitude;
                            //if (magnitude > 0f)
                            //{
                            //    vector /= magnitude;
                            //}
                            float randMult = Random.Range(1.2f, 1.6f);
                            Vector3 velocity = vector * randMult; // * 1.5f;
                            velocity.y += .75f;
                            var motor = victim.characterMotor;
                            if (motor)
                            {
                                motor.velocity += velocity;
                                //}
                                motor.Motor.ForceUnground();
                            }


                            //hurtBox.healthComponent.body.AddTimedBuffAuthority(Buffs.Bane.index, duration);
                            //var dotInfo = new InflictDotInfo()
                            //{
                            //    attackerObject = body.gameObject,
                            //    victimObject = hurtBox.healthComponent.gameObject,
                            //    dotIndex = Buffs.Bane.index,
                            //    duration = damageReport.damageInfo.procCoefficient * duration,
                            //    damageMultiplier = stack
                            //};
                            //DotController.InflictDot(ref dotInfo);
                        }

                    }
                }
                kineticAOEHurtBoxBuffer.Clear();

                //new BlastAttack
                //{
                //    radius = effectiveRadius,
                //    baseDamage = AOEDamage,
                //    procCoefficient = 0f,
                //    crit = Util.CheckRoll(damageReport.attackerBody.crit, damageReport.attackerMaster),
                //    damageColorIndex = DamageColorIndex.Item,
                //    attackerFiltering = AttackerFiltering.Default,
                //    falloffModel = BlastAttack.FalloffModel.None,
                //    attacker = damageReport.attacker,
                //    teamIndex = attackerTeamIndex,
                //    position = corePosition,
                //    //baseForce = 0,
                //    //damageType = DamageType.AOE
                //}.Fire();

                //EntityStates.Mage.Weapon.IceNova.impactEffectPrefab

                //EffectManager.SpawnEffect(iceDeathAOEObject, new EffectData
                //{
                //    origin = corePosition,
                //    scale = effectiveRadius,
                //    rotation = Util.QuaternionSafeLookRotation(obj.damageInfo.force)
                //}, true);
            }
        

            private void GeneratePulse()
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(pulsePrefab, body.transform.position, body.transform.rotation);
                PulseController component = gameObject.GetComponent<PulseController>();
                sphereSearch = new RoR2.SphereSearch()
                {
                    queryTriggerInteraction = UnityEngine.QueryTriggerInteraction.Collide,
                    mask = LayerIndex.entityPrecise.mask,
                    origin = body.transform.position,
                };

                component.finalRadius += buffStacks;
                component.duration += (buffStacks * 0.1f); //Artificially increase duration because in higher radius its almost instant
                component.performSearch += PerformSearch;
                component.onPulseHit += OnPulseHit;
                component.StartPulseServer();
                NetworkServer.Spawn(gameObject);
            }

            private void PerformSearch(PulseController pulseController, Vector3 origin, float radius, List<PulseController.PulseSearchResult> dest)
            {
                sphereSearch.origin = origin;
                sphereSearch.radius = radius;
                if (sphereSearch.radius <= 0)
                {
                    return;
                }
                List<HurtBox> hurtBoxes = CollectionPool<HurtBox, List<HurtBox>>.RentCollection();
                sphereSearch.RefreshCandidates().FilterCandidatesByDistinctHurtBoxEntities().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(body.teamComponent.teamIndex)).GetHurtBoxes(hurtBoxes);
                for (int i = 0; i < hurtBoxes.Count; i++)
                {
                    if (hurtBoxes[i].healthComponent)
                    {
                        RoR2.PulseController.PulseSearchResult pulseSearchResult = new PulseController.PulseSearchResult();
                        pulseSearchResult.hitObject = hurtBoxes[i].healthComponent;
                        pulseSearchResult.hitPos = hurtBoxes[i].healthComponent.body.transform.position;
                        dest.Add(pulseSearchResult);
                    }
                }
                CollectionPool<HurtBox, List<HurtBox>>.ReturnCollection(hurtBoxes);
            }

            private void OnPulseHit(PulseController pulseController, PulseController.PulseHit hitInfo)
            {
                if (hitInfo.hitObject)
                {
                    HealthComponent hc = (HealthComponent)hitInfo.hitObject;
                    if (TeamManager.IsTeamEnemy(hc.body.teamComponent.teamIndex, body.teamComponent.teamIndex))
                    {
                        /*if (BuffCatalog.GetBuffDef(BuffCatalog.FindBuffIndex("BuffFear"))) //Lazy to check for SS2 installation, check if catalog has fear in
                        {
                            hc.body.AddTimedBuff(BuffCatalog.GetBuffDef(BuffCatalog.FindBuffIndex("BuffFear")), (4 + buffStacks) * hitInfo.hitSeverity);
                            return;
                        }*/
                        hc.body.AddTimedBuff(SS2Content.Buffs.BuffVoidLeech, (4 + buffStacks) * hitInfo.hitSeverity);
                        return;
                    }
                }
            }
        }
    }
}
