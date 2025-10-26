using System;
#pragma warning disable CS0105
using UnityEngine.Networking;

//UNetWeaver error: SyncVar[RoR2.SS2HurtBoxReference SS2.BeamController::netTarget] from RoR2.dll cannot be a different module.
//UnityEditor.Networking.WeaverRunner/<>c:< OnCompilationFinished > b__1_1(string)
//Unity.UNetWeaver.Log:Error(string)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / Program.cs:19)
//Unity.UNetWeaver.NetworkBehaviourProcessor:ProcessSyncVars()(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetBehaviourProcessor.cs:2233)
//Unity.UNetWeaver.NetworkBehaviourProcessor:Process()(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetBehaviourProcessor.cs:57)
//Unity.UNetWeaver.Weaver:ProcessNetworkBehaviourType(Mono.Cecil.TypeDefinition)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1143)
//Unity.UNetWeaver.Weaver:CheckNetworkBehaviour(Mono.Cecil.TypeDefinition)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1685)
//Unity.UNetWeaver.Weaver:Weave(string, System.Collections.Generic.IEnumerable`1 < string >, Mono.Cecil.IAssemblyResolver, string, string, string)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1800)
//Unity.UNetWeaver.Weaver:WeaveAssemblies(System.Collections.Generic.IEnumerable`1 < string >, System.Collections.Generic.IEnumerable`1 < string >, Mono.Cecil.IAssemblyResolver, string, string, string)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1895)
//Unity.UNetWeaver.Program:Process(string, string, string, string[], string[], System.Action`1 < string >, System.Action`1 < string >)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / Program.cs:34)
//UnityEditor.EditorApplication:get_isCompiling()
//ThunderKit.Core.Data.ImportConfiguration:StepImporters()(at Library / PackageCache / com.passivepicasso.thunderkit@574342edeb / Editor / Core / Config / ImportConfiguration.cs:55)
//UnityEditor.EditorApplication:Internal_CallUpdateFunctions()

//	UNetWeaver error: SyncVar[RoR2.SS2HurtBoxReference SS2.BeamController::netTarget] from RoR2.dll cannot be a different module.
//UnityEditor.Networking.WeaverRunner/<>c:< OnCompilationFinished > b__1_1(string)
//Unity.UNetWeaver.Log:Error(string)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / Program.cs:19)
//Unity.UNetWeaver.NetworkBehaviourProcessor:ProcessSyncVars()(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetBehaviourProcessor.cs:2233)
//Unity.UNetWeaver.NetworkBehaviourProcessor:Process()(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetBehaviourProcessor.cs:57)
//Unity.UNetWeaver.Weaver:ProcessNetworkBehaviourType(Mono.Cecil.TypeDefinition)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1143)
//Unity.UNetWeaver.Weaver:CheckNetworkBehaviour(Mono.Cecil.TypeDefinition)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1685)
//Unity.UNetWeaver.Weaver:Weave(string, System.Collections.Generic.IEnumerable`1 < string >, Mono.Cecil.IAssemblyResolver, string, string, string)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1800)
//Unity.UNetWeaver.Weaver:WeaveAssemblies(System.Collections.Generic.IEnumerable`1 < string >, System.Collections.Generic.IEnumerable`1 < string >, Mono.Cecil.IAssemblyResolver, string, string, string)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1895)
//Unity.UNetWeaver.Program:Process(string, string, string, string[], string[], System.Action`1 < string >, System.Action`1 < string >)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / Program.cs:34)
//UnityEditor.EditorApplication:get_isCompiling()
//ThunderKit.Core.Data.ImportConfiguration:StepImporters()(at Library / PackageCache / com.passivepicasso.thunderkit@574342edeb / Editor / Core / Config / ImportConfiguration.cs:55)
//UnityEditor.EditorApplication:Internal_CallUpdateFunctions()

