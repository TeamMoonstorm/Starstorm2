  **Warning: content spoilers below!**
### 0.6.10
* Items / Equipment
	* Composite Injector no longer throws errors with some other UI mods
	* Universal Charger's glow no longer appears for other players
	* Universal Charger's sound effect is no longer global
* Enemies
	* Empyrean elite whitelist is now functional
	* Fixed missing spawn sound
* Nemesis Commando
	* Fixed softlock when using Decisive Strike for the first time
* Other
	* Fixed some config options
### 0.6.9
* Items / Equipment
	* Cloaking Headband duration changed to 12 seconds, from 16
	* Broken Blood Tester heal increased to 15, from 10
	* New visuals for Broken Blood Tester
	* Cryptic Source now explodes automatically after using most dash skills. New config option to disable this behavior.
	* Hunter's Sigil now properly stacks
	* Added extra damage numbers for Hunter's Sigil's bonus damage
	* Coffee Bag now grants the full buff duration to allies
	* Pickups pulled by Simple Magnet, like Coffee Beans and Ammo Packs, now prioritize the player over the magnet.
	* New visuals and extra damage numbers for Green Chocolate
	* Universal Charger's glow now properly disappears when consumed
	* Composite Injector no longer throws errors with some UI mods
	* Composite Injector now properly uses elite overlays when they aren't in the first equipment slot
	* Blacklisted the following items from monsters: Detritive Trematodes, Portable Reactor, and Low Quality Speakers
	* Removed colliders from Haunted Lamp. Again.
* Enemies
	* Increased health of Empyrean monsters
	* Empreans now only drop one item
	* Empyreans are now much more resistant to knockback
	* Fixed Empyrean particle effects stacking on top of eachother
	* Reduced chance for Empyreans to drop elite aspects
	* Fixed Empyrean aspect dropping more often than intended
	* Added spawn animation for Empyrean monsters
	* Empyreans can no longer be Stormborn
* Events
	* Adjusted storm visuals on several stages
	* Added new config option to adjust the visual intensity of storms
	* Config option to disable storms now works properly
	* Storms now only appear with the SS2 expansion enabled.
	* Reworked storms are now only permanent on Typhoon difficulty
	* Stormborn spawn and death effects are less intense on certain stages
	* Stormborn monsters no longer spawn twice as often as intended
	* Stormborn monsters no longer gain bonus health in default Storms
	* Added healthbar icon for Stormborn monsters
	* Slightly nerfed bonus movement speed and attack speed for Stormborn monsters
	* Storm buffs no longer persist between runs
	* Nemesis invaders now spawn targeting the player holding the Condemned Bond
	* Networked indicator ring and boss healthbar for Nemesis invaders
* Artifacts
	* Artifact of Cognation no longer spawns enemies infinitely
* Chirr
	* Fixed Abduct crashes
	* Fixed some debuffs not being cleansed from friends
	* Added config option to change whether Abduct can grab friendly drones. Off by default.
* Executioner
	* Restored VFX for mastery skins
* Nemesis Commando
	* Implemented updated VFX for alt skins
	* Added missing sound effect for Blade of Cessation
	* Fixed missing materials after using Decisive Strike
* Nemesis Mercenary
	* Fixed model jittering at higher FPS
	* Fixed rare Proliferate crash
* Duplicator Drone
	* Can now only clone a dropped item once
	* Cannot clone an item that was spawned by a Duplicator Drone
* Other
	* Added config options for Typhoon
	* Reimplemented config options to disable content
	* Fixed Language files
	* Drones no longer create two broken drones when destroyed
	* Fixed skins being unselectable
	* Fixed many achievements being unobtainable, or being obtained incorrectly.
	* Fixed some startup errors
### 0.6.8
* Items / Equipment
    * Various visual improvements to existing items.
    * Added: Broken Blood Tester*, Diary*, Guarding Amulet, Universal Charger, Cryptic Source, Man-o'-War, Strange Can*, Composite Injector, Erratic Gadget*, Simple Magnet, White Flag.
        * Items marked with an asterisk were from a previous version of the mod, but have since been reworked. Details of each item can be found on the mod's wiki.
    * Reworked / Rebalanced: Coffee Bag, Detritive Trematode, Needles, Field Accelerator, Hunter's Sigil, Prototype Jet Boots, Low Quality Speakers, Watch Metronome.
     * <details>
       <summary>Full list of changes</summary>

       * (NEW) Broken Blood Tester - Every 30 seconds, rapidly heal for 10 (+10 per stack) health for every 25 gold you currently have. Scales with time.
       * (NEW) Empty Diary - On each stage, gain 3 levels and consume this item.
       * (NEW) Guarding Amulet - Reduce damage taken from behind by 30%.
       * (NEW) Universal Charger - Every 18 seconds (-10% per stack), instantly refresh the cooldown of your next skill.
       * (NEW) Cryptic Source - Upon ending your sprint, unleash a burst of energy in a 15m (+3m per stack) radius for 250% (+250% per stack) base damage.
       * (NEW) Man-o'-War - On killing an enemy, create an electric discharge for 200% base damage on up to 3 (+2 per stack) targets within 20m (+4m per stack).
       * (NEW) Strange Can - 10% (5% per stack) chance on hit to intoxicate an enemy dealing 50% damage per seoond. Intoxicate stacks up to 10 times. Killing enemies heals for 10 (+5 per stack) health per stack of intoxicate.
       * (NEW) Composite Injector - Gain +1 (+1 per stack) equipment slots. Trigger all of your equipment simultaneously.
       * (NEW) Erratic Gadget - Your lightning strikes twice. 10% chance on hit to fire chain lightning for 300% TOTAL damage on up to 1(+1 per stack) targets.
       * (NEW) Simple Magnet - Pull surrounding pickups in a 150m radius towards you.
       * (NEW) White Flag - Place a white flag that disables skill usage within 25m. Lasts 15 seconds.
       * (REWORKED) Coffee Bag - 8% chance on hit to drop a coffee bean that grants 8% attack speed and movement speed for 5 (+5 per stack) seconds.
       * (REWORKED) Needles - 4% chance on hit to guarantee critical strikes on an enemy for the next 3 (+3 per stack) hits.
       * (REWORKED) Field Accelerator - After defeating the boss, the radius of the Teleporter is increased by 20% (+20% per stack) and killing enemies grants +1% teleporter charge.
       * (REWORKED) Hunter's Sigil - After standing still for at least 1 second, create an 8m zone granting 30 armor (+15 armor per stack) and 30% increased damage (+15% per stack). The zone lasts until you leave it.
       * (REWORKED) Prototype Jet Boots - Gain +1 extra jump that ignites enemies for 500% base damage in a 7.5m (+5m per stack) radius. Recharges 5 seconds after landing.
       * (REWORKED) Low Quality Speakers - Chance when getting hit to stun enemies in a 13m (+7m per stack) radius. Chance starts at 10% and increases with low health.
       * (ADJUSTED) Detritive Trematode - Health threshold now increases per stack, and the first tick of damage applies instantly. New visuals and sounds
       * (ADJUSTED) Watch Metronome - Maximum sprint boost is now capped at 80%. Stacks of the item instead increase the duration.
       * (ADJUSTED) Green Chocolate - Buff no longer stacks. New visuals.
       * (ADJUSTED) Portable Reactor - Additionally grants +100% movespeed for the duration. New visuals and sounds.
       * (ADJUSTED) Swift Skateboard - Buff lasts longer, but no longer activates from your primary skill. New visuals.
       * (ADJUSTED) Cloaking Headband - Invisibility lasts longer.
       * (MISC) Malice - New visuals.
       * (MISC) Molten Coin - New sounds.
       * (MISC) Fork - Bonus damage is now displayed as an extra damage number.
       * (MISC) Insecticide - New visuals and sounds.
       </details>

