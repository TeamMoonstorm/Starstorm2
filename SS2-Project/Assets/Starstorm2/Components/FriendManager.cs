using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using RoR2;
namespace SS2
{
    // technically we can just throw any rpc methods in here????????? does that really work???????????????? that feels fucking stupid?????????????????????????????????
    public class FriendManager : NetworkBehaviour
    {
        public static FriendManager instance;
        private static GameObject prefab;
        [SystemInitializer]
        private static void Init()
        {
            Run.onRunStartGlobal += (run) =>
            {
                if (!NetworkServer.active) return;

                prefab = SS2Assets.LoadAsset<GameObject>("FriendManager", SS2Bundle.Base);
                if (prefab == null)
                {
                    SS2Log.Fatal("FriendManager missing prefab!");
                    return;
                }
                instance = GameObject.Instantiate<GameObject>(prefab, run.transform).GetComponent<FriendManager>();
                NetworkServer.Spawn(instance.gameObject);
            };
        }

        private void OnEnable()
        {
            instance = this;
        }
        private void OnDisable()
        {
            instance = null;
        }

        private void Start()
        {
            base.transform.SetParent(Run.instance.transform); // 
        }
        // am i fucking stupid? i couldnt think of any other way to do this
        // couldnt be in ChirrFriendController since its a monobehavior added to the player master at runtime
        // couldnt be in ChirrFriendTracker since it could only be used if the master's body object was ChirrBody
        // theres like 10 different classes for all of chirr's behavior. theres no way this is correct
        [ClientRpc]
        public void RpcSetupFriend(GameObject masterObject, bool unfriend, bool isScepter)
        {
            CharacterMaster master = masterObject.GetComponent<CharacterMaster>();
            if (!unfriend)
            {
                if (isScepter)
                    DontDestroyOnLoad(master); // BORING BORING BORING. CLAY TEMPLAR = AUTOWIN AND -1 ABILITY WHOLEW GAME. scepter ok tho :)
            }
            else
            {
                CharacterBody body = master.GetBody();
                if (body && body.teamComponent.indicator) // teammate indicator doesnt go away when changing team
                    Destroy(body.teamComponent.indicator);
                if (Stage.instance)
                    SceneManager.MoveGameObjectToScene(master.gameObject, Stage.instance.gameObject.scene); // i think this is how it works? // this is how it works :3
            }

        }

        [ClientRpc]
        public void RpcSetupNemBoss(GameObject bodyObject, string prefabName)
        {
            CharacterBody body = bodyObject.GetComponent<CharacterBody>();
            GameObject prefab = SS2Assets.LoadAsset<GameObject>(prefabName, SS2Bundle.Events);
            if (prefab)
            {
                var effect = GameObject.Instantiate(prefab, body.corePosition, Quaternion.identity, body.coreTransform);
                body.master.onBodyDeath.AddListener(RemoveEffect);
                void RemoveEffect() => Destroy(effect);
            }
            else SS2Log.Warning("No effect prefab for " + body.GetDisplayName());
            body.gameObject.AddComponent<Components.NemesisResistances>();
            if (body.mainHurtBox)
            {
                CapsuleCollider capsuleCollider = body.mainHurtBox.GetComponent<CapsuleCollider>();
                if (capsuleCollider)
                {
                    capsuleCollider.height = 4f;
                    capsuleCollider.radius = 4f;
                }

            }
        }

        [ClientRpc]
        public void RpcAddStock(GameObject bodyObject, int skillSlot, int count = 1, bool obeyMaxStock = true)
        {
            if(Util.HasEffectiveAuthority(bodyObject))
            {
                GenericSkill skill = bodyObject.GetComponent<SkillLocator>().GetSkill((SkillSlot)skillSlot);
                if (skill)
                {
                    skill.stock += count;
                    if(obeyMaxStock && skill.stock > skill.maxStock)
                    {
                        skill.stock = skill.maxStock;
                    }
                }
            }
        }
    }
}