//	UNetWeaver error: SyncVar[RoR2.SS2HurtBoxReference SS2.BeamController::netTarget] from RoR2.dll cannot be a different module.
//UnityEditor.Networking.WeaverRunner/<>c:< OnCompilationFinished > b__1_1(string)
//Unity.UNetWeaver.Log:Error(string)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / Program.cs:19)
//Unity.UNetWeaver.NetworkBehaviourProcessor:ProcessSyncVars()(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetBehaviourProcessor.cs:2233)
//Unity.UNetWeaver.NetworkBehaviourProcessor:Process()(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetBehaviourProcessor.cs:57)
//Unity.UNetWeaver.Weaver:ProcessNetworkBehaviourType(Mono.Cecil.TypeDefinition)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1143)
//Unity.UNetWeaver.Weaver:CheckNetworkBehaviour(Mono.Cecil.TypeDefinition)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1685)
//Unity.UNetWeaver.Weaver:Weave(string, System.Collections.Generic.IEnumerable`1 < string >, Mono.Cecil.IAssemblyResolver, string, string, string)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1800)
//Unity.UNetWeaver.Weaver:WeaveAssemblies(System.Collections.Generic.IEnumerable`1 < string >, System.Collections.Generic.IEnumerable`1 < string >, Mono.Cecil.IAssemblyResolver, string, string, string)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1895)
//Unity.UNetWeaver.Program:Process(string, string, string, string[], string[], System.Action`1 < string >, System.Action`1 < string >)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / Program.cs:34)
//UnityEditor.EditorApplication:get_isCompiling()
//ThunderKit.Core.Data.ImportConfiguration:StepImporters()(at Library / PackageCache / com.passivepicasso.thunderkit@574342edeb / Editor / Core / Config / ImportConfiguration.cs:55)
//UnityEditor.EditorApplication:Internal_CallUpdateFunctions()

//	UNetWeaver error: SyncVar[RoR2.SS2HurtBoxReference SS2.BeamController::netTarget] from RoR2.dll cannot be a different module.
//UnityEditor.Networking.WeaverRunner/<>c:< OnCompilationFinished > b__1_1(string)
//Unity.UNetWeaver.Log:Error(string)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / Program.cs:19)
//Unity.UNetWeaver.NetworkBehaviourProcessor:ProcessSyncVars()(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetBehaviourProcessor.cs:2233)
//Unity.UNetWeaver.NetworkBehaviourProcessor:Process()(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetBehaviourProcessor.cs:57)
//Unity.UNetWeaver.Weaver:ProcessNetworkBehaviourType(Mono.Cecil.TypeDefinition)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1143)
//Unity.UNetWeaver.Weaver:CheckNetworkBehaviour(Mono.Cecil.TypeDefinition)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1685)
//Unity.UNetWeaver.Weaver:Weave(string, System.Collections.Generic.IEnumerable`1 < string >, Mono.Cecil.IAssemblyResolver, string, string, string)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1800)
//Unity.UNetWeaver.Weaver:WeaveAssemblies(System.Collections.Generic.IEnumerable`1 < string >, System.Collections.Generic.IEnumerable`1 < string >, Mono.Cecil.IAssemblyResolver, string, string, string)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1895)
//Unity.UNetWeaver.Program:Process(string, string, string, string[], string[], System.Action`1 < string >, System.Action`1 < string >)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / Program.cs:34)
//UnityEditor.EditorApplication:get_isCompiling()
//ThunderKit.Core.Data.ImportConfiguration:StepImporters()(at Library / PackageCache / com.passivepicasso.thunderkit@574342edeb / Editor / Core / Config / ImportConfiguration.cs:55)
//UnityEditor.EditorApplication:Internal_CallUpdateFunctions()

//	UNetWeaver error: SyncVar[RoR2.SS2HurtBoxReference SS2.BeamController::netTarget] from RoR2.dll cannot be a different module.
//UnityEditor.Networking.WeaverRunner/<>c:< OnCompilationFinished > b__1_1(string)
//Unity.UNetWeaver.Log:Error(string)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / Program.cs:19)
//Unity.UNetWeaver.NetworkBehaviourProcessor:ProcessSyncVars()(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetBehaviourProcessor.cs:2233)
//Unity.UNetWeaver.NetworkBehaviourProcessor:Process()(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetBehaviourProcessor.cs:57)
//Unity.UNetWeaver.Weaver:ProcessNetworkBehaviourType(Mono.Cecil.TypeDefinition)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1143)
//Unity.UNetWeaver.Weaver:CheckNetworkBehaviour(Mono.Cecil.TypeDefinition)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1685)
//Unity.UNetWeaver.Weaver:Weave(string, System.Collections.Generic.IEnumerable`1 < string >, Mono.Cecil.IAssemblyResolver, string, string, string)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1800)
//Unity.UNetWeaver.Weaver:WeaveAssemblies(System.Collections.Generic.IEnumerable`1 < string >, System.Collections.Generic.IEnumerable`1 < string >, Mono.Cecil.IAssemblyResolver, string, string, string)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1895)
//Unity.UNetWeaver.Program:Process(string, string, string, string[], string[], System.Action`1 < string >, System.Action`1 < string >)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / Program.cs:34)
//UnityEditor.EditorApplication:get_isCompiling()
//ThunderKit.Core.Data.ImportConfiguration:StepImporters()(at Library / PackageCache / com.passivepicasso.thunderkit@574342edeb / Editor / Core / Config / ImportConfiguration.cs:55)
//UnityEditor.EditorApplication:Internal_CallUpdateFunctions()

