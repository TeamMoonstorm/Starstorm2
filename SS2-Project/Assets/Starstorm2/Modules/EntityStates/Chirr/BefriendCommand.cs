using Moonstorm.Starstorm2.Components;
using Moonstorm.Starstorm2.Orbs;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Navigation;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Chirr
{
    public class BefriendCommand : BaseSkillState
    {
        // TODO: Tap = call ally to you, Hold = remove ally as friend.

        public static float baseDuration;
        public static float tapMaxmimum;
        public static float minDistanceAir;
        public static float minDistanceGround;
        public static float secondaryEffectFloat;
        public static float minDistanceSiphon;
        public static float siphonRate;
        public static float siphonPercent;
        public static GameObject siphonOrbPrefab;

        private float duration;
        private float siphonTimer;
        private ChirrNetworkInfo chirrInfo;
        private SecondaryEffect secondaryEffect;

        public enum SecondaryEffect
        {
            remove,
            siphon,
            share
        }

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration;
            chirrInfo = characterBody.GetComponent<ChirrNetworkInfo>();
            secondaryEffect = (SecondaryEffect)secondaryEffectFloat;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            float charge = CalcCharge();
            if (base.isAuthority && !base.IsKeyDownAuthority())
            {
                if (charge <= tapMaxmimum)
                {
                    TapEffect();
                    outer.SetNextStateToMain();
                }
                else
                {
                    switch (secondaryEffect)
                    {
                        case SecondaryEffect.remove:
                            HoldEffectRemove();
                            outer.SetNextStateToMain();
                            break;
                        case SecondaryEffect.siphon:
                            outer.SetNextStateToMain();
                            break;
                        case SecondaryEffect.share:
                            outer.SetNextStateToMain();
                            break;
                    }
                }
            }
            if (base.isAuthority && base.IsKeyDownAuthority() && charge >= tapMaxmimum)
            {
                switch (secondaryEffect)
                {
                    case SecondaryEffect.remove:
                        break;
                    case SecondaryEffect.siphon:
                        HoldEffectSiphon();
                        break;
                    case SecondaryEffect.share:
                        HoldEffectShare();
                        break;
                }
            }
        }

        private float CalcCharge()
        {
            return Mathf.Clamp01(base.fixedAge / duration);
        }

        private void TapEffect()
        {
            if (!chirrInfo.friend || !NetworkServer.active) return;
            float minDistance;
            MapNodeGroup.GraphType graphType;
            if (chirrInfo.friend.gameObject.GetComponent<VectorPID>())
            {
                minDistance = minDistanceAir;
                graphType = MapNodeGroup.GraphType.Air;
            }
            else
            {
                minDistance = minDistanceGround;
                graphType = MapNodeGroup.GraphType.Ground;
            }
            TeleportFriend(chirrInfo.friend, minDistance, graphType);
        }

        private void TeleportFriend(CharacterBody friendBody, float minDistance, MapNodeGroup.GraphType graphType)
        {
            if (!FriendDistanceCheck(minDistance))
            {
                if (TeleportBody(friendBody, characterBody.corePosition, graphType)) return;
                else
                {
                    base.skillLocator.special.AddOneStock();
                }
            }
        }

        private bool TeleportBody(CharacterBody friendBody, Vector3 desiredPosition, MapNodeGroup.GraphType graphType)
        {
            SpawnCard spawnCard = ScriptableObject.CreateInstance<SpawnCard>();
            spawnCard.hullSize = friendBody.hullClassification;
            spawnCard.nodeGraphType = graphType;
            spawnCard.prefab = Resources.Load<GameObject>("SpawnCards/HelperPrefab");
            GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, new DirectorPlacementRule
            {
                placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                position = desiredPosition,
                minDistance = 5,
                maxDistance = 15
            }, RoR2Application.rng));
            if (gameObject)
            {
                TeleportHelper.TeleportBody(friendBody, gameObject.transform.position);
                GameObject teleportPrefab = Run.instance.GetTeleportEffectPrefab(friendBody.gameObject);
                if (teleportPrefab)
                {
                    EffectManager.SimpleEffect(teleportPrefab, gameObject.transform.position, Quaternion.identity, true);
                }
                Destroy(gameObject);
                Destroy(spawnCard);
                return true;
            }
            else
            {
                Destroy(spawnCard);
                return false;
            }
        }

        // Remove ally
        private void HoldEffectRemove()
        {
            if (!chirrInfo.friend) return;
            chirrInfo.friend.teamComponent.teamIndex = chirrInfo.friendOrigIndex;
            chirrInfo.friend.master.teamIndex = chirrInfo.friendOrigIndex;
            BaseAI friendAI = chirrInfo.friend.master.GetComponent<BaseAI>();
            if (friendAI)
            {
                friendAI.currentEnemy.Reset();
                friendAI.UpdateTargets();
                friendAI.leader.gameObject = null;
                friendAI.master.inventory.GetComponent<MinionOwnership>().SetOwner(null);
                //friendAI.master.inventory.
                ArenaMissionController arenaController = ArenaMissionController.instance;
                if (arenaController)
                {
                    ArenaMissionController missionController = arenaController.GetComponent<ArenaMissionController>();
                    if (missionController)
                    {
                        friendAI.master.inventory = new Inventory();
                        friendAI.master.inventory.CopyItemsFrom(missionController.inventory);
                    }
                }
            }
            chirrInfo.friend = null;
        }

        // Siphon ally health
        private void HoldEffectSiphon()
        {
            if (!chirrInfo.friend) return;
            siphonTimer += Time.fixedDeltaTime;
            if (siphonTimer >= siphonRate && FriendDistanceCheck(minDistanceSiphon))
            {
                SiphonAlly();
                siphonTimer = 0;
            }
        }

        private void SiphonAlly()
        {
            if (!chirrInfo.friend || !NetworkServer.active) return;
            HealthComponent hc = chirrInfo.friend.healthComponent;
            if (hc)
            {
                if (siphonOrbPrefab)
                {
                    ChirrSiphonOrb siphonOrb = new ChirrSiphonOrb();
                    siphonOrb.siphonBody = chirrInfo.friend;
                    siphonOrb.playerBody = characterBody;
                    siphonOrb.orbEffect = siphonOrbPrefab;
                    siphonOrb.overrideDuration = 0.2f;
                    siphonOrb.siphonPercent = siphonPercent;
                    OrbManager.instance.AddOrb(siphonOrb);
                }
                else
                {
                    DamageInfo damageInfo = new DamageInfo();
                    damageInfo.damage = hc.fullHealth * siphonPercent;
                    damageInfo.position = chirrInfo.friend.corePosition;
                    damageInfo.procCoefficient = 0f;
                    damageInfo.damageType = DamageType.BypassArmor;
                    damageInfo.damageColorIndex = DamageColorIndex.Poison;
                    hc.TakeDamage(damageInfo);

                    healthComponent.HealFraction(siphonPercent, default(ProcChainMask));
                }

                if (!chirrInfo.friend.healthComponent.alive)
                {
                    outer.SetNextStateToMain();
                }
            }
        }

        // Share damage with ally
        private void HoldEffectShare()
        {
            if (!chirrInfo.friend) return;

        }

        private bool FriendDistanceCheck(float maxDistance)
        {
            float distance = Vector3.Distance(chirrInfo.friend.corePosition, characterBody.corePosition);

            if (distance <= maxDistance)
                return true;
            else return false;
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
