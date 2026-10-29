**Warning: content spoilers below!**
### 0.6.47
* General
    * Fixed incompat bug with R2API.Director and SS2 that caused players to spawn and fall down the pit in Soulbound
* Items
    * Fixed NRE spam with Primal Birthright's animator

### 0.6.46
* Survivors
    * All SS2 Survivors now have KeyItemComponent and should be able to use N'Tormant
    * Nemesis Commando no longer throws his gun if just sword special is equipped
* Items
    * Tagged items with canBeRental to allow them to properly be obtained in trials.
    * Fixed Armed Backpack targeting yourself while using self damage items such as Helfire Tincture
    * Fixed an issue where Primal Birthright item display spawns in pink but becomes normal after stage transition or character respawn
    * Simple Magnet can now be disabled 
* Drones
    * Duplicator Drone no longer duplicates ethereal items and pages

### 0.6.45
* Enemies
	* Fixed various material issues with Security Chests. They should now appear identical to regular chests.
* Items
	* New effect for Broken Blood Tester
		* OLD: Every 30 seconds, rapidly heal for 15 (+15 per stack) health for every 25 gold you currently have. Scales with time.
		* NEW: After gaining $25, rapidly heal for 25 plus an additional 8% (+8% per stack) of maximum health.
	* Coffee Bag pickups now work with Collector's Compulsion.
	* Molten Coin now deals TOTAL damage instead of base damage.
	* New effect for Needles
		* OLD: +4% chance on hit to guarantee critical strikes on an enemy for the next 3 (+3 per stack) hits.
		* NEW: Gain 5% (+5% per stack) critical chance. Critical strikes have a 20% chance to needle an enemy for 3x10% (+10% per stack) TOTAL damage.
	* Adjusted Prototype Jet Boots
		* OLD: Gain +1 extra jump that ignites enemies for 500% base damage in a 7.5m (+5m per stack) radius. Recharges 5 seconds after landing. 
		* NEW: Gain +1 extra jump that ignites enemies for 400% (+200% per stack) base damage in a 7.5m (+2.5m per stack) radius. Recharges after 10 seconds.
		* Increased jump height to match Hopoo Feather's bonus jump.
	* Lowered Man O' War damage from 200% -> 150%
* Survivors
	* New Artificer Utility skill - Seismic Spear
	* New Chirr skin - "Isopod"
	* New Commando skin - "Specialist"
        * Huge thank you to FORCED_REASSEMBLY and Redacted for their work on the model!
* Beta
	* Pedestal will now display its item's name when pinged.
	* Added Snowy and Sandy pedestal variants.
	* Added Archer Bugs to the Larva family event.

### 0.6.42
* 0.6.41 was accidentally a debug build.... Same changes applied instead this is a production build now!

### 0.6.41
* General
    * Welcome longtime community member Kina to the SS2 dev team! Everyone falls for the pipeline eventually...
    * Coganition artifact ghost mat fix
    * Fixes shock drones being too big in trishops (sorry we left this bug in the mod for so long!)
* Items
    * Coffee Bag pick-ups now trigger Collector's Compulsion
    * Primal Birthright rework!
    	* OLD: Grants 1 (+1 per stack) Legendary Chest(s) on each stage, but you cannot leave without claiming your birthright. 
		* NEW: Grants 1 (+1 per stack) Legendary Chest(s) on each stage, but you cannot leave without claiming your birthright and must claim it within 200 seconds (+125s per stack) of entering the stage or face punishment. 
    * Fixed a bug with Drifter being able to throw Primal Birthright chest off the map
    * Birthright chests should now properly display the name of the player with the item
* Survivors
    * Executioner luminous shot fix (real)
    * Fixes bug that allowed Chirr to pickup and rearrange Solus Wing's weakspots
    * Fixes material bug with Merc Vestige Skin Sword
    * Fixes Engineer's Laser Focus failing to set LaserEnds and breaking VFX
