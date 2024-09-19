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
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.VersionControl;

[UnityEditor.InitializeOnLoad]
public abstract class AkPlatformPluginActivator
{
    public abstract string WwisePlatformName { get; }
    public abstract string PluginDirectoryName { get; }
    public virtual ISet<string> Architectures => null;

    public virtual string DSPDirectoryPath => string.Empty;

    public virtual string StaticPluginRegistrationName => null;
    public virtual string StaticPluginDefine => null;

    public virtual bool RequiresStaticPluginRegistration => false;
    
    public virtual void FilterOutPlatformIfNeeded(BuildTarget target, PluginImporter pluginImporter,
        string pluginPlatform)
    {
    }

    public virtual bool IsPluginSDKVersionCompatible(string pluginSDKVersion)
    {
        return true;
    }

    internal virtual bool IsBuildEnvironmentValid()
    {
        return true;
    }
    
    internal string[] GetPluginPathParts(string pluginPath)
    {
        var indexOfPluginFolder = pluginPath.IndexOf(AkPluginActivatorConstants.WwisePluginFolder, StringComparison.OrdinalIgnoreCase);
        return pluginPath.Substring(indexOfPluginFolder + AkPluginActivatorConstants.WwisePluginFolder.Length + 1).Split('/');
    }

    public abstract AkPluginActivator.PluginImporterInformation GetPluginImporterInformation(PluginImporter pluginImporter);
    internal abstract bool ConfigurePlugin(PluginImporter pluginImporter, AkPluginActivator.PluginImporterInformation pluginImporterInformation);

}
#endif