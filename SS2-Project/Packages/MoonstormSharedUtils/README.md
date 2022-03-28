# Moonstorm Shared Utils

- Moonstorm Shared Utils (Abreviated as MSU) is a library mod with the intention to help with the creation of mods, mainly ones that are developed through ThunderKit. The library mod is used by the members of TeamMoonstorm. But anyone can use it as long as they give credit.

## Key Features

---

### Modules

MSU was built to be a Modular based library, as such, MSU comes bundled with what are called "ModuleBases"

Module bases are basically "Hubs" for MSU to communicate between your content, and the game, a Module base has the main capability of automatically handling the behavior, and implementation of the content you desire to add. They're abstract by nature, so you must inherit from them to properly interact with the module.

Currently, MSU comes bundled with 11 Modules:
* ArticatModule: Handles Artifacts and the unhooking and hooking of methods
* BuffModule: Handles buffs and their itemBehaviors
* CharacterModule: Handles CharacterBodies, alongside implementing Monsters to stages and Monster Families
* DamageTypeModule: Handles ModdedDamageTypes from R2API's DamageAPI.
* EliteModule: Handles Elites via a combination of it's systems and R2API's EliteAPI
* InteractableModule: Handles all sorts of Interactables, alongside implementing them to stages
* ItemDisplayModule: Handles the ItemDisplay related scriptable objects.
* PickupModule: Handles Items and Equipments, alongside their behaviors and on use behaviors
* ProjectileModule: Handles Projectiles
* SceneModule: Handles Scenes, compatible with Rain of Stages
* UnlockableModuleBase: Handles Unlockables and AchievementDefs

Module bases by themselves are extensible, so you can inherit from the "ModuleBase" class and create your own module.

