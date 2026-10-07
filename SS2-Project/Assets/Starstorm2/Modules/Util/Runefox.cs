using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using RoR2;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace RuneFoxMods
{
	internal class Utils
	{
		public static ChildLocator GetChildLocator(GameObject body)
		{
			ChildLocator childLocator = null;
			Transform child = body.transform.GetChild(0);
			if (child)
			{
				Transform child2 = child.GetChild(0);
				if (child2)
				{
					childLocator = child2.GetComponent<ChildLocator>();
					if (childLocator)
					{
						Debug.Log("Locator Found");
					}
				}
			}
			return childLocator;
		}

		public static Transform FindChildInTree(Transform Root, string name)
		{
			Queue<Transform> queue = new Queue<Transform>();
			queue.Enqueue(Root);
			while (queue.Count != 0)
			{
				Transform val = queue.Dequeue();
				if ((val).name == name)
				{
					return val;
				}
				for (int i = 0; i < val.childCount; i++)
				{
					Transform child = val.GetChild(i);
					queue.Enqueue(child);
				}
			}
			return null;
		}

		public static string RemoveCloneNaming(string str)
		{
			return str.Remove(str.Length - 7);
		}
	}
}
namespace RuneFoxMods.DynamicSkins
{
	public static class DynamicSkinHelpers
	{
		public static SkinnedMeshRenderer[] GetBaseSkinRenderers(GameObject modelObject)
		{
			SkinnedMeshRenderer[] componentsInChildren = modelObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
			List<SkinnedMeshRenderer> list = new List<SkinnedMeshRenderer>();
			SkinnedMeshRenderer[] array = componentsInChildren;
			foreach (SkinnedMeshRenderer val in array)
			{
				if (((Component)val).transform.parent == modelObject.transform)
				{
					list.Add(val);
				}
			}
			return list.ToArray();
		}

		public static Transform[] BoneArrayBuilder(Transform NewBoneRoot)
		{
			List<Transform> list = new List<Transform>();
			BoneArrayBuilderHelper(NewBoneRoot, list);
			return list.ToArray();
		}

		public static void BoneArrayBuilderHelper(Transform parent, List<Transform> list)
		{
			if (!(parent).name.EndsWith("_end"))
			{
				list.Add(parent);
			}
			for (int i = 0; i < parent.childCount; i++)
			{
				BoneArrayBuilderHelper(parent.GetChild(i), list);
			}
		}

		public static Transform GetArmature(GameObject obj)
		{
			return GetArmatureHelper(obj);
		}

		public static Transform GetArmatureHelper(GameObject obj)
		{
			if ((obj).name.ToLower().Contains("armature"))
			{
				return obj.transform;
			}
			for (int i = 0; i < obj.transform.childCount; i++)
			{
				Transform armatureHelper = GetArmatureHelper(((Component)obj.transform.GetChild(i)).gameObject);
				if ((armatureHelper))
				{
					return armatureHelper;
				}
			}
			return null;
		}

		public static Transform GetTopParent(Transform obj)
		{
			Transform val = obj;
			while (val.parent != null)
			{
				val = val.parent;
			}
			return val;
		}

		public static string GetPrevBoneInList(Transform targetBone, SkinnedMeshRenderer meshRenderer)
		{
			Transform[] bones = meshRenderer.bones;
			for (int i = 0; i <= bones.Length - 1; i++)
			{
				if ((bones[i + 1]).name == (targetBone).name)
				{
					return (bones[i]).name;
				}
			}
			return null;
		}

		public static int GetBoneIndexInList(Transform targetBone, SkinnedMeshRenderer meshRenderer)
		{
			Transform[] bones = meshRenderer.bones;
			for (int i = 0; i <= bones.Length; i++)
			{
				if ((bones[i]).name == (targetBone).name)
				{
					return i;
				}
			}
			return -1;
		}

		public static int GetPrevBoneIndexInList(Transform targetBone, SkinnedMeshRenderer meshRenderer)
		{
			Transform[] bones = meshRenderer.bones;
			for (int i = 0; i <= bones.Length - 1; i++)
			{
				if ((bones[i + 1]).name == (targetBone).name)
				{
					return i;
				}
			}
			return -1;
		}
	}
	internal class DynamicSkinManager
	{
		internal Dictionary<string, SkinDef> SkinDefs = new();

		private GameObject LastModelObject;

		internal Dictionary<string, SortedList<int, Modification>> ModificationList = new();

		private Dictionary<GameObject, AppliedModifications> ModifiedObjects = new();

		internal ManualLogSource InstanceLogger;

