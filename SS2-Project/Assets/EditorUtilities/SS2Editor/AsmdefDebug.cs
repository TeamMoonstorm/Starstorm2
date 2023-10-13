#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

/// <summary>
/// https://gist.github.com/karljj1/9c6cce803096b5cd4511cf0819ff517b
/// </summary>
[InitializeOnLoad]
public class AsmdefDebug
{
    const string AssemblyReloadEventsEditorPref = "AssemblyReloadEventsTime";
    const string AssemblyCompilationEventsEditorPref = "AssemblyCompilationEvents";
    static readonly int ScriptAssembliesPathLen = "Library/ScriptAssemblies/".Length;
    private static string AssemblyTotalCompilationTimeEditorPref = "AssemblyTotalCompilationTime";

    static Dictionary<string, DateTime> s_StartTimes = new Dictionary<string, DateTime>();

    static StringBuilder s_BuildEvents = new StringBuilder();
    static double s_CompilationTotalTime;

    static AsmdefDebug()
    {
        CompilationPipeline.assemblyCompilationStarted += CompilationPipelineOnAssemblyCompilationStarted;
        CompilationPipeline.assemblyCompilationFinished += CompilationPipelineOnAssemblyCompilationFinished;
        AssemblyReloadEvents.beforeAssemblyReload += AssemblyReloadEventsOnBeforeAssemblyReload;
        AssemblyReloadEvents.afterAssemblyReload += AssemblyReloadEventsOnAfterAssemblyReload;
    }

    static void CompilationPipelineOnAssemblyCompilationStarted(string assembly)
    {
        s_StartTimes[assembly] = DateTime.UtcNow;
    }

    static void CompilationPipelineOnAssemblyCompilationFinished(string assembly, CompilerMessage[] arg2)
    {
        var timeSpan = DateTime.UtcNow - s_StartTimes[assembly];
        s_CompilationTotalTime += timeSpan.TotalMilliseconds;
        s_BuildEvents.AppendFormat("{0:0.00}s {1}\n", timeSpan.TotalMilliseconds / 1000f,
            assembly.Substring(ScriptAssembliesPathLen, assembly.Length - ScriptAssembliesPathLen));
    }

    static void AssemblyReloadEventsOnBeforeAssemblyReload()
    {
        var totalCompilationTimeSeconds = s_CompilationTotalTime / 1000f;
        s_BuildEvents.AppendFormat("compilation total: {0:0.00}s\n", totalCompilationTimeSeconds);
        EditorPrefs.SetString(AssemblyReloadEventsEditorPref, DateTime.UtcNow.ToBinary().ToString());
        EditorPrefs.SetString(AssemblyCompilationEventsEditorPref, s_BuildEvents.ToString());
        EditorPrefs.SetString(AssemblyTotalCompilationTimeEditorPref, totalCompilationTimeSeconds.ToString(CultureInfo.InvariantCulture));
    }

    static void AssemblyReloadEventsOnAfterAssemblyReload()
    {
        var binString = EditorPrefs.GetString(AssemblyReloadEventsEditorPref);
        var totalCompilationTimeSeconds = float.Parse(EditorPrefs.GetString(AssemblyTotalCompilationTimeEditorPref));

        long bin;
        if (long.TryParse(binString, out bin))
        {
            var date = DateTime.FromBinary(bin);
            var time = DateTime.UtcNow - date;
            var compilationTimes = EditorPrefs.GetString(AssemblyCompilationEventsEditorPref);
            var totalTimeSeconds = totalCompilationTimeSeconds + time.TotalSeconds;
            if (!string.IsNullOrEmpty(compilationTimes))
            {
                Debug.Log($"Compilation Report: {totalTimeSeconds:F2} seconds\n" + compilationTimes + "Assembly Reload Time: " + time.TotalSeconds + "s\n");
            }
        }
    }
}
//
#endif