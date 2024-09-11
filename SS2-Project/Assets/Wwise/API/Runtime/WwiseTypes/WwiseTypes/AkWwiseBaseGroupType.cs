/*******************************************************************************
The content of this file includes portions of the proprietary AUDIOKINETIC Wwise
Technology released in source code form as part of the game integration package.
The content of this file may not be used without valid licenses to the
AUDIOKINETIC Wwise Technology.
Note that the use of the game engine is subject to the Unity(R) Terms of
Service at https://unity3d.com/legal/terms-of-service
 
License Usage
 
Licensees holding valid licenses to the AUDIOKINETIC Wwise Technology may use
this file in accordance with the end user license agreement provided with the
software or, alternatively, in accordance with the terms contained
in a written agreement between you and Audiokinetic Inc.
Copyright (c) 2024 Audiokinetic Inc.
*******************************************************************************/

#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.

namespace AK.Wwise
{
	[System.Serializable]
	///@brief This type represents the base for all Wwise Types that also require a group GUID, such as State and Switch.
	public abstract class BaseGroupType : BaseType
	{
		public WwiseObjectReference GroupWwiseObjectReference
		{
			get
			{
				var reference = ObjectReference as WwiseGroupValueObjectReference;
				return reference ? reference.GroupObjectReference : null;
			}
		}

		public abstract WwiseObjectType WwiseObjectGroupType { get; }

		public uint GroupId
		{
			get { return GroupWwiseObjectReference ? GroupWwiseObjectReference.Id : AkSoundEngine.AK_INVALID_UNIQUE_ID; }
		}

		public override bool IsValid()
		{
			return base.IsValid() && GroupWwiseObjectReference != null;
		}

		#region Obsolete
		[System.Obsolete(AkSoundEngine.Deprecation_2018_1_2)]
		public int groupID
		{
			get { return (int)GroupId; }
		}

		[System.Obsolete(AkSoundEngine.Deprecation_2018_1_6)]
		public byte[] groupGuid
		{
			get
			{
				var objRef = ObjectReference;
				return !objRef ? null : objRef.Guid.ToByteArray();
			}
		}
		#endregion

		#region WwiseMigration
#pragma warning disable 0414 // private field assigned but not used.
		[UnityEngine.HideInInspector]
		[UnityEngine.SerializeField]
		[UnityEngine.Serialization.FormerlySerializedAs("groupID")]
		private int groupIdInternal;
		[UnityEngine.HideInInspector]
		[UnityEngine.SerializeField]
		[UnityEngine.Serialization.FormerlySerializedAs("groupGuid")]
		private byte[] groupGuidInternal;
#pragma warning restore 0414 // private field assigned but not used.
		#endregion
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.