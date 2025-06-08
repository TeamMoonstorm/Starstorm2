using UnityEngine;
using RoR2;
using UnityEngine.Networking;

namespace SS2.Components
{
	[RequireComponent(typeof(NetworkIdentity))]
	public class MimicSpawner : MonoBehaviour
    {
		public static GameObject masterObject;

        private void Start()
        {
			Vector3 vec = this.transform.position;
			vec.y += 0.05f;
			SS2Log.Warning(" im tired  : " + masterObject);
			GameObject masterInstance = Object.Instantiate<GameObject>(masterObject, vec, Quaternion.identity);

			CharacterMaster component = masterInstance.GetComponent<CharacterMaster>();
			component.teamIndex = TeamIndex.Monster;

			NetworkServer.Spawn(masterInstance);
			component.SpawnBody(vec, Quaternion.identity);

			//MasterSummon obj = new MasterSummon
			//{
			//	masterPrefab = masterObject,
			//	ignoreTeamMemberLimit = false,
			//	position = vec,
			//	summonerBodyObject = null,
			//	useAmbientLevel = true,
			//	teamIndexOverride = TeamIndex.Monster
			//	//preSpawnSetupCallback += new Action<CharacterMaster>(PreSpawnSetup));
			//};
			//CharacterMaster instance = obj.Perform();
			//if (instance)
			//{
			//	SS2Log.Warning("YIIPE !");
			//}
			//else
			//{
			//	SS2Log.Error("IM CRYING !!!!");
			//}
			//CharacterDirection component = targetBody.GetComponent<CharacterDirection>();
			//obj.rotation = (component ? Quaternion.Euler(0f, component.yaw, 0f) : targetBody.transform.rotation);
			//obj.summonerBodyObject = (ownerBody ? ownerBody.gameObject : null);
			//obj.inventoryToCopy = targetBody.inventory;
			//obj.useAmbientLevel = null;

		}


		public void SpawnMonster(CharacterMaster monsterMaster)
		{
			//Vector3 position = firstLocalUser.cachedBody.transform.position;
			//GameObject bodyPrefab = monsterMaster.GetComponent<CharacterMaster>().bodyPrefab;
			//EquipmentIndex equipmentIndex = this.eliteMap[this.eliteIndexDropDown.options[this.eliteIndexDropDown.value].text];
			//TeamIndex teamIndex = (TeamIndex)TeamIndex.Monster;

			//GameObject gameObject = MasterCatalog.FindMasterPrefab(this.masterName);
			GameObject masterInstance = Object.Instantiate<GameObject>(masterObject, this.transform.position, Quaternion.identity);
			CharacterMaster component = masterInstance.GetComponent<CharacterMaster>();
			component.teamIndex = TeamIndex.Monster;
			//foreach (KeyValuePair<ItemDef, uint> keyValuePair in this.itemCounts)
			//{
			//	ItemDef itemDef;
			//	uint num;
			//	keyValuePair.Deconstruct(out itemDef, out num);
			//	ItemDef itemDef2 = itemDef;
			//	uint count = num;
			//	component.inventory.GiveItem(itemDef2, (int)count);
			//}
			//bool flag = this.eliteIndex != EquipmentIndex.None;
			//if (flag)
			//{
			//	component.inventory.SetEquipmentIndex(this.eliteIndex);
			//}
			NetworkServer.Spawn(masterInstance);
			//component.bodyPrefab = BodyCatalog.FindBodyPrefab(masterInstance.);
			component.SpawnBody(this.transform.position, Quaternion.identity);
		}
	}

}
