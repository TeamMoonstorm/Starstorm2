# Starstorm 2

## About

Starstorm 2 is a work-in-progress RoR2 port/sequel of Starstorm, a diverse gameplay and content mod for Risk of Rain 1. It features two new survivors, over 20 new items and equipment, and a new, dangerous event to shake up your runs.

Starstorm 2 is currently in a bleeding edge state as it's being updated to Survivors of the void, we look for any kind of contribution from outsiders.

## Developing/Contributions

Contributions to the mod via Models and Programming is heavily appreciated, as such, below is a guide on how to clone and properly set up your Starstorm 2 project.

Before we start, here are some guidelines that need to be followed for developing and contributing to Starstorm2:

* (Add something here i cant come up with something atm lol)

## How to clone and develop

* You'll need:
    * Unity Hub
    * Unity version 2019.4.26f1
    * A GIT client (IE: GithubDesktop, Gitkraken, etc)

* Begin by cloning the repository to your hard drive

![](https://cdn.discordapp.com/attachments/1035279289668616202/1035279344601399397/unknown.png)

* Once the project downloads, open the `SS2-Project` folder with Unity 2019.4.26f1, keep in mind that opening the project for the first time will take time, so patience is key

![](https://media.discordapp.net/attachments/1035279289668616202/1035282400088952832/unknown.png)

* During your importing or opening the project, or any other scenario, there is a chance you'll see this box pop up, Always hit "No Thanks", as hitting the other option WILL cause issues with specific assemblies.

![](https://cdn.discordapp.com/attachments/1035279289668616202/1035282616154337411/unknown.png)

* Once the project opens, you'll have a bunch of errors, these errors **are normal**, and are caused by missing ror2 assemblies, to fix this, Go to ``Assets/ThunderKitSettings`` folder, and look for the "ImportConfiguration" file
    * If there are no Configuration Executors, delete the ImportConfiguration so thunderkit can regenerate it.
    * If no configurations exist after this, please contact Nebby and ask for help

![](https://cdn.discordapp.com/attachments/1035279289668616202/1035284101839720520/unknown.png)

* Once the import configurations are created, go to Tools/Thunderkit/Settings

![](https://cdn.discordapp.com/attachments/1035279289668616202/1035292228303736853/unknown.png)

* Select the Import Configurations, ensure that your configurations match the one from the image.
    * If you cant find one of the configurations, delete teh import configuration.

* Make sure your import config matches the following:

| Importer Name | Enabled or Disabled | Extra Config |
|--|--|--|
| PostProcessing Unity Package Installer | Enabled |  |
| TextMeshPro Uninstaller | Enabled |  |
| Unity GUI Uninstaller | Enabled |  |
| Wwise Blacklister | Enabled |  |
| Assembly Publicizer | Enabled | Publicize at least RoR2.dll and KinematicCharacterController.dll |
| MMHook Generator | Enabled | Generate MMHook assemblies for at least RoR2.dll and KinematicCharacterController.dll |
| Import Assemblies | Enabled |  |
| RoR2 LegacyResourceAPI Patcher | Enabled |  |
| Import Project Settings | Enabled | Set the enum to Everything |
| Set Deferred Shading | Enabled |  |
| Create Game Package | Enabled |  |
| Import Addressable Catalog | Enabled |  |
| Configure Addressable Graphics Settings | Enabled |  |
| Ensure RoR2 Thunderstore Source | Enabled |  |
| Install BepInEx | Enabled |  |
| R2API Submodule Installer | Enabled | Make sure the following are enabled: ArtifactCode; Colors; ContentManagement; Core; DamageType; Difficulty; Director; Dot; Elites; Networking; Prefab; RecalculateStats |
| Install Ancient Scepter | Enabled |  |
| Install RoR2MultiplayerHLAPI | Disabled |  |
| Install RoR2EditorKit | Disabled |  |
| Install DebugToolkit | Enabled |  |
| Get Bitness | Enabled |  |

* Select `ThunderKit Settings` and click Browse.
    * Find your RoR2 Executable, select it
    * Hit Import

![](https://cdn.discordapp.com/attachments/1035279289668616202/1035292666038071296/unknown.png)

* You're now ready to start development.