## DebugToolkit

This mod adds various debugging commands to the console. See below for all commands plus explanation.

[Harb](https://thunderstore.io/package/Harb/), [iDeathHD](https://thunderstore.io/package/xiaoxiao921/) and ['s](https://thunderstore.io/package/paddywan/) reimplementation of [Morris1927's](https://thunderstore.io/package/Morris1927/) [RoR2Cheats](https://thunderstore.io/package/Morris1927/RoR2Cheats/). Derived with permission. 

Also adds autocompletion for arguments and networked commands giving their information to the right people to the console.

Track update progress, get support and suggest new features over at the [DebugToolkit discord](https://discord.gg/yTfsMWP).

Some vanilla console functions you might not know:

* The console can be opened with `ctrl+alt+~`.
* `help {command}` may be used to get help for a specific command
* `find {term}` can be used to find commands with that term.
* `max_messages {nr}` changes how much scroll back the console has. We auto change this to 100 for you if it's on default.

Mods recommended for combined use:

* [KeyBindForConsole](https://thunderstore.io/package/kristiansja/KeyBindForConsole/) for easier enabling of the console. Especially useful for non-US keyboard layouts.
* [R2DSE](https://thunderstore.io/package/Harb/R2DSEssentials/) for running DT on dedicated servers.
* [MidRunArtifacts](https://thunderstore.io/package/KingEnderBrine/MidRunArtifacts/) for enabling and disabling artifacts during a run.

You may contact us at any time through [issues on GitHub](https://github.com/harbingerofme/DebugToolkit/issues/new/choose), the [dedicated discord server]((https://discord.gg/yTfsMWP) or through the [Risk of Rain 2 modding Discord](https://discord.gg/5MbXZvd) found at the top of the Thunderstore website. 

---

## Additional Contributors ##

* [DestroyedClone](https://thunderstore.io/package/DestroyedClone/) ([Github](https://github.com/DestroyedClone))
* [Rays](https://github.com/SuperRayss)

---

## COMMANDS ##

Verbiage: if an argument is encapsulated with brackets, it means it's either `(choose one)`, `{needed freeform}`, or `[optional freeform]`. The following may be used to indicate the default value: `def X`, a `*` denotes the default value cannot be entered.

* **next_stage** - Advance to the next stage: `next_stage [specific stage]`. If no stage is entered, the next stage in progression is selected.
* **force_family_event** - Forces a Family Event to happen in the next stage, take no arguments. `family_event`
* **next_boss** - Sets the teleporter boss to the specified boss. `({localised_object_name}|{DirectorCard}) [count def 1] [EliteIndex def -1/None]`
* **fixed_time** - Sets the time that has progressed in the run. Affects difficulty. `fixed_time [time]`. If no time is supplied, prints the current time to console.
* **add_portal** - Teleporter will attempt to spawn a blue, gold, null, or celestial portal: `add_portal (blue|gold|celestial|null|void|deepvoid|all)`
* **seed** - Set the seed for all next runs this session. `seed [new seed]`. Use `0` to specify the game should generate its own seed. If used without argument, it's equivalent to the vanilla `run_get_seed`.
* **kill_all** - Kills all members of a specified team. `kill_all [teamindex def 2]` Team indexes: 0=neutral,1=player,2=monster. 
* **true_kill** - Truly kill a player, ignoring revival effects `true_kill [player def *you]`
* **respawn** - Respawn a player at the map spawnpoint: `respawn [player def *you]`
* **time_scale** -  Sets the timescale of the game. 0.5 would mean everything happens at half speed. `time_scale [time_scale]`. If no argument is supplied, gives the current timescale.
* **post_sound_event** - Post a sound event to the AkSoundEngine (WWise) by its event name: `post_sound_event [eventName]`

* **list_player** - Shows list of players with their ID
* **list_body** - List all Bodies and their language invariants.
* **list_ai** - List all Masters and their language invariants
* **list_item** - List all items and if they are in the current drop pool.
* **list_equip** - List all equipment and if they are in the current drop pool.
    

* **give_item** - Give item directly to the player's inventory: `give_item {localised_object_name} [count def 1] [player def *you]`
* **random_items** - Generate random items from the available droptables. `random_items {Count} [player def *you]`
* **give_equip** - Give equipment directly to a player's inventory: `give_equip {localised_object_name|'random'} [player def *you]`
* **give_money** - Gives the desired player/team money `give_money {amount} [(all | [player]) def all]`
* **give_lunar** - Gives the specified amount of lunar coins to the issuing player. A negative count may be specified to remove that many. `give_lunar {amount def 1}`
* **remove_item** - Removes an item from a player's inventory. `remove_item (localised_object_name | 'all') [(player | 'all') def *you]`
* **remove_equip** - Sets the equipment of a player to 'None'. `remove_equip {localised_object_name} [player def *you]`
* **create_pickup** - Creates a pickup in front of the issuing player. Pickups are items, equipment and lunar coins. Additionally 'item' or 'equip' may be specified to only search that list. `create_pickup (localized_object_name| "coin") [('item'|'equip') def *both]`

* **spawn_interactable/spawn_interactible** - Spawns an interactible in front of the player. `(spawn_interactable|spawn_interactible) {InteractableSpawnCard}`
* **spawn_ai** - Spawn an AI: `Requires 1 argument: spawn_ai {localised_objectname} [Count:1] [EliteIndex:-1/None] [Braindead:0/false(0|1)] [TeamIndex:2/Monster]`. Elite indexes: -1=None, 0=Fire,1=Overloading,2=Ice,3=Malachite,4=Celestine. Team indexes: 0=neutral,1=player,2=monster. 
* **spawn_as** - Spawn as a new character. Type body_list for a full list of characters: `spawn_as {localised_objectname} {playername}`
* **spawn_body** - Spawns a CharacterBody: `spawn_body {localised_objectname}`
* **change_team** - Change team to Neutral, Player or Monster: `change_team {teamindex}`. Team indexes: 0=neutral,1=player,2=monster. 

* **no_enemies** - Toggles enemy spawns.
* **god** - Toggles HealthComponent.TakeDamage for all players. AKA: you can't take damage.
* **lock_exp** - Prevents EXP gain for the player team.
* **noclip** - Toggles noclip. Allow you to fly and going through objects. Sprinting will double the speed.

* **dt_bind** - Bind a key to execute specific commands. `dt_bind {key} {consolecommands seperated by ;}`
* **dt_bind_delete** Remove a custom bind. `dt_bind_delete {key}`
* **dt_bind_reload** Reload the macro system from file. `dt_bind_reload` 

* **kick** - Kicks the specified Player Name/ID from the game.
* **ban** - Session bans the specified Player Name/ID from the game.

* **perm_enable** - Enable or disable the permission system.
* **perm_mod** - Change the permission level of the specified PlayerID/Username with the specified Permission Level.

* **perm_reload** - Reload the permission system, updates user and commands permissions.
* **reload_all_config** - Reload all default config files from all loaded BepinEx plugins.

### Unlocked Vanilla Commands ###

* **sv_time_transmit_interval** - How long it takes for the server to issue a time update to clients. `sv_time_transmit_interval [time]`
* **run_scene_override** - Overrides the first scene to enter in a run. `run_scene_override [stage]`
* **stage1_pod** - Whether or not to use the pod when spawning on the first stage. `stage1_pod [(0|1)]`
* **run_set_stages_cleared**  - Sets the amount of stages cleared. This does not change the current stage. `run_set_stages_cleared {stagecount}`. This obsoletes `stage_clear_count` from previous RoR2Cheats versions.
* **team_set_level** - Sets the specified team to the specified level: `team_set_level {teamindex} {level}` Team indexes: 0=neutral,1=player,2=monster. This obsoletes `give_exp` from previous RoR2Cheats versions.
* **loadout_set_skill_variant** - Sets the skill variant for the sender's user profile: `loadout_set_skill_variant {body_name} {skill_slot_index} {skill_variant_index}`. Note that this does not use the loose bodymatching from custom commands.
* **set_scene** - Removed the cheat check on this. Functions similar but not really to our `next_stage`, doesn't have our cool autocomplete features, and doesn't advance the stagecount, but can advance menus. `set_scene {scene}`

### Additional Macros ###

* **midgame** - This is the preset HopooGames uses for midgame testing. Gives all users random items, and drops you off in the bazaar. `midgame`
* **lategame** - This is the preset HopooGames uses for endgame testing. Gives all users random items, and drops you off in the bazaar. `lategame`
* **dtzoom** - Gives you 20 hooves and 200 feathers to get around the map quickly. Based on a command in the initial release of Risk of Rain 2.

---

## Changelog ##

### 3.7 ###

* "Survivors of the Void"
* **General**
    * Updated to work with the new game update.
* **3.7.1**
    * Fixed: `scene_list` was not prodiving all scenes. Thank you [DestroyedClone](https://thunderstore.io/package/DestroyedClone/)!
    * Fixed: `spawn_as` made people softlock.
    * Added: Support for void portals. Thank you [DestroyedClone](https://thunderstore.io/package/DestroyedClone/)!
    * Fixed: `set_scene` was broken.
    * Added: `set_scene` now also allows numbers from `scene_list` to be passed.
    * 'Fixed': `set_scene` and `next_stage` allowed players to visit DLC scenes when the DLC was not enabled.

### 3.6 ###

* "DebugToolkit"
* **General**
    * Microsoft.CSharp.dll dependency is gone.
* **Additions**
    * `spawn_interactable` Now support custom interactables.
    * `spawn_interactible` Does the same as `spawn_interactable`.
    * `lock_exp` toggles EXP gain, note that this applies to all players. Thank you [DestroyedClone](https://thunderstore.io/package/DestroyedClone/)!
*  **Fixes**
    * Description of `add_portal` now mention the null portal, also you can now spawn it in teleporter-less stages.

### 3.5 ###

* "Anniversary Update"
*  **Fixes**
    * Updated for anniversary update.
    * **3.5.1** Updated for the update to the anniversary update.

### 3.4 ###

* "Full Release"
* **General**
    * **3.4.1** Made some internal classes public.
    * **3.4.1** Now supports custom stages too.
    * **3.4.1** Now has keybinds and macros.
    * **3.4.1** We are considering moving the permissions module to it's own seperate mod. Please give your feedback.
    * **3.4.2** Harb was a dumb dumb and didn't put the Microsoft assembly in the zip.
    * Updated for game version `1.0`.
    * ~~We are considering adding macros and keybinds to the base mod. [We would like your input on this.](https://github.com/harbingerofme/DebugToolkit/issues/101)~~
* **Additions**
    * **3.4.1** `dt_bind` You can now bind keys to macros. 
    * `list_item` When we removed this command, we forgot that people can forget. As the game now counts over 100 items, there's value in adding these lists back. They have been improved for more user readability.
    * `list_equip` (see list_item)
* **Fixes**
    * **3.4.1** Fixed an issue introduced in Gameversion 1.0.1 where `list_item` and `list_equip` wouldn't work.
    * **3.4.2** Fixed needing to have a config file so a config file could be generated.
    * **3.4.2** Fixed not having the Microsoft assembly in the zip.

### 3.3 ###

* "Artifacts"
* **General**
    * **3.3.2** Improved networking
    * **3.3.1** Removed minirpclib as a dependency. [Learn how for your own mod here](https://github.com/risk-of-thunder/R2Wiki/wiki/Networking-with-Weaver---The-Unity-Way)
    * Updated for artifacts.
    * `give_money` is now compatible with ShareSuite's money sharing.
    * We did a lot of cleanup behind the scenes. You won't notice this (hopefully), but it makes everything easier to maintain.
    * We've added some predefined macros for testing. We still recommend the macro plugins linked in our description if you want to define your own macros.
* **Additions**
    * **3.3.2** Permission System can now be enabled on the RoR2 Assembly Console Commands.
    * `random_items` generates an inventory of items for you.
    * `give_equip random` now gives you a random equipment from your available drop pool.
    * `midgame` and `lategame` allow you to test your mod in a typical mid/endgame scenario. The presets come from HopooGames.
    * `dtzoom` gives you some items to make you move around the map faster. This is a macro that was present in the original release of RoR2.
* **Fixes**
    * **3.3.2** Fix noclip failing to work in any subsequent run after it was activated.
    * **3.3.1** Fix incorrect parsing on some arguments in commands. Thank you Violet for reporting these!
    * **3.3.1** Removed double embbeded dependency. This shaves off about half the file size!
    * Fix a faulty ILHook in CCSetScene
    * Fix `spawn_as`.

### 3.2 ###

* "Dedicated Servers"
* **General**
	* Commands are now safe to run with a dedicated server as the sender. Want to know how you can issue commands during runtime as a server? [R2DSE](https://github.com/harbingerofme/R2DS-Essentials)
		* Some commands have not yet been implemented this way and merely do nothing, expect fixes for that in `3.2.X` updates.
		* When a player is needed for a command, dedicated servers will have to fully qualify their command.
* **Additions**
    * **3.2.1** reworked `spawn_ai` so that it can now spawn multiple enemies. As a result the command arguments have been shuffled around a bit.
    * Added the null portal to `add_portal`
    * `reload_all_config` makes a best effort to reload all plugins' configurations.
    * Permission system:
        * There's a config for the permission system located in your `bepinex/config/` folder after running the new version once.
        * The permission system is by default DISABLED.
* **Fixes**
    * **3.2.2** Fix seeveral premature game ends with noclip enabled crashing outside stuff.
    * **3.2.2** DT now correctly handles nonpostive enums. this fixes an annoying persistent issue in `spawn_ai`.
    * **3.2.1** `spawn_ai` now parses braindead if it's an integer.
    * **3.2.1** Fix `spawn_as` crashing dedicated servers.
    * **3.2.1** fixed_time now actually sets/displays the ingame time. Thank you [@Rayss](https://github.com/SuperRayss).
	* `Noclip` now disables the out of bounds teleport. Go take a look at that collossus head!
    * `add_portal` now gives a nicer error message with all available portals
    * Reenabled `next_boss`. Behaviour might still be weird.
* **Known Bugs**
    * Some commands work improperly when connected to a dedicated server with this enabled. Due to the amount of testing required basically doubling because of dedicated servers, we ask you to report these issues as you spot them.
* Other things:
    * Want to get bleeding edge builds? Check out our [Discord](https://discord.gg/yTfsMWP)!

### 3.1 ###

* "DebugToolkit"
* **General**
	* **3.1.2:** Disabled `next_boss` so that everyone can use it to update their mods.
	* **3.1.1:** Now known as DebugToolkit on Thunderstore.
	* **3.1.1:** Removed obsoleted commands.
	* **Paddywan** has left the modding community. We thank them for their contributions to this project and wish them the best of luck in future endavours.
    * **iDeathHD** has joined the team behind RoR2Cheats/DebugToolkit, their expertise has been of amazing use and we're excited to have a joint venture in this.
    * You may have noticed *MiniRPCLib* is a new dependency. This is to network cheats better over the network. Functionally nothing has changed.
    * A secret new convar is added: `debugtoolkit_debug`. Only available for those people who read the changelog. ❤️
    * Various commands have had their ingame descriptions updated to better inform what args are expected.
	* added the *"all"* overload to `add_portal`.
    * *Modders stuff:* Hooks that do not permanently need to exists have been been made to only exist for as long as they are needed.
    * *Modders stuff:* All hooks, temporary or not, have been added to the readme to help resolve mod conflicts.
* **New Commands**
	* **3.1.1:** `post_sound_event` Sounds be hard. This should help.
    * `next_boss` We've worked hard on this. We hope you find use for it. *(And with 'we', Harb means the other contributor who isn't Harb nor iDeathHD.)*
    * `give_lunar` Editing your save is an unnessecary task. This command is restricted to the issuing player to prevent grieving.
    * `remove_item` While this functionality could already be achieved with *give_item* and a negative amount, this was not obvious.
    * `remove_equip` While this functionality could already be achieved with `give_equip None, this was not obvious.
    * `create_pickup` A lot of custom item mods also need to test their descriptions. Maybe you have an on pickup hook.
    * `force_family_event` We initially tried it being able to do any family, but this proved to be hard. So instead we force an event to happen next stage.
    * `noclip` Fly freely through the map.
    * `kick` Not much to say there are better ways to resolve your issues with players. 
    * `ban`You can talk it out.
    * `spawn_interactible` Implemented with full range of interactles, not limited to types. Accepts InteractableSpawnCard partial as parameter.
* **Fixes**
    * `Spawn_as` now temporarily disables arriving in the pod to prevent not being able to get out of a pod.
    * Clients now see the output of commands with the ExecuteOnServer flag. *This change only applies to commands created by this mod.*
    * Host now sees the input of commands with the ExecuteOnServer flag. *This change applies to all commands, even that of other mods.*
    * `set_team` is now smarter in detecting which team you want.
	* `add_portal` now sets better values to get the vanilla orb messages to appear in chat.
    * Several issues with argument matching are now resolved better.
    * Special characters are now handled better (or rather, are ignored completely) when matching arguments.
	* `set_scene` is now no longer denying access to you because you don't have cheats enabled. (you do, after all.)

### 3.0 ###

* "Initial Release"
* **General**
    * Reworked almost every command to be more maintainable. This should have no impact on current users.
    * FOV is now outside the scope of this project and has thus been removed
* **New Features**
    * Object & Character names can now be queried using partial language invariants. For example: `give_item uku` will give one ukulele under "en".
    * Several vanilla cheats have now been unlocked.
* **Fixes**
    * No longer forcefully changes FOV settings.
    * No longer hooks stuff for disabling enemies, improving mod inter compatibility.
    * `seed` is now networked.
    * Elites now spawn with correct stats, and should be update-proofed.

### < 3.0 ###

See [the old package](https://thunderstore.io/package/Morris1927/)

---

## Hooks ##

This mod always hooks the following methods:

* `On.RoR2.Console.InitConVar` - We do this to 'free' the vanilla convars and change some vanilla descriptions.
* `On.RoR2.Console.RunCmd` -  We do this to log clients sending a command to the Host.
* `IL.RoR2.Console.Awake` - We do this to 'free' `run_set_stages_cleared`.
* `IL.RoR2.Networking.GameNetworkManager.CCSetScene` - We do this to 'free' `set_scene`.


This mod hooks the following methods when prompted:

* `On.RoR2.PreGameController.Awake` - We use this to change the seed if needed.
* `On.RoR2.CombatDirector.SetNextSpawnAsBoss` - We use this for `set_boss`.
* `On.RoR2.ExperienceManager.AwardExperience` - We use this for `lock_exp`.
* `On.RoR2.Stage.Start` - We hook this to remove the IL hook on ClassicStageInfo.
* `IL.RoR2.ClassicStageInfo.Awake` - We hook this to set a family event.
* NoClip requires 3 hooks:
	* `On.RoR2.Networking.GameNetworkManager.Disconnect` 
	* These two trampoline hooks are initialized, but are only applied on prompt:
		1. UnityEngine.NetworkManager.DisableOnServerSceneChange
		2. UnityEngine.NetworkManager.DisableOnClientSceneChange
	
