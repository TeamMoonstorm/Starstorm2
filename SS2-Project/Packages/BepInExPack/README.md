## BepInEx Framework

This is the pack of all the things you need to both start using mods, and start making mods using the BepInEx framework.

To install, either refer to [installation guide on R2Wiki](https://github.com/risk-of-thunder/R2Wiki/wiki/BepInEx),

or extract contents of the inner BepInExPack to the gamefolder, such that the `winhttp.dll` file sits right next to `RiskOfRain2.exe`.

The zip looks like:

```
\BepInExPack    <----- move the contents of this folder
manifest.json
readme.md
icon.png
```

### What each folder is for:

-   `BepInEx/plugins` - This is where normal mods/plugins are placed to be loaded. If a mod is just a `NAME.dll` file, it probably goes in here. For your own organisation, consider putting them inside folders, eg: `plugins/ActualRain/ActualRain.dll`

-   `BepInEx/patchers` - These are more advanced types of plugins that need to access Mono.Cecil to edit .dll files during runtime. Only copy paste your mods here if the author tells you to.

-   `BepInEx/config` - If your plugin has support for configuration, you can find the config file here to edit it.

-   `BepInEx/core` - Core BepInEx .dll files, you'll usually never want to touch these files (unless you're updating manually)

### What is included in this pack

**BepInEx 5.4** - https://github.com/BepInEx/BepInEx
This is what loads all of your plugins/mods.

**Customized BepInEx configuration**
BepInEx config customized for use with RoR2.

### Writing your own mods

There's 2 documentation pages available:

-   [R2Wiki](https://risk-of-thunder.github.io/R2Wiki/)
-   [BepInEx docs](https://docs.bepinex.dev/)

Places to talk:

-   [RoR2 modding discord](https://discord.gg/5MbXZvd)
-   [General BepInEx discord](https://discord.gg/MpFEDAg)

BepInEx contains helper libraries like [MonoMod.RuntimeDetour](https://github.com/MonoMod/MonoMod/blob/master/README-RuntimeDetour.md) and [HarmonyX](https://github.com/BepInEx/HarmonyX/wiki)

### Changelog

-   **5.4.2103**

    -   RoR2 BepInExPack specific changes :
        -   Fix for DynamicBones log spam.
        -   Fix for log spam on some deaths.

-   **5.4.2102**

    -   RoR2 BepInExPack specific changes :
        -   Fix FixPluginTypesSerialization sometimes making the game crash on exit.
        -   Fix Copy Log File button in the BepInEx GUI not working correctly.

-   **5.4.2101**

    -   RoR2 BepInExPack specific changes :
        -   Remove the console dev check in the BepInEx.GUI, replaced by a disclaimer instead: If you want #tech-support in the modding discord, please use the "Copy Log to Clipboard" button and then paste it in the discord channel.

-   **5.4.2100**

    -   Updated BepInEx to 5.4.21
    -   RoR2 BepInExPack specific changes :
        -   New BepInEx GUI, it should fix some performance issues the old one had + the .zip is now much smaller, you can still go back to the old console by setting to `true` the `Enables showing a console for log output` option in the `BepInEx/config/BepInEx.cfg` file
        -   Thunderstore Mod Manifests if available are printed in the log file to allow for better debugging.
        -   `FixPluginTypesSerialization` patcher is now shipped by default
        -   The `RoR2BepInExPack` (v1.1.0) plugin now contains a mod compatibility fix for when multiple corruption (void items) targets for an item are present, a config is available to determine which gets the new stack:
            -   Random -> (Default Option) picks randomly
            -   First -> Oldest Target Picked Up
            -   Last -> Newest Target Picked Up
            -   Rarest -> Rarest Target Picked Up (falls back to Newest on ambiguity)
            -   Alternate -> All targets get a turn in acquisition order

-   **5.4.1905**

    -   RoR2 BepInExPack specific changes :
        -   Fix achievements not working correctly. For real this time.

-   **5.4.1904**

    -   RoR2 BepInExPack specific changes :
        -   Fix achievements not working correctly

-   **5.4.1903**

    -   RoR2 BepInExPack specific changes :
        -   Fix the BepInEx GUI sometimes not being visible at all / full white

-   **5.4.1902**

    -   RoR2 BepInExPack specific changes :
        -   Plugin Entrypoint is now at RoR2.FlashWindow..ctor

-   **5.4.1901**

    -   RoR2 BepInExPack specific changes :
        -   HideAndDontSave set back to false in config
        -   Plugin Entrypoint is now at RoR2.RoR2Application.Awake

-   **5.4.1900**

    -   Added basic fix for cases where games try to ship their own Harmony
    -   Updated HarmonyX to 2.9.0
    -   Updated MonoMod to 22.01.29.01
    -   RoR2 BepInExPack specific changes :
        -   Added BepInEx.GUI which replace the default console (you can disable it in the settings)
        -   HideAndDontSave set to true in config
        -   Add a plugin which detour old Resources.Call to Addressable equivalent

-   **5.4.1801**

    -   Updated MonoMod to 22.01.04.03, fixing problems in some Unity environments (wine)

-   **5.4.18**

    -   Fixed some console messages being cut off (especially if using non-ASCII characters)
    -   Updated HarmonyX to 2.7.0
    -   Updated MonoMod to 21.12.13.01

-   **5.4.17**

    -   Fixed console not opening in Outer Wilds and other games that ship custom `user32.dll`

-   **5.4.16**

    -   Fixed when DumpAssemblies is enabled, dumped assemblies are now put to `BepInEx/DumpedAssemblies/<ProcessName>`. If assembly is in use (e.g. multiple game processes open), dumped assemblies will have a number postfix.
    -   Game executable timestamp is not included in console title now (fixes issue with some window managers)
    -   Updated HarmonyX to 2.5.5
    -   Updated MonoMod.RuntimeDetour to 21.9.19.1

-   **5.4.15**

    -   Update HarmonyX to 2.5.4
    -   Update MonoMod to 21.8.5.1

-   **5.4.14**

    -   Update HarmonyX to 2.5.3
    -   Update MonoMod to 21.7.22.3

-   **5.4.13**

    -   Update HarmonyX to 2.5.2
        -   Fixes an issue that prevented BepInEx from launching on certain games

-   **5.4.12**

    -   Log executable timestamp in preloader logs
    -   Fix BepInEx \*nix launch script
        -   Add support for the new Steam bootstrapper
        -   Add experimental fix for resolving symlinks
    -   Update Doorstop to 3.4.0.0
    -   Fix crash in paths with non-ASCII characters
    -   Experimental fix for HarmonyXInterop not working sometimes on first game launch
    -   Update HarmonyX to 2.5.1

-   **5.4.11**

    -   Update Bepin GOs to use HideAndDontSave flags
        -   Disable setting BepInEx manager to HideAndDontSave by default
        -   New config option: [Chainloader].HideManagerGameObject which enables HideAndDontSave for manager GameObjects
    -   Fix ChainLoader.HasBepinPlugins returning false when inheriting from class from another Assembly

-   **5.4.10**
    -   Updated HarmonyX to 2.4.2
    -   Updated UnityDoorstop.Unix to 1.5.1.0
    -   Marked BepInEx plugin manager and ThreadingHelper GameObjects with HideAndDontSave flag to prevent it from being destroyed in some games
    -   Converted configuration files to always use UTF-8 without BOM
    -   Fixed headless mode check throwing an exception in some games
