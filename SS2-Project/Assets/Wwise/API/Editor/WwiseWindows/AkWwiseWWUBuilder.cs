#if UNITY_EDITOR

using System.Collections.Generic;
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

#pragma warning disable 0168
[UnityEditor.InitializeOnLoad]
public class AkWwiseWWUBuilder : UnityEditor.AssetPostprocessor
{
	private const string s_progTitle = "Populating Wwise Picker";
	private const int s_SecondsBetweenChecks = 3;

	private static string s_wwiseProjectPath = System.IO.Path.GetDirectoryName(AkWwiseEditorSettings.WwiseProjectAbsolutePath);

	private static System.DateTime s_lastFileCheck = System.DateTime.Now.AddSeconds(-s_SecondsBetweenChecks);
	private static readonly FileInfo_CompareByPath s_FileInfo_CompareByPath = new FileInfo_CompareByPath();

	private readonly HashSet<string> m_WwuToProcess = new HashSet<string>();

	private int m_currentWwuCnt;
	private int m_totWwuCnt = 1;

	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
	{
		if (UnityEditor.AssetDatabase.IsAssetImportWorkerProcess())
		{
			return;
		}

		if (didDomainReload)
		{
			// This method gets called from InitializeOnLoad and uses the AkWwiseProjectInfo later on so it needs to check if it can run right now
			InitializeWwiseProjectData();
		}
	}
	
	static AkWwiseWWUBuilder()
	{
		UnityEditor.EditorApplication.playModeStateChanged += (UnityEditor.PlayModeStateChange playMode) =>
		{
			if (playMode == UnityEditor.PlayModeStateChange.EnteredEditMode)
			{
				RestartWWUWatcher();
			}
		};
	}

	private static void Tick()
	{
		if (System.DateTime.Now.Subtract(s_lastFileCheck).Seconds < s_SecondsBetweenChecks)
		{
			return;
		}

		if (AkWwiseProjectInfo.GetData() == null)
		{
			return;
		}

		if (UnityEditor.EditorApplication.isCompiling)
		{
			return;
		}

		if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
		{
			return;
		}

		if (!AkWwiseProjectInfo.GetData().autoPopulateEnabled)
		{
			return;
		}

		if (Populate())
		{
			AkWwiseXMLBuilder.Populate();
			//check if WAAPI or not
			AkWwisePicker.Refresh(ignoreIfWaapi: true);
			//Make sure that the Wwise picker and the inspector are updated
			AkUtilities.RepaintInspector();
		}

		AkUtilities.SoundBankDestinationsUpdated(AkWwiseEditorSettings.Instance.WwiseProjectPath);
		s_lastFileCheck = System.DateTime.Now;
	}

	public static void InitializeWwiseProjectData()
	{
		try
		{
			if (string.IsNullOrEmpty(AkWwiseEditorSettings.Instance.WwiseProjectPath))
			{
				UnityEngine.Debug.LogError("WwiseUnity: Wwise project needed to populate from Work Units. Aborting.");
				return;
			}

			var fullWwiseProjectPath = AkWwiseEditorSettings.WwiseProjectAbsolutePath;
			s_wwiseProjectPath = System.IO.Path.GetDirectoryName(fullWwiseProjectPath);

			AkUtilities.IsWwiseProjectAvailable = System.IO.File.Exists(fullWwiseProjectPath);
			if (!AkUtilities.IsWwiseProjectAvailable || UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || string.IsNullOrEmpty(s_wwiseProjectPath) ||
				UnityEditor.EditorApplication.isCompiling)
				return;

			var builder = new AkWwiseWWUBuilder();
			builder.GatherModifiedFiles();
			builder.UpdateFiles();
		}
		catch (System.Exception exception)
		{
			UnityEngine.Debug.LogError("Exception occured while initializing project data : \n" + exception.Message);
		}
	}

	public static bool Populate()
	{
		try
		{
			if (string.IsNullOrEmpty(AkWwiseEditorSettings.Instance.WwiseProjectPath))
			{
				UnityEngine.Debug.LogError("WwiseUnity: Wwise project needed to populate from Work Units. Aborting.");
				return false;
			}

			var fullWwiseProjectPath = AkWwiseEditorSettings.WwiseProjectAbsolutePath;
			s_wwiseProjectPath = System.IO.Path.GetDirectoryName(fullWwiseProjectPath);

			AkUtilities.IsWwiseProjectAvailable = System.IO.File.Exists(fullWwiseProjectPath);
			if (!AkUtilities.IsWwiseProjectAvailable || UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || string.IsNullOrEmpty(s_wwiseProjectPath) ||
				(UnityEditor.EditorApplication.isCompiling && !AkUtilities.IsMigrating))
				return false;

			AkPluginActivator.Update();

			var builder = new AkWwiseWWUBuilder();
			if (!builder.GatherModifiedFiles())
				return false;

			builder.UpdateFiles();
			return true;
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogError(e.ToString());
			UnityEditor.EditorUtility.ClearProgressBar();
			return true;
		}
	}

