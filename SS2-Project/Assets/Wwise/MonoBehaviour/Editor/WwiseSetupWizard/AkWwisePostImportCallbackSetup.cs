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
using UnityEditor;

[UnityEditor.InitializeOnLoad]
public class AkWwisePostImportCallbackSetup
{
	private static int m_scheduledMigrationStart;
	private static bool m_scheduledReturnToLauncher;
	private static bool m_pendingExecuteMethodCalled;
	private static string s_CurrentScene;

	static AkWwisePostImportCallbackSetup()
	{
		if (UnityEditor.AssetDatabase.IsAssetImportWorkerProcess())
		{
			return;
		}

		var arguments = System.Environment.GetCommandLineArgs();
		if (UnityEngine.Application.isBatchMode
			&& System.Array.IndexOf(arguments, "-wwiseEnableWithNoGraphics") == -1)
			return;

		UnityEditor.EditorApplication.delayCall += CheckMigrationStatus;

		AkUtilities.GetEventDurations = (uint eventID, ref float maximum, ref float minimum) =>
		{
			var eventInfo = AkWwiseProjectInfo.GetData().GetEventInfo(eventID);
			if (eventInfo != null)
			{
				minimum = eventInfo.minDuration;
				maximum = eventInfo.maxDuration;
			}
		};
	}

	private static void CheckMigrationStatus()
	{
		try
		{
			int migrationStart;
			bool returnToLauncher;
			if (IsMigrationPending(out migrationStart, out returnToLauncher))
			{
				m_scheduledMigrationStart = migrationStart;
				m_scheduledReturnToLauncher = returnToLauncher;
				ScheduleMigration();
			}
			else
			{
				RefreshCallback();
			}
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogError("WwiseUnity: Error during migration: " + e);
		}
	}

	private static void ScheduleMigration()
	{
		// TODO: Is delayCall wiped out during a script reload?
		// If not, guard against having a delayCall from a previously loaded code being run after the new loading.

		if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || UnityEditor.EditorApplication.isCompiling)
		{
			// Skip if not in the right mode, wait for the next callback to see if we can proceed then.
			UnityEditor.EditorApplication.delayCall += ScheduleMigration;
			return;
		}