//	UNetWeaver error: SyncVar[RoR2.SS2HurtBoxReference SS2.BeamController::netTarget] from RoR2.dll cannot be a different module.
//UnityEditor.Networking.WeaverRunner/<>c:< OnCompilationFinished > b__1_1(string)
//Unity.UNetWeaver.Log:Error(string)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / Program.cs:19)
//Unity.UNetWeaver.NetworkBehaviourProcessor:ProcessSyncVars()(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetBehaviourProcessor.cs:2233)
//Unity.UNetWeaver.NetworkBehaviourProcessor:Process()(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetBehaviourProcessor.cs:57)
//Unity.UNetWeaver.Weaver:ProcessNetworkBehaviourType(Mono.Cecil.TypeDefinition)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1143)
//Unity.UNetWeaver.Weaver:CheckNetworkBehaviour(Mono.Cecil.TypeDefinition)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1685)
//Unity.UNetWeaver.Weaver:Weave(string, System.Collections.Generic.IEnumerable`1 < string >, Mono.Cecil.IAssemblyResolver, string, string, string)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1800)
//Unity.UNetWeaver.Weaver:WeaveAssemblies(System.Collections.Generic.IEnumerable`1 < string >, System.Collections.Generic.IEnumerable`1 < string >, Mono.Cecil.IAssemblyResolver, string, string, string)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1895)
//Unity.UNetWeaver.Program:Process(string, string, string, string[], string[], System.Action`1 < string >, System.Action`1 < string >)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / Program.cs:34)
//UnityEditor.EditorApplication:get_isCompiling()
//ThunderKit.Core.Data.ImportConfiguration:StepImporters()(at Library / PackageCache / com.passivepicasso.thunderkit@574342edeb / Editor / Core / Config / ImportConfiguration.cs:55)
//UnityEditor.EditorApplication:Internal_CallUpdateFunctions()

//	UNetWeaver error: SyncVar[RoR2.SS2HurtBoxReference SS2.BeamController::netTarget] from RoR2.dll cannot be a different module.
//UnityEditor.Networking.WeaverRunner/<>c:< OnCompilationFinished > b__1_1(string)
//Unity.UNetWeaver.Log:Error(string)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / Program.cs:19)
//Unity.UNetWeaver.NetworkBehaviourProcessor:ProcessSyncVars()(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetBehaviourProcessor.cs:2233)
//Unity.UNetWeaver.NetworkBehaviourProcessor:Process()(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetBehaviourProcessor.cs:57)
//Unity.UNetWeaver.Weaver:ProcessNetworkBehaviourType(Mono.Cecil.TypeDefinition)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1143)
//Unity.UNetWeaver.Weaver:CheckNetworkBehaviour(Mono.Cecil.TypeDefinition)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1685)
//Unity.UNetWeaver.Weaver:Weave(string, System.Collections.Generic.IEnumerable`1 < string >, Mono.Cecil.IAssemblyResolver, string, string, string)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1800)
//Unity.UNetWeaver.Weaver:WeaveAssemblies(System.Collections.Generic.IEnumerable`1 < string >, System.Collections.Generic.IEnumerable`1 < string >, Mono.Cecil.IAssemblyResolver, string, string, string)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / UNetWeaver.cs:1895)
//Unity.UNetWeaver.Program:Process(string, string, string, string[], string[], System.Action`1 < string >, System.Action`1 < string >)(at Library / PackageCache / com.unity.multiplayer - hlapi@faf9310011 / Editor / Tools / Weaver / Program.cs:34)
//UnityEditor.EditorApplication:get_isCompiling()
//ThunderKit.Core.Data.ImportConfiguration:StepImporters()(at Library / PackageCache / com.passivepicasso.thunderkit@574342edeb / Editor / Core / Config / ImportConfiguration.cs:55)
//UnityEditor.EditorApplication:Internal_CallUpdateFunctions()

