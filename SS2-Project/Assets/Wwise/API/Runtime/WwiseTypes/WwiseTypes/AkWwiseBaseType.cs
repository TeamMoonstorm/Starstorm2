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
	///@brief This type represents the base for all Wwise Types that require a GUID.
	public abstract class BaseType
	{
		// System.Web.Util.HashCodeCombiner.CombineHashCodes(System.Int32, System.Int32): http://referencesource.microsoft.com/#System.Web/Util/HashCodeCombiner.cs,21fb74ad8bb43f6b
		// System.Array.CombineHashCodes(System.Int32, System.Int32): http://referencesource.microsoft.com/#mscorlib/system/array.cs,87d117c8cc772cca
		public static int CombineHashCodes(int[] hashCodes)
		{
			int hash = 5381;

			foreach (var hashCode in hashCodes)
				hash = ((hash << 5) + hash) ^ hashCode;

			return hash;
		}

		public abstract WwiseObjectReference ObjectReference { get; set; }

		public abstract WwiseObjectType WwiseObjectType { get; }

		public virtual string Name
		{
			get { return IsValid() ? ObjectReference.DisplayName : string.Empty; }
		}

		public uint Id
		{
			get { return IsValid() ? ObjectReference.Id : AkSoundEngine.AK_INVALID_UNIQUE_ID; }
		}
		public static uint InvalidId
		{
			get { return AkSoundEngine.AK_INVALID_UNIQUE_ID; }
		}

		public virtual bool IsValid()
		{
			return ObjectReference != null;
		}

		public bool Validate()
		{
			if (IsValid())
				return true;

			UnityEngine.Debug.LogWarning("Wwise ID has not been resolved. Consider picking a new " + GetType().Name + ".");
			return false;
		}

		protected void Verify(AKRESULT result)
		{
#if UNITY_EDITOR
			if (result != AKRESULT.AK_Success && AkSoundEngine.IsInitialized())
				UnityEngine.Debug.LogWarning("Unsuccessful call made on " + GetType().Name + ".");
#endif
		}

		public override string ToString()
		{
			return IsValid() ? ObjectReference.ObjectName : ("Empty " + GetType().Name);
		}

#if UNITY_EDITOR
		public void SetupReference(string name, System.Guid guid)
		{
			ObjectReference = WwiseObjectReference.FindOrCreateWwiseObject(WwiseObjectType, name, guid);
		}
#endif

		public override int GetHashCode()
		{
			int[] hashCodes = new[]
			{
				ObjectReference.GetHashCode(),
				WwiseObjectType.GetHashCode(),
				Name.GetHashCode(),
				Id.GetHashCode()
			};

			return CombineHashCodes(hashCodes);
		}

		#region Obsolete
		[System.Obsolete(AkSoundEngine.Deprecation_2018_1_2)]
		public int ID
		{
			get { return (int)Id; }
		}

		[System.Obsolete(AkSoundEngine.Deprecation_2018_1_6)]
		public byte[] valueGuid
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
		[UnityEngine.Serialization.FormerlySerializedAs("ID")]
		private int idInternal;
		[UnityEngine.HideInInspector]
		[UnityEngine.SerializeField]
		[UnityEngine.Serialization.FormerlySerializedAs("valueGuid")]
		private byte[] valueGuidInternal;
#pragma warning restore 0414 // private field assigned but not used.
		#endregion
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.