* Interactabes
    * Adjusted spawning weight of drones.
* Enemies
    * Rewrote Follower AI; no longer should get stuck hugging eachother.
    * Hopefully fixed Follower corpse desync on clients for real this time.
    * Slightly increasd health of Follower and Rushrum.
* Events
    * Storms have updated visuals, and a new surprise!
    * An early version of the 0.7 Storm rework is available as a config option (disabled by default).
    * Empyrean event removed - Empyrean elites now are integrated into late-loop director spawns.
    * Empyreans now select their Elite traits from a whitelist, which can be found in the config options.
    * Empyrean monsters no longer have Adaptive Armor
    * Empyrean monsters now ignore the level cap of 99.
    * Nemesis Invaders now have Adaptive Armor, like Mithrix.
    * Nemesis invaders now ignore the standard level cap of 99.
    * Nemesis invaders now spawn at a minimum level of 24, and always have at least ~25 items. Don't fight them if you aren't prepared!
    * Nemesis invaders spawn at the beginning of the stage, every three stages
* Executioner
    * Reduced base max secondary stocks from 10 to 5. Ideally, this should make the move feel a bit better to use, creating less of a 'hoarding' mindset and functioning better with extra secondary stocks.
* Nemesis Commando
    * Utility no longer goes as far/fast, but now starts with 2 stocks. This should help create a finer sense of control while weaving in and out of combat.
    * Alt Special now stuns on hit.
* Nemesis Mercenary
    * Increased Alt Utility damage (300% -> 600%).
    * Reduced Alt Utility speed.
    * Reduced Special damage (1500% -> 1200%).
* Other
    * Many performance improvements
    * Reduced file size
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
    * Increased visibility of Mysterious Cuffs.
    * Adjusted model / texture of Captain and Huntress Grand Mastery skins.

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

### 0.4.6
* General
    * Fixed a bug with how Nemesis Commando checks to reload after Single Tap's stock count changes.
* Items
    * All items and equipment have better positioning in the logbook.
    * Relic of Termination now grants double time for bosses and drops the boss's relevant boss item, assuming it has one. The mark now properly works on bosses as well.
    * Adjusted Relic of Termination's drop rates.
    * Adjusted how Relic of Termination scales marked enemy health - it has a lower multiplier, but also grants an additional flat amount of health.
    * Relic of Termination's marker now immediately disappears upon killing the marked enemy.
    * Detritive Trematode can no longer be applied to the Artifact Reliquary.
    * Adjusted how Relic of Force adjusts skill cooldowns to fix some mod incompatibilities.
    * X4 and Termination's icons have been cleaned up.
    * Coffee Bag and Needles now have item displays.
    * Disabled Baby's Toys. The item does work, but it has a lot of issues which would take a decent amount of time to address, so it's being disabled for now.

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
    * Thanks TimeSweeper!

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
    * Fixed Hunter's Sigil adding 20,000% damage instead of 20%. Additionally, fixed the description mentioning crit chance over damage.

### 0.3.49
* General
    * Fixed Spanish translation not appearing.
* Survivors
    * Nemesis Commando's sword properly deals damage again.

### 0.3.48
* Emergency update due to improper file strucutre.

### 0.3.47
* General
    * Updated R2API dependencies to proper modules.
    * Added new Grand Mastery skins "Weapon" and "Prestige" for Acrid and Captain respectively - happy holidays!
    * Added new "Stealth" skin for Commando. Currently unlocked by default; planned to be locked by default at a later date.
    * Lowered event frequency significantly and readjusted event weights.
    * Updated Russian translation.
    * Added Ukranian translation.
* Items & Equipment
    * Items can now individually be enabled / disabled via config file. Note that equipment remain unaffected by this change at the moment.
    * Reworked Needles: "Hitting enemies has a 6% chance to prick them, converting all incoming damage to critical hits for 2 seconds (+2s per stack)." Does this feel more balanced? Should the pricked debuff still allow debuffs to critically hit?
    * Reworked Coffee Bag: "Using interactables increases movement speed by 21% (+21% per stack) and attack speed by 22.5% (+22.5% per stack) for 15 seconds."
    * Reworked X-4 Stimulant: "Reduces secondary skill cooldown by 10% (+10% per stack) and increases base health regen by +2 hp/s (+1 hp/s per stack) for 3 seconds on secondary use." Should probably one have one stacking condition - which?
    * Disabled Diary. Item suffers a constant identity crisis without ever feeling impactful. Have been talks of reworks internally, but nothing finalized yet.
    * Hunter's Sigil now adds +20% damage (+15% per stack) rather than +25% crit chance (+20% per stack) while criteria are met.
    * Nkota's Heritage no longer guarantees items of a specific tier after level thresholds.
    * Fixed Portable Reactor's buff icon.
    * Blacklisted Relic of Termination from enemies.
    * Nerfed Relic of Force: The damage dealt by additional hits now starts off at 5% TOTAL damage, and becomes 5% more powerful with consecutive hits on a single enemy. Breaking the chain resets damage.
    * Nerfed Relic of Termination: Reduced duration to kill an enemy from 30 to 20 seconds. Added a 30 second cooldown between enemy marks. Buffs given to unkilled marks are even more powerful. Finally, adjusted drop rates from 60/30/9/1 to 70/25/4.9/0.1.
    * Greater Warbanner no longer automatically despawns after 90 seconds. Fixed a major bug causing massive lag when switching stages related to the equipment.
* Executioner
    * Gained Ion Burst charges are now based on the hull size of a slain enemy, rather than a hard-coded index.
* Nemesis Commando
    * Slightly tweaked Gouge values.
* Events
    * Drastically lowered event frequency, and readjusted event costs and weights.
    * Enemies now recieve 20% CDR during storms.
    * Storms now should force an initial wave of enemies upon starting.
    * Redid event VFX for all weather and elite events.
    * Added Mending Elite event. Requires Survivors of the Void expansion.
    * Event VFX should be networked.
    * Restored Nemesis Commando invasion event.

### 0.3.46
* Emergency update for R2API 5.0.

### 0.3.45β
* Even more fixes.
    * Fixed Relic of Termination's material.
    * Adjusted Relic of Termination's VFX.
    * Reduced the cooldown on Termination attempting to find a new enemy if it couldn't find one (15s -> 5s).
    * Termination should no longer mark the same enemy twice.
    * Fixed Termination's logbook lying about what stats the enemies get, and also adjusted the text coloration to be like that of Shipping Request Form.
    * Fixed Coffee Bag's mid-stage pickup effect, mostly.
    * Capped Swift Skateboard's buff stacks again.
    * Fixed X4's cooldown reduction.
    * Fixed X4 causing NREs with EliteReworks active.
    * Added displays for X4.
    * Fixed Reactor's buff icon being the one for Needles.
    * Hopefully fixed an NRE caused by Nkota's Heritage
    * Disabled Genesis Loop's particle effect for Nemmando. No more going blind!
    * Nemmando now has the dot crosshair - he'll be given a custom crosshair eventually.
    * Fixed Mastery and Grandmastery unlocks.
    * Relic of Force can no longer be activated by Malice.
    * Chirr has been disabled again. They'll be ready soon, hopefully

