﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using JetBrains.Annotations;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using R2API;
namespace Moonstorm.Starstorm2.Components
{
    public class RemunerationShopBehavior : NetworkBehaviour
    {
        //cringe

        [SystemInitializer]
        private static void Init()
        {
            //TEMP HOPEFULLY
            deleteEffectPrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifierExplosion.prefab").WaitForCompletion(), "WAWA");
            deleteEffectPrefab.GetComponent<EffectComponent>().soundName = "Play_nullifier_death_vortex_explode";
            ContentAddition.AddEffect(deleteEffectPrefab); // i think this is how r2api works? not gonna look it up tehe
        }

        public float effectScale = 6f;// need for unityexplorer
        public static GameObject deleteEffectPrefab;

        public List<RemunerationChoiceBehavior> choices;

        //private Xoroshiro128Plus rng;

        public int numDrops = 3;

        public int choicesDropped;

        public CharacterMaster owner;

        public enum ShopOption //??????????
        {
            RedItem,
            //VoidShop,
            //Money,
        }

        void Start()
        {
            this.choices = new List<RemunerationChoiceBehavior>();
            //if (NetworkServer.active)
            //{
            //    this.rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
            //}
            if (NetworkClient.active)
            {

            }


        }

        public void DiscoverChoice(RemunerationChoiceBehavior choice)
        {
            this.choices.Add(choice);

            if(this.choices.Count >= numDrops)
            {
                this.OnAllChoicesDiscovered();
            }
        }

        public void OnAllChoicesDiscovered()
        {
            foreach(RemunerationChoiceBehavior choice in this.choices)
            {
                choice.alive = true;
                choice.temp_peniswenis.SetAvailable(true);
            }
        }

        public void OnChoicePicked(RemunerationChoiceBehavior choice)
        {
            foreach(RemunerationChoiceBehavior chois in this.choices)
            {
                EffectData data = new EffectData
                {
                    origin = chois.transform.position + Vector3.up * 3, // XD
                    rotation = Quaternion.identity,
                    scale = effectScale,
                };
                EffectManager.SpawnEffect(deleteEffectPrefab, data, true);
                Util.PlaySound("Play_nullifier_death_vortex_explode", base.gameObject); // im rarted sry
                Destroy(chois.gameObject);
            }
        }

        // 3 am shitcode. so lazy. must releaes. sry
        public void SpawnDroplet(Vector3 position, Vector3 velocity)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(RemunerationDropletController.dropletPrefab, position, Quaternion.identity);
            RemunerationDropletController component = gameObject.GetComponent<RemunerationDropletController>(); //// :3
            component.shop = this;
            Rigidbody component2 = gameObject.GetComponent<Rigidbody>();
            component2.velocity = velocity;
            component2.AddTorque(UnityEngine.Random.Range(150f, 120f) * UnityEngine.Random.onUnitSphere);
            NetworkServer.Spawn(gameObject);
        }



    }
}