Example of a mod implementing a module base can be found [here](https://github.com/swuff-star/LostInTransit/blob/master/LIT/Assets/LostInTransit/Modules/Buffs/Buffs.cs).

---

### Content Bases

In a nutshell, a "ContentBase" is a type of class that's used to represent some kind of Content from RiskOfRain2.

Most of the ContentBases dont do much by themselves, it is the duty of their respective ModuleBase to interact with the ContentBases given to it and implement their functionality ingame. They're abstract by nature, so you must inherit from them to properly create a new piece of content.

Currently, MSU Comes bundled with 13 ContentBases:
* Artifact: A representation of an Artifact, has methods for unhooking and hooking to events for creating the artifact's behavior
* Buff: A representation of a buff, utilizes ItemBehaviors for implementing said buff's behavior.
* CharacterBase: a generic representation of a CharacterBody, you're probably looking for the two below
* Survivor: A representation of a Survivor, inherits from CharacterBase
* Monster: A representation of a Monster, inherits from CharacterBase
* DamageType: A representation of a ModdedDamageType from R2Api's DamageAPI
* Item: A representation of an Item, utilizes ItemBehaviors for implementing said item's behavior.
* Equipment: A representation of an Equipment, has methods for handling the equipment's activation, can have an ItemBehavior for implementing custom behaviors.
* EliteEquipment: A representation of an EliteEquipment, moreso an Elite itself.
* Interactable: A representation of an Interactable
* Projectile: A representation of a projectile
* Scene: A representation of a Scene
* Unlockable: A representation of an Unlockable and Achievement pair.

Content bases by themselves are extensible, so you can inherit from the "ContentBase" class and create your own

---

### Scriptable Objects

MSU uses heavy use of ScriptableObjects for interacting with the game. these scriptable objects help the creation of content, or help by working alongside ror2's more sensible systems.

MSU comes bundled with 12 ScriptableObjects, these can be split into 5 categories.

* General:
    * MSUnlockableDef: Used by the unlockable module, the MSUnlockableDef not only works as a regular UnlockableDef, but it can also be used to implement an Unlockable's AchievementDef.
    * SerializableDifficultyDef: A Serialized version of a DifficultyDef, Implementation of the difficulty itself is done thru R2API's DifficultyAPI
    * VanillaSkinDef: Can be used to create a skin for a vanilla character, avoids the needless implementation of doing a hook on the skindef's Awake method.
    * MSMonsterFamily: Allows for the creation of a custom monster family, it is implemented inside the CharacterModule and uses DirectorAPI to add them.

* DirectorCards:
    * MSInteractableDirectorCard: Used alongside the Interactable module, the InteractableDirectorCard works as an extension of the InteractableSpawnCard, and has special fields that allow for implementation of the interactable on stages thru R2API's DirectorAPI.
    * MSMonsterDirectorCard: Used alongside the Character moudle, the MonsterDirectorCard works as an extension of the CharacterSpawnCard, and has special fields that allow for implementation of the monster on stages thru R2API's DirectorAPI

* Elites:
    - MSEliteDef: an extended version of an EliteDef, the MSEliteDef has the ability to automatically set the Elite's ramp, on which vanilla tier it spawns, what overlays to use, Effects, and more.
    - SerializedEliteTierDef: (Coming Soon)

* Events:
    * EventdirectorCard: A DirectorCard for MSU's EventDirector, holds information such as the Identifier, likelyhood of spawning, flags, required unlockables and more
    * EventSceneDeck: A Holder for EventDirectorCards, allows the end user to add new events to specific scenes.
    * (More information about events coming soon)

* IDRS:
    * KeyAssetDisplayPairHolder: Used for handling the addition of key assets and display prefabs to the IDRS module
    * MSIDRS: A string based, serializable IDRS that's used in the editor. Replaces key assets and follower prefabs for strings that are used in the module's dictionaries.
    * MSSingleItemDisplayRule: a variation of the MSIDRS, the MSSIDRS works by handling a single key asset and a single display prefab, and can add it to as many item display rule sets as wanted.

---

### Utilities, Interfaces, Attributes and More

MSU would not be a complete library without miscealeous tidbits that help boost the creation of mods, these range from the minimal, but useful, to the incredibly helpful.

* MSDebug: MSU has a configuration option that enables debug features, these features are minimal but can help speedup the debugging of mods. it allows you to connect with a second instance of ror2, automatically deploys the no_enemies command from debug toolkit, adds components for helping with IDRS, and allows you to spawn the MaterialTester

* MaterialTester: MSU comes bundled with KomradeSpectre's Runtime Material Controller. which allows you to insert a variety of materials that use Hopoo Shaders, and modify them in real time.

* Interfaces: MSU uses a large amount of Interfaces for creating the ItemBehaviors for handling buffs, items, and more. These interfaces allow the mod creator to implement new ways of interacting with RoR2's Systems
    * IBodyStatArgModifier - Interface that allows the implementation of R2api's RecalculateStatsAPI.
    * IStatItemBehavior - A more primitive version of IBodyStatArgModifier, it basically allows you to run code that happens before and after the orig(self) of RecalculateStats
    * IOnIncomingDamageOtherServerReceiver - An interface used to modify the incoming damage of a soon to be victim in the damage report.
    * Comes with a fix for IOnKilledOtherServerReceiver, it'll no longer run code twice in a row

* Attributes: MSU comes with attributes that can handle mild issues, and extremely annoying issues that come with creating mods.
    * DisabledContent: Does nothing by itself, put it on top of a content base inheriting class and MSU will ignore it.
    * TokenModifier: allows for the run-time modification of LanguageTokens by using String.Format(), allowing for changes in configuration to be displayed correctly in the Token itself. Requires the language to be loaded using ror2's systems, using R2API's language API will not work.
    * ConfigurableField: allows for the run-time creation of a Config entry of your mod. Easily make an aspect of an item configurable by placing this on a field.

---

### Loaders

 Due to the innate need of MSU to work with RoR2's systems, MSU also comes with so called "Loader" classes.

Loader classes allow the end user to easily load things like Assetbundles, Language files, and handle the Asynchronous loading of the mod using ContentPacks.

- AssetsLoader: Class for handling loading assetbundles, contains method for automatically swapping the stubbed shaders from MoonstormSharedUtils and creation of EffectDefs.
- ContentLoader: Class for handling loading your mod's content, it's main appeal is the ability to load and set up content asynchronously, instead of doing everything in Awake. Contains arrays of Actions for both Loading content and Setting static fields on static types, much like RoR2Content does.
- LanguageLoader: Class for handling loading Language folders, automatically handles loading the Language files into the game's systems for use with the TokenModifier attribute.

## Documentation & Sourcecode

* The Documentation and Sourcecode can be found in MoonstormSharedUtil's Github Repository, which can be found [here](https://github.com/TeamMoonstorm/MoonstormSharedUtils)

## Thanks and Credits

* General Help and Advice: Twiner, KingEnderBrine, IDeathHD, Harb.
* Contributors: KomradeSpectre.
* Programmers: Nebby, KevinFromHPCustomerService.
* Everyone from the Risk of Rain 2 community for appreciating my work.

## Changelog

(Old Changelog can be found [here](https://github.com/TeamMoonstorm/MoonstormSharedUtils/blob/main/MSU/Assets/MoonstormSharedUtils/README-OLD.md))

### '0.8.0'

* Additions:
    * Added a method on MSUtil for playing a networked sound event def
    * The HGCloudRemap controller now has the option to modify the Src and Dst blend enums.
    * Revamped the CharacterModuleBase class
        * Now allows for proper implementation of Monsters, including having them spawn on stages
        * Added the MonsterDirectorCard scriptable object
* Fixes:
    * Added missing Submodule dependency for UnlockableAPI and DirectorAPI
* Other:
    * Moved the entire codebase and project to the github, instead of being inside Starstorm2's Github
    * Rewrote parts of the ReadMe


### '0.7.0'

* Additions:
    * Now finally comes bundled with proper XML based documentation, huzzah!
    * Added Assets, Content and Language Loaders
        * These classes handle external loading of assets, such as assetbundles and language files
        * ContentLoader works as a simplified version of a class implementing IContentPackProvider, and helps with loading content Asynchronously

* Changes:
    * The AchievementDefs are now added directly to the game using R2API

### '0.6.0'

* Changes:
    * MSU no longer has any kind of dependency on AspectAbilities
    * MSU no longer handles the implementation of an aspect ability by itself
    * Removed dependency on Microsoft.Csharp
    * Event Director:
        * No longer should gain negative amounts of credits on custom difficulties with indexes on the negatives.

* Additions
    * Interfaces:
        * Added IBodyStatArgModifier Interface
            * Used for interacting with R2Api's RecalculateStatsAPI
    * Unlockables:
        * Added an Unlockables Module
        * Unlockables module handles the implementation of UnlockableDefs and the creation of AchievementDefs
        * UnlockableDefs and Achievementdefs are made inside the MSUnlockableDef class
        * Unlockables are registered inside UnlockableBase classes. the norm is also having it's related Achievement as a nested class
        * Unlockables can have dependencies on other ContentBases
        * If a dependency is not enabled, the unlockable will not be added to the game
        * In case the dependency is a custom made content base, you can override OnFailedToCheck() method to handle it.
    * Interactables:
        * Added an Interactables Module
        * Interactable Module handles the implementation of custom Interactables to the game
        * Interactables are created from the MSInteractableDirectorCard, which itself inherits from the InteractableSpawnCard
        * Interactablkes are automatically added to stages via DirectorAPI

### '0.5.1'

* Fixed the Damn Readme file.

* Changes:
    * EventAPI
        * Fixed the Director trying to spawn events when there where no available events.
        * Director no longer spawns when there are no events registered.
    * ItemDisplayModuleBase
        * Changed how vanilla IDRS are populated, theyre not taken directly from the BodyCatalog when its initialized.
        * This in turn enables people to add IDRS to other characterBodies from mods.
        * Deprecated "PopulateVanillaIDRSFromAssetBundle()"
    * MSIDRSUtil
        * Deprecated as we're trying to change the standard on how modded IDRS are done

### '0.5.0'

~~* Additions:
    * Added Event system API (*Look, I normally don't do this, okay? I don't really know what else has been done , but this is Starstorm 2's Event API, forcefully ripped out and put in a place where YOU can use it. There is NO documentation. I don't even know if it works. But you can (probably) use it to do cool stuff!
     ...I hope Nebby forgives me for this one.)~~

No, I do not.

Actual changelog:

* Additions:
    * Added the ability to extend from the MoonstormItemManager component.
        * Extending from the manager requires you to extend from the "ManagerExtension" component.
        * Immediate References to the characterBody attatched to the manager extension, the manager itself as well.
        * Virtual methods for GetInterfaces, CheckForItems and CheckForBuffs.
    * Added the EventAPI from Starstorm2Nightly into MSU.
        * The Event API itself is not documented and very much WIP.
        * EventAPI should have everything to add custom events.
        * EventAPI works via a custom director, events themselves are simply entity states.
        * All Events should inherit from the GenericEvent entitystate, which is found in the EntityStates.Events namespace.
* Changes
    * Artifact Content Base:
        * Added OnArtifactEnabled() and OnArtifactDisabled() abstract methods, subscribe and unsuscribe from hooks in these methods. System closely resembles how the Artifact Managers of RoR2 Work.
        * Added an Abstract field for an ArtifactCode from R2API's ArtifactCodeAPI, can be left null.
    * Artifact Module Base:
        * Added some actual hooks onto the RunArtifactManager.
    * Pickups Module Base: Added an Event when the ItemManager is added.
    * Material Tester:
        * Can no longer be spawned outside of runs
        * Renderer is no longer null by default
        * Can now be destroyed easily by enabling the "DestroyOnEnable" component.