#if UNITY_EDITOR
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
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

public static class AkWwiseProjectInfo
{
	private const string _dataFileName = "AkWwiseProjectData.asset";
	private static string s_wwiseEditorDirectory = System.IO.Path.Combine("Wwise", "Editor").Replace('\\','/');
	private static string s_dataRelativeDirectory = System.IO.Path.Combine(s_wwiseEditorDirectory, "ProjectData").Replace('\\','/');
	private static string s_dataRelativePath = System.IO.Path.Combine(s_dataRelativeDirectory, _dataFileName).Replace('\\','/');
	private static string s_dataAssetPath = System.IO.Path.Combine("Assets", s_dataRelativePath).Replace('\\','/');

	public static AkWwiseProjectData ProjectData;
	private static AkWwiseTreeWAAPIDataSource _waapiPickerData = new AkWwiseTreeWAAPIDataSource();
	private static AkWwiseTreeProjectDataSource _projectPickerData = new AkWwiseTreeProjectDataSource();

	public enum DataSourceType
	{
		WwiseAuthoring,
		FileSystem
	}

	public static AkWwiseTreeWAAPIDataSource WaapiPickerData
	{
		get
		{
			return _waapiPickerData;
		}
	}

	public static AkWwiseTreeProjectDataSource ProjectPickerData
	{
		get
		{
			return _projectPickerData;
		}
	}

	public static AkWwiseTreeDataSource GetTreeData()
	{
		AkWwiseTreeDataSource treeData;
		if (GetData().currentDataSource == DataSourceType.WwiseAuthoring)
		{
			treeData = WaapiPickerData;
		}
		else
		{
			treeData = ProjectPickerData;
		}
		return treeData;
	}

	private static bool WwiseFolderExists()
	{
		return System.IO.Directory.Exists(System.IO.Path.Combine(UnityEngine.Application.dataPath, "Wwise"));
	}

	public static AkWwiseProjectData GetData()
	{
		if (ProjectData == null && WwiseFolderExists())
		{
			try
			{
				ProjectData = UnityEditor.AssetDatabase.LoadAssetAtPath<AkWwiseProjectData>(s_dataAssetPath);

				if (ProjectData == null)
				{
					var dataAbsolutePath = System.IO.Path.Combine(UnityEngine.Application.dataPath, s_dataRelativePath);
					var dataExists = System.IO.File.Exists(dataAbsolutePath);
					
					if (dataExists)
					{
						UnityEngine.Debug.LogWarning("WwiseUnity: Unable to load asset at <" + dataAbsolutePath + ">.");
					}
					else
					{
						var dataAbsoluteDirectory = System.IO.Path.Combine(UnityEngine.Application.dataPath, s_dataRelativeDirectory);
						if (!System.IO.Directory.Exists(dataAbsoluteDirectory))
							System.IO.Directory.CreateDirectory(dataAbsoluteDirectory);
					}

					CreateWwiseProjectData();
				}
			}
			catch (System.Exception e)
			{
				UnityEngine.Debug.LogError("WwiseUnity: Unable to load Wwise Data: " + e);
			}
		}

		return ProjectData;
	}

	private static void CreateWwiseProjectData()
	{
		ProjectData = UnityEngine.ScriptableObject.CreateInstance<AkWwiseProjectData>();
		//ProjectData is null when CreateInstance is called too early during editor initialization
		if (ProjectData != null)
		{
			//Handle edge cases where we might queue up multiple calls to CreateWwiseProjectData
			//This happens on editor open if the asset is deleted while Unity is closed
			if (!UnityEditor.AssetDatabase.Contains(ProjectData))
			{
				Debug.Log("WwiseUnity : Created new AkWwiseProjectData asset");
				UnityEditor.AssetDatabase.CreateAsset(ProjectData, s_dataAssetPath);
			}
		}
		else
		{
			Debug.Log("WwiseUnity : Can't create AkWwiseProjectData asset because it is null");
		}
	}

	public static bool Populate()
	{
		var bDirty = false;
		if (AkUtilities.IsWwiseProjectAvailable)
		{
			bDirty = AkWwiseWWUBuilder.Populate();
			bDirty |= AkWwiseXMLBuilder.Populate();
			if (bDirty)
			{
				UnityEditor.EditorUtility.SetDirty(GetData());
				UnityEditor.AssetDatabase.SaveAssets();
				UnityEditor.AssetDatabase.Refresh();
			}
		}

		return bDirty;
	}
}
#endif
