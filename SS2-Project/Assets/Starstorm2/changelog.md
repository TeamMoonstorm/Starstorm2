  **Warning: content spoilers below!**
### 0.6.8
* General
    * Added a new "rulebook" section to the character select screen, inspired by ruleset settings of Starstorm 1 and Risk of Rain Returns. The intent is to allow players to customize runs in major ways that are less significant than artifacts, without needing to manually edit a config.
* Items / Equipment
    * Added: Broken Blood Tester*, Diary*, Guarding Amulet, Universal Charger, Ice Tool, Cryptic Source, Man-o'-War, Strange Can*, Rainbow Root, Composite Injector, Erratic Gadget*, Simple Magnet, White Flag.
        * Items marked with an asterisk were from a previous version of the mod, but have since been reworked. Details of each item can be found on the mod's wiki.
    * Reworked / Rebalanced: Coffee Bag, Detritive Trematode, Needles, Field Accelerator, Hunter's Sigil, Prototype Jet Boots, Low Quality Speakers, Watch Metronome.
        * For the sake of brevity, new item effects can be found on the wiki rather than this changelog.
* Interactabes
    * Adjusted spawning weight of drones.
* Enemies
    * Rewrote Follower AI; no longer should get stuck hugging eachother.
    * Hopefully fixed Follower corpse desync on clients for real this time.
    * Slightly increasd health of Follower and Rushrum.
* Events
    * Storms reworked fully: rather than randomly occuring, storms now gradully build up throughout a stage, are permanent, and introduce new gameplay effects at higher levels.
    * Empyrean event removed - Empyrean elites now are integrated into late-loop director spawns.
    * Nemesis Invasions ... ??????????????????
* Executioner
    * Reduced base max secondary stocks from 10 to 5. Ideally, this should make the move feel a bit better to use, creating less of a 'hoarding' mindset and functioning better with extra secondary stocks.
* Nemesis Commando
    * Utility no longer goes as far/fast, but now starts with 2 stocks. This should help create a finer sense of control while weaving in and out of combat.
    * Alt Special now stuns on hit.
    * When selected as a survivor, now drops from a drop pod.
* Nemesis Mercenary
    * When selected as a survivor, now drops from a drop pod.

### 0.6.7
* Items
    * Fork adjusted (+2 flat damage => +8% damage)
    * Molten Coin now properly scales with proc coefficient.
    * Reduced volume of Molten Coin procs.
* Enemies
    * Wayfarer animations adjusted to face the player more often.
    * Wayfarer no longer fails to disappear after dying.
* Events
    * Empyrean event selects tougher enemies more often.
    * Empyrean events and storms can now occur simultaneously
    * Empyrean boss now gains adaptive armor and teleports when falling out of bounds.
    * Added 50% movespeed, 50% cooldown reduction, and 4x health to Empyreans.
    * Increased hurtbox size of nemesis invaders.
* Chirr
    * Friends now gain a flower turret that automatically fires at enemies for 100% damage per second.
    * Corrected Soothing Venom's tooltip (450% damage -> 750% damage).
    * Soothing Venom no longer uses a custom debuff for its slow effect.
    * Abduct's indicator is more accurate and no longer undershoots with taller enemies.
    * Enemies thrown by abduct can now collide with other enemies.
    * Enemies no longer lose their own items when befriended.
    * Enemies are now forced to half health when befriended.
    * Fixed friend's Celestine aura not updating its team.
    * Fixed friend's health regen being capped at 0, which was drastically reducing Soothing Venom's heal.
    * Fixed friend's cooldown reduction not properly scaling with attackspeed.
    * Fixed missing logbook entry.
* Other
    * Increased visibility of Mysterious Cuffs

### 0.6.6
* Events
    * Storms and Empyrean Manifestations now require the Starstorm expansion to be enabled.
    * Really fixed Nemesis Invasions... 😅
* Artifacts
    * Artifact of Havoc now requires the Starstorm expansion to be enabled to be selected.
* Rushrum
    * Further increased visibility of glowing tip of head.
* Other
    * Fixed a networking issue with Follower, Wayfarer, and Shock Drone's attacks.
    * Networked Mysterious Cuff interactable spawning.

### 0.6.5
* Interactables
    * Drone Table no longer counts as a Shrine.
    * Mysterious Cuff now drops a pickup rather than depositing the Condemned Bond directly in the player's inventory.
* Monsters
    * Disabled the white Christmas pom-pom at the end of the Rushrum's head.
* Events
    * Fixed events spawning infrequently, if at all.

### 0.6.4
* Interactables
    * Fixed Mysterious Cuff spawning in unintended locations.

### 0.6.3
* Monsters
    * Disabled something! 😉

### 0.6.2
* New Content
    * Interactable: Mysterious Cuff.
    * Item: Condemned Bond.
* Follower
    * Fixed a bug causing Followers to inherit Duplicator Drone sound and visual effects.
    * Adjusted AI to prioritize fighting the player in close range.
    * Hopefully addressed buggs pertaining to performance issues & corpse cleanup.
* Wayfarer
    * Adjusted projectiles of primary attack to be more interesting to play against.
    * Secondary now creates a small explosion around Wayfarer while creating the buff ward.
    * Adjusted AI to better utilize tweaked skills.
    * Hopefully addressed buggs pertaining to performance issues & corpse cleanup.
* Events
    * All Nemesis Invasions now only occur if a player acquires the Condemned Bond during a run. Looping is no longer a requirement.
    * Nemesis Mercenary Invasion now only occurs if Mercenary is unlocked.
    * Refined selection of enemies for Empyrean Manifestation.
* Other
    * Added proper strings for Artifact of Havoc unlockable name / description.
    * A guaranteed Mysterious Cuff can be found in the Void Fields.

### 0.6.1
* Chirr
    * Reduced hurtbox size by about 33%.
    * Friends now give you the money and experience you would have gotten by killing them.
    * Updated position in Character Select.
* General
    * Added missing information to the README.

### 0.6.0
* New Content
    * Survivor: Chirr.
    * Event: Empyrean Invasion.
    * Elite: Empyrean.
    * Artifact: Artifact of Havoc.
* General
    * Added ScrollableLobbyUI as a dependency.
* Nemesis Mercenary
    * New Mastery Skin: Rogue


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
    * Partially implemented Portugese translation.
    * Captain GM now has dithering.
* Items
    * Needles' item display now has dithering.
    * Haunted Lamp no longer drops when disabled.
    * Haunted Lamp actually fires if the follower doesn't exist.
* Executioner
    * Fixed Execution softlocking the player when failing to land after falling OoB.
* Nemesis Mercenary
    * Improved blacklist for Proliferate's clone.
* Drones
    * Shock and Duplicator drones now drop purchasable interactables when destroyed.
 
### 0.5.4
* General
    * Updated Portugese translation.
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
