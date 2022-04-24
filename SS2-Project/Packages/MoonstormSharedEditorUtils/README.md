# Moonstorm Shared Editor Utils

Moonstorm shared editor utils (Abreviated as MSEU) is a package of classes specifically designed for the unity editor to aid in the creation of Risk of Rain 2 Mods that use [Moonstorm Shared Utils](https://thunderstore.io/package/TeamMoonstorm/MoonstormSharedUtils/). It can also be used as a tool for learning how to extend from RoR2EditorKit.

## Features:

---

### Material Related

MSEU comes prepackaged with Stubbed shaders for the HG shaders, Decalicious and CalmWater. These shaders are used during the Shader swap method on MSU's AssetLoader

Alongside this, MSEU also comes bundled with Material Utilities

* Custom Editor utilities for managing material's shaders. Including KingEnderBrine's Shader Asset Picker.

![](https://i.gyazo.com/fbcc764992e87e2f9cf08f68ecc86f69.png)

* Pick Shader Asset: Allows you to properly select the real hopoo shader.
* Upgrade to real shader: In case the custom PipelineJob fails to swap the stubbed shaders to real shaders, you can click this button and it'll swap automatically to the real shaders.
* (PipelineJobs can be found below)

---

### PipelineJobs

MSEU also comes with PipelineJobs, these new jobs can be used for speeding up or simplifying workload.

* A custom pipeline based off "StageAssetBundles". which will:
    * Swap all the materials that are using the real hopoo shaders to their stubbed versions.
    * Build the Assetbundle.
    * Swap all the materials that are using the stubbed shaders to their real hopoo shaders versions.
* The end result is that materials will always use the real shaders in the editor, but the packaged assetbundle contains no references to the real shaders, avoiding possible copyright issues that entail redistributing Risk of Rain 2 assets.

![](https://i.gyazo.com/27fd721d3a2a0595a8f32c284a550015.png)

* A Pipeline called "Deploy To folders", which can be used as an Enumerable version of the "Copy" job, useful if you need to copy your mod to multiple places (Projects, or mod manager profiles)

![](https://i.gyazo.com/75cc19c0b6730991de871a76d388e4fc.png)

# Editor Windows & Inspectors

MSU comes bundled with custom editor, these editors can be split into two categories, Inspectors, and Editor Windows.

* Editor Windows:
    * Key Asset Display Pair Holder
    * MSIDRS
    * MSSingleItemDisplayRuleSet
    * Vanilla Skin Def

* Inspectors
    * MSInteractableDirectorCard
    * MSMonsterDirectorCard
    * MSMonsterFamily
    * MSUnlockableDef

Just like all other inspectors and windows in RoR2EditorKit, they can be enabled and disabled.

![](https://i.gyazo.com/c013972c8d8c4dcea6d6532cf7de067f.png)
![](https://i.gyazo.com/6a470498d5a5b41f3a51ba1f4aab52a8.png)

# Asset Creation Windows

* Creation windows for the IDRS system in MSU:
    * MSIDRS: Easily create an MSIDRS with the IDRS flags system, each flag corresponds to a tier/type of item/equipment in vanilla risk of rain 2.
        * These Flags are:
            * Vanilla Whites
            * Vanilla Greens
            * Vanilla Reds
            * Vanilla Yellows
            * Vanilla Lunars
            * Vanilla Equipments
            * Vanilla Lunar Equipments
            * Vanilla Elite Equipments
        * If you need to add your own mod's IDRS key values, you can do so by populating the key asset display pair holder object.
    
    * SIDR: Easily create a Single Item Display Rule Set with the SIDR flag system, each flag corresponds to a category of characterBodies in vanilla risk of rain 2.
        * These flags are:
            * Survivors
            * Enemies
            * Scavenger
        * If you want to add one of your IDRS, or even a modded IDRS, you can do so by creating an IDRSHolder, MSEU also comes with an asset creation for IDRSHolder!

![](https://i.gyazo.com/75d2c04c9d04f6ceb10d88cde3e86b75.png)

* IDRS Holder
    * The IDRS holder holds either a direct reference to an IDRS on your project, or a string reference to the key of another mod's IDRS.
    * To ease the creation of these, MSEU comes with an IDRS holder creation window.
    * The IDRSHolder window accepts either direct IDRS references, or flags from the modded bodies list.
    * The modded bodies flags include:
        * Lost in Transit Enemies (Archaic Wisp, Claymen, Ancient Wisp)
        * RoR1 Returning Survivors (CHEF, HAND, Miner, Nemesis Enforcer, Enforcer, Sniper Classic)
        * Starstorm2 (Executioner, Nemmando, SecurityDrone)
        * Manipulator (Manipulator, ILY vale)
        * Risk of Ruina (Arbiter, RedMist)
        * Paladin (Paladin, ILY Jame)
    * Keep in mind that the strings in the modded bodies flags might be out of date, and are prone to failing.

![](https://i.gyazo.com/715e534c3bb01b0aa2f339924bed8d61.png)

## Troubleshooting

* Any kind of bug or issue when using these editor utilities should be informed in the Starstorm2 Discord server, specifically towards "Nebby", the developer that takes care of this project.

# Changelog

(Old Changelogs can be found [here]())

# '1.3.0'

* Updated to use RoR2EditorKit 1.0.0
* Updated to use MSU 0.7.0
* Probably changed some interal things that i cant remember
* Moved the project from Starstorm2's github to its own github.
* Added the following inspectors:
    * MSMonsterDirectorCard
    * MSMonsterFamily

# '1.2.0'

* Updated to use RoR2EditorKit 0.2.3
* Updated to use MSU 0.6.0

* Added the following asset creators:
    * MSUnlockableDef

* Added the following inspectors:
    * MSUnlockableDef
    * MSInteractableDirectorCard

* Added the following property drawers:
    * AchievementStringAssetReference (MSunlockableDef)

* Added DeployToFolders pipeline job
    * Works as an iteratable version of the "Copy" pipeline job

* Added the ability to downgrade or upgrade all the shaders in the project
    * Found inside "Tools" menu item.

# '1.1.2'

* Fixed mod shipping with a duplicate assmebly definition

# '1.1.1'

* Updated to use RoR2EditorKit 0.1.4
* MSIDRS Editor window now has a dictionary with the values that depend on its flags.
* Added the ability to add missing key assets to the MSIDRS editor window
* Added the ability to delete a selected key asset in the MSIDRS editor window

* SIDR editor window now has a dictionary with the values that depend on its flags.
* Added the ability to add a missing IDRS to the SIDR editor window
* Added the ability to delete a selected IDRS in the SIDR editor window

# '1.0.0'

* Moved a lot of the code that made this work into RoR2EditorKit.
* Added RoR2EditorKit as a dependency.
* Removed a lot of unused assemblies from the assembly definition file
* Should properly work without needing to manually set MSU as the dll reference.
* Added an asset creation window for the following types:
    * MSIDRS
    * MSSingleItemDisplayRuleSet
    * ItemDisplayHolder
* Added shader dictionary support for decalicious

# '0.1.0'
* Added MSU as a Needed Dependency for the mod to work properly.
* Added Editor Scripts for custom editor windows for the following types:
    * EntityStateConfigurations
    * Key Asset Display Pair Holder
    * MSIDRS
    * SerializableContentPack
    * SingleItemDisplayRuleSet
    * Vanilla Skin Def
* SwapShadersAndStageAssetBundles now fix any stubbed shaders that are found
* Added CalmWater stubbed shaders.

# '0.0.1'
* Real initial release

# '0.0.0'
* Test release

