# BepInEx.MonoMod.HookGenPatcher

Generates [MonoMod.RuntimeDetour.HookGen's](https://github.com/MonoMod/MonoMod) `MMHOOK` file during the [BepInEx](https://github.com/BepInEx/BepInEx) preloader phase. 

Installation:
Put in `BepInEx\patchers` folder.
Make sure the `MonoMod.exe` and `MonoMod.RuntimeDetour.HookGen.exe` files are also present.

**This project is not officially linked with BepInEx nor with MonoMod.**

## See also:
[LighterPatcher](https://thunderstore.io/package/Harb/LighterPatcher/) which is designed to easy the load on the game when having particularly large MMHOOK files by stripping unneeded types. LighterPatcher and HookGenPatcher work in conjunction with each other to prevent multiple runs.
