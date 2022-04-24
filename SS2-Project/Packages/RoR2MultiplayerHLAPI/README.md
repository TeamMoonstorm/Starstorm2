# README #

The Unity Multiplayer High Level API (HLAPI) is the open source component of the Unity Multiplayer system,
this was formerly a Unity extension DLL with some parts in the engine itself, now it all exists in a package.

This version of the package was modified for use in creating Mods for Risk of Rain 2.

### How do I get started? ###

* Clone this repository into the Packages folder.

or

* Download this version of the package from the Thunderkit Thunderstore extension store.

### Will this weave my assemblies and network assets?

* Yes, it will.

### Why not use the original HLAPI package.

* Not only does the original package force a dependency on mono.cecil, which makes it impossible to work with bepinex, havint he original HLAPI package will cause issues with certain structs such as SkillFamilies or DCCS.

### Anything i should be aware of?

* The package does not come with a forced dependency on mono.cecil, the assemblyDefinition will look for the assembly. Keep in mind that if you install this package to the editor and do not have a mono.cecil dll, the editor will warn you and highly recommend installing BepInEx or another way of getting the mono.cecil dll.