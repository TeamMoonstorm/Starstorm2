# RoR2EditorKit

The Risk of Rain 2 Editor Kit (Abreviated as ROR2EK) is a Thunderkit Extension designed specifically for helping mod creators create content for Risk of Rain 2.

The main goal of ROR2EK is to bring a friendly, easy to use editor experience for creating content, ranging from items, all the way to prefabs.

## Features:

---

## Inspectors

RoR2EditorKit comes bundled with inspectors that overwrite the default view of specific Components and ScriptableObjects in RoR2. These inspectors work with the new UIToolkit system and can be extended from to create your own inspectors for types.

Most inspectors will notify users when they're not following the naming conventions of objects that hoopo games uses, or when there are common mistakes in the inspected object. These naming convention messages can be disabled in the settings window.

Currently, RoR2EK comes bundled with 7 Inspectors.

* ObjectScaleCurve: Ticking "Use overall curve only" will hide the other 3 animation curves
* HGButton: Creates an inspector for using the HGButton class, which is used in a variety of UI on RoR2
* ChildLocator: Modifies the entries of the ChildLocator to use only one line instead of two per element.
* CharacterBody: Makes the base and level stats foldable, so you can hide or expand them at will
* BuffDef: Hides the "Icon Path" field and implements utility messages.
* Entity State Configuration: Easily select an entity state from the target type, when selected, the inspector will automatically populate the serialized fields array with the necesary fields to serialize.

![](https://i.gyazo.com/6e7e1d8aa698c43dfeca231e5bcbe7e7.png)
###### EntityStateConfiguration inspector.

![](https://i.gyazo.com/f8660459ed2e3a02939f44d10485093e.png)
###### BuffDef inspector, notice the Info message on naming conventions and the warning regarding EliteDefs.

All the inspectors of ROR2EK can be toggled ON or OFF via a toggle switch on the Editor header GUI

---

### Property Drawers

RoR2EK comes with property drawers for specific types and type attributes for Risk of Rain 2, the main example is the SerializableEntityStateType and SerializableSystemType drawer.

![](https://cdn.discordapp.com/attachments/575431803523956746/903754837940916234/unknown.png)

ROR2EK also comes with the following property drawers:
* EnumMask: Used by almost all flag enums, the EnumMask property drawer will allow you to actually set the flags properly.
* PrefabReference: Used by the SkinDef as an example, the Prefab Reference drawer makes it possible to use the SkinDef scriptable object properly
* SkillFamily: Simply hides the unlockableName field of the skill family.

---

### MaterialEditor

ROR2EK comes bundled with a special MaterialEditor, the material editor itself is used for handling the difficult to work with Shaders from Risk of Rain 2. It helps via either letting you modify important aspects that arent available by default, or by hiding entire sections if the shader keyword is not enabled.

Currently, ROR2EK comes bundled with 3 Material Editors
* HGStandard
* HGSnowTopped
* HGCloudRemap

All of these material editors work with either the real hopoo shaders, or with stubbed versions.

![](https://i.gyazo.com/172f157cefaefbfb619611b836a8f8fe.png)
###### (Notice how the PrintBehavior, Screenspace Dithering, Fresnell Emission, Splatmapping, flowmap and limb removal are hidden when their keywords are not enabled)

---

### Other:

* ScriptableCreators: A lot of MenuItems to create a myriad of hidden SkillDefs.

## Credits

* Coding: Nebby, Passive Picasso (Twiner), KingEnderBrine, KevinFromHPCustomerService
* Models & Sprite: Nebby
* Mod Icon: SOM

## Changelog

(Old Changelogs can be found [here](https://github.com/risk-of-thunder/RoR2EditorKit/blob/main/RoR2EditorKit/Assets/RoR2EditorKit/OldChangelogs.md))

### '2.1.0'

* Actually added ValidateUXMLPath to the expended inspector.
* Added IMGUToVisualElementInspector editor. Used to transform an IMGUI inspector into a VisualElement inspector.
* Fixed StageLanguageFiles not working properly
* Fixed StageLanguageFiles not copying the results to the manifest's staging paths.
* Improved StageLanguageFiles' logging capabilities.
* RoR2EK assets can no longer be edited if the package is installed under the "Packages" folder.
* Split Utils.CS into 5 classes
	* Added AssetDatabaseUtils
	* Added ExtensionUtils
	* Added IOUtils
	* Added MarkdownUtils
	* Added ScriptableObjectUtils
* Removed SkillFamilyVariant property drawer

### '2.0.2'

* Fixed an issue where ExtendedInspectors would not display properly due to incorrect USS paths.
* Added ValidateUXMLPath to ExtendedInspector, used to validate the UXML's file path, override this if youre making an ExtendedInspector for a package that depends on RoR2EK's systems.
* Added ValidateUXMLPath to ExtendedEditorWindow
* Hopefully fixed the issue where RoR2EK assets can be edited.

### '2.0.1'

* Fixed an issue where ExtendedInspectors would not work due to incorrect path management.

### '2.0.0'

* Updated to unity version 2019.4.26f1
* Updated to Survivors of The Void
* Added a plethora of Util Methods to Util.CS, including Extensions
* Removed UnlockableDef creation as it's been fixed
* Added "VisualElementPropertyDrawer"
* Renamed "ExtendedPropertyDrawer" to "IMGUIPropertyDrawer"
* Rewrote ExtendedInspector sistem to use VisualElements
* Rewrote CharacterBody inspector
* Rewrote BuffDef inspector
* Rewrote ExtendedEditorWindow to use VisualElements
* Added EliteDef inspector
* Added EquipmentDef inspector
* Added NetworkStateMachine inspector
* Added SkillLocator inspector
* Removed Entirety of AssetCreator systems
* Removed SerializableContentPack window
