using HG;
using MSU;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace SS2.Equipments
{
#if DEBUG

    public sealed class AffixKinetic : SS2EliteEquipment
    {
        //public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("EliteKineticEquipment", SS2Bundle.Indev);

        //public override List<MSEliteDef> EliteDefs => new List<MSEliteDef>
        //{
        //    SS2Assets.LoadAsset<MSEliteDef>("edKinetic", SS2Bundle.Indev),
        //    SS2Assets.LoadAsset<MSEliteDef>("edKineticHonor", SS2Bundle.Indev)
        //};

        //public override bool FireAction(EquipmentSlot slot)
        //{
        //    return false;
        //}
        public override List<EliteDef> EliteDefs => _eliteDefs;
        private List<EliteDef> _eliteDefs;

        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;

        public override EquipmentDef EquipmentDef => _equipmentDef;
        private EquipmentDef _equipmentDef;

        public BuffDef _buffKinetic; //{ get; } = SS2Assets.LoadAsset<BuffDef>("bdEliteKinetic", SS2Bundle.Indev);


        public override bool Execute(EquipmentSlot slot)
        {
            return false;
        }

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override IEnumerator LoadContentAsync()
        {
            // TODO: MSU 2.0 Load content!
            yield break;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        public sealed class KineticBuffBehaviour : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => SS2Content.Buffs.bdEliteKinetic;
            public static GameObject pulsePrefab = SS2Assets.LoadAsset<GameObject>("VoidElitePulse", SS2Bundle.Indev);
            public static GameObject auraPrefab = SS2Assets.LoadAsset<GameObject>("KineticAOEIndicator", SS2Bundle.Indev);
            GameObject tempAuraPrefab;
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

                if (!CharacterBody.outOfCombat && timer > 4f)
                {
                    PullEnemies();
                    timer = 0;
                }
                timer += Time.fixedDeltaTime;
            }

            protected override void OnFirstStackGained()
            {
                // TODO from Buns: Remember to remove log after implementation is done Zen :)
                SS2Log.Info("ahhh");
                tempAuraPrefab = UnityEngine.Object.Instantiate<GameObject>(auraPrefab, CharacterBody.corePosition, Quaternion.identity);
                tempAuraPrefab.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(CharacterBody.gameObject, null);
            }

            protected override void OnAllStacksLost()
            {
                // TODO from Buns: Remember to remove log after implementation is done Zen :)
                SS2Log.Info("oooohohohohohohh");
                if (tempAuraPrefab)
                {
                    // TODO from Buns: Remember to remove log after implementation is done Zen :)
                    SS2Log.Info("yay");
                    Destroy(tempAuraPrefab);
                }
            }

            private void PullEnemies()
            {
                float aoeRadius = 20;// + (aoeRangeStacking.Value * (float)(cryoCount - 1));
                float bodyRadius = CharacterBody.radius;
                float effectiveRadius = aoeRadius;// + (bodyRadius * .5f);

                var bodyTeam = CharacterBody.teamComponent.teamIndex;

                Vector3 corePosition = CharacterBody.corePosition;

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

                            Vector3 v1 = CharacterBody.transform.position;
                            Vector3 v2 = victim.transform.position;

                            Vector3 vector = v1 - v2;
                            Vector3.Normalize(vector);

                            string effectName = "RoR2/DLC1/MoveSpeedOnKill/MoveSpeedOnKillActivate.prefab";
                            GameObject effectPrefab = Addressables.LoadAssetAsync<GameObject>(effectName).WaitForCompletion();
                            EffectManager.SimpleImpactEffect(effectPrefab, v2, vector, true);

                            float randMult = Random.Range(1.2f, 1.6f);
                            Vector3 velocity = vector * randMult; // * 1.5f;
                            velocity.y += .75f;
                            var motor = victim.characterMotor;
                            if (motor)
                            {
                                motor.velocity += velocity;
                                motor.Motor.ForceUnground();
                            }
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
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(pulsePrefab, CharacterBody.transform.position, CharacterBody.transform.rotation);
                PulseController component = gameObject.GetComponent<PulseController>();
                sphereSearch = new RoR2.SphereSearch()
                {
                    queryTriggerInteraction = UnityEngine.QueryTriggerInteraction.Collide,
                    mask = LayerIndex.entityPrecise.mask,
                    origin = CharacterBody.transform.position,
                };

                component.finalRadius += BuffCount;
                component.duration += (BuffCount * 0.1f); //Artificially increase duration because in higher radius its almost instant
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
                sphereSearch.RefreshCandidates().FilterCandidatesByDistinctHurtBoxEntities().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(CharacterBody.teamComponent.teamIndex)).GetHurtBoxes(hurtBoxes);
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
                    if (TeamManager.IsTeamEnemy(hc.body.teamComponent.teamIndex, CharacterBody.teamComponent.teamIndex))
                    {
                        // TODO: THis was commented out before, idk if it needs to be kept
                        if (BuffCatalog.GetBuffDef(BuffCatalog.FindBuffIndex("BuffFear"))) //Lazy to check for SS2 installation, check if catalog has fear in
                        {
                            hc.body.AddTimedBuff(BuffCatalog.GetBuffDef(BuffCatalog.FindBuffIndex("BuffFear")), (4 + BuffCount) * hitInfo.hitSeverity);
                            return;
                        }
                        hc.body.AddTimedBuff(SS2Content.Buffs.BuffVoidLeech, (4 + BuffCount) * hitInfo.hitSeverity);
                        return;
                    }
                }
            }
        }
    }
#endif
}