		internal void AddModification(string skinNameToken, int boneIndex, Modification modification)
		{
			if (!ModificationList.TryGetValue(skinNameToken, out var value))
			{
				value = new SortedList<int, Modification>();
				ModificationList.Add(skinNameToken, value);
			}
			value.Add(boneIndex, modification);
		}

		internal void AddSkinDef(SkinDef skinDef)
		{
			SkinDefs.Add(skinDef.nameToken, skinDef);
		}

		internal void SkinDefApply(Action<SkinDef, GameObject> orig, SkinDef self, GameObject modelObject)
		{
			orig(self, modelObject);
			RemoveInvalidModelObjects();
			ModifiedObjects.TryGetValue(modelObject, out var value);
			try
			{
				if (!SkinDefs.TryGetValue(self.nameToken, out var _))
				{
					if (value != null)
					{
						ClearSkinModifications(LastModelObject, value);
					}
				}
				else if (value == null)
				{
					AppliedModifications appliedModifications = new AppliedModifications();
					ModifiedObjects.Add(modelObject, appliedModifications);
					ApplySkinModifications(self, modelObject, appliedModifications);
				}
			}
			catch (Exception ex)
			{
				InstanceLogger.LogWarning("An error occured while adding accessories to a skin");
				InstanceLogger.LogError(ex);
			}
		}

		private void RemoveInvalidModelObjects()
		{
			foreach (GameObject item in ModifiedObjects.Keys.Where(el => !el).ToList())
			{
				ModifiedObjects.Remove(item);
			}
		}

		private void ClearSkinModifications(GameObject modelObject, AppliedModifications modifications)
		{
			while (modifications.BaseModelModifications.Count != 0)
			{
				Modification modification = modifications.BaseModelModifications.Pop();
				clearModification(modification, modelObject, modifications);
			}
			while (modifications.OtherModifications.Count != 0)
			{
				clearModification(modifications.OtherModifications[0], modelObject, modifications);
			}
			ModifiedObjects.Remove(modelObject);
		}

		private void ApplySkinModifications(SkinDef skindef, GameObject modelObject, AppliedModifications modifications)
		{
			CharacterModel component = modelObject.GetComponent<CharacterModel>();
			LastModelObject = modelObject;
			if (!ModificationList.TryGetValue(skindef.nameToken, out var value))
			{
				return;
			}
			foreach (KeyValuePair<int, Modification> item in value)
			{
				ApplyModification(modelObject, component, item.Value, modifications);
			}
		}

		private static void ApplyModification(GameObject modelObject, CharacterModel characterModel, Modification modification, AppliedModifications modifications)
		{
			string bodyname = modification.bodyname;
			string parentname = modification.parentname;
			Transform val = Utils.FindChildInTree(modelObject.transform, parentname);
			GameObject val2;
			if (modification.affectsbasemodel)
			{
				val2 = Object.Instantiate(modification.prefab, val, false);
				(val2).name = (Utils.RemoveCloneNaming((val2).name));
				modification.instance = val2;
				modification.inst_armature = val2;
				ModificationApplyBones(modelObject, modification, modifications);
			}
			else
			{
				val2 = Object.Instantiate(modification.prefab, modelObject.transform, false);
				val2.name = Utils.RemoveCloneNaming(val2.name);
				modification.instance = val2;
				Transform armature = DynamicSkinHelpers.GetArmature(val2);
				armature.transform.SetParent(val, false);
				modification.inst_armature = armature.gameObject;
			}
			modification.instance = val2;
			if (modification.dynamicBoneData != null)
			{
				ModificationApplyDynamicBones(modelObject, modification);
			}
			ModificationAddRenderers(val2, characterModel);
			modifications.OtherModifications.Add(modification);
		}

		private static void ModificationApplyBones(GameObject modelObject, Modification modification, AppliedModifications modifications)
		{
			SkinnedMeshRenderer[] baseSkinRenderers = DynamicSkinHelpers.GetBaseSkinRenderers(modelObject);
			List<Transform> list = baseSkinRenderers[0].bones.ToList();
			Transform[] array = DynamicSkinHelpers.BoneArrayBuilder(modification.instance.transform);
			list.InsertRange(modification.boneIndex, array);
			modification.boneCount = array.Length;
			SkinnedMeshRenderer[] array2 = baseSkinRenderers;
			foreach (SkinnedMeshRenderer val in array2)
			{
				val.bones = (list.ToArray());
			}
			modifications.BaseModelModifications.Push(modification);
		}

