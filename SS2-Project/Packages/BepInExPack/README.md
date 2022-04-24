### BepInEx Framework + API

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
* `BepInEx/plugins` - This is where normal mods/plugins are placed to be loaded. If a mod is just a `NAME.dll` file, it probably goes in here.For your own organisation, consider putting them inside folders, eg: `plugins/ActualRain/ActualRain.dll`

* `BepInEx/patchers` - These are more advanced types of plugins that need to access Mono.Cecil to edit .dll files during runtime. Only copy paste your mods here if the author tells you to.

* `BepInEx/config` - If your plugin has support for configuration, you can find the config file here to edit it.

* `BepInEx/core` - Core BepInEx .dll files, you'll usually never want to touch these files (unless you're updating)


### What is included in this pack

**BepInEx 5.4** - https://github.com/BepInEx/BepInEx
This is what loads all of your plugins/mods. 

**Customized BepInEx configuration**
BepInEx config customized for use with RoR2

### Writing your own mods

There's 2 documentation pages available:

* [R2Wiki](https://github.com/risk-of-thunder/R2Wiki/wiki)
* [BepInEx docs](https://bepinex.github.io/bepinex_docs/v5.3/articles/index.html)

Places to talk:
* [RoR2 modding discord](https://discord.gg/5MbXZvd)
* [General BepInEx discord](https://discord.gg/MpFEDAg)


BepInEx contains helper libraries like [MonoMod.RuntimeDetour](https://github.com/MonoMod/MonoMod/blob/master/README-RuntimeDetour.md) and [HarmonyX](https://github.com/BepInEx/HarmonyX/wiki)