using System.Collections.Generic;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
namespace SS2.Components
{
    public class RemunerationShopBehavior : NetworkBehaviour
    {
        public float effectScale = 4.5f;// need for unityexplorer
        

        public List<RemunerationChoiceBehavior> choices;

        //private Xoroshiro128Plus rng;

        public int numDrops = 3;
        public float soundPitch = 1.5f;

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
                choice.interaction.SetAvailable(true);
            }
        }

        public void OnChoicePicked(RemunerationChoiceBehavior choice)
        {
            Util.PlayAttackSpeedSound("Play_nullifier_death_vortex_explode", base.gameObject, soundPitch); // im rarted!!!!!!!
            foreach (RemunerationChoiceBehavior chois in this.choices)
            {
                EffectData data = new EffectData
                {
                    origin = chois.transform.position + Vector3.up * 3, // XD
                    rotation = Quaternion.identity,
                    scale = effectScale,
                };
                EffectManager.SpawnEffect(Items.Remuneration.deleteEffectPrefab, data, true); // :(        
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


        private bool hasFailed;
        public void FailDroplet(RemunerationDropletController droplet)
        {
            if(!hasFailed)
            {
                this.hasFailed = true;
                if (NetworkServer.active)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage { baseToken = "SS2_ITEM_REMUNERATION_FAILURE" });
                    this.OnAllChoicesDiscovered();

                    EffectData data = new EffectData
                    {
                        origin = droplet.transform.position,
                        rotation = Quaternion.identity,
                        scale = effectScale,
                    };
                    EffectManager.SpawnEffect(Items.Remuneration.deleteEffectPrefab, data, true); // :(
                    Destroy(droplet.gameObject);
                }
                    
            }         
        }



    }
}