		private static void ModificationApplyDynamicBones(GameObject modelObject, Modification modification)
		{
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01da: Unknown result type (might be due to invalid IL or missing references)
			DynamicBone dynamicBone = (modification.inst_dynamicBone = modification.instance.AddComponent<DynamicBone>());
			List<DynamicBoneCollider> list = new List<DynamicBoneCollider>();
			foreach (DynamicBoneColliderData collider in modification.dynamicBoneData.m_Colliders)
			{
				Transform val = Utils.FindChildInTree(modelObject.transform, collider.m_parent_name);
				DynamicBoneCollider dynamicBoneCollider = ((Component)val).gameObject.AddComponent<DynamicBoneCollider>();
				dynamicBoneCollider.m_Direction = collider.m_Direction;
				dynamicBoneCollider.m_Center = collider.m_Center;
				dynamicBoneCollider.m_Bound = collider.m_Bound;
				dynamicBoneCollider.m_Radius = collider.m_Radius;
				dynamicBoneCollider.m_Height = collider.m_Height;
				list.Add(dynamicBoneCollider);
			}
			modification.inst_DB_colliders = list;
			Transform root = (dynamicBone.m_Root = Utils.FindChildInTree(modification.inst_armature.transform, modification.dynamicBoneData.m_Root));
			dynamicBone.m_Damping = modification.dynamicBoneData.m_Damping;
			dynamicBone.m_DampingDistrib = modification.dynamicBoneData.m_DampingDistrib;
			dynamicBone.m_Elasticity = modification.dynamicBoneData.m_Elasticity;
			dynamicBone.m_ElasticityDistrib = modification.dynamicBoneData.m_ElasticityDistrib;
			dynamicBone.m_Stiffness = modification.dynamicBoneData.m_Stiffness;
			dynamicBone.m_StiffnessDistrib = modification.dynamicBoneData.m_StiffnessDistrib;
			dynamicBone.m_Inert = modification.dynamicBoneData.m_Inert;
			dynamicBone.m_InertDistrib = modification.dynamicBoneData.m_InertDistrib;
			dynamicBone.m_Radius = modification.dynamicBoneData.m_Radius;
			dynamicBone.m_RadiusDistrib = modification.dynamicBoneData.m_RadiusDistrib;
			dynamicBone.m_EndLength = modification.dynamicBoneData.m_EndLength;
			dynamicBone.m_EndOffset = modification.dynamicBoneData.m_EndOffset;
			dynamicBone.m_Gravity = modification.dynamicBoneData.m_Gravity;
			dynamicBone.m_Force = modification.dynamicBoneData.m_Force;
			dynamicBone.m_Colliders = list;
			dynamicBone.m_Exclusions = new List<Transform>();
			foreach (string exclusion in modification.dynamicBoneData.m_Exclusions)
			{
				Transform val2 = Utils.FindChildInTree(root, exclusion);
				if (val2 != null)
				{
					dynamicBone.m_Exclusions.Add(val2);
				}
				else
				{
					Debug.LogWarning("Tried to exclude a transform that could not be found");
				}
			}
			dynamicBone.m_FreezeAxis = modification.dynamicBoneData.m_FreezeAxis;
		}

		private static void ModificationAddRenderers(GameObject newPart, CharacterModel characterModel)
		{
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			SkinnedMeshRenderer[] componentsInChildren = newPart.GetComponentsInChildren<SkinnedMeshRenderer>(true);
			Array.Resize(ref characterModel.baseRendererInfos, characterModel.baseRendererInfos.Length + componentsInChildren.Length);
			if (componentsInChildren.Length != 0)
			{
				int num = componentsInChildren.Length;
				SkinnedMeshRenderer[] array = componentsInChildren;
				foreach (SkinnedMeshRenderer val in array)
				{
					characterModel.baseRendererInfos[^num] = new CharacterModel.RendererInfo
					{
						renderer = (Renderer)componentsInChildren[^num],
						ignoreOverlays = false,
						defaultShadowCastingMode = (ShadowCastingMode)1,
						defaultMaterial = ((Renderer)val).sharedMaterial
					};
					num--;
				}
			}
		}