		try
		{
			WwiseSetupWizard.PerformMigration(m_scheduledMigrationStart);

			// Force the user to return to the launcher to perform the post-installation process if necessary
			if (m_scheduledReturnToLauncher)
			{
				if (UnityEditor.EditorUtility.DisplayDialog("Wwise Migration Successful!",
					"Please close Unity and go back to the Wwise Launcher to complete the installation.", "Quit"))
					UnityEditor.EditorApplication.Exit(0);
			}
			else
				UnityEditor.EditorApplication.delayCall += RefreshCallback;
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogError("WwiseUnity: Error during migration: " + e);
		}
	}

	private static bool IsMigrationPending(out int migrationStart, out bool returnToLauncher)
	{
		migrationStart = 0;
		returnToLauncher = false;

		var filename = UnityEngine.Application.dataPath + "/../.WwiseLauncherLockFile";

		if (!System.IO.File.Exists(filename))
			return false;

		var fileContent = System.IO.File.ReadAllText(filename);

		// Instantiate the regular expression object.
		var r = new System.Text.RegularExpressions.Regex(
			"{\"migrateStart\":(\\d+),\"migrateStop\":(\\d+)(,\"returnToLauncher\":(true|false))?,.*}",
			System.Text.RegularExpressions.RegexOptions.IgnoreCase);

		// Match the regular expression pattern against a text string.
		var m = r.Match(fileContent);

		if (!m.Success || m.Groups.Count < 2 || m.Groups[1].Captures.Count < 1 || m.Groups[2].Captures.Count < 1 ||
		    !int.TryParse(m.Groups[1].Captures[0].ToString(), out migrationStart))
			throw new System.Exception("Error in the file format of .WwiseLauncherLockFile.");

		// Handle optional properties
		if (m.Groups.Count > 3 && m.Groups[4].Captures.Count > 0)
			bool.TryParse(m.Groups[4].Captures[0].ToString(), out returnToLauncher);

		return true;
	}

	private static void RefreshCallback()
	{
		PostImportFunction();
		if (WwiseSettings.Exists)
		{
			AkPluginActivator.Update();
			AkPluginActivator.ActivatePluginsForEditor();
		}
	}

	private static void PostImportFunction()
	{
#if UNITY_2018_1_OR_NEWER
        UnityEditor.EditorApplication.hierarchyChanged += CheckWwiseGlobalExistance;
#else
        UnityEditor.EditorApplication.hierarchyWindowChanged += CheckWwiseGlobalExistance;
#endif
        UnityEditor.EditorApplication.delayCall += CheckPicker;

		if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || UnityEditor.EditorApplication.isCompiling)
			return;

		try
		{
			if (!string.IsNullOrEmpty(AkWwiseEditorSettings.Instance.WwiseProjectPath))
			{
				AkWwisePicker.Refresh(ignoreIfWaapi: true); 
				if (AkWwiseProjectInfo.GetData().autoPopulateEnabled)
					AkWwiseWWUBuilder.StartWWUWatcher();
			}
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.Log(e.ToString());
		}

		//Check if a WwiseGlobal object exists in the current scene	
		CheckWwiseGlobalExistance();
	}

	private static void RefreshPlugins()
	{
		if (string.IsNullOrEmpty(AkWwiseProjectInfo.GetData().CurrentPluginConfig))
			AkWwiseProjectInfo.GetData().CurrentPluginConfig = AkPluginActivatorConstants.CONFIG_PROFILE;

		AkPluginActivator.ActivatePluginsForEditor();
	}

	public static void CheckPicker()
	{
		if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || UnityEditor.EditorApplication.isCompiling)
		{
			// Skip if not in the right mode, wait for the next callback to see if we can proceed then.
			UnityEditor.EditorApplication.delayCall += CheckPicker;
			return;
		}

		var settings = AkWwiseEditorSettings.Instance;
		if (!settings.CreatedPicker)
		{
			// Delete all the ghost tabs (Failed to load).
			var windows = UnityEngine.Resources.FindObjectsOfTypeAll<UnityEditor.EditorWindow>();
			if (windows != null && windows.Length > 0)
			{
				foreach (var window in windows)
				{
					var windowTitle = window.titleContent.text;

					if (windowTitle.Equals("Failed to load") || windowTitle.Equals("AkWwisePicker"))
					{
						try
						{
							window.Close();
						}
						catch
						{
							// Do nothing here, this shouldn't cause any problem, however there has been
							// occurrences of Unity crashing on a null reference inside that method.
						}
					}
				}
			}

			// TODO: If no scene is loaded and we are using the demo scene, automatically load it to display it.

			// Populate the picker
			AkWwiseProjectInfo.GetData(); // Load data
			if (!string.IsNullOrEmpty(settings.WwiseProjectPath))
			{
				AkWwiseProjectInfo.Populate();
				AkWwisePicker.InitPickerWindow();

				if (AkWwiseProjectInfo.GetData().autoPopulateEnabled)
					AkWwiseWWUBuilder.StartWWUWatcher();

				settings.CreatedPicker = true;
				settings.SaveSettings();
			}
		}

		UnityEditor.EditorApplication.delayCall += CheckPendingExecuteMethod;
	}

	// TODO: Put this in AkUtilities?
	private static void ExecuteMethod(string method)
	{
		string className = null;
		string methodName = null;

		var regex = new System.Text.RegularExpressions.Regex("(.+)\\.(.+)",
			System.Text.RegularExpressions.RegexOptions.IgnoreCase);

		var regexMatchResult = regex.Match(method);

		if (!regexMatchResult.Success || regexMatchResult.Groups.Count < 3 || regexMatchResult.Groups[1].Captures.Count < 1 || regexMatchResult.Groups[2].Captures.Count < 1)
		{
			UnityEngine.Debug.LogError("WwiseUnity: Error parsing wwiseExecuteMethod parameter: " + method);
			return;
		}

		className = regexMatchResult.Groups[1].Captures[0].ToString();
		methodName = regexMatchResult.Groups[2].Captures[0].ToString();

		try
		{
			System.Reflection.MethodInfo methodToExecute = null;

			if (className == "AkTestUtilities")
			{
				var assembly = System.Reflection.Assembly.Load("Ak.Wwise.IntegrationTestsEditor");
				methodToExecute = assembly.GetType(className).GetMethod(methodName,
					System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
			}
			else
			{
				var type = System.Type.GetType(className);
				if (type == null)
				{
					type = System.Type.GetType(className + ", Assembly-CSharp-Editor");
				}
				methodToExecute = type.GetMethod(methodName,
					System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
			}

			if (methodToExecute == null)
			{
				UnityEngine.Debug.LogError("WwiseUnity: Error in AkWwisePostImportCallbackSetup::ExecuteMethod(): Could not find method: " + method);
				return;
			}

			methodToExecute.Invoke(null, null);
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogError("WwiseUnity: Exception caught when calling " + method + ": " + e);
		}
	}

	private static void CheckPendingExecuteMethod()
	{
		var arguments = System.Environment.GetCommandLineArgs();
		var indexOfCommand = System.Array.IndexOf(arguments, "-wwiseExecuteMethod");

		if (!m_pendingExecuteMethodCalled && indexOfCommand != -1 && arguments.Length > indexOfCommand + 1)
		{
			var methodToExecute = arguments[indexOfCommand + 1];

			ExecuteMethod(methodToExecute);
			m_pendingExecuteMethodCalled = true;
		}
	}

	// Called when changes are made to the scene and when a new scene is created.
	public static void CheckWwiseGlobalExistance()
	{
		var activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
		if (!string.IsNullOrEmpty(s_CurrentScene) && s_CurrentScene.Equals(activeSceneName))
			return;

		var settings = AkWwiseEditorSettings.Instance;
		// Look for a game object which has the initializer component
		var AkInitializers = UnityEngine.Object.FindObjectsOfType<AkInitializer>();
		if (AkInitializers.Length == 0)
		{
			if (settings.CreateWwiseGlobal)
			{
				UnityEngine.Debug.LogFormat("WwiseUnity: No Wwise object in the scene ({0}), creating one.", s_CurrentScene);
				//No Wwise object in this scene, create one so that the sound engine is initialized and terminated properly even if the scenes are loaded
				//in the wrong order.
				var objWwise = new UnityEngine.GameObject("WwiseGlobal");

				//Attach initializer and terminator components
				var akInitializer = UnityEditor.Undo.AddComponent<AkInitializer>(objWwise);
				akInitializer.InitializeInitializationSettings();

			}
		}
		else if (AkInitializers.Length > 0)
		{
			foreach(AkInitializer Initializer in AkInitializers)
			{
				if (!Initializer.InitializationSettings)
				{
					UnityEngine.Debug.LogFormat("WwiseUnity: Initializing {0} (GO {1}).", Initializer.name, Initializer.gameObject.name);
					Initializer.InitializeInitializationSettings();
				}
			}
		}
		else if (settings.CreateWwiseGlobal == false && AkInitializers[0].gameObject.name == "WwiseGlobal")
		{
			UnityEngine.Debug.LogFormat("WwiseUnity: CreateWwiseGlobal is false. Removing the AkInitializer in scene ({0}).", s_CurrentScene);
			UnityEditor.Undo.DestroyObjectImmediate(AkInitializers[0].gameObject);
		}

#if AK_WWISE_ADDRESSABLES && UNITY_ADDRESSABLES
		var bankHolder = UnityEngine.Object.FindObjectOfType<AK.Wwise.Unity.WwiseAddressables.InitBankHolder>();
		if (bankHolder == null)
		{
			UnityEngine.GameObject wwiseGlobalObject = UnityEngine.GameObject.Find("WwiseGlobal");
			if (wwiseGlobalObject != null)
			{
				bankHolder = UnityEditor.Undo.AddComponent<AK.Wwise.Unity.WwiseAddressables.InitBankHolder>(wwiseGlobalObject);
			}
		}
		
		if (bankHolder!=null && bankHolder.InitBank == null)
		{
#if WWISE_ADDRESABLES_2022_1_0_OR_NEWER
			var initBankPath = System.IO.Path.Combine(AkWwiseEditorSettings.WwiseScriptableObjectRelativePath, "InitBank.asset");
			var initbank = UnityEditor.AssetDatabase.LoadAssetAtPath<AK.Wwise.Unity.WwiseAddressables.WwiseInitBankReference>(initBankPath);
			if (initbank)
			{
				bankHolder.InitBank = initbank;
				EditorUtility.SetDirty(bankHolder);
			}
#else
			var initBankPath = System.IO.Path.Combine("Assets",settings.GeneratedSoundbanksPath,"Init.asset");
			var initbank = UnityEditor.AssetDatabase.LoadAssetAtPath<AK.Wwise.Unity.WwiseAddressables.WwiseAddressableSoundBank>(initBankPath);
			bankHolder.InitBank = initbank;
			EditorUtility.SetDirty(bankHolder);
#endif

		}
#endif

		if (settings.CreateWwiseListener)
		{
			WwiseSetupWizard.AddAkAudioListenerToMainCamera(true);
		}

		s_CurrentScene = activeSceneName;
	}
}

#endif // UNITY_EDITOR