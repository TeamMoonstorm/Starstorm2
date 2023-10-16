//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using RoR2;
//using UnityEngine.Networking;
//using Moonstorm.Starstorm2.Components;
//using RoR2.Navigation;
//namespace EntityStates.NemMerc
//{
//    public class Scan : BaseSkillState
//    {
//        public static float scanRadius = 75f;
//        public static float randomHologramRadius = 40f;
//        public static float scanDuration = 1f;
//        public static float hologramDuration = 10f;

//        public static float targetedHologramOffset = 8f;

//        public static int maxTotalHolograms = 4;
//        public static int maxTargetedHolograms = 3;
//        public static int bullseyeHolograms = 2;
//        public static int maxRandomHolograms = 1;


//        public static float maxHologramYOffsetInsideUnitSphereIdk = 0.25f;
//        public static float minimumRandomHologramDistance = 25f;

//        public static GameObject hologramPrefab;

//        // xdd
//        public static GameObject scanEffect;// = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/RadarTower/ActivateRadarTowerEffect.prefab").WaitForCompletion();
//        public static string soundString;// should be on the effect but im lazy

//        private List<CharacterBody> scannedBodies;
//        public override void OnEnter()
//        {
//            base.OnEnter();

//            //EntityStateMachine body = EntityStateMachine.FindByCustomName(base.gameObject, "Body");
//            //if (body) body.SetInterruptState(new NemAssaulter(), InterruptPriority.Pain);

//            this.scannedBodies = new List<CharacterBody>();
//            this.SpawnHolograms();
//            //vfx (radar scanner)
//            //anim
//            //sound
//            EffectManager.SpawnEffect(scanEffect, new EffectData
//            {
//                origin = base.transform.position,
//                scale = scanRadius,
//            }, false);
//            this.outer.SetNextStateToMain();
//            //sound
//            Util.PlaySound(soundString, base.gameObject);
//        }


//        //probably doesnt work in mp
//        //fucking stupid\
//        //fix walls

//        //SHITTET CODE ALL TIME
//        private void SpawnHolograms()
//        {
//            if (!NetworkServer.active) return;

//            Ray aimRay = base.GetAimRay();

//            TeamMask filter = TeamMask.allButNeutral;
//            filter.RemoveTeam(this.teamComponent.teamIndex);

//            List<HealthComponent> alreadyScannedTargets = new List<HealthComponent>();

//            BullseyeSearch search = new BullseyeSearch
//            {
//                teamMaskFilter = filter,
//                filterByLoS = false,
//                searchOrigin = aimRay.origin,
//                searchDirection = aimRay.direction,
//                sortMode = BullseyeSearch.SortMode.Angle,
//                maxDistanceFilter = scanRadius,
//                maxAngleFilter = 120f,
//            };
//            search.RefreshCandidates();
//            search.FilterOutGameObject(base.gameObject);
//            List<HurtBox> hurtBoxes = search.GetResults().ToList();

//            //max total holograms = 4
//            //max targeted holograms = 3
//            //max bullseye holograms = 2
//            //spheresearch holograms = max targeted - bullseye
//            //random holograms = max total - targeted 
//            //^^(up to 1) (smaller radius)
//            // im in too deep

//            int maxTargeted = Scan.maxTargetedHolograms;
//            int maxBullseyeHolograms = Mathf.Min(Scan.bullseyeHolograms, hurtBoxes.Count);
//            int bullseyeHolograms = 0;
//            for (int i = 0; i < maxBullseyeHolograms; i++)
//            {
//                HurtBox target;
//                if (i == 0)
//                {
//                    target = hurtBoxes.FirstOrDefault();
//                    hurtBoxes.RemoveAt(0);
//                }
//                else
//                {
//                    int index = UnityEngine.Random.Range(0, hurtBoxes.Count - 1);
//                    target = hurtBoxes[index];
//                    hurtBoxes.RemoveAt(index);
//                }

//                SpawnHologramTargeted(target);
//                alreadyScannedTargets.Add(target.healthComponent);
//                bullseyeHolograms++;
//            }


