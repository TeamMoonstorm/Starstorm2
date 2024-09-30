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

using System.IO;
using UnityEditor;

#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES
using AK.Wwise.Unity.WwiseAddressables;
#endif

/// @brief Represents Wwise banks as Unity assets.
/// 
public class WwiseBankReference : WwiseObjectReference
{
#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES
	[UnityEngine.SerializeField, AkShowOnly]
	private WwiseAddressableSoundBank bank;
	
	public WwiseAddressableSoundBank AddressableBank => bank;


#if UNITY_EDITOR

	public override void CompleteData()
	{
		SetAddressableBank(AkAssetUtilities.GetAddressableBankAsset(DisplayName));
	}

	public override bool IsComplete()
	{
		return bank != null;
	}

	public void SetAddressableBank(WwiseAddressableSoundBank asset)
	{
		bank = asset;
		EditorUtility.SetDirty(this);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	public bool UpdateAddressableBankReference(WwiseAddressableSoundBank asset, string name)
	{
		if (name == ObjectName)
		{
			SetAddressableBank(asset);
			return true;
		}
		return false;
	}

	public static bool FindBankReferenceAndSetAddressableBank(WwiseAddressableSoundBank addressableAsset, string name)
	{
		var guids = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(WwiseBankReference).Name);
		WwiseBankReference asset;
		foreach (var assetGuid in guids)
		{
			var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(assetGuid);
			asset = UnityEditor.AssetDatabase.LoadAssetAtPath<WwiseBankReference>(assetPath);
			if (asset && asset.ObjectName == name)
			{
				asset.SetAddressableBank(addressableAsset);
				return true;
			}
		}
		return false;
	}

#endif
#endif

	public override WwiseObjectType WwiseObjectType { get { return WwiseObjectType.Soundbank; } }
}