	public static void UpdateWwiseObjectReferenceData()
	{
		UnityEngine.Debug.Log("WwiseUnity: Updating Wwise Object References");

		WwiseObjectReference.ClearWwiseObjectDataMap();
		UpdateWwiseObjectReference(WwiseObjectType.AuxBus, AkWwiseProjectInfo.GetData().AuxBusWwu);
		UpdateWwiseObjectReference(WwiseObjectType.Event, AkWwiseProjectInfo.GetData().EventWwu);
		UpdateWwiseObjectReference(WwiseObjectType.Soundbank, AkWwiseProjectInfo.GetData().BankWwu);
		UpdateWwiseObjectReference(WwiseObjectType.GameParameter, AkWwiseProjectInfo.GetData().RtpcWwu);
		UpdateWwiseObjectReference(WwiseObjectType.Trigger, AkWwiseProjectInfo.GetData().TriggerWwu);
		UpdateWwiseObjectReference(WwiseObjectType.AcousticTexture, AkWwiseProjectInfo.GetData().AcousticTextureWwu);
		UpdateWwiseObjectReference(WwiseObjectType.StateGroup, WwiseObjectType.State, AkWwiseProjectInfo.GetData().StateWwu);
		UpdateWwiseObjectReference(WwiseObjectType.SwitchGroup, WwiseObjectType.Switch, AkWwiseProjectInfo.GetData().SwitchWwu);
	}

