using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using JetBrains.Annotations;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
namespace Moonstorm.Starstorm2.Components
{
    public class RemunerationShopBehavior : NetworkBehaviour
    {
        //cringe

        [SystemInitializer]
        private static void Init()
        {
            deleteEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifierExplosion.prefab").WaitForCompletion();
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

        //enable choices 
        public void OnAllChoicesDiscovered()
        {
            foreach(RemunerationChoiceBehavior choice in this.choices)
            {
                choice.alive = true;
                choice.temp_peniswenis.available = true;
            }
        }

        //destroy others
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
                Destroy(chois.gameObject);
            }
        }

        // 3 am shitcode. so lazy. must releaes. sry
        public void SpawnDroplet(Vector3 position, Vector3 velocity)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(PickupDropletController.pickupDropletPrefab, position, Quaternion.identity);
            Destroy(gameObject.GetComponent<PickupDropletController>()); // XDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD
            RemunerationDropletController component = gameObject.AddComponent<RemunerationDropletController>(); //// :3
            component.shop = this;
            Rigidbody component2 = gameObject.GetComponent<Rigidbody>();
            component2.velocity = velocity;
            component2.AddTorque(UnityEngine.Random.Range(150f, 120f) * UnityEngine.Random.onUnitSphere);
            NetworkServer.Spawn(gameObject);
        }



    }
}