* Beta
    * Makes ethereal sapling a single interact to activate and makes it not NRE on teleporter 
    * Added new content: Toy Soldiers (red item) (visuals not properly networked, toy commandos wont appear green)
    * Fixes Scavenger's Fortune NREs when given to enemies
    * Makes sure the special elite gets the new equipment via updated SetEquipment method


### 0.6.40
* General
    * Fixed a bug that caused the game to not load during the week of Chilean Independence Day (September 18th). Sorry about that! 

### 0.6.39 Happy Pride!
* General
    * NOTE: The mod will still say 0.6.38 when loading because I built the mod before updating this in Unity. Its a small technical detail that doesnt matter to most people except those filing bug reports
    * White Flag will now display a random Pride flag every run during the month of June. We got to this late! There is a config to enable this year round. (Thanks bread)
* Nemesis Commando
    * Re-implemented unique set of animations for when gun isn't in use (Thanks Domi!)
    * Updated animations for Decisive Strike, Submission
    * Fixed missing VFX for Decisive Strike
    * Fixed Gouge keyword using incorrect string
* Beta
	* Ethereals now add a flat number of levels to monsters
	* Ethereal difficulties now properly show on run end/run history screens
	* Fixed Drizzle being selected by default with Beta content enabled
    * Slate mines interactables fixed (Thanks Lordryn)
    * Fixes to Empyrean VFX overlay, HP bar being tiny, and having wrong shield amount (Thanks Lordryn)
    * If Buns (me) didnt forget to set the correct Slate Mines water material that should be fixed too

### 0.6.38 
* Beta
    * Got rid of MULE's hold to jump higher mechanic to fix MULE not working in release builds. If this doesnt work I give up I need to go work on the MULE rework anyways 

### 0.6.37 buns updated the readme to inflate download numbers
* General
    * Updated the minimum version dependencies in the manifest
    * Updated the README cross-compat section to add more mods
    * Added a "more great mods" section in the README to highlight community efforts
* Beta
    * MULE for real this time

### 0.6.36 buns wheres the changelogs
* General
  * Now includes all changes from the 0.6.34 version that 0.6.35 left out
  * Its very nice people read these. You are a lovely person. Have you been drinking enough water?
	* Fixed an issue with the camera getting yanked when looking too far up or down with many "aiming" skills, such as Executioner's secondary.
* Beta
	* Adjusted Pyro's VFX.
	* Lowered the damage of Archer Bug's projectiles.

### 0.6.35
* Due to a difficult to reproduce bug in 0.6.34 (on dev machines) that prevents SS2 from loading when Beta is enabled. 0.6.35 reverts back to version 0.6.33. Once we figure out the issue 0.6.36 will contain the fixes of 0.6.34

### 0.6.34 - the bread & caves & cliffs part 1 update
* General 
    * Added configs for storms/elite events in moon2/any final stage, and simulacrum. (Thanks bread!)
