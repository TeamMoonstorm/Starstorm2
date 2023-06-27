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
namespace EntityStates.NemMerc
{
    public class Scan : BaseSkillState
    {
        public static float scanRadius = 75f;
        public static float scanDuration = 1f;
        public static float hologramDuration = 10f;
        public static int targetedHolograms = 3;
        public static float targetedHologramOffset = 8f;
        public static int randomHolograms = 2;


        public static float maxHologramYOffsetInsideUnitSphereIdk = 0.25f;
        public static float minimumRandomHologramDistance = 25f;

        public static GameObject hologramPrefab;

        // xdd
        private static GameObject scanEffect = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/RadarTower/ActivateRadarTowerEffect.prefab").WaitForCompletion();
        public override void OnEnter()
        {
            base.OnEnter();

            //EntityStateMachine body = EntityStateMachine.FindByCustomName(base.gameObject, "Body");
            //if (body) body.SetInterruptState(new NemAssaulter(), InterruptPriority.Pain);

            this.SpawnHolograms();
            //vfx (radar scanner)
            //anim
            //sound
            EffectManager.SpawnEffect(scanEffect, new EffectData
            {
                origin = base.transform.position,
                scale = scanRadius,
            }, false);
            this.outer.SetNextStateToMain();
        }


        //probably doesnt work in mp
        //fucking stupid\
        //fix walls
        private void SpawnHolograms()
        {
            if (!NetworkServer.active) return;
            
            Ray aimRay = base.GetAimRay();
            BullseyeSearch search = new BullseyeSearch
            {
                teamMaskFilter = TeamMask.GetEnemyTeams(this.teamComponent.teamIndex),
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

            #region bullshit
            /////////////////////////////////////////////DIPSHITTTTTTTTTTTTTTTTTTTTTTTTTTTT
            /////pick (random?) target with bullseyesearch in radius. if only one target, spawn hologram near it
            // JK i dont want to do this ^^^^^^^
            //HurtBox initialTarget = null;
            //if (hurtBoxes.Count > 0)
            //{
            //    bool oneTarget = hurtBoxes.Count == 1;
            //    int i = UnityEngine.Random.Range(0, hurtBoxes.Count - 1);
            //    initialTarget = hurtBoxes[i];
            //    hurtBoxes.RemoveAt(i);

            //    if (oneTarget)
            //    {
            //        Vector2 circle = UnityEngine.Random.insideUnitCircle;
            //        Vector3 direction1 = new Vector3(circle.x, 0, circle.y);
            //        float offsetDistance = 5f;
            //        Vector3 offset = offsetDistance * direction1.normalized;

            //        //prefab
            //        hologramsSpawned++;
            //        GameObject hologram = new GameObject("NemMercHologram");
            //        hologram.transform.position = initialTarget.transform.position + offset;
            //        Hologram h = hologram.AddComponent<Hologram>();
            //        float distanceBetween = (hologram.transform.position - base.transform.position).magnitude;
            //        h.timeUntilReveal = distanceBetween / (Scan.scanRadius / Scan.scanDuration);
            //        h.lifetime = Scan.hologramDuration;
            //        h.owner = base.gameObject;
            //    }

            //}
            ////pick a different (random? high health? boss?) target within dash range. spawn 2 holograms such that both targets are between them
            //if (hurtBoxes.Count > 0)
            //{
            //    int i = UnityEngine.Random.Range(0, hurtBoxes.Count - 1);
            //    HurtBox target2 = hurtBoxes[i];

            //    Vector2 circle = UnityEngine.Random.insideUnitCircle;
            //    Vector3 direction1 = new Vector3(circle.x, 0, circle.y);
            //    float offsetDistance = 5f;
            //    Vector3 offset = offsetDistance * direction1.normalized;

            //    //prefab
            //    hologramsSpawned++;
            //    GameObject hologram = new GameObject("NemMercHologram");
            //    hologram.transform.position = target2.transform.position + offset;
            //    Hologram h = hologram.AddComponent<Hologram>();
            //    float distanceBetween = (hologram.transform.position - base.transform.position).magnitude;
            //    h.timeUntilReveal = distanceBetween / (Scan.scanRadius / Scan.scanDuration);
            //    h.lifetime = Scan.hologramDuration;
            //    h.owner = base.gameObject;


            //    Vector3 between = initialTarget.transform.position - target2.transform.position;
            //    Vector3 direction = between.normalized;
            //    // DASH RANGE///////////////////
            //    Vector3 position = (between.magnitude + offsetDistance) * direction + target2.transform.position;

            //    hologramsSpawned++;
            //    GameObject hologram2 = new GameObject("NemMercHologram");
            //    hologram2.transform.position = position;
            //    Hologram h2 = hologram2.AddComponent<Hologram>();
            //    float distanceBetween2 = (hologram2.transform.position - base.transform.position).magnitude;
            //    h2.timeUntilReveal = distanceBetween2 / (Scan.scanRadius / Scan.scanDuration);
            //    h2.lifetime = Scan.hologramDuration;
            //    h2.owner = base.gameObject;
            //}
            ////repeat if we want
            #endregion

            //TODO:
            //max total holograms = 4
            //max bullseye + random holograms = 3
            //initial bullseyesearch holograms = 2
            //max spheresearch holograms = 2
            //missed spheresearch holograms instead go to random holograms (in smaller radius)

            //targetting interactables could be neat. would double as a "combat" hologram near the teleporter
            int targetedHolograms = Mathf.Min(Scan.targetedHolograms, hurtBoxes.Count);
            for (int i = 0; i < targetedHolograms; i++)
            {
                HurtBox target;
                if(i == 0)
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
                

                this.SpawnHologramSingle(position, target);
            }
            for (int i = 0; i < Scan.randomHolograms; i++)
            {

                // random node in radius
                NodeGraph groundNodes = SceneInfo.instance.groundNodes;
                NodeGraph airNodes = SceneInfo.instance.airNodes;
                List<NodeGraph.NodeIndex> groundIndices = groundNodes.FindNodesInRange(base.transform.position, Scan.minimumRandomHologramDistance, Scan.scanRadius, HullMask.Human);
                float numGround = groundIndices.Count;
                List<NodeGraph.NodeIndex> airIndices = airNodes.FindNodesInRange(base.transform.position, Scan.minimumRandomHologramDistance, Scan.scanRadius, HullMask.Human);
                float numAir = airIndices.Count;
                bool useAir = UnityEngine.Random.Range(0, numGround + numAir) > numGround;

                Vector3 position;
                NodeGraph.NodeIndex indexDebug;
                if(useAir)
                {
                    int index = UnityEngine.Random.Range(0, airIndices.Count - 1);
                    indexDebug = airIndices[index];
                    airNodes.GetNodePosition(airIndices[index], out position);
                }
                else
                {
                    int index = UnityEngine.Random.Range(0, groundIndices.Count - 1);
                    indexDebug = groundIndices[index];
                    groundNodes.GetNodePosition(groundIndices[index], out position);
                    position.y += 4f;
                }

                Debug.Log("------RANDOM HOLOGRAM " + i + "------");
                Debug.Log("NUM AIR NODES IN RANGE: " + numAir);
                Debug.Log("NUM GROUND NODES IN RANGE: " + numGround);
                Debug.Log("CHANCE TO USE AIR NODES: " + numAir / (numAir + numGround));
                Debug.Log("USED AIR NODES: " + useAir);
                Debug.Log("NODE INDEX: " + indexDebug.nodeIndex);
                Debug.Log("NODE POSITION " + position.ToString());

                this.SpawnHologramSingle(position, null);
            }

        }
        private void SpawnHologramSingle(Vector3 position, HurtBox target)
        {
            NemMercHologram hologram = GameObject.Instantiate(Scan.hologramPrefab, position, Quaternion.identity).GetComponent<NemMercHologram>();
            float distanceBetween = (hologram.transform.position - base.transform.position).magnitude;
            hologram.timeUntilReveal = distanceBetween / (Scan.scanRadius / Scan.scanDuration);
            hologram.lifetime = Scan.hologramDuration;
            hologram.owner = base.gameObject;
            hologram.target = target;
        }
        
    }
}
