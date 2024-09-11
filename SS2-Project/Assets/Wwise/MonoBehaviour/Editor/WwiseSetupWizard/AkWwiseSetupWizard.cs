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

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
public class WwiseSetupWizard
{
	public static void RunModify()
	{
		try
		{
			UnityEngine.Debug.Log("WwiseUnity: Running modify setup...");

			UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects);

			ModifySetup();

			UnityEngine.Debug.Log("WwiseUnity: Refreshing asset database.");
			UnityEditor.AssetDatabase.Refresh();

            // IMPORTANT: This log line is parsed by the Launcher. Do not modify it.
			UnityEngine.Debug.Log("WwiseUnity: End of setup, exiting Unity.");
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogError("WwiseUnity: Exception caught: " + e);
		}
	}

	public static void RunSetup()
	{
		try
		{
			UnityEngine.Debug.Log("WwiseUnity: Running install setup...");

			Setup();

			UnityEngine.Debug.Log("WwiseUnity: Refreshing asset database.");
			UnityEditor.AssetDatabase.Refresh();

            // IMPORTANT: This log line is parsed by the Launcher. Do not modify it.
			UnityEngine.Debug.Log("WwiseUnity: End of setup, exiting Unity.");
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogError("WwiseUnity: Exception caught: " + e);
		}
	}

	public static void RunDemoSceneSetup()
	{
		try
		{
			UnityEngine.Debug.Log("WwiseUnity: Running demo scene setup...");

			Setup();

			UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/WwiseDemoScene/WwiseDemoScene.unity");

			UnityEngine.Debug.Log("WwiseUnity: Refreshing asset database.");
			UnityEditor.AssetDatabase.Refresh();

			UnityEngine.Debug.Log("WwiseUnity: End of demo scene setup, exiting Unity.");
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogError("WwiseUnity: Exception caught: " + e);
		}
	}

	private static int TotalNumberOfSections = 3;
	private static void UpdateProgressBar(int sectionIndex, int subSectionIndex = 0, int totalSubSections = 1)
	{
		subSectionIndex = UnityEngine.Mathf.Clamp(subSectionIndex, 0, totalSubSections);
		sectionIndex = UnityEngine.Mathf.Clamp(sectionIndex, 0, TotalNumberOfSections);

		float progress = ((float)subSectionIndex / totalSubSections + sectionIndex) / TotalNumberOfSections;
		UnityEditor.EditorUtility.DisplayProgressBar("Wwise Integration", "Migration in progress - Please wait...", progress);
	}

	public static void RunMigrate()
	{
		try
		{
			UnityEngine.Debug.Log("WwiseUnity: Running migration setup...");
			UnityEngine.Debug.Log("WwiseUnity: Reading parameters...");

			var arguments = System.Environment.GetCommandLineArgs();
			string migrateStartString = null;
			var indexMigrateStart = System.Array.IndexOf(arguments, "-wwiseInstallMigrateStart");

			if (indexMigrateStart != -1)
				migrateStartString = arguments[indexMigrateStart + 1];
			else
			{
				UnityEngine.Debug.LogError("WwiseUnity: ERROR: Missing parameter wwiseInstallMigrateStart.");
                return;
			}

			int migrateStart;

			if (!int.TryParse(migrateStartString, out migrateStart))
			{
				UnityEngine.Debug.LogError("WwiseUnity: ERROR: wwiseInstallMigrateStart is not a number.");
				return;
			}

			PerformMigration(migrateStart - 1);

			UnityEditor.AssetDatabase.SaveAssets();

			UnityEngine.Debug.Log("WwiseUnity: Refreshing asset database.");
			UnityEditor.AssetDatabase.Refresh();

            // IMPORTANT: This log line is parsed by the Launcher. Do not modify it.
			UnityEngine.Debug.Log("WwiseUnity: End of setup, exiting Unity.");
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogError("WwiseUnity: Exception caught: " + e);
		}
	}

	private static void MigrateCurrentScene(System.Type[] wwiseComponentTypes)
	{
		var objectTypeMap = new System.Collections.Generic.Dictionary<System.Type, UnityEngine.Object[]>();

		foreach (var objectType in wwiseComponentTypes)
		{
			// Get all objects in the scene with the specified type.
			var objects = UnityEngine.Object.FindObjectsOfType(objectType);
			if (objects != null && objects.Length > 0)
				objectTypeMap[objectType] = objects;
		}

		for (var ii = AkUtilities.MigrationStartIndex; ii < AkUtilities.MigrationStopIndex; ++ii)
		{
			var migrationMethodName = "Migrate" + ii;
			var preMigrationMethodName = "PreMigration" + ii;
			var postMigrationMethodName = "PostMigration" + ii;

			foreach (var objectTypePair in objectTypeMap)
			{
				var objectType = objectTypePair.Key;
				var objects = objectTypePair.Value;
				var className = objectType.Name;

				var preMigrationMethodInfo = objectType.GetMethod(preMigrationMethodName,
					System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
				if (preMigrationMethodInfo != null)
				{
					UnityEngine.Debug.Log("WwiseUnity: PreMigration step <" + ii + "> for class <" + className + ">");
					preMigrationMethodInfo.Invoke(null, null);
				}

				var migrationMethodInfo = objectType.GetMethod(migrationMethodName,
					System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
				if (migrationMethodInfo != null)
				{
					UnityEngine.Debug.Log("WwiseUnity: Migration step <" + ii + "> for class <" + className + ">");

					// Call the migration method of each object.
					foreach (var currentObject in objects)
						migrationMethodInfo.Invoke(currentObject, null);
				}

				var postMigrationMethodInfo = objectType.GetMethod(postMigrationMethodName,
					System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
				if (postMigrationMethodInfo != null)
				{
					UnityEngine.Debug.Log("WwiseUnity: PostMigration step <" + ii + "> for class <" + className + ">");
					postMigrationMethodInfo.Invoke(null, null);
				}
			}
		}
	}

	private static System.Type[] GetWwiseComponentTypes()
	{
		var wwiseComponentTypes = new System.Collections.Generic.List<System.Type>();

		var wwiseComponentFolder = UnityEngine.Application.dataPath + "/Wwise/MonoBehaviour/Runtime";
		if (System.IO.Directory.Exists(wwiseComponentFolder)) 
		{
			var files = new System.IO.DirectoryInfo(wwiseComponentFolder).GetFiles("*.cs", System.IO.SearchOption.AllDirectories);
			foreach (var file in files)
			{
				var className = System.IO.Path.GetFileNameWithoutExtension(file.Name);
				var objectType = System.Type.GetType(className + ", AK.Wwise.Unity.MonoBehaviour");
				if (objectType != null && objectType.IsSubclassOf(typeof(UnityEngine.Object)))
				{
					wwiseComponentTypes.Add(objectType);
				}
			}
		}

		return wwiseComponentTypes.ToArray();
	}

	private static void MigrateObject(UnityEngine.Object obj)
	{
		if (obj == null)
		{
			UnityEngine.Debug.LogWarning("WwiseUnity: Missing script! Please consider resolving the missing scripts before migrating your Unity project. Any WwiseType on this object will NOT be migrated!");
			return;
		}

		var migratable = obj as AK.Wwise.IMigratable;
		if (migratable == null && !AkUtilities.IsMigrationRequired(AkUtilities.MigrationStep.WwiseTypes_v2018_1_6))
			return;

		var hasChanged = false;
		var serializedObject = new UnityEditor.SerializedObject(obj);
		if (migratable != null)
			hasChanged = migratable.Migrate(serializedObject);
		else
			hasChanged = AK.Wwise.TypeMigration.SearchAndProcessWwiseTypes(serializedObject.GetIterator());

		if (hasChanged)
			serializedObject.ApplyModifiedPropertiesWithoutUndo();
	}

	private static void MigratePrefabs()
	{
		// The only migration operation done in this method is to call MigrateObject on MonoBehaviours attached to prefabs.
		// MigrateObject only runs if a migration is required for the "WwiseTypes_v2018_1_6" migration step.
		// Add an early return here to avoid lots of potentially slow code if we are in a case where MigrateObject would
		// do nothing.
		if (!AkUtilities.IsMigrationRequired(AkUtilities.MigrationStep.WwiseTypes_v2018_1_6))
		{
			return;
		}

		var guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
		for (var i = 0; i < guids.Length; i++)
		{
			UpdateProgressBar(0, i, guids.Length);

			var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
			UnityEngine.Debug.Log("WwiseUnity: Migrating prefab: " + path);

			var prefabObject = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(path);
			if (prefabObject == null)
			{
				UnityEngine.Debug.LogWarning("WwiseUnity: Failed to migrate prefab: " + path);
				continue;
			}

			var objects = prefabObject.GetComponents<UnityEngine.MonoBehaviour>();
			// The rather convoluted way of iterating through all objects here has a very specific reason.
			// The call to MigrateObject ends up calling SerializedObject.ApplyModifiedPropertiesWithoutUndo.
			// This function call will invalidate all references that are held by the code that is running
			// (the objects array here).
			// In order to iterate properly on all MonoBehaviours available, we get their instance IDs,
			// which do not change when Applying modified properties. Then, for each iteration of the 
			// migration loop, we need to get a valid array of MonoBehaviours again, because it might
			// have been invalidated by the call to MigrateObject. We then migrate the objects that
			// need migration by making sure their InstanceID is in the list of unmigrated MonoBehaviours.
			var instanceIds = new System.Collections.Generic.List<int>();
			foreach (var obj in objects)
			{
				if (obj == null)
					continue;

				var id = obj.GetInstanceID();
				if (!instanceIds.Contains(id))
					instanceIds.Add(id);
			}

			for (; instanceIds.Count > 0; instanceIds.RemoveAt(0))
			{
				var id = instanceIds[0];
				var obj = UnityEditor.EditorUtility.InstanceIDToObject(id);
				if (obj && obj.GetInstanceID() == id)
				{
					MigrateObject(obj);
				}
			}
		}
	}

	private static string[] ScriptableObjectGuids = null;

	private static bool ShouldProcessScriptableObject(UnityEngine.Object obj)
	{
		if (obj == null)
			return true;

		if (!(obj is UnityEngine.ScriptableObject))
			return false;

		if (obj is UnityEngine.GUISkin)
			return false;

		if (obj is AkWwiseProjectData)
			return false;

		if (obj is AkWwiseInitializationSettings)
			return false;

		if (obj is AkCommonPlatformSettings)
			return false;

		if (obj is WwiseObjectReference)
			return false;

		return true;
	}

	private static void MigrateScriptableObjects()
	{
		var guids = ScriptableObjectGuids;
		if (guids == null)
			return;

		var processedGuids = new System.Collections.Generic.HashSet<string>();

		for (var i = 0; i < guids.Length; i++)
		{
			UpdateProgressBar(2, i, guids.Length);

			var guid = guids[i];
			if (processedGuids.Contains(guid))
				continue;

			processedGuids.Add(guid);

			var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
			UnityEngine.Debug.Log("WwiseUnity: Migrating ScriptableObject: " + path);

			var objects = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
			foreach (var obj in objects)
			{
				if (ShouldProcessScriptableObject(obj))
				{
					MigrateObject(obj);
				}
			}
		}
	}

	private static void MigrateScenes()
	{
		var wwiseComponentTypes = GetWwiseComponentTypes();
		var guids = UnityEditor.AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
		for (var i = 0; i < guids.Length; i++)
		{
			UpdateProgressBar(1, i, guids.Length);

			var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
			UnityEngine.Debug.Log("WwiseUnity: Migrating scene: " + path);

			UnityEditor.EditorUtility.UnloadUnusedAssetsImmediate();

			var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path);

			MigrateCurrentScene(wwiseComponentTypes);

			// From this point on, the only migration operation done in this loop is to call MigrateObject on MonoBehaviours.
			// MigrateObject only runs if a migration is required for the "WwiseTypes_v2018_1_6" migration step. Simply
			// continue the loop here to avoid lots of potentially slow code if we are in a case where MigrateObject would
			// do nothing.
			if (!AkUtilities.IsMigrationRequired(AkUtilities.MigrationStep.WwiseTypes_v2018_1_6))
			{
				continue;
			}
			var objects = UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.MonoBehaviour>();

			// The rather convoluted way of iterating through all objects here has a very specific reason.
			// The call to MigrateObject ends up calling SerializedObject.ApplyModifiedPropertiesWithoutUndo.
			// This function call will invalidate all references that are held by the code that is running
			// (the objects array here).
			// In order to iterate properly on all MonoBehaviours available, we get their instance IDs,
			// which do not change when Applying modified properties. Then, for each iteration of the 
			// migration loop, we need to get a valid array of MonoBehaviours again, because it might
			// have been invalidated by the call to MigrateObject. We then migrate the objects that
			// need migration by making sure their InstanceID is in the list of unmigrated MonoBehaviours.
			var instanceIds = new System.Collections.Generic.List<int>();
			foreach (var obj in objects)
			{
				if (obj == null)
					continue;

				var id = obj.GetInstanceID();
				if (!instanceIds.Contains(id))
					instanceIds.Add(id);
			}

			for (; instanceIds.Count > 0; instanceIds.RemoveAt(0))
			{
				var id = instanceIds[0];
				var obj = UnityEditor.EditorUtility.InstanceIDToObject(id);
				if (obj && obj.GetInstanceID() == id)
				{
					MigrateObject(obj);
				}
			}

			if (UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene))
				if (!UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene))
					throw new System.Exception("Error occurred while saving migrated scenes.");

			UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
		}
	}

	public static void PerformMigration(int migrateStart)
	{
		AkUtilities.BeginMigration(migrateStart);

		UpdateProgressBar(0);

		UnityEngine.Debug.Log("WwiseUnity: Migrating from Unity Integration Version " + migrateStart + " to " + AkUtilities.MigrationStopIndex);

		AkPluginActivator.DeactivateAllPlugins();
		AkPluginActivator.Update();
		AkPluginActivator.ActivatePluginsForEditor();

		// Get the name of the currently opened scene.
		var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
		var loadedScenePath = activeScene.path;

		if (!string.IsNullOrEmpty(loadedScenePath))
			AkUtilities.FixSlashes(ref loadedScenePath, '\\', '/', false);

		UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects);

		// obtain a list of ScriptableObjects before any migration is performed
		ScriptableObjectGuids = UnityEditor.AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets" });

		if (AkUtilities.IsMigrationRequired(AkUtilities.MigrationStep.NewScriptableObjectFolder_v2019_2_0))
		{
			var oldScriptableObjectPath = System.IO.Path.Combine(System.IO.Path.Combine("Assets", "Wwise"), "Resources");
			AkUtilities.MoveFolder(oldScriptableObjectPath, AkWwiseEditorSettings.WwiseScriptableObjectRelativePath);
		}

		if (!UnityEditor.AssetDatabase.IsValidFolder(AkWwiseEditorSettings.WwiseScriptableObjectRelativePath))
		{
			UnityEngine.Debug.LogFormat("WwiseUnity: Creating ScriptableObjects folder at <{0}>", AkWwiseEditorSettings.WwiseScriptableObjectRelativePath);
			AkUtilities.CreateFolder(AkWwiseEditorSettings.WwiseScriptableObjectRelativePath);
		}

		AkWwiseProjectInfo.GetData().Migrate();
		AkWwiseWWUBuilder.UpdateWwiseObjectReferenceData();

		UnityEngine.Debug.LogFormat("WwiseUnity: Migrating Prefabs...");
		MigratePrefabs();
		UnityEngine.Debug.LogFormat("WwiseUnity: Done migrating Prefabs");
		
		UnityEngine.Debug.LogFormat("WwiseUnity: Migrating Scenes...");
		MigrateScenes();
		UnityEngine.Debug.LogFormat("WwiseUnity: Done migrating Scenes");
		
		UnityEngine.Debug.LogFormat("WwiseUnity: Migrating ScriptableObjects...");
		MigrateScriptableObjects();
		UnityEngine.Debug.LogFormat("WwiseUnity: Done migrating ScriptableObjects");

		UnityEditor.EditorUtility.UnloadUnusedAssetsImmediate();
		
		UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects);
		AkUtilities.EndMigration();
		
		UpdateProgressBar(TotalNumberOfSections);

		// Reopen the scene that was opened before the migration process started.
		if (!string.IsNullOrEmpty(loadedScenePath))
		{
			UnityEditor.SceneManagement.EditorSceneManager.OpenScene(loadedScenePath);
		}

		SetAddressablesDefines();

		UnityEngine.Debug.Log("WwiseUnity: Removing lock for launcher.");

		// TODO: Moving one folder up is not nice at all. How to find the current project path?
		try
		{
			System.IO.File.Delete(UnityEngine.Application.dataPath + "/../.WwiseLauncherLockFile");
		}
		catch
		{
			// Ignore if not present.
		}

		UnityEditor.EditorUtility.ClearProgressBar();
	}

	public static void ModifySetup()
	{
		var currentConfig = AkPluginActivator.GetCurrentConfig();

		if (string.IsNullOrEmpty(currentConfig))
			currentConfig = AkPluginActivatorConstants.CONFIG_PROFILE;

		AkPluginActivator.DeactivateAllPlugins();
		AkPluginActivator.Update();
		AkPluginActivator.ActivatePluginsForEditor();
		
		SetAddressablesDefines();
	}

	// Perform all necessary steps to use the Wwise Unity integration.
	private static void Setup()
	{
		UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects);

		AkPluginActivator.IsVerboseLogging = true;
		UnityEngine.Debug.Log("WwiseUnity: Deactivating all plugins...");
		AkPluginActivator.DeactivateAllPlugins();

		// 0. Make sure the SoundBank directory exists
		var sbPath = AkUtilities.GetFullPath(UnityEngine.Application.streamingAssetsPath, AkWwiseEditorSettings.Instance.SoundbankPath);
		if (!System.IO.Directory.Exists(sbPath))
			System.IO.Directory.CreateDirectory(sbPath);

		// 1. Disable built-in audio
		if (!DisableBuiltInAudio())
		{
			UnityEngine.Debug.LogWarning(
				"WwiseUnity: Could not disable built-in audio. Please disable built-in audio by going to Project->Project Settings->Audio, and check \"Disable Audio\".");
		}

		// 2. Create a "WwiseGlobal" game object and set the AkSoundEngineInitializer and terminator scripts
		// 3. Set the SoundBank path property on AkSoundEngineInitializer
		CreateWwiseGlobalObject();

		// 5. Disable the built-in audio listener, and add AkAudioListener component to camera
		if (AkWwiseEditorSettings.Instance.CreateWwiseListener)
		{
			AddAkAudioListenerToMainCamera();
		}

		// 6. Enable "Run In Background" in PlayerSettings (PlayerSettings.runInbackground property)
		UnityEditor.PlayerSettings.runInBackground = true;

		UnityEngine.Debug.Log("WwiseUnity: Updating PluginActivator...");
		AkPluginActivator.Update();
		UnityEngine.Debug.Log("WwiseUnity: Activating plugins for editor...");
		AkPluginActivator.ActivatePluginsForEditor();

		// 9. Activate WwiseIDs file generation, and point Wwise to the Assets/Wwise folder
		// 10. Change the SoundBanks options so it adds Max Radius information in the Wwise project
		if (!SetSoundbankSettings())
			UnityEngine.Debug.LogWarning("WwiseUnity: Could not modify Wwise Project to generate the header file!");

		// 11. Activate XboxOne network sockets.
		AkXboxOneUtils.EnableXboxOneNetworkSockets();
		
		// 12. Add addressables version define
		SetAddressablesDefines();
	}

	private static HashSet<BuildTargetGroup> AvailableBuildTargetGroups = new HashSet<BuildTargetGroup>();

	public static void AddBuildTargetGroup(BuildTargetGroup NewGroup)
	{
		AvailableBuildTargetGroups.Add(NewGroup);
	}
	private static void SetAddressablesDefines()
	{
		string wwiseVersion = AkSoundEngine.WwiseVersion;
		string shortWwiseVersion = wwiseVersion.Substring(0, 4);
		int wwiseVersionAsInteger = int.Parse(shortWwiseVersion);
		string wwiseAddressablePost2023 = "WWISE_ADDRESSABLES_POST_2023";

		if (wwiseVersionAsInteger >= 2023)
		{
			foreach (var TargetGroup in AvailableBuildTargetGroups)
			{
				string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(TargetGroup);
				Match match = Regex.Match(defines, wwiseAddressablePost2023);
				if (!match.Success)
				{
					defines += ";" + wwiseAddressablePost2023;
				}

				PlayerSettings.SetScriptingDefineSymbolsForGroup(TargetGroup, defines);
			}
		}
	}

	// Create a Wwise Global object containing the initializer and terminator scripts. Set the SoundBank path of the initializer script.
	// This game object will live for the whole project; there is no need to instanciate one per scene.
	private static void CreateWwiseGlobalObject()
	{
		// Look for a game object which has the initializer component
		var AkInitializers = UnityEngine.Object.FindObjectsOfType<AkInitializer>();
		if (AkInitializers.Length > 0)
			UnityEditor.Undo.DestroyObjectImmediate(AkInitializers[0].gameObject);

		var WwiseGlobalGameObject = new UnityEngine.GameObject("WwiseGlobal");

		// attach initializer component
		UnityEditor.Undo.AddComponent<AkInitializer>(WwiseGlobalGameObject);

		// Set focus on WwiseGlobal
		UnityEditor.Selection.activeGameObject = WwiseGlobalGameObject;
	}

	private static bool DisableBuiltInAudio()
	{
		UnityEditor.SerializedObject audioSettingsAsset = null;
		UnityEditor.SerializedProperty disableAudioProperty = null;

		var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/AudioManager.asset");
		if (assets.Length > 0)
			audioSettingsAsset = new UnityEditor.SerializedObject(assets[0]);

		if (audioSettingsAsset != null)
			disableAudioProperty = audioSettingsAsset.FindProperty("m_DisableAudio");

		if (disableAudioProperty == null)
			return false;

		disableAudioProperty.boolValue = true;
		audioSettingsAsset.ApplyModifiedProperties();
		return true;
	}

	// Modify the .wproj file to set needed SoundBank settings
	private static bool SetSoundbankSettings()
	{
		var settings = AkWwiseEditorSettings.Instance;
		if (string.IsNullOrEmpty(settings.WwiseProjectPath))
			return true;

		var r = new System.Text.RegularExpressions.Regex("_WwiseIntegrationTemp.*?([/\\\\])");
		var SoundbankPath = AkUtilities.GetFullPath(r.Replace(UnityEngine.Application.streamingAssetsPath, "$1"), settings.SoundbankPath);
		var WprojPath = AkUtilities.GetFullPath(UnityEngine.Application.dataPath, settings.WwiseProjectPath);
#if UNITY_EDITOR_OSX
		SoundbankPath = "Z:" + SoundbankPath;
#endif

		SoundbankPath = AkUtilities.MakeRelativePath(System.IO.Path.GetDirectoryName(WprojPath), SoundbankPath);
		if (AkUtilities.EnableBoolSoundbankSettingInWproj("SoundBankGenerateHeaderFile", WprojPath))
			if (AkUtilities.SetSoundbankHeaderFilePath(WprojPath, SoundbankPath))
				return AkUtilities.EnableBoolSoundbankSettingInWproj("SoundBankGenerateMaxAttenuationInfo", WprojPath);

		return false;
	}

	public static void AddAkAudioListenerToMainCamera(bool logWarning = false)
	{
		UnityEngine.Camera camera = UnityEngine.Camera.main;

		// Workaround for some versions of Unity not setting properly the MainCamera tag
		// on the first scene of a new project
		if (camera == null)
		{
			var cameraArray = UnityEngine.Object.FindObjectsOfType<UnityEngine.Camera>();
			if (cameraArray.Length > 0)
			{
				foreach (var entry in cameraArray)
				{
					if (entry.name == "Main Camera")
					{
						camera = entry;
						break;
					}
				}
			}

			if (camera == null)
			{
				return;
			}
		}

		if (camera.GetComponentInChildren<AkAudioListener>())
		{
			return;
		}

		var oldListener = camera.gameObject.GetComponent<UnityEngine.AudioListener>();
		if (oldListener != null)
		{
			UnityEditor.Undo.DestroyObjectImmediate(oldListener);
		}

		var akAudioListener = UnityEditor.Undo.AddComponent<AkAudioListener>(camera.gameObject);
		if (!akAudioListener)
		{
			return;
		}

		var akGameObj = akAudioListener.GetComponentInChildren<AkGameObj>();
		if (akGameObj)
		{
			akGameObj.isEnvironmentAware = false;
		}

		if (logWarning)
		{
			UnityEngine.Debug.LogWarning("Automatically added AkAudioListener to Main Camera. Go to \"Edit > Wwise Settings...\" to disable this functionality.");
		}
	}
}

#endif // UNITY_EDITOR