using RoR2; using RoR2; using RoR2; using RoR2; using RoR2; using RoR2; using RoR2; using RoR2; using RoR2; using RoR2; using RoR2; using RoR2;
using RoR2;                                                                                                                         using RoR2;
using RoR2;                                                                                                                         using RoR2;
using RoR2;                                                                                                                         using RoR2;
using RoR2;                                                                                                                         using RoR2;
using RoR2;                                                   using UnityEngine;                                                    using RoR2;
using RoR2;                                                                                                                         using RoR2;
using RoR2;                                                                                                                         using RoR2;
using RoR2;                                                                                                                         using RoR2;
using RoR2;                                                                                                                         using RoR2;
using RoR2; using RoR2; using RoR2; using RoR2; using RoR2; using RoR2; using RoR2; using RoR2; using RoR2; using RoR2; using RoR2; using RoR2;
/*
Manual
Scripting API

    unity.com

Version: 2017.4



    C#
    JS

Script language

Select your preferred scripting language. All code snippets will be displayed in this language.
Scripting API

    UnityEngine
        UnityEngine.Accessibility
        UnityEngine.AI
        UnityEngine.Analytics
        UnityEngine.Animations
        UnityEngine.Apple
        UnityEngine.Assertions
        UnityEngine.Audio
        UnityEngine.CrashReportHandler
        UnityEngine.Events
        UnityEngine.EventSystems
        UnityEngine.Experimental
        UnityEngine.iOS
        UnityEngine.Networking
            UnityEngine.Networking.Match
            UnityEngine.Networking.NetworkSystem
            UnityEngine.Networking.PlayerConnection
            UnityEngine.Networking.Types
            Classes
            Interfaces
            Enumerations
            Attributes
                ClientAttribute
                ClientCallbackAttribute
                ClientRpcAttribute
                CommandAttribute
                NetworkSettingsAttribute
                ServerAttribute
                ServerCallbackAttribute
                SyncEventAttribute
                SyncVarAttribute
                TargetRpcAttribute
        UnityEngine.Playables
        UnityEngine.Profiling
        UnityEngine.Rendering
        UnityEngine.SceneManagement
        UnityEngine.Scripting
        UnityEngine.Serialization
        UnityEngine.SocialPlatforms
        UnityEngine.SpatialTracking
        UnityEngine.Sprites
        UnityEngine.TestTools
        UnityEngine.Tilemaps
        UnityEngine.Timeline
        UnityEngine.Tizen
        UnityEngine.U2D
        UnityEngine.UI
        UnityEngine.Video
        UnityEngine.Windows
        UnityEngine.WSA
        UnityEngine.XR
        Classes
        Interfaces
        Enumerations
        Attributes
    UnityEditor
    Other

This version of Unity is unsupported.
SyncVarAttribute

class in UnityEngine.Networking
Leave feedback
Description

[SyncVar] is an attribute that can be put on member variables of NetworkBehaviour classes. These variables will have their values sychronized from the server to clients in the game that are in the ready state.

Setting the value of a [SyncVar] marks it as dirty, so it will be sent to clients at the end of the current frame. Only simple values can be marked as [SyncVars]. The type of the SyncVar variable cannot be from an external DLL or assembly.

using UnityEngine;
using UnityEngine.Networking;

public class Ship : NetworkBehaviour
{
    [SyncVar]
    public int health = 100;

    [SyncVar]
    public float energy = 100;
}

The allowed SyncVar types are;

• Basic type(byte, int, float, string, UInt64, etc)
• Built -in Unity math type (Vector3, Quaternion, etc),
• Structs containing allowable types .
Properties
hook	The hook attribute can be used to specify a function to be called when the sync var changes value on the client.

Is something described here not working as you expect it to? It might be a Known Issue. Please check with the Issue Tracker at issuetracker.unity3d.com.
Copyright © 2020 Unity Technologies. Publication Date: 2020 - 06 - 05.
TutorialsCommunity AnswersKnowledge BaseForumsAsset StoreLegalPrivacy PolicyCookiesDo Not Sell or Share My Personal InformationYour Privacy Choices (Cookie Settings)
Your Opt Out Preference Signal is Honored
By clicking “Accept Cookies”, you agree to the storing of cookies on your device to enhance site navigation, analyze site usage, and assist in our marketing efforts.
*/
namespace SS2 //  The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly.The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly. The type of the SyncVar variable cannot be from an external DLL or assembly.
{
	// Token: 0x020008F1 RID: 2289
	[Serializable]
	public struct SS2HurtBoxReference : IEquatable<SS2HurtBoxReference>
	{
		// Token: 0x06003372 RID: 13170 RVA: 0x000EC390 File Offset: 0x000EA590
		public static SS2HurtBoxReference FromHurtBox(HurtBox hurtBox)
		{
			SS2HurtBoxReference result;
			if (!hurtBox)
			{
				result = default(SS2HurtBoxReference);
				return result;
			}
			result = new SS2HurtBoxReference
			{
				rootObject = (hurtBox.healthComponent ? hurtBox.healthComponent.gameObject : null),
				hurtBoxIndexPlusOne = (byte)(hurtBox.indexInGroup + 1)
			};
			return result;
		}

