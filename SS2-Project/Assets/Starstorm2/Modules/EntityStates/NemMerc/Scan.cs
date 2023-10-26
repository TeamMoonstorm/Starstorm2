using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using Moonstorm.Starstorm2.Components;
using RoR2.Navigation;
using Moonstorm.Starstorm2;

namespace EntityStates.NemMerc
{
    public class Scan : BaseSkillState
    {
        public static float scanRadius = 75f;
        public static float randomHologramRadius = 40f;
        public static float scanDuration = 1f;

        public static float hologramDuration = 10f;

        [SerializeField]
        public int targetedHolograms = 3;
        public static float targetedHologramOffset = 8f;
        
        [SerializeField]
        public int randomHolograms = 1;


        public static float maxHologramYOffsetInsideUnitSphereIdk = 0.25f;
        public static float minimumRandomHologramDistance = 25f;

        public static GameObject hologramPrefab;

        // xdd
        public static GameObject scanEffect;// = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/RadarTower/ActivateRadarTowerEffect.prefab").WaitForCompletion();
        public static string soundString;// should be on the effect but im lazy

        public override void OnEnter()
        {
            base.OnEnter();

            if (NetworkServer.active)
                this.SpawnHolograms();

            //anim

            // POST PROCESSING!!! NEED TO LEARN THIS REAL QUICK
            //HELMET GLOW!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            Transform modelTransform = base.GetModelTransform();
            TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
            temporaryOverlay.duration = Scan.hologramDuration;
            temporaryOverlay.alphaCurve = AnimationCurve.Constant(0, 1, 1);
            temporaryOverlay.animateShaderAlpha = true; // hopoo shitcode. stopwatch doesnt run w/o this
            temporaryOverlay.destroyComponentOnEnd = true;
            temporaryOverlay.originalMaterial = SS2Assets.LoadAsset<Material>("matNemMercGlow", SS2Bundle.NemMercenary);
            temporaryOverlay.AddToCharacerModel(modelTransform.GetComponent<CharacterModel>());


            // GAS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            ChildLocator childLocator = base.GetModelChildLocator();
            if(childLocator)
            {
                Transform left = childLocator.FindChild("GasL");
                if(left)
                {
                    ParticleSystem particle = left.GetComponent<ParticleSystem>();
                    var main = particle.main;
                    main.duration = Scan.hologramDuration;
                    particle.Play();
                }
                Transform right = childLocator.FindChild("GasR");
                if (right)
                {
                    ParticleSystem particle = right.GetComponent<ParticleSystem>();
                    var main = particle.main;
                    main.duration = Scan.hologramDuration;
                    particle.Play();
                }
            }



            EffectManager.SpawnEffect(scanEffect, new EffectData
            {
                origin = base.transform.position,
                scale = scanRadius,
            }, false);
            this.outer.SetNextStateToMain();

            Util.PlaySound(soundString, base.gameObject);
        }
        private void SpawnHolograms()
        {
            Ray aimRay = base.GetAimRay();

            TeamMask filter = TeamMask.allButNeutral;
            filter.RemoveTeam(this.teamComponent.teamIndex);

            BullseyeSearch search = new BullseyeSearch
            {
                teamMaskFilter = filter,
                filterByLoS = false,
                searchOrigin = aimRay.origin,
                searchDirection = aimRay.direction,
                sortMode = BullseyeSearch.SortMode.Angle,
                maxDistanceFilter = scanRadius,
                maxAngleFilter = 120f,
            };
            search.RefreshCandidates();
            search.FilterOutGameObject(base.gameObject);
            List<HurtBox> hurtBoxes = search.GetResults().ToList();

            int targetedHolograms = Mathf.Min(this.targetedHolograms, hurtBoxes.Count);
            for (int i = 0; i < targetedHolograms; i++)
            {
                HurtBox target;
                if (i == 0)
                {
                    target = hurtBoxes.FirstOrDefault();
                    hurtBoxes.RemoveAt(0);
                }
                else
                {
                    int index = UnityEngine.Random.Range(0, hurtBoxes.Count - 1);
                    target = hurtBoxes[index];
                    hurtBoxes.RemoveAt(index);

                }

                Vector2 circle = UnityEngine.Random.insideUnitCircle;
                Vector3 direction = new Vector3(circle.x, 0, circle.y);

                Vector3 position = target.transform.position + (direction * Scan.targetedHologramOffset);
                if (Physics.Raycast(target.transform.position, direction, out RaycastHit hit, Scan.targetedHologramOffset, LayerIndex.world.mask))
                {
                    position = hit.point;
                }

                this.SpawnHologramSingle(position, target);
            }
            for (int i = 0; i < this.randomHolograms; i++)
            {

                // random node in radius
                NodeGraph groundNodes = SceneInfo.instance.groundNodes;
                NodeGraph airNodes = SceneInfo.instance.airNodes;
                List<NodeGraph.NodeIndex> groundIndices = groundNodes.FindNodesInRange(base.transform.position, Scan.minimumRandomHologramDistance, Scan.randomHologramRadius, HullMask.Human);
                float numGround = groundIndices.Count;
                List<NodeGraph.NodeIndex> airIndices = airNodes.FindNodesInRange(base.transform.position, Scan.minimumRandomHologramDistance, Scan.randomHologramRadius, HullMask.Human);
                float numAir = airIndices.Count;
                bool useAir = UnityEngine.Random.Range(0, numGround + numAir) > numGround;

                Vector3 position;
                if (useAir)
                {
                    int index = UnityEngine.Random.Range(0, airIndices.Count - 1);
                    airNodes.GetNodePosition(airIndices[index], out position);
                }
                else
                {
                    int index = UnityEngine.Random.Range(0, groundIndices.Count - 1);
                    groundNodes.GetNodePosition(groundIndices[index], out position);
                    position.y += 4f;
                }

                this.SpawnHologramSingle(position, null);
            }

        }
        private void SpawnHologramSingle(Vector3 position, HurtBox target)
        {
            NemMercHologram hologram = GameObject.Instantiate(Scan.hologramPrefab, position, Quaternion.identity).GetComponent<NemMercHologram>();
            hologram.lifetime = Scan.hologramDuration;
            hologram.owner = base.gameObject;
            hologram.target = target;

            //Moonstorm.Starstorm2.SS2Log.Info("-------------------");
            //Moonstorm.Starstorm2.SS2Log.Info("timeUntilReveal: " + hologram.timeUntilReveal);
            //Moonstorm.Starstorm2.SS2Log.Info("-------------------");
            NetworkServer.Spawn(hologram.gameObject);
        }

    }
}