* Items
    * Red Cross hired assassins to come after us and successfully eliminated all SS2 programmers. bread had to stop their rampage by listening to their demands. Blood tester no longer has a red cross (It's green now). Starstorm 2 no longer follows breaks the Geneva convetion. Except for our large stockpile of chemical weapons hidden in New Jersey. (Thanks bread!)
* Beta
    * Remove #if DEBUG check from MULE so he should be in beta config now! (Thanks bread!)
    * You can now leave voidshop, sorry! (Thanks bread!)
    * Ethereal elites should now properly receive death reports (Thanks bread!)
    * Fixed ethereal sapling ephemeral coins incompat (Thanks bread!)
    * Ethereal saplings no longer set difficulty scaling to 0.5 after using them on modded gamemodes/eclipse (Thanks bread!)

### 0.6.33
* Items
	* Added new sounds and visual effects for Armed Backpack, and increased its proc chance.
	* New effect for Watch Metronome
		* OLD: "While not sprinting, charge up to a 100% increase to sprint speed. Decays over 5s (+3s per stack) seconds while sprinting."
		* NEW: "While not sprinting, charge up to a 150% increase to sprint speed that decays over 3s (+2 per stack). While active, your skills can be used while sprinting."
	* Re-enabled Simple Magnet.
	* Low Quality Speakers now knocks enemies back slightly.
* Beta
	* Pyro reworked
	* Knight adjusted
		- Removed passive
		- Adjusted knockback and timing of various skills.
		- Rally reworked. Allies within the radius of Rally now "smite" enemies with their attacks, healing themselves and reducing the enemy's armor.
	* Added Ambushes. Ambushes are invisible areas in the stage that spawn a wave of monsters when approached. Typhoon exclusive (for now?).
	* Added Pedestals. Pedestals are uncommon interactables that allow you to swap an item from your inventory with the item on the Pedestal.
	* Archer Bug buffed. Increased projectile speed and damage, reduced spread.
	* Blast Knuckles nerfed (500% damage -> 400% damage).
	* Fixed Pedestal network errors, and fixed an issue where only Pedestals would spawn with Artifact of Command enabled.

### 0.6.32
This update is largely thanks to long time SS2 community members bread and Lordryn, both of whom had some SS2 fixes mods on Thunderstore. While the SS2 team is still shackled away in prison, they both agreed to port some of their fixes to SS2 codebase. Thanks you two!
* General
    * Bug where item icons were too large when scrapped is now fixed.
    * Updated Korean translation thanks to Dice-001!
    * Adds linux deployment to the wwise project for the gorillion Linux users (Thanks bread!)
    * Stops storms from happening on planetarium, commencement and simulacrum (Thanks bread!)
    * Fixes bread on the readme (Thanks bread!)
    * Orbeez fixed a few bugs but he never documented them so idfk what he fixed, but they had to do with Chirr, and Executioner! 
* Skins
    * Fixes old skin particles on commandos vestige skin and mult gm skin (Thanks bread!)
* Items
    * Fixes stirring soul icon being null (Thanks bread!)
    * Removed temp tags from relic of mass and force (fThanks bread!)
    * Galvanic Core NRE checks 
    * Removed broken achievements from X4 stimulant, insecticide, and field accelerator (Thanks Lordryn!)
* Beta
    * MULE and DU-T has been added to the beta config, but be warned he is in a very early state. Play with caution!
        * NOTE: Seriously, they are very very early. They're being added as a treat for players to enjoy, but could be removed from the config at any point. MULE specifically has a full kit rework planned. DU-T will see less kit changes.
    * Ethereal saplings now auto charge after hitting once,  and show the next difficulty above them.  Hit them again to cancel (Thanks bread!)
    * Some new lang tokens for the ethereal saplings (Thanks bread!)
    * Fixes various lunar gambler bugs (Thanks bread!)
    * Portal in void shop is replaced with current portal (Thanks Lordryn!)
    * Replaced the "hehe oortal" message with existing lang token for the appearance message (Thanks Lordryn!)
    * Fixed an error related to ultra enemies' AI being modified (Thanks Lordryn!)
    * Fixed knight CSS idle, knight normal idle, and executioner normal idle animations all not looping (Thanks Lordryn!)
    * Fixed pink textures in slate mines (Thanks Lordryn!)
    * Gave all shard pickups the "world unique" tag so they can be picked up with substandard duplicator (and it makes sense for them to be considered world unique too) (Thanks Lordryn!)
    * Re-enabled the storm shard and made it only appear when the beta is enabled (Thanks Lordryn!)
        * The original method that dropped storm shards from the teleporter is commented out so I haven't re-implemented them dropping, but the item itself is back + is restricted to beta config (Thanks Lordryn!)
    * Made all shards require the SS2 expansion (Thanks Lordryn!)
    * Replaced Knight's skill descriptions with proper ones that have damage numbers (Thanks Lordryn!)

### 0.6.31
* General
    * Mul-T GM skins are fixed too! All Vanilla skins are back!
	* Temporarily disabled wavy/shaky text on certain logbooks and item descriptions due to memory leaks
* Executioner
    * Fear's execution threshold on the healthbar should now reflect the correct value (fixed in 0.6.30 but not mentioned)
	* Ion Manipulators now counts as a skill activation when firing each charge, rather than skill press.
* Chirr
	* Friends no longer keep Chirr's items when unfriended.
	* Fixed Symbiotic Link not dealing damage
* Beta
    * Fixed a missing tag in Pyro's passive description
    * Nerfed Pyro's armor add as a stopgap measure for an infinite armor add bug
    * Pyro does have infinite flight though! Its a bug and will be fixed! Enjoy!
    * To combat infinite flight while a more comprehensive fix is found, Pyro's utility has increased heat consumption from 5->8 and inceased heat cost  5->8
	* Fixed missing VFX for Archer Bug

### 0.6.30
* General
    * SS2 0.6.30 requires MSU 2.5.1 or higher. Update both mods!
    * Vanilla GM skins are now fixed! Woohoo thanks Nebby! (Except Mul-T)
* Items
    * Added additional null checks to Galvanic Core to catch a bug where attacker inventories was null
    * Added additional null checks to Primal Birthright to catch a bug where we didnt add required components to the stages teleporter variant
    * Primal Birthright should now work on Conduit Canyon
	* Low Quality Speakers now also destroys projectiles
* Enemies
    * Mimic should no longer spawn if playing with SS2 expansion disabled
* Beta (enabled via config)
    * Additional null checks on Poision Elites (AffixPurple), Night Time (Bandit alt primary), Dormant Fungus (item), and Poisonous Gland (item) to catch reported issues
    * New survivor: Pyro!
    * New enemy: Archer Bug! (different enough from Enemies Returns Archer Bug version dont worry)

### 0.6.29
* General
    * Added a new compatability section to the README
    * Fix NRE from Santa Hat event
    * Updated French translation thanks to DarSitam!
* Mimic
    * Mimic will no longer steal items with the AIBlacklist, BrotherBlacklist, or CannotSteal tags
* Executioner
    * Fixed Fear threshold not working. Sorry this took so long!
* Engineer
    * Fixed VFX issues with Laser Focus primary
* Wayfarer
    * Fixed the Tricorn not working on the Wayfarer due to 0 drop weight
* Beta (Content from the 0.7 Beta, enabled via config)
    * Nerfed Poison Elites across the board (dot duration, radius, damage coefficients)
    * 2 new skills added to beta config. Details below
* Bandit (Beta config only)
    * New primary skill: Night Time
    * Adapted from Bandit's SS1 Variant: Poacher
* Railgunner (Beta config only)
    * New utility skill: Tactical Roll
    * A new utility to encourage a quick scoping playstyle

### 0.6.28
* General
    * Updated French translation thanks to DarSitam!
    * Updated Chinese translation thanks to EternitySwarmCrowm!
    * Chirr's Empyrean unlock bug is now fixed
* Items
    * Re-enabled Galvanic Core thanks to a fix from Chinchi!
* Executioner
    * Increased the range of Exe's Taser alt primary from 28m -> 36m so its easier for him to hit flying targets
* Drones
    * Fixed bug where Droid Head's Security drones would drop a purchasable duplicator drone when destroyed
    * Droid Head's Security drones now have a proper icon


### 0.6.27
* General
    * Lots of various fixes/cleanups to our IL Hooks that should improve stability and catch some uncommon errors. Thanks to Chinchi for this work!
    * Deleted plead.mp4
    * Updated French translation thanks to DarSitam!
    * Updated Chinese translation thanks to EternitySwarmCrowm!
    * Additional NRE checks on our CustomEliteDirector to fix a NRE with Simulacrum
* Items
    * Additional NRE checks to X4Achievement that should resolve some bugs and crashes folks got.
    * Disabled Galvanic Core due to boss healing bug. Will be re-enabled once fixed, but this bug was too run ending to leave enabled any longer.
* Monsters
    * Additional nullchecks for Wayfarer death state and Follower beam, and their associated buffs for some rare race conditions folks were experiencing. 
* Drones
    * Made Shock drone use the electric damage type so its buffed by faulty conductor
    * SS2 Drones now should now function with drone combiner
* Operator
    * Add Shock, Security, and Duplicator drone compat for Operator. Currently they only have basic ram skills. We will give both our drones unique skills in a future update. Thanks to Chicken for this addition!

### 0.6.26
* General
    * Updated mod for Alloyed Collective
    * Disabled Composite Injector and Drone Table due to similar content being added as part of AC. Will be reworked in a future update.
	* Fixed X4 and Insecticide not appearing in game
    * Fixed an incompat with SS2 Beta Super Elites and Enemies Returns. Thanks to Lordryn for the fix!
	* Various fixes for certain projectiles and effects not appearing on clients
    * Added an updated French translation thanks to DarSitam
    * Damglador requested to remove the Ukrainian translation as they wanted it to be in their own mod
* Items
    * Double the cost of the chest Lunar Birthright grants.
	* Galvanic Core's stun chance now scales with proc coefficient.
	* Field Accelerator no longer prevents a certain portal from spawning
* Executioner
	* Lowered max stock of Consecration from 10 to 5.
	* Added Ancient Scepter upgrade for Execution
	* Luminous Shot charges are now only granted when firing Ion Bullets.
    * Fixes and skin specific VFX to Mastery skin. Contribution by bread!
* Interactables
	* Security Chest no longer appears with the Starstorm 2 expansion disabled.
	
### 0.6.25
* General
	* Removed more unfinished content

### 0.6.24
* General
	* Disabled some unfinished skills
	* Fixed incompat with Mortician
* Engineer
	* Updated skill icons for Laser Focus and Rapid Displacement

### 0.6.23
* General
	* Added a cap to the amount of Starstorm 2's sound effects that can play at once. Should improve performance for items like Insecticide and Erratic Gadget.
	* Add some graceful error handling to our SS2Monster base class.
	* Moved some of the debug only logs to production builds so your bug report logs contain more useful errors for us when debugging!
	* Added config options for disabling enemies and survivors (why would you do that).
	* Fixed Condemned Bond pickup not appearing for clients.
	* New PT-BR translation for items thanks to Balorfist
	* Updated Chinese translation thanks to EternitySwarmCrowm
	* New config option to lower the intensity of certain flashing effects.
		* DISCLAIMER: This is not a universal solution for flashing. Currently, only Executioner and Stormborn monsters are affected
* Items
	* (NEW) Galvanic Core - 10% (+10% per stack) chance on hit to stun. Stunned enemies apply razed to enemies within 15m (+5m per stack), reducing their maximum health by 20%, damage by 30%, and movement speed by 80%.
	* Coffee Bag adjusted. 
		* OLD: 8% chance on hit to drop a coffee bean that grants 8% attack speed and movement speed for 5 (+5 per stack) seconds. 
		* NEW: 10% chance on hit to drop a coffee bean that grants 8% attack speed and movement speed for 5 seconds. Maximum cap of 40% (+40% per stack).
		* Increased pickup radius and lifetime of coffee beans.
	* Buffed Molten Coin burn damage (100% => 200%). Gold amount now scales with difficulty instead of stages cleared.
	* Fixed bug with Greater Warbanner's VFX overlay not displaying
* Executioner
	* New model, animations, and VFX.
	* New Alternate Secondary: Consecration - Consume Ion Charges to heal, gain movement speed, and reduce your cooldowns.
	* Ion Manipulators 
		* No longer gains Ion Charges over time. Instead, Service Pistol is fired at 0 Ion Charges.
		* No longer shocks and pierces enemies. 
		* Added new crosshair
	* Increased Crowd Dispersion cooldown (6 seconds -> 7 seconds).
	* Execution
		* Hitting a single target now restores 3 ion charges.
		* Execution now extends the duration of Crowd Dispersion.
		* Execution can no longer be held to aim.
* Chirr
	* New textures.
* Enemies
	* Reworked Follower and Wayfarer
	* Added SFX for Rushrum.
	* Increased hurtbox sizes for Follower and Rushrum.
* ... and so much more!

### 0.6.22
* Survivors
    * Fear fix provided by Moffein, thanks Moffein!
* Items
    * Some items were missing disable configs, added them in. Might have missed some still. Please report!

### 0.6.21
* General
    * Got rid of the Unlock All config, just use RealerCheatUnlocks or any force unlock mod
    * Recompiled the mod for Memop update
    * NOTE: Grandmastery Skins for Vanilla survivors are currently broken due to changes to the vanilla game from Memop that broke how we load content for vanilla survivors. Will be fixed in a later update
* Items
    * Configs now work again! You can freely disable items and equipment once more
* Drones
    * Lower the director credit cost of Shock and Dupe drones.
        * Shock is down 15 -> 12
        * Dupe is down 40 -> 32
* Enemies
    * A wide range of buffs to Wayfarer's health, damage, speed, and projectile attack
    * Improvements to Wayfarer AI to make them more lethal
    * Followers will now miss the first shot if youre off screen like other flying enemies
    * Increased Rushroom hurtbox and a tiny health buff with it to compensate

### 0.6.20
* General
    * New RU translation thanks to DSOverlord
* Survivors
    * Attempt has been made to fix Executioners secondary not working with backup mag
* Items
    * Fixed hook on Jet Boots
    * Relic of Extinction has been added as a tierless item you can test with debug toolkit! 
    * Items reimported to hopefully fix the logbook issues
    * Primal Birthright networked objective fixes / fixed issues with clients not seeing the chets

### 0.6.19
* Survivors
    * Executioner's fear execute is still broken for now. We are working on a more comphrensive fix alongside Jet Boots
* Items
    * Fixed a bug caused by the new lunar which broke void fields
    * The legendary chest spawned by the new lunar is now blue!!!
    * It also adds an objective to the sidebar

### 0.6.18
* General
    * Recompiled the mod so it works with latest SOTS Update. This should fix issues with NemMerc Teleport and Executioner's fear
    * Increased the spawn weight of Dupe Drones + Drone Refabricator, and increased how many stages they can spawn on
* Items
    * Added a new Lunar item from long time SS2 community member Yarrow/Resident Providence! The item will tie in nicely with reworked storms coming in a later update :) Enjoy!
    * Added a log for Skateboard also from Yarrow

