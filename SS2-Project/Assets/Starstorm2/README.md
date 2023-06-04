
![Starstorm 2](https://github.com/swuff-star/Starstorm2/blob/main/SS2-Project/Assets/Starstorm2/ReadmeAssets/ss2logopurple.gif?raw=true)

**Starstorm 2** is a work-in-progress adaptation of the Risk of Rain 1 mod "Starstorm". It currently features two new survivors, over 20 new items and equipment, and numerous new challenges to shake up your runs.

![New Survivors...](https://github.com/swuff-star/Starstorm2/blob/main/SS2-Project/Assets/Starstorm2/ReadmeAssets/ss2survivorpromo.gif?raw=true)

* **Executioner**  - An aggressive, versatile survivor with an arsenal made for picking off targets and chaining kills. His powerful Ion Manipulators gain charges with every kill.

* **???** - A survivor of unknown origin, familiar yet unfamiliar. Under the right conditions, you may meet him on the Planet, but he certainly won't be on your side.

![New Items...](https://github.com/swuff-star/Starstorm2/blob/main/SS2-Project/Assets/Starstorm2/ReadmeAssets/ss2itemspromo.gif?raw=true)

* **New Items & Equipment** - A plethora of new items and equipment to support new and existing playstyles. Watch your six with an Armed Backpack, rush through teleporter events with a Field Accelerator, all while keeping on the move with a Swift Skateboard - and more! 

![New Challenges...](https://github.com/swuff-star/Starstorm2/blob/main/SS2-Project/Assets/Starstorm2/ReadmeAssets/ss2challengepromo.gif?raw=true)

* **Events** - Between severe weather, magical buildups, and mysterious invaders, learn to expect the unexpected as forces both natural and supernatural interfere with your runs.

* **Typhoon** - The Planet grows restless, with fearsome events and monsters coming in greater numbers than ever before. Unlock new skins for survivors* by proving yourself in this new challenge exceeding even Monsoon!
	 *currently includes Commando, MUL-T, Acrid, Captain, and Commando's Vestige.
	 
* **Artifact of Cognation** - A new, unlockable artifact to shape up your runs.

![Credits](https://github.com/swuff-star/Starstorm2/blob/main/SS2-Project/Assets/Starstorm2/ReadmeAssets/ss2credits.gif?raw=true)

Starstorm 2 is currently being developed and maintained by...
* Nebby, prod, swuff★, Zenithrium

With previous and one-off code contributions by...
* Anreol, bread, Flan, Gaforb, GrooveSalad, HIFU, KevinFromHPCustomerService, malfaisant, Moffein, Nebby, Noop, Phreel, rob, swuff★, sebacame, Shared, TheTimesweeper, Vale-X, xpcybic, Xubas

With Art/Modelling/Animation contributions by...
* Alexstarwras, Anreol, Bolty, bread, bruh, Cexeub, dotflare, Draymarc, Domi, JestAnotherAnimator, Gem, goodguy, GrooveSalad, LucidInceptor, Neik, KevinFromHPCustomerService, PapaZach, Plexus, prodzpod, QuietAnon, redacted, rob, SkeletorChampion, SOM, Spenny, swuff★, xpcybic, Reithierion, UnknownGlaze, Zenithrium

Additional thanks to... 
* Writing - Blimblam, Lyrical Endymion, swuff★, T_Dimensional, Zenithrium
* Translations - Anreol (Spanish), MysticSword (Russian), Damglador (Ukrainian), 乃茶, Etundit, Dying_Twilight (Chinese)
* Sound - KevinFromHPCustomerService, Neik, SOM, UnknownGlaze
* Special thanks - Altzeus, DestroyedClone, Dee'd, don, Gnome, JesusPxP, KomradeSpectre, MinimalEffort, Moshinate, MysticSword, Riskka, Ruxbieno, SalvadorBunny, SlipSkip, Twiner, valerie ♥ 

## Feedback, Bug Reporting & Known Issues

Bugs can be reported at our [GitHub page](https://github.com/TeamMoonstorm/Starstorm2/issues). A list of known & previously reported issues can also be found here. Please include an [Output Log](https://h3vr-modding.github.io/wiki/installing/troubleshooting/log_file.html) and detailed steps to help recreate your error! Feedback about the modded characters pertaining to balance and gameplay can be discussed in our [Discord server](https://discord.com/invite/SgFxwKT7nY).
## Changelog

**Warning: content spoilers below!**

### 0.4.5
* General
    * Fixed a crash caused by a conflict with other mods.
    * Fixed specific configs for Needles and Coffee Bag not appearing in the correct config file.
* Nemesis Commando
    * Skill unlocks now use updated skill icons.
    * Fixed a bug where Nemesis Commando would be invulnerable after exiting the portal for the first stage.
* Executioner
	* Fixed kill assists not counting towards Ion Manipulator stocks and increased window for which attacks will count as kill assists.
* Items
    * Needles no longer spams debug logs, and also now checks that the target is not on the same team as the attacker when applying the debuff.


### 0.4.4
* General
    * Instances of "DestroyImmediate" in code have been replaced with Destroy. May fix some crashes.
* Executioner
    * Kills on feared enemies no longer grants CDR.
* Items
    * Items and equipments should now be properly disabled when using the sweeping item/equip disables.
    * Fixed a bug where Needles messed up how crits should be handled, and could stack to a ridiculous stack amount. 
    * Termination's cooldown debuff can no longer be cleared by Blast Shower.

### 0.4.3
* Executioner
	* Fixed a bug where Execute would spawn extra indicators.
	* Thanks TimeSweeper

### 0.4.2
* Oops

### 0.4.1
* General
    * Overhauled Executioner and Nemesis Commando entirely. BE SURE TO RESET YOUR CONFIG.
    * Reimplemented survivor-related unlocks. Executioner is intentionally unlocked by default now.
    * SS2 Survivors have item displays for all base and SOTV items. 
    * Added significantly more configs, split into individual files. You can now disable events individually, disable equipments, and items that were lacking in configurable options now have them.
    * Adjusted Typhoon - Now additionally has +25% increased spawn rate and -20% enemy rewards.
    * Added Chinese translation. 
    * Adjusted Cloaking Headband's Russian translation.
    * Commando now correctly fires phase round projectiles in multiplayer with the Stealth skin equipped.
    * Fixed "Key Not Found" exception being thrown after ally death.
    * Added R2API TempVFX dependency.
* Items & Equipment
    * Added Insecticide.
    * Reworked Relic of Termination: "Every 30 seconds, gain a buff which marks the next spawning enemy. If defeated in time, they drop an item; if not, they drop nothing and delay the next mark. Marked enemies have significantly increased stats. (Rarity of the dropped item increases with stacks)." Additionally, it has clearer mark VFX.
    * Reworked Needles: "Non-critical strikes add +1% critical chance against the struck enemy. Additional critical chance is cleared after 1 (+1 per stack) critical strikes against them." 
    * Reworked Coffee Bag: "Using interactables increases movement speed by 21% and attack speed by 22.5% for up to 15 (+15 per stack) seconds."
    * Reworked Pressurized Cannister: "Grants an upward boost upon activating, and grants a temporary additional jump until landing."
    * Adjusted Detritive Trematode - The debuff no longer expires, but the low health threshold was lowered to 25% for consistency with other "low health" items.
    * Adjusted Droid Head - Spawned drones are now more aggressive and should be less likely to spawn clipping into the ground.
    * Adjusted MIDAS - When given to an equipment drone, the drone now splits the gold it would recieve amongst all players.
    * Adjusted Greater Warbanner - No longer directly gives CDR, but instead makes cooldowns tick faster.
    * Greater Warbanner's buff now has a visual on the player, like Warbanner.
    * Fixed Coffee Bag behaving strangely in multiplayer.
    * Blacklisted Coffee Bag from enemies.
    * Fixed Relic of Force giving primary skills cooldowns.
    * Relic of Force's pickup animator now works.
    * Should have fixed Greater Warbanner's cooldown breaking.
    * Potentially fixed X4 buff-related networking issues.
    * Relic of Termination now has a blacklist for enemies that are not fit for being marked (specific bosses, destructible props, etc), and can no longer mark Cognation ghosts.
    * Relic of Force now properly sets its damage type.
    * Reduced the brightness of Jet Boot's visual effect.
    * Item displays have proper ditheirng.
    * Item icons are properly sized when scrapping.
* Artifacts
    * Cognation's icon now better matches with vanilla artifact icons.
    * Cognation's unlock code now works, and the artifact properly spawns ghosts. Cognation ghosts now have the "Cognate" prefix.
    * Metamorphosis no longer makes the game unplayable. 
    * Nemesis Commando's fight now works correctly with Swarms.
* Events
    * Added new Fog event.
    * Storm VFX has been greatly improved.
    * Invading Nemesis Commando has been rebalanced.
    * Mending Orbs no longer gain elite effects during elite events.
    * Married Lemurians no longer have their elite type overriden during elite events.

### 0.3.50
* Items
    * Fixed Hunter's Sigil adding 20000% damage and not 20%. Additionally fixed its description still mentioning crit chance and not damage.

### 0.3.49
* General
    * Fixed Spanish translation not appearing.
* Survivors
    * Nemesis Commando's sword now actually does damage again.
