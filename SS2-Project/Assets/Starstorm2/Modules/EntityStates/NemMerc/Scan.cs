using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using Moonstorm.Starstorm2.Components;

namespace EntityStates.NemMerc
{
    public class Scan : BaseSkillState
    {
        public static float scanRadius = 75f;
        public static float scanDuration = 1f;
        public static float hologramDuration = 10f;
        public static int targetedHolograms = 6;
        public static int randomHolograms = 2;

        public static GameObject hologramPrefab;

        // xdd
        private static GameObject scanEffect = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/RadarTower/ActivateRadarTowerEffect.prefab").WaitForCompletion();
        public override void OnEnter()
        {
            base.OnEnter();


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
                teamMaskFilter = TeamMask.GetUnprotectedTeams(this.teamComponent.teamIndex),
                filterByLoS = true,
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

            //spawn some holograms near targets, rest randomly
            int targetedHolograms = Mathf.Min(Scan.targetedHolograms, hurtBoxes.Count);
            for (int i = 0; i < targetedHolograms; i++)
            {
                Vector2 circle = UnityEngine.Random.insideUnitCircle;
                int index = UnityEngine.Random.Range(0, hurtBoxes.Count - 1);
                Vector3 direction = new Vector3(circle.x, 0, circle.y);
                float offsetDistance = 5f;
                Vector3 position = hurtBoxes[index].transform.position + (direction * offsetDistance);
                hurtBoxes.RemoveAt(index);

                this.SpawnHologramSingle(position);
            }
            for (int i = 0; i < Scan.randomHolograms; i++)
            {
                // USE NODEGRAPH ?
                Vector2 circle = UnityEngine.Random.insideUnitCircle;
                Vector3 position = new Vector3(circle.x, 0, circle.y) * Scan.scanRadius + base.transform.position;
                this.SpawnHologramSingle(position);
            }

        }
        private void SpawnHologramSingle(Vector3 position)
        {
            NemMercHologram hologram = GameObject.Instantiate(Scan.hologramPrefab, position, Quaternion.identity).GetComponent<NemMercHologram>();
            float distanceBetween = (hologram.transform.position - base.transform.position).magnitude;
            hologram.timeUntilReveal = distanceBetween / (Scan.scanRadius / Scan.scanDuration);
            hologram.lifetime = Scan.hologramDuration;
            hologram.owner = base.gameObject;
        }
        
    }
}