### 0.3.44β
* More fixes.
    * Adds Relic of Termination, with temp VFX.
    * Reworked Coffee Bag.
    * Added a new icon for Executioner's alt primary by goodguy#3899. Thank you!
    * Droid Head will no longer spawn void infestors.
    * Fixed VFX for Relic of Force.
    * Nkota's Heritage can no longer drop itself.
    * Losing Baby's Toys no longer reduces your level - it now adds the levels gained from it back to your current level. This readding does not proc Nkota's Heritage.
    * On skills with multiple base stocks, X4 will only proc once (+1 per additional stock (ex backup mag)) per recharge interval. This primarily normalizes the healing gained form skills like Nemmando's Single Tap and Mul-T's Power Mode.
    * X4 heals on firing a sniper shot on Railgunner, like backup mag, rather than healing you on scoping in for their default secondary. No change to the alt secondary.
    * Added all DLC0 item displays for Executioner.
    * Bottled Chaos can no longer activate Back Thruster, MIDAS, and Pressurized Canister.
    * Executioner should no longer rarely become a lantern.
    * Portable Reactor can appear on enemies again (with fourthed duration) because it's hilarious. Also reduces turret / ally invulnerability duration to one fourth.
    * Enemies can no longer recieve Armed Backpack. Additionally, lowered the minimum chance for the item to activate as it was overpreforming.
    * Slightly increased the speed at which Watch Metronome gains charges.
    * Increased the cooldown of MIDAS from 30s to 75s.
    * Swift Skateboard no longer breaks Bandit's backstabs.
    * Swift Skateboard should no longer only buff the host whilst destroying the ears of the clients.
    * Fixed config name for Low Quality Speakers.
    * Fixed the icon for Back Thruster's buff.
    * Fixed up the logbook. Items are now positioned better and have cleaner descriptions.

### 0.3.41-43β
* We do not speak about these updates.

### 0.3.40β
* Continued fixes and adjustments.
    * M.I.D.A.S gold gain is now scaled based on difficulty (and thus, player count). This largely essentially only comes in to play fairly late into runs.
    * Nkota's Heritage shouldn't cause NREs as much. 
    * Cleaned up some X4 and Nkotas debug statements. 
    * Nemmando's animations no longer break after getting frozen while charging his alternate special.
    * Nemmando's alt special no longer lies about scaling the number of hits with attack speed. It and the scepter variant actually scale now. 
    * Nemmando no longer gains additional stacks of certain items. Additionally, more of his item displays are functional.
    * Relic of Force now only procs on hits with a proc coefficent and the additional hits have half the proc coefficient of the hit that caused them. Still might be a bit strong. 
    * Droid Head base duration increased from 15s -> 20s, and gains an additional 5s lifetime per stack. Additionally, they should hopefully fire at enemies a bit more consistently. 
    * Updated logbook descriptions for Armed Backpack, Fork, and Jet Boots to be more clear and accurate. Made X4's logbook entry actually readable, and cleaned up text coloring for Diary.
    * AIs can no longer receive Portable Reactor, as it was hilariously unfair.
    * Jet Boots no longer deal self damage with Artifact of Chaos enabled. 

### 0.3.39β
* Minor Fixes/Adjustments.
    * X4 no longer allows enemies to heal on using their secondary skills when they don't have it.
    * Slightly reduced the height of Executioner's special. It's a bit more reasonable now.
    * Buffed Armed Backpack's damage from 100% to 400%. It's an on damage proccing item that only hits one enemy, so it having a large payoff is justified.
    * Actually properly adds Nemmando's Scepter skills. Executioner will come later!
    * Prevents Blizzards from completely blinding you. The snow is lighter now.

### 0.3.38β
* Updated to Survivors of the Void & MSU 1.0.1. 
    * This update has notable balance changes! Please delete your configs. 
	* Was a long time coming, with of course more to come.
    * The dev team has nearly entirely changed hands. Yay!

* General:
	* Fixed bugs caused by various interactions between new items / survivors. (ex. Molten coin works with ignition tank.)
	* Disabled items that were replaced by SOTV items, those being Coffee Bag, Dormant Fungus, and Erratic Gadget. These items may get reworks some time down the line. 
	* Adjusted some descriptions for items and abilities to be more clear / accurate. 
    * Added configs for more items that previously lacked them.

* Survivors:
    * <b>Executioner: </b>
    * Adjusted base stats to be more in line with normal survivors. 

	* <b>Nemesis Commando: </b>
    * Adjusted base stats to be more in line with a melee survivors.
	* Reduced damage for Submission, his default special, from 48x60% to 24x70%.
	* Added a new keyword - Rend. Pistol skills have this keyword, which makes them deal more damage to enemies with gouge applied.
	* Readded Ancient Scepter skills.

* Items 
	* <b>Tier 1: </b>
	* Armed Backpack - reworked. Chance to fire a missile on taking damage. 
	* Detritive Trematode - Low health threshold normalized from 40% -> 25%, and increased DoT damage to compensate 
	* Diary - reworked. Gain bonus EXP upon killing a boss enemy. 
	* Fork - Adjusted to be +2 base damage. Ends up being largely the same for most survivors, with only Mul-T getting a slight buff and Acrid getting a slight nerf. Additionally makes the item way less scary when enemies get it.
	* Malice - Only procs from survivor attacks. Damage reduced from 55% -> 35% total damage. Maintains damage through the chain, rather than decaying per jump.
	* Needles - Required procs for full crits reduced from 5 -> 2. 
	* Added X4 Stimulant. (Does this item feel meaningful? Too strong? Feedback is especially welcome)
	* <b>Tier 2: </b>
	* Blood Tester and Hottest Sauce removed. Will likely get reworks.
	* Jet Boots - Replaced stunning in the description with description. It never stunned. 
	* Low Quality Speaker - has a model and new icon.
	* Watch Metronome - Now builds stacks while not moving and not sprinting at the same rate. Slightly reduced maximum build rate to compensate.
	* <b>Tier 3: </b>
	* Baby's Toys - Now actually reduces level by 3, keeping the stats.
	* Nkota's Heritage - Stacking now gives multiple items per level up rather than increasing drooped item rarity. 
	* Portable Reactor - has a model and icon. 
	* Swift Skateboard - Now only plays a sound effect if you don't already have the buff, to prevent sound effect spamming.
	* <b>Lunar: </b>
	* Added Relic of Force. 
	
### 0.3.37β
* Events
	* Fixed Nemesis Commando spawning early into runs, for really real. Thank you Sebacame.

### 0.3.36β
* Events
	* Fixed Nemesis Commando spawning early into runs, for real. Sorry about that.

### 0.3.35β
* Events
	* Fixed Nemesis Commando spawning early into runs (I hope)

### 0.3.34β
(*Hello again. Sorry for the delay - we are a lot smaller than we once were. There are still plans for this mod. Stay tuned. P.S. Please forgive me for the poor changelog. -★*)
* General
	* Updated the mod to work with Moonstorm Shared Utils 0.8.0.
	* A decent amount of networking fixes - I'm not really sure what specifically. Report any non-networked behaviors in Discord, please.
* Events
	* Greatly increased the rarity of storms. Thoughts on this?