//            int maxSpheresearchHolograms = maxTargeted - bullseyeHolograms;
//            int sphereSearchHolograms = 0;
//            if (maxSpheresearchHolograms > 0)
//            {
//                SphereSearch sphereSearch = new SphereSearch
//                {
//                    mask = LayerIndex.entityPrecise.mask,
//                    radius = Scan.scanRadius,
//                    origin = aimRay.origin,
//                };

//                HurtBox[] hurtBoxes2 = sphereSearch.RefreshCandidates().OrderCandidatesByDistance().FilterCandidatesByHurtBoxTeam(filter).FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();
//                HurtBox[] sphereTargets = hurtBoxes2.Where(hb => !alreadyScannedTargets.Contains(hb.healthComponent)).ToArray();

//                sphereSearchHolograms = Mathf.Min(maxSpheresearchHolograms, sphereTargets.Count());
//                for (int i = 0; i < sphereSearchHolograms; i++)
//                {
//                    int index = UnityEngine.Random.Range(0, hurtBoxes.Count - 1);
//                    HurtBox target = sphereTargets[i];
//                    this.SpawnHologramTargeted(target);
//                    alreadyScannedTargets.Add(target.healthComponent);
//                }
//            }


//            int randomHolograms = Mathf.Min(Scan.maxTotalHolograms - alreadyScannedTargets.Count, Scan.maxRandomHolograms);

//            for (int i = 0; i < randomHolograms; i++)
//            {
//                // random node in radius
//                NodeGraph groundNodes = SceneInfo.instance.groundNodes;
//                NodeGraph airNodes = SceneInfo.instance.airNodes;
//                List<NodeGraph.NodeIndex> groundIndices = groundNodes.FindNodesInRange(base.transform.position, Scan.minimumRandomHologramDistance, Scan.randomHologramRadius, HullMask.Human);
//                float numGround = groundIndices.Count;
//                List<NodeGraph.NodeIndex> airIndices = airNodes.FindNodesInRange(base.transform.position, Scan.minimumRandomHologramDistance, Scan.randomHologramRadius, HullMask.Human);
//                float numAir = airIndices.Count;
//                bool useAir = UnityEngine.Random.Range(0, numGround + numAir) > numGround;

//                Vector3 position;
//                NodeGraph.NodeIndex indexDebug;
//                if (useAir)
//                {
//                    int index = UnityEngine.Random.Range(0, airIndices.Count - 1);
//                    airNodes.GetNodePosition(airIndices[index], out position);
//                }
//                else
//                {
//                    int index = UnityEngine.Random.Range(0, groundIndices.Count - 1);
//                    groundNodes.GetNodePosition(groundIndices[index], out position);
//                    position.y += 4f;
//                }

//                this.SpawnHologramSingle(position, Quaternion.identity, null);
//            }

//        }

//        private void SpawnHologramTargeted(HurtBox target)
//        {
//            Vector2 circle = UnityEngine.Random.insideUnitCircle;
//            Vector3 direction = new Vector3(circle.x, 0, circle.y);

//            Vector3 position = target.transform.position + (direction * Scan.targetedHologramOffset);
//            if (Physics.Raycast(target.transform.position, direction, out RaycastHit hit, Scan.targetedHologramOffset, LayerIndex.world.mask))
//            {
//                position = hit.point;
//            }

//            Vector3 between = target.transform.position - position;
//            this.SpawnHologramSingle(position, Util.QuaternionSafeLookRotation(between.normalized), target);
//            this.scannedBodies.Add(target.healthComponent.body);
//        }

//        private void SpawnHologramSingle(Vector3 position, Quaternion rotation, HurtBox target)
//        {
//            NemMercHologram hologram = GameObject.Instantiate(Scan.hologramPrefab, position, rotation).GetComponent<NemMercHologram>();
//            float distanceBetween = (hologram.transform.position - base.transform.position).magnitude;
//            hologram.timeUntilReveal = distanceBetween / (Scan.scanRadius / Scan.scanDuration);
//            hologram.lifetime = Scan.hologramDuration;
//            hologram.owner = base.gameObject;
//            hologram.target = target;
//            NetworkServer.Spawn(hologram.gameObject);
//        }

//    }
//}