		private void clearModification(Modification modification, GameObject modelObject, AppliedModifications modifications)
		{
			if (modification.inst_DB_colliders != null)
			{
				foreach (DynamicBoneCollider inst_DB_collider in modification.inst_DB_colliders)
				{
					Object.Destroy(inst_DB_collider);
				}
			}
			if (modification.affectsbasemodel)
			{
				SkinnedMeshRenderer[] baseSkinRenderers = DynamicSkinHelpers.GetBaseSkinRenderers(modelObject);
				List<Transform> list = baseSkinRenderers[0].bones.ToList();
				list.RemoveRange(modification.boneIndex, modification.boneCount);
				SkinnedMeshRenderer[] array = baseSkinRenderers;
				foreach (SkinnedMeshRenderer val in array)
				{
					val.bones = (list.ToArray());
				}
			}
			Object.Destroy(modifications.OtherModifications[0].inst_dynamicBone);
			Object.Destroy(modifications.OtherModifications[0].inst_armature);
			Object.Destroy(modifications.OtherModifications[0].instance);
			if (!modifications.OtherModifications.Remove(modification))
			{
				InstanceLogger.LogError("Skin Modification was not removed");
			}
		}
	}
	internal class Modification
	{
		public string prefabpath;

		public string bodyname;

		public string parentname;

		public GameObject prefab;

		public bool affectsbasemodel;

		public DynamicBoneData dynamicBoneData;

		public string parentSkinToken;

		public int boneIndex;

		public int boneCount;

		public GameObject instance;

		public GameObject inst_armature;

		public DynamicBone inst_dynamicBone;

		public List<DynamicBoneCollider> inst_DB_colliders = new List<DynamicBoneCollider>();

		public Modification(string PrefabPath, string ParentName, string BodyName, string ParentSkinToken, int BoneIndex, bool AffectsBaseModel, AssetBundle assetBundle)
		{
			bodyname = BodyName;
			prefabpath = PrefabPath;
			parentname = ParentName;
			parentSkinToken = ParentSkinToken;
			affectsbasemodel = AffectsBaseModel;
			boneIndex = BoneIndex;
			prefab = assetBundle.LoadAsset<GameObject>(prefabpath);
			if (prefab == null)
			{
				Debug.LogWarning(("Asset at " + PrefabPath + " was not loaded"));
			}
		}
	}
	internal class AppliedModifications
	{
		public Stack<Modification> BaseModelModifications = new Stack<Modification>();

		public List<Modification> OtherModifications = new List<Modification>();
	}
	internal class DynamicBoneData
	{
		public string m_Root;

		public float m_Damping;

		public AnimationCurve m_DampingDistrib;

		public float m_Elasticity;

		public AnimationCurve m_ElasticityDistrib;

		public float m_Stiffness;

		public AnimationCurve m_StiffnessDistrib;

		public float m_Inert;

		public AnimationCurve m_InertDistrib;

		public float m_Radius;

		public AnimationCurve m_RadiusDistrib;

		public float m_EndLength;

		public Vector3 m_EndOffset;

		public Vector3 m_Gravity;

		public Vector3 m_Force;

		public List<DynamicBoneColliderData> m_Colliders;

		public List<string> m_Exclusions;

		public DynamicBone.FreezeAxis m_FreezeAxis;

		public DynamicBoneData(string root, float damping, AnimationCurve damping_dist, float elasticity, AnimationCurve elasticity_dist, float stiffness, AnimationCurve stiffness_dist, float inert, AnimationCurve inert_dist, float radius, AnimationCurve radius_dist, float end_length, Vector3 end_offset, Vector3 gravity, Vector3 force, List<DynamicBoneColliderData> colliders, List<string> exclusions, DynamicBone.FreezeAxis freeze_axis)
		{
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			m_Root = root;
			m_Damping = damping;
			m_DampingDistrib = damping_dist;
			m_Elasticity = elasticity;
			m_ElasticityDistrib = elasticity_dist;
			m_Stiffness = stiffness;
			m_StiffnessDistrib = stiffness_dist;
			m_Inert = inert;
			m_InertDistrib = inert_dist;
			m_Radius = radius;
			m_RadiusDistrib = radius_dist;
			m_EndLength = end_length;
			m_EndOffset = end_offset;
			m_Gravity = gravity;
			m_Force = force;
			m_Colliders = colliders;
			m_Exclusions = exclusions;
			m_FreezeAxis = freeze_axis;
		}
	}
	internal class DynamicBoneColliderData
	{
		public string m_parent_name;

		public DynamicBoneCollider.Direction m_Direction;

		public Vector3 m_Center;

		public DynamicBoneCollider.Bound m_Bound;

		public float m_Radius;

		public float m_Height;

		public DynamicBoneColliderData(string parent_name, DynamicBoneCollider.Direction direction, Vector3 Center, DynamicBoneCollider.Bound bound, float radius, float heaight)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			m_parent_name = parent_name;
			m_Direction = direction;
			m_Center = Center;
			m_Bound = bound;
			m_Radius = radius;
			m_Height = heaight;
		}
	}
}