### 0.6.17
* General
    * Fixed Nemesis Mercenary's ModdedDamageType not being registered.
### 0.6.16
* General
    * Merry Chirristmas!
    * Minions no longer inherit Field Accelerator
### 0.6.15
* General
    * Fixed for new patch
    * Implemented DamageSource for all characters (Luminous Shot and Breaching Fin)
### 0.6.14
* General
    * Re-add lobbyskinsfix and scrollable lobby ui as dependencies
    * Fixed the bug caused by the new bugfix patch that added a new parameter to a method used with Executioner 
### 0.6.13
* General
    * Recompiled the mod (again) to try and fix why some game objects (wards, projectiles) were failing.
### 0.6.12
* General
    * Fixed issue causing wards (Hunter's Sigil, White Flag, etc.) and probably other things to fail.
    * Items / Equipment
    * Fixed an NRE with Erratic Gadget.
### 0.6.11
* General
    * Rebuild for MSU 2.2.1 compat.
### 0.6.10
* Items / Equipment
	* (NEW) Uranium Horseshoe - Increases movement speed and jump height by 10% (+10% per stack)
	* Composite Injector no longer throws errors with some other UI mods
	* Universal Charger's glow no longer appears for players without the item
	* Universal Charger's sound effect is no longer global
	* White Flag now properly appears for clients in multiplayer
	* Fixed several errors thrown by Erratic Gadget
* Enemies
	* Empyrean healthbar and spawn animation now properly appears for clients in multiplayer
	* Empyrean elite whitelist is now functional
	* Fixed missing spawn sound
* Events
	* Removed Post-Processing fog from the "fallback" storm that would get picked for modded stages
* Chirr
	* Fixed Abduct crash for real this time
* Nemesis Commando
	* Fixed softlock when using Decisive Strike for the first time
* Duplicator Drone
	* Cloned items now properly appear for clients in multiplayer
* Other
	* Fixed enable/disable configs
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
