using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using RoR2;
using R2API.Networking.Interfaces;
namespace SS2
{
	// technically we can just throw any rpc methods in here????????? does that really work???????????????? that feels fucking stupid?????????????????????????????????
	public class FriendManager : NetworkBehaviour
	{
		public static FriendManager instance;
		[SystemInitializer]
		private static void Init()
		{
			Run.onRunStartGlobal += (run) =>
			{
				if (!NetworkServer.active) return; // ???
				instance = UnityEngine.Object.Instantiate<GameObject>(SS2Assets.LoadAsset<GameObject>("FriendManager", SS2Bundle.Base), run.transform).GetComponent<FriendManager>();
				NetworkServer.Spawn(instance.gameObject);
			};
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
        public void RpcAddStock(GameObject bodyObject, int skillSlot, int count = 1, bool obeyMaxStock = false)
        {
            if (Util.HasEffectiveAuthority(bodyObject))
            {
                GenericSkill skill = bodyObject.GetComponent<SkillLocator>().GetSkill((SkillSlot)skillSlot);
                if (skill)
                {
                    skill.stock += count;
                    if (obeyMaxStock && skill.stock > skill.maxStock)
                    {
                        skill.stock = skill.maxStock;
                    }
                }
            }
        }
        public class SyncBaseStats : INetMessage
        {
            NetworkInstanceId bodyNetId;
            float maxHealth;
            float regen;
            float maxShield;
            float movementSpeed;
            float acceleration;
            float jumpPower;
            float damage;
            float attackSpeed;
            float crit;
            float armor;
            float visionDistance;
            int jumpCount;

            public void Serialize(NetworkWriter writer)
            {
                writer.Write(bodyNetId);
                writer.Write(maxHealth);
                writer.Write(regen);
                writer.Write(maxShield);
                writer.Write(movementSpeed);
                writer.Write(acceleration);
                writer.Write(jumpPower);
                writer.Write(damage);
                writer.Write(attackSpeed);
                writer.Write(crit);
                writer.Write(armor);
                writer.Write(visionDistance);
                writer.Write(jumpCount);
            }

            public void Deserialize(NetworkReader reader)
            {
                bodyNetId = reader.ReadNetworkId();
                maxHealth = reader.ReadSingle();
                regen = reader.ReadSingle();
                maxShield = reader.ReadSingle();
                movementSpeed = reader.ReadSingle();
                acceleration = reader.ReadSingle();
                jumpPower = reader.ReadSingle();
                damage = reader.ReadSingle();
                attackSpeed = reader.ReadSingle();
                crit = reader.ReadSingle();
                armor = reader.ReadSingle();
                visionDistance = reader.ReadSingle();
                jumpCount = reader.ReadInt32();
            }

            public void OnReceived()
            {
                if (NetworkServer.active)
                {
                    return;
                }
                GameObject bodyObject = Util.FindNetworkObject(bodyNetId);
                if (!bodyObject)
                {
                    SS2Log.Warning($"{typeof(SyncBaseStats).FullName}: Could not retrieve GameObject with network ID {bodyNetId}");
                }
                CharacterBody body = bodyObject.GetComponent<CharacterBody>();
                if (!body)
                {
                    SS2Log.Warning($"{typeof(SyncBaseStats).FullName}: Retrieved GameObject {bodyObject} but the GameObject does not have a CharacterBody");
                    return;
                }

                body.baseMaxHealth = maxHealth;
                body.baseRegen = regen;
                body.baseMaxShield = maxShield;
                body.baseMoveSpeed = movementSpeed;
                body.baseAcceleration = acceleration;
                body.baseJumpPower = jumpPower;
                body.baseDamage = damage;
                body.baseAttackSpeed = attackSpeed;
                body.baseCrit = crit;
                body.baseArmor = armor;
                body.baseVisionDistance = visionDistance;
                body.baseJumpCount = jumpCount;
                body.PerformAutoCalculateLevelStats();
                SS2Log.Info($"Synced base stats of {body}");
            }

            public SyncBaseStats()
            {

            }

            public SyncBaseStats(CharacterBody body)
            {
                NetworkIdentity netID = body.GetComponent<NetworkIdentity>();
                bodyNetId = netID.netId;
                maxHealth = body.baseMaxHealth;
                regen = body.baseRegen;
                maxShield = body.baseMaxShield;
                movementSpeed = body.baseMoveSpeed;
                acceleration = body.baseAcceleration;
                jumpPower = body.baseJumpPower;
                damage = body.baseDamage;
                attackSpeed = body.baseAttackSpeed;
                crit = body.baseCrit;
                armor = body.baseArmor;
                visionDistance = body.baseVisionDistance;
                jumpCount = body.baseJumpCount;
            }
        }
    }	
}