	private int RecurseWorkUnit(AssetType in_type, System.IO.FileInfo in_workUnit, string in_currentPathInProj,
		string in_currentPhysicalPath, LinkedList<AkWwiseProjectData.PathElement> in_pathAndIcons,
		string in_parentPath = "")
	{
		m_WwuToProcess.Remove(in_workUnit.FullName);
		var wwuIndex = -1;
		try
		{
			//Progress bar stuff
			var msg = "Parsing Work Unit " + in_workUnit.Name;
			UnityEditor.EditorUtility.DisplayProgressBar(s_progTitle, msg, m_currentWwuCnt / (float)m_totWwuCnt);
			m_currentWwuCnt++;

			in_currentPathInProj =
				System.IO.Path.Combine(in_currentPathInProj, System.IO.Path.GetFileNameWithoutExtension(in_workUnit.Name));

			var WwuPhysicalPath = System.IO.Path.Combine(in_currentPhysicalPath, in_workUnit.Name);

			var wwu = ReplaceWwuEntry(WwuPhysicalPath, in_type, out wwuIndex);

			wwu.ParentPath = in_currentPathInProj;
			wwu.PhysicalPath = WwuPhysicalPath;
			wwu.Guid = System.Guid.Empty;
			wwu.LastTime = System.IO.File.GetLastWriteTime(in_workUnit.FullName);

			using (var reader = System.Xml.XmlReader.Create(in_workUnit.FullName))
			{
				reader.MoveToContent();
				reader.Read();

				while (!reader.EOF && reader.ReadState == System.Xml.ReadState.Interactive)
				{
					if (reader.NodeType == System.Xml.XmlNodeType.Element && reader.Name.Equals("WorkUnit"))
					{
						if (wwu.Guid.Equals(System.Guid.Empty))
						{
							var ID = reader.GetAttribute("ID");
							try
							{
								wwu.Guid = new System.Guid(ID);
								in_pathAndIcons.AddLast(new AkWwiseProjectData.PathElement(
									System.IO.Path.GetFileNameWithoutExtension(in_workUnit.Name), WwiseObjectType.WorkUnit, wwu.Guid));
								wwu.PathAndIcons = new List<AkWwiseProjectData.PathElement>(in_pathAndIcons);

							}
							catch
							{
								UnityEngine.Debug.LogWarning("WwiseUnity: Error reading ID <" + ID + "> from work unit <" + in_workUnit.FullName + ">.");
								throw;
							}
						}

						var persistMode = reader.GetAttribute("PersistMode");
						if (persistMode == "Reference")
						{
							// ReadFrom advances the reader
							var matchedElement = System.Xml.Linq.XNode.ReadFrom(reader) as System.Xml.Linq.XElement;
							var newWorkUnitPath =
								System.IO.Path.Combine(in_workUnit.Directory.FullName, matchedElement.Attribute("Name").Value + ".wwu");
							var newWorkUnit = new System.IO.FileInfo(newWorkUnitPath);

							if (m_WwuToProcess.Contains(newWorkUnit.FullName))
							{
								// Parse the referenced Work Unit
								RecurseWorkUnit(in_type, newWorkUnit, in_currentPathInProj, in_currentPhysicalPath, in_pathAndIcons,
								WwuPhysicalPath);
							}
						}
						else
						{
							// If the persist mode is "Standalone" or "Nested", it means the current XML tag
							// is the one corresponding to the current file. We can ignore it and advance the reader
							reader.Read();
						}
					}
					else if (reader.NodeType == System.Xml.XmlNodeType.Element && (reader.Name.Equals("AuxBus") || reader.Name.Equals("Folder") || reader.Name.Equals("Bus")))
					{
						WwiseObjectType objType;
						switch (reader.Name)
						{
							case "AuxBus":
								objType = WwiseObjectType.AuxBus;
								break;

							case "Bus":
								objType = WwiseObjectType.Bus;
								break;

							case "Folder":
							default:
								objType = WwiseObjectType.Folder;
								break;
						}
						in_currentPathInProj = System.IO.Path.Combine(in_currentPathInProj, reader.GetAttribute("Name"));
						in_pathAndIcons.AddLast(new AkWwiseProjectData.PathElement(reader.GetAttribute("Name"), objType, new System.Guid(reader.GetAttribute("ID"))));
						bool IsEmptyElement = reader.IsEmptyElement; // Need to cache this because AddElementToList advances the reader.
						AddElementToList(in_currentPathInProj, reader, in_type, in_pathAndIcons, wwu.PhysicalPath, wwuIndex, objType);
						if (IsEmptyElement)
						{
							// This element has no children, step out of it immediately
							// Remove the folder/bus from the path
							in_currentPathInProj = in_currentPathInProj.Remove(in_currentPathInProj.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
							in_pathAndIcons.RemoveLast();
							// Reader was already advanced by AddElementToList
						}
					}
					else if (reader.NodeType == System.Xml.XmlNodeType.EndElement &&
							 (reader.Name.Equals("Folder") || reader.Name.Equals("Bus") || reader.Name.Equals("AuxBus")))
					{
						// Remove the folder/bus from the path
						in_currentPathInProj = in_currentPathInProj.Remove(in_currentPathInProj.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
						in_pathAndIcons.RemoveLast();
						// Advance the reader
						reader.Read();
					}
					else if (reader.NodeType == System.Xml.XmlNodeType.Element && reader.Name.Equals(in_type.XmlElementName))
					{
						// Add the element to the list
						AddElementToList(in_currentPathInProj, reader, in_type, in_pathAndIcons, wwu.PhysicalPath, wwuIndex);
					}
					else
					{
						reader.Read();
					}
				}
			}

			// Sort the newly populated Wwu alphabetically
			SortWwu(in_type, wwuIndex);
		}
		catch (System.Exception e)
		{
			//We have failed to parse a workunit, we can't trust the _WwiseObjectsToRemove will be properly updated
			_WwiseObjectsToRemove.Clear();
			UnityEngine.Debug.LogError(e.ToString());
			wwuIndex = -1;
		}

		in_pathAndIcons.RemoveLast();
		return wwuIndex;
	}

	private static bool isTicking = false;

	public static void StartWWUWatcher()
	{
		if (isTicking)
			return;

		if (!AkWwiseProjectInfo.GetData().autoPopulateEnabled || !AkUtilities.IsWwiseProjectAvailable)
		{
			return;
		}

		isTicking = true;
		Tick();
		UnityEditor.EditorApplication.update += Tick;
	}

	public static void StopWWUWatcher()
	{
		if (!isTicking)
		{
			return;
		}
		UnityEditor.EditorApplication.update -= Tick;
		isTicking = false; 
	}

	private static void RestartWWUWatcher()
	{
		if (AkWwiseProjectInfo.GetData().autoPopulateEnabled)
		{
			StartWWUWatcher();
		}
	}

	private static Dictionary<WwiseObjectType, Dictionary<System.Guid, AkWwiseProjectData.AkBaseInformation>> _WwiseObjectsToRemove
		= new Dictionary<WwiseObjectType, Dictionary<System.Guid, AkWwiseProjectData.AkBaseInformation>>();

	private static Dictionary<WwiseObjectType, List<System.Guid>> _WwiseObjectsToKeep = new Dictionary<WwiseObjectType, List<System.Guid>>();
	private static HashSet<System.Guid> _ParsedWwiseObjects = new HashSet<System.Guid>();

	private static void FlagForRemoval(WwiseObjectType type, int wwuIndex)
	{
		switch (type)
		{
			case WwiseObjectType.AuxBus:
				foreach (var wwobject in AkWwiseProjectInfo.GetData().AuxBusWwu[wwuIndex].List)
					FlagForRemoval(wwobject, type);
				break;

			case WwiseObjectType.Event:
				foreach (var wwobject in AkWwiseProjectInfo.GetData().EventWwu[wwuIndex].List)
					FlagForRemoval(wwobject, type);
				break;

			case WwiseObjectType.Soundbank:
				foreach (var wwobject in AkWwiseProjectInfo.GetData().BankWwu[wwuIndex].List)
					FlagForRemoval(wwobject, type);
				break;

			case WwiseObjectType.GameParameter:
				foreach (var wwobject in AkWwiseProjectInfo.GetData().RtpcWwu[wwuIndex].List)
					FlagForRemoval(wwobject, type);
				break;

			case WwiseObjectType.Trigger:
				foreach (var wwobject in AkWwiseProjectInfo.GetData().TriggerWwu[wwuIndex].List)
					FlagForRemoval(wwobject, type);
				break;

			case WwiseObjectType.AcousticTexture:
				foreach (var wwobject in AkWwiseProjectInfo.GetData().AcousticTextureWwu[wwuIndex].List)
					FlagForRemoval(wwobject, type);
				break;

			case WwiseObjectType.StateGroup:
				foreach (var wwobject in AkWwiseProjectInfo.GetData().StateWwu[wwuIndex].List)
					FlagForRemoval(wwobject, type);
				break;

			case WwiseObjectType.SwitchGroup:
				foreach (var wwobject in AkWwiseProjectInfo.GetData().SwitchWwu[wwuIndex].List)
					FlagForRemoval(wwobject, type);
				break;
		}
	}

	private static void FlagForInsertion(AkWwiseProjectData.AkBaseInformation info, WwiseObjectType type)
	{
		if (!_WwiseObjectsToKeep.ContainsKey(type))
		{
			_WwiseObjectsToKeep.Add(type, new List<System.Guid>());
		}

		_WwiseObjectsToKeep[type].Add(info.Guid);

		if (!AkUtilities.IsMigrating)
		{
			WwiseObjectReference.UpdateWwiseObject(type, info.Name, info.Guid);
		}
	}

	private static void FlagForRemoval(AkWwiseProjectData.AkBaseInformation info, WwiseObjectType type)
	{
		if (!_WwiseObjectsToRemove.ContainsKey(type))
		{
			_WwiseObjectsToRemove.Add(type, new Dictionary<System.Guid, AkWwiseProjectData.AkBaseInformation>());
		}

		if(!_WwiseObjectsToRemove[type].ContainsKey(info.Guid))
		{
			_WwiseObjectsToRemove[type].Add(info.Guid, info);
		}
	}

	private bool GatherModifiedFiles()
	{
		_WwiseObjectsToRemove.Clear();
		_WwiseObjectsToKeep.Clear();

		var bChanged = false;
		var iBasePathLen = s_wwiseProjectPath.Length + 1;
		foreach (var scannedAsset in AssetType.ScannedAssets)
		{
			var dir = scannedAsset.RootDirectoryName;
			var deleted = new List<int>();
			var knownFiles = AkWwiseProjectInfo.GetData().GetWwuListByString(dir);
			var cKnownBefore = knownFiles.Count;

			try
			{
				//Get all Wwus in this folder.
				var di = new System.IO.DirectoryInfo(System.IO.Path.Combine(s_wwiseProjectPath, dir));
				var files = di.GetFiles("*.wwu", System.IO.SearchOption.AllDirectories);
				System.Array.Sort(files, s_FileInfo_CompareByPath);

				//Walk both arrays
				var iKnown = 0;
				var iFound = 0;

				while (iFound < files.Length && iKnown < knownFiles.Count)
				{
					var workunit = knownFiles[iKnown] as AkWwiseProjectData.WorkUnit;
					var foundRelPath = files[iFound].FullName.Substring(iBasePathLen);
					switch (workunit.PhysicalPath.CompareTo(foundRelPath))
					{
						case 0:
							//File was there and is still there.  Check the FileTimes.
							try
							{
								if (files[iFound].LastWriteTime > workunit.LastTime)
								{
									//File has been changed!   
									//If this file had a parent, parse recursively the parent itself
									m_WwuToProcess.Add(files[iFound].FullName);
									FlagForRemoval(scannedAsset.Type, iKnown);
									bChanged = true;
								}
							}
							catch
							{
								//Access denied probably (file does exists since it was picked up by GetFiles).
								//Just ignore this file.
							}

							iFound++;
							iKnown++;
							break;

						case 1:
							m_WwuToProcess.Add(files[iFound].FullName);
							iFound++;
							break;

						case -1:
							//A file was deleted.  Can't process it now, it would change the array indices.                                
							deleted.Add(iKnown);
							iKnown++;
							break;
					}
				}

				//The remainder from the files found on disk are all new files.
				for (; iFound < files.Length; iFound++)
					m_WwuToProcess.Add(files[iFound].FullName);

				//All the remainder is deleted.  From the end, of course.
				if (iKnown < knownFiles.Count)
				{
					for (var i = iKnown; i < knownFiles.Count; ++i)
					{
						FlagForRemoval(scannedAsset.Type, i);
					}

					knownFiles.RemoveRange(iKnown, knownFiles.Count - iKnown);
				}

				//Delete those tagged.
				for (var i = deleted.Count - 1; i >= 0; i--)
				{
					FlagForRemoval(scannedAsset.Type, deleted[i]);
					knownFiles.RemoveAt(deleted[i]);
				}

				bChanged |= cKnownBefore != knownFiles.Count;
			}
			catch (System.Exception exception)
			{
				UnityEngine.Debug.Log(exception);
				_WwiseObjectsToRemove.Clear();
				_WwiseObjectsToKeep.Clear();
				return false;
			}
		}

		return bChanged || m_WwuToProcess.Count > 0;
	}

	private void UpdateFiles()
	{
		_ParsedWwiseObjects.Clear();

		m_totWwuCnt = m_WwuToProcess.Count;

		var iBasePathLen = s_wwiseProjectPath.Length + 1;
		var iUnprocessed = 0;
		while (m_WwuToProcess.Count - iUnprocessed > 0)
		{
			System.Collections.IEnumerator e = m_WwuToProcess.GetEnumerator();
			for (var i = 0; i < iUnprocessed + 1; i++)
				e.MoveNext();

			var fullPath = e.Current as string;
			var relPath = fullPath.Substring(iBasePathLen);
			var typeStr = relPath.Remove(relPath.IndexOf(System.IO.Path.DirectorySeparatorChar));
			if (!CreateWorkUnit(relPath, typeStr, fullPath))
				iUnprocessed++;
		}

		//Add the unprocessed directly.  This can happen if we don't find the parent WorkUnit.  
		//Normally, it should never happen, this is just a safe guard.
		while (m_WwuToProcess.Count > 0)
		{

			System.Collections.IEnumerator e = m_WwuToProcess.GetEnumerator();
			e.MoveNext();
			var fullPath = e.Current as string;
			var relPath = fullPath.Substring(iBasePathLen);
			var typeStr = relPath.Remove(relPath.IndexOf(System.IO.Path.DirectorySeparatorChar));
			UpdateWorkUnit(fullPath, typeStr, relPath);
		}

		if (AkWwiseEditorSettings.Instance.ObjectReferenceAutoCleanup)
		{
			foreach (KeyValuePair<WwiseObjectType, List<System.Guid>> pair in _WwiseObjectsToKeep)
			{
				Dictionary<System.Guid, AkWwiseProjectData.AkBaseInformation> removeDict = null;
				if (!_WwiseObjectsToRemove.TryGetValue(pair.Key, out removeDict))
					continue;

				foreach (System.Guid wwiseObjectGuid in pair.Value)
				{
					removeDict.Remove(wwiseObjectGuid);
				}
			}

			foreach (KeyValuePair<WwiseObjectType, Dictionary<System.Guid, AkWwiseProjectData.AkBaseInformation>> wwiseObjectTypeDictPair in _WwiseObjectsToRemove)
			{
				var type = wwiseObjectTypeDictPair.Key;
				var childType = type == WwiseObjectType.StateGroup ? WwiseObjectType.State : WwiseObjectType.Switch;
				foreach (KeyValuePair<System.Guid, AkWwiseProjectData.AkBaseInformation> wwiseObjectInfoPair in wwiseObjectTypeDictPair.Value)
				{
					var groupValue = wwiseObjectInfoPair.Value as AkWwiseProjectData.GroupValue;
					if (groupValue != null)
					{
						foreach (var childObject in groupValue.values)
						{
							WwiseObjectReference.DeleteWwiseObject(childType, childObject.Guid);
						}
					}

					WwiseObjectReference.DeleteWwiseObject(type, wwiseObjectInfoPair.Key);
				}
			}
		}

		_ParsedWwiseObjects.Clear();
		_WwiseObjectsToRemove.Clear();
		_WwiseObjectsToKeep.Clear();
		UnityEditor.EditorUtility.SetDirty(AkWwiseProjectInfo.GetData());
		UnityEditor.EditorUtility.ClearProgressBar();
	}

	private static void UpdateWwiseObjectReference(WwiseObjectType type, List<AkWwiseProjectData.AkInfoWorkUnit> infoWwus)
	{
		foreach (var infoWwu in infoWwus)
			foreach (var info in infoWwu.List)
				WwiseObjectReference.UpdateWwiseObjectDataMap(type, info.Name, info.Guid);
	}

	private static void UpdateWwiseObjectReference(WwiseObjectType type, List<AkWwiseProjectData.EventWorkUnit> infoWwus)
	{
		foreach (var infoWwu in infoWwus)
			foreach (var info in infoWwu.List)
				WwiseObjectReference.UpdateWwiseObjectDataMap(type, info.Name, info.Guid);
	}

	private static void UpdateWwiseObjectReference(WwiseObjectType groupType, WwiseObjectType type, List<AkWwiseProjectData.GroupValWorkUnit> infoWwus)
	{
		foreach (var infoWwu in infoWwus)
		{
			foreach (var info in infoWwu.List)
			{
				WwiseObjectReference.UpdateWwiseObjectDataMap(groupType, info.Name, info.Guid);
				foreach (var subTypeInfo in info.values)
					WwiseObjectReference.UpdateWwiseObjectDataMap(type, subTypeInfo.Name, subTypeInfo.Guid);
			}
		}
	}

	private static void SortWwu(AssetType in_type, int in_wwuIndex)
	{
		switch (in_type.Type)
		{
			case WwiseObjectType.AuxBus:
				AkWwiseProjectInfo.GetData().AuxBusWwu[in_wwuIndex].List.Sort();
				break;

			case WwiseObjectType.Event:
				AkWwiseProjectInfo.GetData().EventWwu[in_wwuIndex].List.Sort();
				break;

			case WwiseObjectType.Soundbank:
				AkWwiseProjectInfo.GetData().BankWwu[in_wwuIndex].List.Sort();
				break;

			case WwiseObjectType.GameParameter:
				AkWwiseProjectInfo.GetData().RtpcWwu[in_wwuIndex].List.Sort();
				break;

			case WwiseObjectType.Trigger:
				AkWwiseProjectInfo.GetData().TriggerWwu[in_wwuIndex].List.Sort();
				break;

			case WwiseObjectType.AcousticTexture:
				AkWwiseProjectInfo.GetData().AcousticTextureWwu[in_wwuIndex].List.Sort();
				break;

			case WwiseObjectType.StateGroup:
				var stateList = AkWwiseProjectInfo.GetData().StateWwu[in_wwuIndex].List;
				stateList.Sort();
				foreach (var group in stateList)
					if (group.values.Count > 0)
						group.values.Sort();
				break;

			case WwiseObjectType.SwitchGroup:
				var switchList = AkWwiseProjectInfo.GetData().SwitchWwu[in_wwuIndex].List;
				switchList.Sort();
				foreach (var group in switchList)
					if (group.values.Count > 0)
						group.values.Sort();
				break;
		}
	}

	private static AkWwiseProjectData.WorkUnit ReplaceWwuEntry(string in_currentPhysicalPath, AssetType in_type, out int out_wwuIndex)
	{
		var list = AkWwiseProjectInfo.GetData().GetWwuListByString(in_type.RootDirectoryName);
		out_wwuIndex = list.BinarySearch(new AkWwiseProjectData.WorkUnit { PhysicalPath = in_currentPhysicalPath });
		AkWwiseProjectData.WorkUnit out_wwu = null;

		switch (in_type.Type)
		{
			case WwiseObjectType.Event:
				out_wwu = new AkWwiseProjectData.EventWorkUnit();
				break;

			case WwiseObjectType.StateGroup:
			case WwiseObjectType.SwitchGroup:
				out_wwu = new AkWwiseProjectData.GroupValWorkUnit();
				break;

			case WwiseObjectType.AuxBus:
			case WwiseObjectType.Soundbank:
			case WwiseObjectType.GameParameter:
			case WwiseObjectType.Trigger:
			case WwiseObjectType.AcousticTexture:
				out_wwu = new AkWwiseProjectData.AkInfoWorkUnit();
				break;
		}

		if (out_wwuIndex < 0)
		{
			out_wwuIndex = ~out_wwuIndex;
			list.Insert(out_wwuIndex, out_wwu);
		}
		else
			list[out_wwuIndex] = out_wwu;

		return out_wwu;
	}

	private static void AddElementToList(string in_currentPathInProj, System.Xml.XmlReader in_reader, AssetType in_type,
		LinkedList<AkWwiseProjectData.PathElement> in_pathAndIcons,string in_wwuPath, int in_wwuIndex, WwiseObjectType in_LeafType = WwiseObjectType.None)
	{
		switch (in_type.Type)
		{
			case WwiseObjectType.Folder:
			case WwiseObjectType.Bus:
			case WwiseObjectType.AuxBus:
			case WwiseObjectType.Event:
			case WwiseObjectType.Soundbank:
			case WwiseObjectType.GameParameter:
			case WwiseObjectType.Trigger:
			case WwiseObjectType.AcousticTexture:
				{
					var LeafType = in_LeafType == WwiseObjectType.None ? in_type.Type : in_LeafType;
					var name = in_reader.GetAttribute("Name");
					var valueToAdd = in_type.Type == WwiseObjectType.Event ? new AkWwiseProjectData.Event() : new AkWwiseProjectData.AkInformation();
					valueToAdd.Name = name;
					valueToAdd.Guid = new System.Guid(in_reader.GetAttribute("ID"));
					valueToAdd.PathAndIcons = new List<AkWwiseProjectData.PathElement>(in_pathAndIcons);

					FlagForInsertion(valueToAdd, in_type.Type);

					switch (LeafType)
					{
						case WwiseObjectType.AuxBus:
						case WwiseObjectType.Bus:
						case WwiseObjectType.Folder:
							valueToAdd.Path = in_currentPathInProj;
							break;

						default:
							valueToAdd.Path = System.IO.Path.Combine(in_currentPathInProj, name);
							valueToAdd.PathAndIcons.Add(new AkWwiseProjectData.PathElement(name, in_type.Type, valueToAdd.Guid));
							break;
					}

					AddWwiseObjectToProjectData(in_type, in_wwuIndex, valueToAdd, in_wwuPath);
				}

				in_reader.Read();
				break;

			case WwiseObjectType.StateGroup:
			case WwiseObjectType.SwitchGroup:
				{
					var valueToAdd = new AkWwiseProjectData.GroupValue();
					if (in_LeafType == WwiseObjectType.Folder)
					{
						valueToAdd.Name = in_reader.GetAttribute("Name");
						valueToAdd.Guid = new System.Guid(in_reader.GetAttribute("ID"));
						valueToAdd.PathAndIcons = new List<AkWwiseProjectData.PathElement>(in_pathAndIcons);
						valueToAdd.Path = in_currentPathInProj;

						FlagForInsertion(valueToAdd, in_type.Type);
						in_reader.Read();
					}
					else
					{
						var XmlElement = System.Xml.Linq.XNode.ReadFrom(in_reader) as System.Xml.Linq.XElement;
						var ChildrenList = System.Xml.Linq.XName.Get("ChildrenList");
						var ChildrenElement = XmlElement.Element(ChildrenList);
						if (ChildrenElement != null)
						{
							var name = XmlElement.Attribute("Name").Value;
							valueToAdd.Name = name;
							valueToAdd.Guid = new System.Guid(XmlElement.Attribute("ID").Value);
							valueToAdd.Path = System.IO.Path.Combine(in_currentPathInProj, name);
							valueToAdd.PathAndIcons = new List<AkWwiseProjectData.PathElement>(in_pathAndIcons);
							valueToAdd.PathAndIcons.Add(new AkWwiseProjectData.PathElement(name, in_type.Type, valueToAdd.Guid));

							FlagForInsertion(valueToAdd, in_type.Type);

							var ChildElem = System.Xml.Linq.XName.Get(in_type.ChildElementName);
							foreach (var element in ChildrenElement.Elements(ChildElem))
							{
								if (element.Name != in_type.ChildElementName)
									continue;

								var elementName = element.Attribute("Name").Value;
								var childValue = new AkWwiseProjectData.AkBaseInformation
								{
									Name = elementName,
									Guid = new System.Guid(element.Attribute("ID").Value),
								};
								childValue.PathAndIcons = new List<AkWwiseProjectData.PathElement>(valueToAdd.PathAndIcons);
								childValue.PathAndIcons.Add(new AkWwiseProjectData.PathElement(elementName, in_type.ChildType, childValue.Guid));
								valueToAdd.values.Add(childValue);

								FlagForInsertion(childValue, in_type.ChildType);
							}
						}
						else
						{
							valueToAdd = null;
						}
					}

					if (valueToAdd != null)
					{
						AddWwiseObjectToProjectData(in_type, in_wwuIndex, valueToAdd, in_wwuPath);
					}
				}
				break;

			default:
				UnityEngine.Debug.LogError("WwiseUnity: Unknown asset type in WWU parser");
				break;
		}
	}

	private static void AddWwiseObjectToProjectData(AssetType in_type, int in_wwuIndex, AkWwiseProjectData.AkInformation valueToAdd, string in_wwuPath)
	{
		if (!_ParsedWwiseObjects.Add(valueToAdd.Guid))
		{
			UnityEngine.Debug.LogWarning("While parsing " + in_wwuPath + ", an already parsed Wwise Object with name: " + valueToAdd.Name + " GUID: " + valueToAdd.Guid + " was found. Are all work units up to date?");
			return;
		}

		switch (in_type.Type)
		{
			case WwiseObjectType.AuxBus:
				AkWwiseProjectInfo.GetData().AuxBusWwu[in_wwuIndex].List.Add(valueToAdd);
				break;

			case WwiseObjectType.Event:
				AkWwiseProjectInfo.GetData().EventWwu[in_wwuIndex].List.Add(valueToAdd as AkWwiseProjectData.Event);
				break;

			case WwiseObjectType.Soundbank:
				AkWwiseProjectInfo.GetData().BankWwu[in_wwuIndex].List.Add(valueToAdd);
				break;

			case WwiseObjectType.GameParameter:
				AkWwiseProjectInfo.GetData().RtpcWwu[in_wwuIndex].List.Add(valueToAdd);
				break;

			case WwiseObjectType.Trigger:
				AkWwiseProjectInfo.GetData().TriggerWwu[in_wwuIndex].List.Add(valueToAdd);
				break;

			case WwiseObjectType.AcousticTexture:
				AkWwiseProjectInfo.GetData().AcousticTextureWwu[in_wwuIndex].List.Add(valueToAdd);
				break;

			case WwiseObjectType.StateGroup:
				AkWwiseProjectInfo.GetData().StateWwu[in_wwuIndex].List.Add(valueToAdd as AkWwiseProjectData.GroupValue);
				break;

			case WwiseObjectType.SwitchGroup:
				AkWwiseProjectInfo.GetData().SwitchWwu[in_wwuIndex].List.Add(valueToAdd as AkWwiseProjectData.GroupValue);
				break;
		}
	}

	private bool CreateWorkUnit(string in_relativePath, string in_wwuType, string in_fullPath)
	{
		var ParentID = string.Empty;
		try
		{
			using (var reader = System.Xml.XmlReader.Create(in_fullPath))
			{
				reader.MoveToContent();

				//We check if the current work unit has a parent and save its guid if its the case
				while (!reader.EOF && reader.ReadState == System.Xml.ReadState.Interactive)
				{
					if (reader.NodeType == System.Xml.XmlNodeType.Element && reader.Name.Equals("WorkUnit"))
					{
						if (reader.GetAttribute("PersistMode").Equals("Nested"))
							ParentID = reader.GetAttribute("OwnerID");
						break;
					}

					reader.Read();
				}
			}
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.Log("WwiseUnity: A changed Work unit wasn't found. It must have been deleted " + in_fullPath);
			return false;
		}

		if (!string.IsNullOrEmpty(ParentID))
		{
			var parentGuid = System.Guid.Empty;

			try
			{
				parentGuid = new System.Guid(ParentID);
			}
			catch
			{
				UnityEngine.Debug.LogWarning("WwiseUnity: \"OwnerID\" in <" + in_fullPath + "> cannot be converted to a GUID (" + ParentID + ")");
				return false;
			}

			var list = AkWwiseProjectInfo.GetData().GetWwuListByString(in_wwuType);
			var PathAndIcons = new LinkedList<AkWwiseProjectData.PathElement>();
			string PathInProj = string.Empty;
			AkWwiseProjectData.WorkUnit wwu = null;

			if (parentGuid != System.Guid.Empty)
			{
				for (var i = 0; i < list.Count; i++)
				{
					wwu = list[i] as AkWwiseProjectData.WorkUnit;
					if (wwu.Guid.Equals(parentGuid))
					{
						PathInProj = wwu.ParentPath;
						PathAndIcons = new LinkedList<AkWwiseProjectData.PathElement>(wwu.PathAndIcons);
						break;
					}
					else
					{
						var WwuChildren = wwu.GetChildrenArrayList();
						foreach (AkWwiseProjectData.AkInformation child in WwuChildren)
						{
							if (child.Guid.Equals(parentGuid))
							{
								PathInProj = child.Path;
								PathAndIcons = new LinkedList<AkWwiseProjectData.PathElement>(child.PathAndIcons);
								break;
							}
						}
					}
				}
			}
			if (!string.IsNullOrEmpty(PathInProj))
			{
				RecurseWorkUnit(AssetType.Create(in_wwuType), new System.IO.FileInfo(in_fullPath), PathInProj,
					in_relativePath.Remove(in_relativePath.LastIndexOf(System.IO.Path.DirectorySeparatorChar)), PathAndIcons,
					wwu.PhysicalPath);
				return true;
			}

			//Not found. Wait for it to load
			return false;
		}

		//Root Wwu
		UpdateWorkUnit(in_fullPath, in_wwuType, in_relativePath);
		return true;
	}

	private void UpdateWorkUnit(string in_wwuFullPath, string in_wwuType, string in_relativePath)
	{
		var PathAndIcons = new LinkedList<AkWwiseProjectData.PathElement>();
		var currentPathInProj = string.Empty;


		//Add physical folders to the hierarchy if the work unit isn't in the root folder
		var physicalPath = in_relativePath.Split(System.IO.Path.DirectorySeparatorChar);
		for (var i = 0; i < physicalPath.Length -1; i++)
		{
			PathAndIcons.AddLast(
				new AkWwiseProjectData.PathElement(physicalPath[i], WwiseObjectType.PhysicalFolder, System.Guid.Empty));
			currentPathInProj = System.IO.Path.Combine(currentPathInProj,physicalPath[i]);
		}

		//Parse the work unit file
		RecurseWorkUnit(AssetType.Create(in_wwuType), new System.IO.FileInfo(in_wwuFullPath), currentPathInProj,
			in_relativePath.Remove(in_relativePath.LastIndexOf(System.IO.Path.DirectorySeparatorChar)), PathAndIcons);
	}

	public class AssetType
	{
		public string RootDirectoryName { get; set; }
		public string XmlElementName;
		public string ChildElementName;
		public WwiseObjectType Type = WwiseObjectType.None;
		public WwiseObjectType ChildType = WwiseObjectType.None;

		public static AssetType[] ScannedAssets
		{
			get { return _ScannedAssets; }
		}

		public static AssetType Create(string rootDirectoryName)
		{
			foreach (var asset in ScannedAssets)
				if (string.Equals(rootDirectoryName, asset.RootDirectoryName, System.StringComparison.OrdinalIgnoreCase))
					return asset;

			return null;
		}

		private AssetType(string RootFolder, string XmlElemName, WwiseObjectType type)
		{
			RootDirectoryName = RootFolder;
			XmlElementName = XmlElemName;
			Type = type;
		}

		private static readonly AssetType[] _ScannedAssets = new AssetType[]
		{
			new AssetType("Master-Mixer Hierarchy", "AuxBus", WwiseObjectType.AuxBus),
			new AssetType("Events", "Event", WwiseObjectType.Event),
			new AssetType("SoundBanks", "SoundBank", WwiseObjectType.Soundbank),
			new AssetType("States", "StateGroup", WwiseObjectType.StateGroup) { ChildElementName = "State", ChildType = WwiseObjectType.State },
			new AssetType("Switches", "SwitchGroup", WwiseObjectType.SwitchGroup) { ChildElementName = "Switch", ChildType = WwiseObjectType.Switch },
			new AssetType("Game Parameters", "GameParameter", WwiseObjectType.GameParameter),
			new AssetType("Triggers", "Trigger", WwiseObjectType.Trigger),
			new AssetType("Virtual Acoustics", "AcousticTexture", WwiseObjectType.AcousticTexture),
		};
	}

	private class FileInfo_CompareByPath : IComparer<System.IO.FileInfo>
	{
		int IComparer<System.IO.FileInfo>.Compare(System.IO.FileInfo wwuA, System.IO.FileInfo wwuB)
		{
			return wwuA.FullName.CompareTo(wwuB.FullName);
		}
	}
}
#endif
