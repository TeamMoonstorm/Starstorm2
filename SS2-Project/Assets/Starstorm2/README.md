
![Starstorm 2](https://github.com/swuff-star/Starstorm2/blob/main/SS2-Project/Assets/Starstorm2/ReadmeAssets/ss2logopurple.gif?raw=true)

**Starstorm 2** is a work-in-progress adaptation of the Risk of Rain 1 mod "Starstorm". It currently features three new survivors, three new monsters, over 20 new items and equipment, and numerous new challenges to shake up your runs.

![New Survivors...](https://github.com/swuff-star/Starstorm2/blob/main/SS2-Project/Assets/Starstorm2/ReadmeAssets/ss2survivorpromo.gif?raw=true)

* **Executioner**  - An aggressive, versatile survivor with an arsenal made for picking off targets and chaining kills. His powerful Ion Manipulators gain charges with every kill.

* **???** - Survivors of unknown origin, familiar yet unfamiliar. Under the right conditions, you may meet them on the Planet, but they certainly won't be on your side.

![New Items...](https://github.com/swuff-star/Starstorm2/blob/main/SS2-Project/Assets/Starstorm2/ReadmeAssets/ss2itemspromo.gif?raw=true)

* **New Items & Equipment** - A plethora of new items and equipment to support new and existing playstyles. Watch your six with an Armed Backpack, rush through teleporter events with a Field Accelerator, all while keeping on the move with a Swift Skateboard - and more!

* **New Interactables** - New drones, paired with the Refabricator, come in handy to assist you while you fight for survival.

![New Challenges...](https://github.com/swuff-star/Starstorm2/blob/main/SS2-Project/Assets/Starstorm2/ReadmeAssets/ss2challengepromo.gif?raw=true)

* **Events** - Between severe weather, magical buildups, and mysterious invaders, learn to expect the unexpected as forces both natural and supernatural interfere with your runs.

* **New Monsters** - New species of the Planet have come out to put a stop to your invasion of their home.

* **Typhoon** - The Planet grows restless, with fearsome events and monsters coming in greater numbers than ever before. Unlock new skins for survivors* by proving yourself in this new challenge exceeding even Monsoon!
	 *currently includes Commando, Huntress, MUL-T, Acrid, Captain, Executioner, and Commando's Vestige.
	 
* **Artifact of Cognation** - A new, unlockable artifact to shape up your runs.

![Credits](https://github.com/swuff-star/Starstorm2/blob/main/SS2-Project/Assets/Starstorm2/ReadmeAssets/ss2credits.gif?raw=true)

Starstorm 2 is currently being developed and maintained by...
* Buns, Nebby, orbeezeater22, swuff★, Zenithrium

With previous and one-off code contributions by...
* Anreol, bread, Flan, Gaforb, GrooveSalad, HIFU, KevinFromHPCustomerService, MagicGonads, malfaisant, Moffein, Nebby, Noop, Phreel, prod, rob, swuff★, sebacame, Shared, TheTimesweeper, Vale-X, xpcybic, Xubas

With Art/Modelling/Animation contributions by...
* Alexstarwras, Anreol, Bolty, bread, bruh, Cexeub, dotflare, Draymarc, Domi, JestAnotherAnimator, Gem, goodguy, GrooveSalad, JaceDaDorito, LucidInceptor, Neik, KevinFromHPCustomerService, PapaZach, Plexus, prodzpod, QuietAnon, rekkadraws, redacted, rob, SkeletorChampion, SOM, Spenny, swuff★, xpcybic, Reithierion, UnknownGlaze, Zenithrium

Additional thanks to... 
* Writing - Blimblam, Lyrical Endymion, rekkadraws, swuff★, T_Dimensional, Zenithrium
* Translations - Anreol, BannyDee (Spanish), MysticSword (Russian), Damglador (Ukrainian), 乃茶, Etundit, Dying_Twilight (Chinese), Kauzok (Portuguese), StyleMyk (French)
* Sound - KevinFromHPCustomerService, Neik, SOM, UnknownGlaze, NAIRB zepol
* Special thanks - Altzeus, DestroyedClone, Dee', don, Gnome, JesusPxP, KomradeSpectre, MinimalEffort, Moshinate, MysticSword, Riskka, Ruxbieno, SalvadorBunny, SlipSkip, Twiner, VioletChaolan, valerie ♥ 

## Feedback, Bug Reporting & Known Issues

Bugs can be reported at our [GitHub page](https://github.com/TeamMoonstorm/Starstorm2/issues). A list of known & previously reported issues can also be found here. Please include an [Output Log](https://h3vr-modding.github.io/wiki/installing/troubleshooting/log_file.html) and detailed steps to help recreate your error! Feedback about the modded characters pertaining to balance and gameplay can be discussed in our [Discord server](https://discord.com/invite/SgFxwKT7nY).

* Known Issues
    * Elite events are currently disabled.

## Changelog
**Warning: content spoilers below!**
### 0.5.7
* General
    * Added French translation. Thank you StyleMyk!
    * Survivors and monsters are better organized in the config. 
    * Fixed sweeping disables for survivors, monsters, and interactables.
    * Fixed the footstep visuals for all survivors and footstep sound effects for Executioner and Nemesis Commando.
* Nemesis Commando
    * Fixed issue with timing on primary swing. 
* Interactables
    * Refabricator can no longer spawn in Simulacrum.
* Drones
    * Restored Duplicator Drone's cooldown visuals.

### 0.5.6
* General
    * Finally finished implementing Portuguse translation.
    * Survivors and monsters can now be disabled.
    * Unfinished equipments are now entirely disabled.
    * Typhoon has been reverted to +25% spawn rate and -20% enemy rewards.
* Items
    * Hopefully fixed a Relic of Termination nullref with Refightilization.
* Nemeis Mercenary
    * Fixed an bug with Proliferate and sources of damage without an attacker.
* Monsters
    * All monsters are now properly added to the ExpansionDef. 
    * Rushroom's trail VFX now stops when it dies.
* Interactables
    * Interactables are now properly added to the ExpansionDef.

### 0.5.5
* General
    * Partially implemented Portuguse translation. 
    * Captain GM now has dithering.
* Items
    * Needles's item display now has dithering.
    * Haunted Lamp no longer drops when disabled.
    * Haunted Lamp actually fires if the follower doesn't exist.
* Executioner   
    * Fixed Execution softlocking the player when failing to land when falling out of bounds.
* Nemesis Mercenary
    * Improved blacklist for Proliferate's clone.
* Drones
    * Shock and Duplicator drones now drop purchasable interactables when destroyed.

### 0.5.4
* General
    * Updated Portuguese translation. 
* Items
    * Needles has new VFX.
    * Haunted Lamp is now corrupted by Newly Hatched Zoea.
    * Haunted Lamp now fires its projectile from the follower itself, if it exists.
    * Relic of Termination's drop table is now properly instantiated.
* Executioner
    * Slightly nerfed Crowd Dispersion: (Cooldown: 5 -> 6, Debuff Duration: 4 -> 3, Reduced movement speed)
    * Removed the range limit on Execution.
    * Actually fixed interaction with Haunted Lamp and Ion Burst.
* Nemesis Commando
    * Reduced the strength of some normals. 
* Drones
    * Fixed a Duplicator Drone nullref.
* Interactables
    * Refabricator no longer has collision.
    * Refabricator should spawn slightly more often.
    * Refabricator's drop table is now properly instantiated.
    * Broken Shock Drone's price is now better positioned. 

### 0.5.3
* General
    * All survivors now have proper dithering
* Items
    * Remuneration is now less memorable
* Executioner
    * Fixed unintended interaction with Haunted Lamp and Ion Burst.
* Nemesis Mercenary
    * Splatter's reload now scales somewhat better with attack speed.
    * Devitalize's slide now scales lower with movement speed.
    * Proliferate can no longer activate equipments or carry items that grant extra lives.
    * Damage from Proliferate's clone is now attributed to the player.
    * Item displays now correctly replace limbs.
    * Added post processing effects to Exploit and Proliferate.
    * Adjusted various tokens and tooltips.
    * Fixed Dispatch teleporting to 0,0,0.
    * Fixed Nemesis Mercenary's collision breaking when using certain dashes together.
* Events
    * Vestiges are now immune to void fog.
* Monsters
    * Added a trail to Rushrum to help visibility.
    * Wayfarer now crouches under low ceilings.

### 0.5.2
* General
    * Disabled some WIP content that slipped through. ;)
    * Ensured all survivors are properly added to the ExpansionDef.
* Executioner
    * Secondary no longer fires charges gained while already firing unless the button is held.
    * Added some missing equipment displays.
* Monsters
    * Actually fixed Follower and Wayfarer sounds playing globally, and adjusted volume.
    * Networked Follower and Wayfarer attacks.
    * Slightly refined Follower and Wayfarer AIs and body stats. Expect more heavy tuning soon.
    * Adjusted Wayfarer's projectile attack to be much more performance friendly.
    * Updated the icon used for the buff provided by Followers and Wayfarer.
    * Followers now only spawn as a summon from the Wayfarer, rather than showing up in post-loop enemy pools.
* Interactables
    * Networked Refabricator visual effects.

### 0.5.1
* General
    * Fixed an incompability with other mods resulting in a crash, notably when loading into FrozenWall with Gunslinger enabled.
* Nemesis Commando
    * Fixed materials displaying incorrectly on logbook screen.
* Monsters
    * Fixed Follower and Wayfarer sounds playing globally.
* Interactables
    * Restored missing material on the Broken Shock Drone interactable.

### 0.5.0
* New Content
    * Monsters: Rushrum, Follower
    * Bosses: Wayfarer, ???
    * Unlockable Skin: Heel (Mercenary)
    * Grandmastery Skins: Clandestine (Huntress), Electrocutioner (Executioner)
    * Interactables: Refabricator, Shock Drone, Duplicator Drone
* Executioner
    * Slightly buffed the damage of Primary and Special.
    * Secondary's charge rate now scales with the attack speed stat.
    * Optimized the performance of the Fear debuff.
    * Fixed various misplaced item displays.
* Nemesis Commando
    * Various visual polishes.
    * Secondary's reload speed now scales with the attack speed stat.
    * Rebalanced boss fight and improved boss AI.
    * Restored boss immunity to being knocked out of bounds and to fall damage.
    * Fixed various misplaced item displays.
* Items & Equipment
    * Fixed X4 Stimulant cooldown scaling with first stack.
    * Adjusted Nkota's Heritage pickup model to address various issues.
    * Slightly tweaked visuals of Stirring Soul.
* Events
    * Temporarily disabled elite events.
    * Rebalanced event costs to hopefully make them rarer during loops.
    * Reworked the Nemesis invasion event so only one boss may spawn per stage.
* Other
    * Updated translations for Nemesis Commando and Executioner.
    * Added PT-BR translations.
    * Survivor ragdolls no longer disappear when they die off screen.
    * Pressurized canister has been disabled while we work on a multiplayer bug.
    * ...and so much more!