		// Token: 0x06003373 RID: 13171 RVA: 0x000EC3EC File Offset: 0x000EA5EC
		public static SS2HurtBoxReference FromRootObject(GameObject rootObject)
		{
			return new SS2HurtBoxReference
			{
				rootObject = rootObject,
				hurtBoxIndexPlusOne = 0
			};
		}

		// Token: 0x06003374 RID: 13172 RVA: 0x000EC414 File Offset: 0x000EA614
		public GameObject ResolveGameObject()
		{
			if (this.hurtBoxIndexPlusOne == 0)
			{
				return this.rootObject;
			}
			if (this.rootObject)
			{
				ModelLocator component = this.rootObject.GetComponent<ModelLocator>();
				if (component && component.modelTransform)
				{
					HurtBoxGroup component2 = component.modelTransform.GetComponent<HurtBoxGroup>();
					if (component2 && component2.hurtBoxes != null)
					{
						int num = (int)(this.hurtBoxIndexPlusOne - 1);
						if (num < component2.hurtBoxes.Length)
						{
							return component2.hurtBoxes[num].gameObject;
						}
					}
				}
			}
			return null;
		}

		// Token: 0x06003375 RID: 13173 RVA: 0x000EC4A0 File Offset: 0x000EA6A0
		public HurtBox ResolveHurtBox()
		{
			GameObject gameObject = this.ResolveGameObject();
			if (!gameObject)
			{
				return null;
			}
			return gameObject.GetComponent<HurtBox>();
		}

		// Token: 0x06003376 RID: 13174 RVA: 0x000EC4C4 File Offset: 0x000EA6C4
		public void Write(NetworkWriter writer)
		{
			writer.Write(this.rootObject);
			writer.Write(this.hurtBoxIndexPlusOne);
		}

		// Token: 0x06003377 RID: 13175 RVA: 0x000EC4DE File Offset: 0x000EA6DE
		public void Read(NetworkReader reader)
		{
			this.rootObject = reader.ReadGameObject();
			this.hurtBoxIndexPlusOne = reader.ReadByte();
		}

		// Token: 0x06003378 RID: 13176 RVA: 0x000EC4F8 File Offset: 0x000EA6F8
		public bool Equals(SS2HurtBoxReference other)
		{
			return object.Equals(this.rootObject, other.rootObject) && this.hurtBoxIndexPlusOne == other.hurtBoxIndexPlusOne;
		}

		// Token: 0x06003379 RID: 13177 RVA: 0x000EC520 File Offset: 0x000EA720
		public override bool Equals(object obj)
		{
			if (obj is SS2HurtBoxReference)
			{
				SS2HurtBoxReference other = (SS2HurtBoxReference)obj;
				return this.Equals(other);
			}
			return false;
		}

		// Token: 0x0600337A RID: 13178 RVA: 0x000EC545 File Offset: 0x000EA745
		public override int GetHashCode()
		{
			return ((this.rootObject != null) ? this.rootObject.GetHashCode() : 0) * 397 ^ this.hurtBoxIndexPlusOne.GetHashCode();
		}

		// Token: 0x04003C19 RID: 15385
		public GameObject rootObject;

		// Token: 0x04003C1A RID: 15386
		public byte hurtBoxIndexPlusOne;
	}

	public static partial class NetworkExtensions
	{
		// Token: 0x06001E23 RID: 7715 RVA: 0x0008DE1B File Offset: 0x0008C01B
		public static void Write(this NetworkWriter writer, SS2HurtBoxReference SS2HurtBoxReference)
		{
			SS2HurtBoxReference.Write(writer);
		}

		// Token: 0x06001E24 RID: 7716 RVA: 0x0008DE28 File Offset: 0x0008C028
		public static SS2HurtBoxReference ReadSS2HurtBoxReference(this NetworkReader reader)
		{
			SS2HurtBoxReference result = default(SS2HurtBoxReference);
			result.Read(reader);
			return result;
		}

	}
}
#pragma warning restore CS0105