* Items
	* Added new items: Low Quality Speakers (Uncommon), Swift Skateboard (Rare), Portable Reactor (Rare). Be sure to read the logs for these before giving feedback!
	* Baby Toys: Fixed incorrect stat calculations)

### 0.3.33β
* Fixed the mod shipping with debug features enabled.

### 0.3.32β
(*This will hopefully be the last Nightly release before we push an update to the main mod. There might be some undocumented changes. I'm very tired.*)
* Executioner
    * Added new passive: Bloodbath - Killing enemies inflicted with fear reduces all skill cooldowns by 1 second
    * Slightly reworded some skill descriptions and added keywords to hopefully make things clearer and easier to digest
    * Fear now provides a 30% movement speed reduction to enemies when applied
    * Doubled turning radius to be in-line with Vanilla survivors
    * Half reimplemented ragoll
    * KNOWN ISSUE: Executioner's ragdoll hangs itself via psychic force
* Nemesis Commando
    * Fixed most animations, exception being everything related to Single Tap. I don't want to touch it anymore, so expect a full fix to be alongside the remodel in 0.4.0
    * Added new Sprint animation
    * Doubled turning radius to be in-line with Vanilla survivors
* Items
    * All: Rewrote many descriptions to match Vanilla wording and phrasing better
    * A Lot: Modified internally to use RecalculateStatsAPI, which means better stat stacking and less bugs.
    * Armed Backpack: Added
    * Back Thruster: Slight rework - now functions like Metroid's Speed Booster when activated, accelerating the user when running in a straight line (*Would this feel better if you could 'run over' enemies for damage? Like a Bighorn Bison?*)
    * Baby Toys: Added, looking for feedback on current implementation
    * Broken Blood Tester: Now uses flat values instead of whatever nonsense was going on before
    * Dormant Fungus: Reduced healing to not heal for 100% of health on proc
    * Greater Warbanner: Multiple buff wards can now be present at a time
    * Greater Warbanner: Buff wards now have a lifetime of 90 seconds before disappearing
    * Hunter's Sigil: Stacks of the item now affect received buffs properly
    * Malice: Reintroduced. Now uses an orb that you can actually watch bounce between enemies to deal damage. TO-DO: Make it purple
    * KNOWN ISSUE: Jetboots pickup prefab only applies interactability overlay to a single boot
    * KNOWN ISSUE: Baby Toys lack model, sprites and formatted tokens
* Artifacts
    * Added code for Cognation, alongside a proper trial.
* Interactables
    * Added the mod's first Interactable: the Escape Pod. These escape pods can be opened up for free, and will open up to reveal items, monsters, or less fortunate survivors. Heavy feedback on them is much appreciated (Are spawn rates too high? chances for goodies too low?)
    * KNOWN ISSUE: Drop Pods dont have proper tokens
* Events
    * Nemesis Invasion: Nemesis survivors are now given inventories when spawning
    * KNOWN ISSUE: Event rates might be messed up still|
* Other
    * Miscellaneous: We've began to add disabled content to a lot of things that should not be included on the next nightly release. this includes Slate Mines
    * Miscellaneous: Added a Starstorm2Content Class, functionality is basically the same as RoR2's RoR2Content class.
    * Unlockables: Rewritten which means all unlocks have been reset, they should work properly now.
    * Unlockables: Added missing unlocks (EG: Single Tap)
    * Language Files: Reformatted internally, let us know if issues arise as a result of this
    * Bugfix: Void cells should now always drop a reward
    * KNOWN ISSUE: MUL-T's Grand Mastery lacks emissions and tokens.

### 0.3.31β
(*I don't think this is appropriate for a full on dev log, but right now current focus is on fixing up content to reach version parity with Stable + push ThunderKit build to there. After that, we'll be looking at future content, including Cyborg and Ethereal Teleporters. For now, feedback is CRUCIAL - this is intended as a 'final test' for a lot of features, and specific things still need to be fixed that are skimmed over just to get this out ASAP. Things generally should be working more than before, but some things are still messed up. Be as critical of anything in the mod that seems off as you can.*)
* General
    * Title screen is now a bit less purple and a bit more Risk of Rain 2
    * KNOWN ISSUE: It's a long standing one, but it's on my to-fix list: MUL-T Grandmastery uses incorrect material.
    * KNOWN ISSUE: Unlocks are broken.
    * KNOWN ISSUE: Hottest Sauce, Jet Boots, and Stirring Soul have missing or broken visual effects.
    * KNOWN ISSUE: When Canticum Vitae ends it doesnt resume the original stage's song.
* Events
    * Significantly lowered intensity of Storm visuals
    * Enemies now receive a unique buff icon while storms are active to help signify that storms buff them
    * Slightly tweaked storm spawn rates, should now be less common. (*How do storm spawn rates feel? Too common? Too rare? Please test on multiple difficulties, and respect that the game is designed to be played on Rainstorm - though feedback for all is crucial.*)
    * Removed regular storms from Sky Meadow in favor of 100% Purple Rain
    * Nemesis Invasion: Removed from Commencement spawn pool (*This was a weird fix that may have broken things. Let me know if you see more than one Nemmando during your run. Also, the Void Event is being dropped entirely until we get a better look at the DLC. Until then, I may try to merge the old visuals into this if there's interest.*)
* Executioner
    * Fear: Updated buff icon
    * Crowd Dispersion: Lowered speed multiplier from 3.0 to 2.0, as it was pre-ThunderKit - this should make it more controllable
    * Supercharged: Updated buff icon
* Nemesis Commando
    * Fixed materials of all skins to match pre-ThunderKit
    * Gouge: Now refreshes duration of existing stacks when applied, again
    * Distant Gash: Now properly applies gouge
    * Added IDRS for all the Vanilla Items & Equipments.
    * (This just leaves his animator as an outlying issue.)*
* Items
(Developer Note: *You should probably delete your old config.*)
    * Detritive Trematode: Changed stacking behavior to directly increase damage rather than increase debuff duration
    * Detritive Trematode: Buffed movement speed reduction (10% -> 20%)
    * Dormant Fungus: Fixed scaling being 1/100th as good as it should've been. Oops
    * Dormant Fungus: Increased duration between procs from 0.5 seconds to 0.75 seconds 
    * Dormant Fungus: Rebalance healing amounts (2% + 0.5% per stack -> 1% + 1% per stack) (*This was done as per suggestion of Moffein, as most Common items tend to scale linearally like this. This item is currently a candidate for a slight rework with the announcement of Weeping Fungus. Stay tuned.*)
    * Fork: Changed behavior to Starstorm 1's. The days of overpowered fork are no more! (+7% TOTAL damage -> +25% BASE damage, without levelups *This is probably a sad change but I think it's for the best.*)
    * Green Chocolate: Updated buff icon
    * Greater Warbanner: (*Would people prefer this item if you could place more than one at a time, but they disappeared after a duration? Or is it fine it is?*) 
    * Hunter's Sigil: Updated buff icon
    * Malice: Temporarily disabled (*Previous version of this item was breaking the game. Will have finished for final release.*)
    * Needles: Updated long description for clarity on how the item works post-rework
    * Needles: Changed stacking behaviors to increase duration of the 'pricked' debuff preceeding the 'needles' debuff
    * Watch Metronome: Updated buff icon
    * KNOWN ISSUE: I don't think Pressurized Cannister or Back Thruster are networked. 
* Artifacts
    * Added Artifact of Cognation.

### 0.3.30β
* Items
    * Updated config to make almost every numerical value for Items / Equipment configurable. Let there be config!
    * Coffee Bag: Additional attack speed is now configurable
    * Coffee Bag: Additional movement speed is now configurable
    * Cloaking Headband: Duration of the Invisiblity buff is now configurable
    * Detritive Trematode: Corrected pickup text / description text to match changes made in last patch
    * Detritive Trematode: Health theshold is now configurable
    * Detritive Trematode: Trematode debuff length is now configurable
    * Dormant Fungus: Corrected pickup text / description text to match changes made in last patch
    * Dormant Fungus: Base amount of healing is now configurable
    * Dormant Fungus: Stacking amount of healing can now be configured
    * Droid Head: No longer adds a movement speed bonus.
    * Droid Head: Stacking damage is now configurable
    * Droid Head: Lifetime can now be configured
    * Erratic Gadget: Flat crit chance can now be configured
    * Erratic Gadget: Additional crit damage is now configurable
    * Field Accelerator: Charge per kill is now configurable
    * Field Accelerator: Maximum stacking charge per kill is now configurable
    * Field Accelerator: Now uses inverse hyperbolic scaling, opposed to linear with a hardcoded capacity
    * Greater Warbanner: Extra health regen is now configurable
    * Greater Warbanner: Extra crit chance is now configurable
    * Greater Warbanner: Cooldown reduction is now configurable
    * Hottest Sauce: Radius is now configurable
    * Fork: Additional damage is now configurable
    * Hottest Sauce: Duration of the Burn debuff is now configurable
    * Hunter's Sigil: Added armor is now configurable
    * Hunter's Sigil: Added armor per stack is now configurable
    * Hunter's Sigil: Extra crit is now configurable
    * Hunter's Sigil: Extra crit per stack is now configurable
    * Hunter's Sigil: The duration of the buff lingering after beginning to move again is now configurable
    * Hunter's Sigil: The additional duration to the above per stack is now configurable
    * Molten Coin: Proc chance is now configurable
    * Needles: Corrected pickup text / description text to match changes made in last patch
    * Needles: Proc chance is now configurable
    * Needles: The duration of the buildup debuff is now configurable
    * Needles: The amount of needed buildup debuffs for the Needles debuff to apply is now configurable
    * Needles: Duration of the Needles debuff can now be configured
    * Prototype Jet Boots: Base radius of the explosion can now be configured
    * Prototype Jet Boots: Stacking radius of the Prototype Jet Boots can now be configured
    * Prototype Jet Boots: Max radius of the explosion can now be configured
    * Prototype Jet Boots: Base damage of the explosion can now be configured
    * Prototype Jet Boots: Stacking damage of the explosion an now be configured
    * Watch Metronome: Base capacity for buffs is now configurable
    * Watch Metronone: Additional capacity is now configurable
    * Watch Metronome: Maximum movespeed bonus is now configurable
    * Hates will say this is an artifically inflated changelog.
* Stages:
    * Stages can now be enabled / disabled via config file
    * Slate Mines: Added Solus Probes to the enemy pool

### 0.3.29β
* General
    * Updated config to allow players to enable / disable specific items
* Nemesis Commando
    * Fixed unlock condition
    * Submission is now fully agile
    * Distant Gash properly spawns a projectile
* Maps
    * Added a beta version of Slate Mines as part of the Stage 2 pool - heavy feedback on this is appreciated, and should be directed towards JestAnotherAnimator (Lui) in feedback threads. This is a VERY EARLY PROTOTYPE, and we are currently looking for feedback on stage layout and playability, rather than texture / model work
* Items
    * Fixed a lot of items that were broken due to the deprecation of StaticValues.cs - this includes Detritive Trematode, Molten Coin, Prototype Jet Boots, Droid Head, Relic of Mass, and probably others
    * Erratic Gadget: Damage is now applied to the crit itself, rather than as a second damage instance
    * Detritive Trematode: Now applies a 10% slowness debuff alongside the damage over time
    * Dormant Fungus: Uses old % based behavior rather than the weird level-based scaling
    * Droid Head: Security Drones are temporarily missing displays for EliteVariety and Aetherium elites. Now supporting Lost in Transit, though.
    * Needles: Now uses a building debuff rather than immediately applying. Think of it like a chance-based Shattering Justice
    * Needles: Lowered odds of applying debuff (8% -> 6%)

### 0.3.28β
* General
    * Now dependant on MoonstormSharedUtils
    * General language changes that we didn't keep track of
    * Added Russian localization
    * Added Turkish localization
    * Mod now properly loads on non-English operating systems
* Executioner
    * Networked gaining Ion Burst charges
* Nemesis Commando
    * Fixed Nemesis Commando transforming into a mess of polygons when having an overlay active.
    * Drastically improved Nemesis Commando Boss AI 
    * Nemesis Commando now must manually enter the stage like other survivors, and will only use a void portal to enter the stage on Stage 1
* Events
    * Added Canticum Vitae back to the Nemesis encounter event
    * Added Sandstorm, Blizzard, Ashstorm and Purple Rain events to Abandoned Aqueduct, Rallypoint Delta, Abyssal Depths and Sky Meadow respectively
* Items
    * Back Thruster: Added
    * Nkota's Heritage: No longer drops items on the ground if the item is held by a non-player body (e.g. Mithrix, Engineer Turrets), and puts them straight into their inventory instead

### 0.3.27β
* General
   	* Fixed frequency of events as to not permanently rain later into runs
   	* Fixed event messages showing something along the lines of "<spritename=MARIOSTARBIT>" instead of sprites 
* Executioner
   	* Wastelander skin is now armed with a gun, rather than a Petrichorian Apocalypse Series Collectable Action Figure
* Items
   	* Dormant Fungus: Updated logbook description for accuracy
* Other
   	* Changed naming scheme for beta versions in the readme to now include the manifest's release version, marked with a "β"

### 0.3.26 - 0.3.25
* ???

### 0.3.24
* Hotfix for the Aetherium crosscompat breaking the whole game

### 0.3.23
* General
   	* Added challenge: "MUL-T: Grand Mastery"
   	* I don't know why 0.4.0 released either
* Executioner 
   	* Fixed massive visual effects when using Execution
   	* Fixed gaining permanent supercharge when killing AWU/Direseeker with Execution
   	* Killing Aurelionite and Brother Glass now grants temporary supercharge
   	* Updated description texts to reflect recent changes to Secondary
* Nemesis Commando
   	* Updated various animations
   	* Fixed typo in character description
   	* Sword swing visuals are now blue when using the Commando skin
   	* Single Tap: Reduced damage on hit (320% -> 210%)
   	* Single Tap: Now has six stocks by default (previously one)
   	* Single Tap: Added slick new reload animation. Very fancy.
   	* Decisive Strike (Scepter): Now stuns all affected enemies
   	* Decisive Strike (Scepter): Updated visuals
   	* Gouge debuff is no longer applied if the hit is blocked (e.g. Tougher Times)
   	* Gouge damage decreased by 75% when applied to players (Dev note: Getting hit by Decisive Strike felt like a bit too much of a death sentence)
* Items
   	* Detritive Trematode: Now no longer applies debuff when hitting an enemy with an attack that has a proc coefficient of 0
	   * Hottest Sauce: Updated sound effects
   	* Prototype Jet Boots: Updated sound effects
* Void Event
   	* Nemesis boss theme now loops properly
* ss_test
   	* Cyborg: Added "Cyberstream" skin
   	* Cyborg: Added "Metamorphic" skin (Dev note: these skins will eventually become rewards for Cyborg's Mastery and Grand Mastery challenges, respectively. They've both been left unlocked by default for testing reasons.}

### 0.3.22
* General
   	* Fix Shrines of the Mountain not spawning additional bosses
* Executioner
   	* Added animations for Heresy set of items
* Nemesis Commando
   	* Added animations for Heresy set of items
* Void Event
   	* Fixed new soundtrack not playing for clients in multiplayer

### 0.3.21
* Void Event
   	* Nemesis survivors now have a unique boss theme
   	* Shrine of the Mountain no longer spawns multiple Nemeses, instead it buffs the stats of the one that spawns
   	* Nemesis survivors now have a much larger item blacklist
	   * Greatly increased the spawn weight of Nemesis survivors
* ss_test
   	* Cyborg: Fixed missing text strings
   	* Chirr: Fixed missing text strings
* Cursed Config
   	* Now disabled by default

### 0.3.20
* General
   	* Updated mod icon - now in HD!
   	* Added another new config option, the "cursed" config. Behind this config are various comedic skins, shitposts of features, or other things that we felt were worth making the time to make, despite them not fitting in within the core mod. Here be dragons!
* Nemesis Commando
   	* Single Tap and Commando skins are now locked by default
   	* Updated appearance of Commando skin
   	* Added new challenge: Nemesis Nemesis
   	* Decisive Strike is now locked by default
   	* Added new challenge: Zandatsu
* ss_test
   	* Added new survivors: Chirr and Cyborg
   	* Nucleator: Added complete item displays for Vanilla items
* Cursed Config
   	* Added new "Motivator" skin for Nemesis Commando
   	* Old Nemesis Commando "Commando" skin is now behind this config option

### 0.3.19
* Executioner
   	* Fixed softlock when dying on Glass
   	* Added new survivor icon
   	* Improved cloth physics for skirt
   	* Overhauled all visual effects
   	* Updated icon for Fear debuff
   	* Fixed aim animation being off center
   	* Added landing animation
* Nemesis Commando
   	* Lowered recoil on Single Tap
   	* Added new survivor icon
   	* Overhauled sword swing visual effect

### 0.3.18
* General
   	* Fixed various descriptions and name tokens being incorrect
   	* Fixed a typo in the config
   	* Tweaked the volume of various sound effects to prevent future hearing loss lawsuits
   	* Fixed massive item displays... Again
	   * Fixed the whole mod breaking when a survivor was disabled
* Executioner
   	* Landing kills with Execution now gives double the charges of Ion Burst (was previously one extra)
   	* Execution now has a maximum height, to prevent teleporting out of the top of stages
   	* Properly networked bonus secondary charges from Execution
   	* Fixed buggy overshield visuals
   	* Fixed being able to cancel any skill with Crowd Dispersion
   	* Fixed Forgive Me Please not working
* Items
   	* Equipment items exist again
* ss_test
   	* Executioner: Properly networked Deadly Voltage primary
   	* Executioner: Raised proc coefficient of Deadly Voltage (.3 -> .75)
   	* Executioner: Increased range of Deadly Voltage (20 -> 28)
   	* Nucleator: Properly networked all skills
   	* Nucleator: Fixed charge meter vanishing when sprinting or using certain skills
   	* Nucleator: Word-for-word from Rob: "updated vfx i think, not sure"

### 0.3.17
* General
	   * Added "ss_test" config option to enable certain work in progress features. As of now, this enables the Nucleator, a new survivor who can risk his own health for massive damage output, and the "Deadly Voltage" primary skill for Executioner. Note that these features, as well as future features locked behind this config, may be buggy or notably unpolished compared to other content in the mod, and as such, are recommended for experienced users only.
	   * Fixed unlockables
* Executioner
   	* Added a new visual effect when gaining charges of Secondary
   	* Damaging an enemy within five seconds of its death grants a kill assist, adding a charge of secondary, regardless of who landed the finishing blow
   	* Executioner now gains bonus charges of Secondary when defeating tougher foes
   	* Secondary now becomes "supercharged" for ten seconds after defeating the Alloy Worship Unit or Direseeker (added by "Direseeker" mod), and permanently after defeating Mithrix. While supercharged, the ability has unlimited stocks. (Dev note: There was no need for this other than it's cool)
* Nemesis Commando
	   * Added sound effect on character select screen
	   * Updated sound effects for Distant Gash
	   * Single Tap (alternate secondary) no longer interrupts Primary skill
* Items
   	* Most items can now have various numerical values changed via config file
	   * Hunter's Sigil: Updated sound effect
   	* Malice: Properly cleans up VFX, should help with stuttering / lag issues
   	* Pressurized Can: No longer breaks the console if given to an equipment drone
   	* Prototype Jet Boots: Now have a config setting to reduce or disable visual effects

### 0.3.16
* General
	   * Disable Void elites while issues with custom elite spawns are being fixed
   	* Fix character select screen being empty if any SS2 survivors are disabled via config
* Nemesis Commando
   	* Updated sprint animation
   	* Added out-of-combat sprint animation (only applies to Minuano skin for the moment)
   	* Lowered the recoil from Distant Gash
   	* Fixed Distant Gash having inconsistent damage
   	* Doubled the hit count and increased the frequency of hits for Decisive Strike with Ancient Scepter
   	* Tweaked Decisive Strike for the boss version of Nemesis Commando: now has a visual indicator, now always fully charges the attack, stays stationary for the entire attack
* Items
	   * Fix giant item displays on Aetherium items and Ancient Scepter
   	* Greater Warbanner: fix missing buff icon, change indicator material to be distinct from regular warbanners
   	* Hunter's Sigil: fix networking on effects
   	* Malice: fix effects appearing on player instead of enemies
   	* Diary: fix Engineer turrets gaining exp from the item

### 0.3.15
* General
   	* Fix Void elites not spawning correctly, creating "fake elites" with health boost
   	* Fix Void elites spawning on Commencement
* Executioner
   	* Enemies afflicted with Fear now take 50% more damage (was 20%)

### 0.3.14
* General
   	* Compatibility with the Anniversary Update
   	* Text strings are no longer in a separate language file
   	* Adjust SS2 survivor/item sorting order in character select and logbook
* Items
   	* Cloaking Headband: reduce cooldown (90sec -> 45sec)

### 0.3.13
* General
   	* Fix enemy damage not scaling with level during storms
* Items
   	* Coffee Bag: adjusted stats to be in line with Risky 2's balancing (10% -> 7% move speed, 7% -> 7.5% attack speed)
   	* Dormant Fungus: reduce stacking behavior (1.5% per stack -> 2% + 0.5% per stack)
   	* Hottest Sauce: change stacking behavior to increase damage instead of duration, increase initial burst damage (100% -> 150%)
   	* Hunter's Sigil: hide buff effect when near the camera, add new sound for buff
   	* Pressurized Canister: only applies upward force if you hold jump instead of constantly for the whole duration. Config option added to make it behave like prior releases if you, like Rob, thought that was hilarious.

### 0.3.12
* General
   	* Fix a certain unfinished elite type being able to spawn when using some modded artifacts
* Nemesis Commando
   	* Added Ancient Scepter support
* Items
   	* Publicize item/equipment classes so other modded characters can add display rules for SS2 items
   	* Hunter's Sigil: make buff visual smaller
   	* Green Chocolate: add a damage reduction upon taking heavy damage
   	* Green Chocolate: fix 20% max HP threshold not factoring in shield
   	* Green Chocolate: update model/icon
   	* Stirring Soul: increase lifetime on souls before they despawn (10s -> 20s)
   	* Stirring Soul: blacklist item from being given to late joiners if DropinMultiplayer is in use

### 0.3.11
* Executioner
   	* Increased Primary damage (280% -> 300%)
   	* Added new skin inspired by Altzeus and JesusPxP's Wastelander mod
   	* Added new unlock for Wastelander skin
   	* Fix being able to unlock Executioner by destroying rocks on Sky Meadow
   	* Updated skill icons
* Nemesis Commando
   	* Full visual effects pass
   	* Altered visual effects on sword for Grand Mastery skin
   	* Added unique sound effects for Grand Mastery skin
   	* Added unlock condition for Commando skin
   	* Updated Commando skin
   	* Added unique visual & sound effects for Commando skin
   	* Significantly reduced Submission's knockback
   	* Changed scaling for Nemesis Commando as a boss
   	* Blacklisted certain unfair / unfun items from boss Nemesis Commando
   	* Tweaked hitstop to help with mobility issues
   	* Networked sound effects
* Items
   	* Stirring Soul: reduced odds of item drops (7% -> 5%)
   	* Stirring Soul: fix Sky Meadow rocks dropping souls
   	* Stirring Soul: added item displays
   	* Broken Blood Tester: added item displays
   	* Coffee Bag: added item displays
   	* Dormant Fungus: added item displays
   	* Droid Head: added item displays
   	* Fork: added item displays
   	* Hottest Sauce: added item displays
   	* Hunter's Sigil: tweaked sound effects
* Other
   	* Fixed storm ambience volume / looping
   	* Refactored code to allow other mods to add their own Nemesis bosses to void event
	
### 0.3.10
* Executioner
   	* Added "Host Only" to unlock description
	   * Fixed special immediately snapping to ground
	   * Updated visuals while carrying Visions of Heresy
   	* Axe is now invisible while carrying Shattering Justice
* Nemesis Commando
   	* Nerfed Distant Gash damage (200% min - 800% max -> 160% min - 500% max)
   	* Cleaned up visual effects
   	* Gun is now holstered and crosshair is changed when running an all-sword loadout
   	* Fixed compatibility issue with Phase Round Lightning mod
   	* Changed gunshot sound effect
   	* Tactical Roll has a shorter cooldown (6s -> 5s), and cooldown begins immediately, rather than after the skill is performed in full
   	* Fixed some sound effects being played globally
* Items
   	* Molten Coin: added sound effects
   	* Molten Coin: now properly scales with proc coefficient
   	* Diary: added sound effects
   	* Hottest Sauce: added sound effects
   	* Detritive Trematode: debuff length is now based on proc coefficient
   	* Prototype Jet Boots: updated visual and sound effects
   	* Hunter's Sigil: visual effects now properly networked
   	* Stirring Soul: added sound effects
* Void Elites
   	* Fixed Void Elite debuff permanently applying in multiplayer widepeepoHappy
* Other
   	* Added storm ambience during storms

### 0.3.9
* Executioner
   	* Execution's drop can no longer be overridden by Milky Chrysalis
* Nemesis Commando
   	* Add unique visual effect to boss variant
   	* Lower base damage on primary (200% -> 160%)
   	* Tweak Decisive Strike visuals
   	* Increase base damage on Decisive Strike (375% -> 380%)
   	* Fix incorrect damage values on Submission
   	* Change new secondary name to Single Tap
   	* Fix Single Tap not working in multiplayer
   	* Fix core position being too high up
   	* Fix camera angle being locked on spawn
* Items
   	* Hunter's Sigil buff now lingers for 0.75s after you start moving
   	* Add visual efffect and sound for Hunter's Sigil activation
   	* Raised Cloaking Headband's cooldown (45s -> 90s)

### 0.3.8
* Executioner
    * Tweaked sound volume
    * Lower Execution jump height
    * Add crit sound to primary
    * Remove proc coefficient from utility
    * Lower utility fear radius (25m -> 20m)
    * Fix cloth physics on mastery skin
* Nemesis Commando
    * Gouge stacks now refresh duration like bleed
    * Lower base damage on primary (220% -> 200%)
    * Rework Submission to a chargeable shotgun blast
    * Added an alternate secondary, Phase Shot
    * Update overview text to change secondary name
    * Fix keywords on Submission and Decisive Strike
    * Fix excessive hitstop on primary
    * Fix primary not dealing damage with too much attack speed
    * Update sprint animation
    * Added and tweaked sounds for everything
    * Boss variant now sprints
    * Adjusted camera position on spawn

### 0.3.7
* General
	   * Fix no monsters spawning on any run after the first
* Items
   	* Remove incomplete Baby's Toys

### 0.3.6
* General
   	* Fixed custom sounds not working
   	* Registered some skill states that may or may not have been causing networking issues
   	* Made Typhoon's text about increasing enemy cap only display if setting is enabled in config
* Executioner
   	* Added an umbra for the Artifact of Vengeance
   	* Added work in progress rest emote
   	* Added taunt emote (bound to 2 by default)
   	* Tweaked Execution animation
   	* Adjusted cloth physics, no linger clips through body
   	* Execution is now agile and moves faster while ascending
* Nemesis Commando
   	* Added an umbra for the Artifact of Vengeance
   	* Added taunt emote
   	* Added footstep sounds to sprint
   	* Increase base damage on primary (160% -> 220%)
   	* Increase max damage on secondary (60% -> 800%)
   	* Reduce base charge time on secondary (1.5s -> 1.25s)
   	* Increase cooldown on secondary (3s -> 4s)
   	* Secondary cooldown now starts when the skill starts rather than ends
   	* Updated jump animations

### 0.3.5
* General
   	* Fixed config not properly disabling survivors

### 0.3.4
* Executioner
   	* Updated description of Fear for accuracy (enemies take 20% more damage while afflicted)
   	* Primary bullet size slightly increased
   	* Secondary can now be held down to fire more than 10 shots
   	* Using Special doesn't cut velocity as abruptly
   	* Update sound on primary
   	* Fixed sounds not playing for some players
   	* Fixed strange shield overlays
* Nemesis Commando
   	* Retimed sitting emote to not play at hyperspeed
   	* Fixed sounds not playing for some players
   	* Fixed strange shield overlays
   	* Updated description of Submission for accuracy
   	* Updated description of Decisive Strike for accuracy
* Enemies
   	* Add sound for spawning Void Elites

### 0.3.3
* General
   	* Fixed issue with Nemesis Commando automatically unlocking for some players

### 0.3.2
* General
	   * Fix an issue causing an ESO-related error and possibly preventing Void elites from spawning in some cases
* Enemies
   	* Add item blacklisting to Nemesis Commando boss
	   * Nemesis Commando boss is given roughly half as many items as before and is no longer given unique boss damage boost (he is still very dangerous to be near)
* Executioner
   	* Increase base damage on primary (240% -> 280%)
   	* Reduce base fire rate on primary (0.5s -> 0.65s)
   	* Secondary fires a maximum of 10 shots at a time, even if more are stored via Backup Magazines
   	* Increase base/maximum damage on special (1000%/1600% -> 1100%/2200%)
   	* Special grants additional secondary restock on kill instead of reducing cooldowns
   	* Adjust health and damage scaling very slightly
   	* Update animations and VFX
* Nemesis Commando
   	* Secondary projectile speed scales with charge
   	* Update description on Submission (default special) to better reflect what it does
   	* Update animations
   	* New sword sounds
* Items
   	* Re-enable Green Chocolate. Buff lasts longer and can stack.
   	* Add Relic of Mass lore
   	* Dormant Fungus: reduce healing (2% -> 1.5%)
   	* Fork: reduce damage bonus (10% -> 7%)
   	* Diary: adjust scaling to give less exp at low levels and a bit more at higher levels
   	* Prototype Jet Boots: enemies no longer hurt themselves when using the item
   	* Malice: reduce damage scaling on stack (each additional enemy hit takes 55% of the previous enemy's damage)
   	* Droid Head: increase drone damage and movespeed; stacking now scales drone damage instead of duration

### 0.3.1
* Fix missing ESO dependency

### 0.3.0
* *First Thunderstore release*
* Gameplay
   	* Add storms and run-wide event system. Storms are more frequent and have shorter forewarning at higher difficulties.
   	* Add Void elites (tier 2)
   	* Visiting Void Fields at any point during a run makes Void Elites start appearing early and releases Commando's vestige into our reality...
   	* Removed Herobrine
* Executioner
   	* Add unlock condition
   	* Add mastery skin
   	* Add grand mastery skin, obtainable by winning/obliterating on Typhoon difficulty
   	* New model and animations, based more closely on Starstorm's Executioner
   	* Gun glow now reflects number of stored secondary charges
   	* Update item displays to fit new model
   	* Reworked base/maximum damage on special (1200%/2400% -> 1000%/1600%)
   	* Fix special causing self-damage/utility causing self-fear when Artifact of Chaos is active
   	* Add remaining item displays (if any are missing, yell at me -swuff★)
* Nemesis Commando
   	* Add unlock condition
   	* New mastery skin (old one is available by default)
   	* Add grand mastery skin, obtainable by winning/obliterating on Typhoon difficulty
   	* Update sounds and visuals, especially on Decisive Strike
   	* Update description
   	* Add remaining item displays (same as with Exe -★)
* Commando
   	* Add grand mastery skin
* Items
   	* Add Relic of Mass (lunar) and Stirring Soul
   	* Internal item code rewrite to support item displays for our items
   	* Add per-item on/off toggles to config
   	* Diary: scale up exp gain with level
   	* Molten Coin: update icon
   	* Hottest Sauce: deals 100% base damage before applying burn; fix lighting yourself on fire when Artifact of Chaos is active
   	* Nkota's Heritage: adjust rarity scaling to work similarly to RoR1 Starstorm, add chance for red items, prevent Engineer turrets from dropping items
   	* Erratic Gadget: add non-stacking 10% crit chance
   	* Greater Warbanner: only one banner may be placed at a time (previous banner is removed when a new one is placed)

### 0.2.7
* hotfix for nemmando networking for real this time

### 0.2.6
* hotfix for networking
* fix for strides of heresy breaking on nemmando

### 0.2.5
* Nemesis Commando
   	* Fixed networking issues
   	* Lowered Decisive Strike (Special) charge time from 2s to 1.75s and buffed damage per hit from 350% to 375%
   	* Removed auto-aim and Awareness buff on Submission, added proper attack speed sclaing.

### 0.2.4
* Executioner
   	* Primary is slower but more powerful
   	* Special now deals 1200% flat on groups, and 2400% to solo targets (from 1500% to solo target, split between # of targets)
* Nemesis Commando
   	* New animations for pretty much everything
   	* Primary has had damage / attack speed adjustments, hitbox changes, is now a one-two combo like Loader's
	   * Secondary is now a proper sword beam, visually
   	* Secondary now applies Gouge
   	* Adjusted interrupt priorities for abilities to make them flow together better
   	* New alternate special ability, WIP.

### 0.2.3
* Nemesis Commando
   	* Fix sword combo not resetting when not using primary
   	* Fix slightly bad aim on special
   	* Fix gouge applying twice
   	* Adjust skill priorities so other skills can be used while holding primary attack
* Executioner
   	* Add some more item displays
* Items
   	* Diary: increase exp gain rate (3 per 3sec -> 3 per 2sec)
   	* Molten Coin: scale earnings by number of stages cleared ($1 per stage)
   	* Hottest Sauce: increase burn damage (50% -> 100% damage/sec)
   	* Nkota's Heritage: chance of uncommon item scales with level
   	* Pressurized Canister: fix poor thrust force when used with heavy characters e.g. Acrid
   	* Update item descriptions to match item behavior
* Gameplay
   	* Typhoon: increase soft cap on enemy spawns to 80. This is an experimental feature and can be disabled in config.

### 0.2.2
* Executioner
	   * Fix bad aim on secondary
   	* Add some item displays

### 0.2.1
* Executioner
   	* Increase aim snapping on secondary again
   	* Add display rules for equipment that needs them to work
* Nemesis Commando
   	* Primary is now agile (can be used while sprinting)
   	* All hits of primary apply gouge status
   	* Gouge duration resets when reapplied
   	* New secondary: a chargeable sword beam which behaves like Commando's Phase Round
   	* Add more item display rules
* Items
   	* Broken Jet Boots: change vfx to something less visually intrusive
   	* Watch Metronome: increase base decay duration (2 -> 4sec)
   	* Fix some typos in item logbook entries
   	* Broken Blood Tester: disable proc while run timer is paused
   	* Molten Coin: fix missing texture
* Other
   	* Fix known multiplayer incompatibilities (e.g. Executioner's secondary not storing charges)

### 0.2.0
* Items
   	* Add Malice, Fork, Coffee Bag, Watch Metronome, Hunter's Sigil, Green Chocolate, Droid Head, Pressurized Canister, and Cloaking Headband
   	* Disable Nucleator until he is more finished
   	* New icons for items that were missing them
   	* Add logbook entries for items
   	* Fix Molten Coin not working
   	* Hottest Sauce: Increase range, fix missing stacking behavior, fix proc on unusable equipment e.g. fuel array
   	* Change Dormant Fungus healing (10% per 5sec -> 2% per sec)
   	* Fix oddities on some item models
   	* Add debuff icons for Trematode, Strange Can
* Executioner
   	* Utility procs fear for its entire duration
   	* Activating utility causes stun and slight pushback
   	* Slightly decrease fear proc radius on utility
   	* Enemies under fear debuff take 20% more damage from all sources
   	* Tone down particles/VFX on utility
   	* Slightly increase aim snapping while using secondary (easier to land shots)
* Nemesis Commando
   	* Add mastery achievement + skin
   	* Adjust base stats to more closely match Commando's
   	* Fix attacks having 0 proc coefficient
   	* Add skill icons
   	* Add descriptions for survivor and skills
   	* Add logbook entry
   	* Add sounds
   	* Ragdoll, animation tweaks, some item displays, and lots of other quality-of-life details
   	* Fix jogging in place on character select screen

### 0.1.0
* Initial release - includes Executioner, Nemesis Commando, Nucleator (WIP), 11 items, and typhoon difficulty
