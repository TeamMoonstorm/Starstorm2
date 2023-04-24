# Starstorm 2

## Overview

Starstorm 2 is a work-in-progress sequel / adaptation of the legendary Risk of Rain 1 mod "Starstorm". It features two new survivors, over 20 new items and equipment, and new, dangerous events to shake up your runs.

## Features

![](https://i.imgur.com/YFiPuTM.png)

* **Executioner** - An aggressive, versatile survivor with an arsenal made for picking off targets and chaining kills. His powerful ion gun gains a shot for every enemy you take down.

* **???** - A survivor of unknown origin, familiar yet unfamiliar. Under the right conditions, you may meet him on the Planet, but he certainly won't be on your side.

![](https://i.imgur.com/c0GoQOf.png)

* **New items** - 20 new items and 5 new equipment to support new and existing playstyles. Watch your six with an Armed Backpack, rush through teleporter events with a Field Accelerator, all while keeping on the move with a Swift Skateboard - and more! 

![](https://i.imgur.com/vGTL8l9.png)

* **Typhoon** - The Planet grows restless, and hordes grow more quickly on this new difficulty surpassing Monsoon. Unlock new skins for survivors* by proving yourself.
    *currently includes Commando, MUL-T, Acrid, Captain, and survivors added by the mod.

* **Events** - Expect the unexpected as forces both natural and supernatural interfere with your runs.

## Credits
Starstorm 2 is currently being developed and maintained by...
* Nebby, swuff★, Zenithrium

With previous code contributions by...
* Anreol, Flan, Gaforb, GrooveSalad, HIFU, KevinFromHPCustomerService, malfaisant, Moffein, Nebby, Noop, Phreel, rob, swuff★, sebacame, Shared, TheTimesweeper, Vale-X, xpcybic, Xubas

With Art/Modelling/Animation contributions by...
* Anreol, Bolty, bruh, Cexeub, dotflare, Draymarc, Domi, JestAnotherAnimator, Gem, goodguy, GrooveSalad, LucidInceptor, Neik, KevinFromHPCustomerService, PapaZach, Plexus, prodzpod, QuietAnon, redacted, rob, SkeletorChampion, SOM, Spenny, swuff★, xpcybic, Reithierion, UnknownGlaze, Zenithrium

Additional thanks to... 
* Writing - Blimblam, Lyrical Endymion, swuff★, T_Dimensional, Zenithrium
* Translations - Anreol (Spanish), MysticSword (Russian), Damglador (Ukrainian)
* Sound - KevinFromHPCustomerService, Neik, SOM, UnknownGlaze
* Special thanks - Altzeus, DestroyedClone, don, Gnome, JesusPxP, KomradeSpectre, MinimalEffort, Moshinate, MysticSword, Riskka, Ruxbieno, SalvadorBunny, SlipSkip, Twiner, valerie ♥ 

## Feedback, Bug Reporting & Known Issues

Feedback can be best made via our Discord server, found at https://discord.com/invite/SgFxwKT7nY.  

Bugs can be reported at our GitHub page: https://github.com/TeamMoonstorm/Starstorm2/issues. A list of known issues can also be found here. Please include an Output Log and detailed steps to help recreate your error!

The most important issues you may encounter are listed below, to help expect things that may seem off:

* Known Issues
	* Item Displays for the mod's items on survivors are absent. Starstorm 2 survivors may lack item displays.
    * Artifact of Metamorphosis is broken by the mod.
    * Artifact of Cognation's unlock is broken. The artifact is unlocked by default.
    * Event effects may not be networked.

## Changelog

**Warning: content spoilers below!**

### 0.3.51
* General
    * Adjusted Clocking Headband's Russian translation.
* Items & Equipment
    * Added Equipment disabling.
    * Reworked Relic of Termination - now grants a buff that is consumed upon an enemy spawning, and then goes on a 30 second cooldown displayed via a buff stack.
    * Reworked Needles - Now grants a +1% stacking debuff which increases crit chance against that enemy until 1 (+1 per stack) crits happen on that enemy.
    * Adjusted Detritive Trematode - The debuff no longer expires, but the items low health threshold was lowered to 25% for consistency with other "low health" items.
    * Adjusted MIDAS - When given to an equipment drone, the drone now splits the gold it would recieve amongst all players.
    * Fixed Coffee Bag behaving strangely in multiplayer.
    * Blacklisted Coffee Bag from enemies.
    * Fixed Relic of Force giving primary skills cooldowns.
    * Should have fixed Greater Warbanner's cooldown breaking.
    * Relic of Termination now has a blacklist for enemies that are not fit for being marked (bosses, destructible props, etc)
    * Added R2API TempVFX dependency.
* Events
    * Invading Nemesis Commando now does less damage. (10x -> 7x)
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

### 0.3.48
* Emergency update due to improper file structure.

### 0.3.47
* General
    * Updated R2API dependencies to proper modules.
    * Added new Grand Mastery skins "Weapon" and "Prestige" for Acrid and Captain respectively. Happy Holidays.
    * Added new "Stealth" skin for Commando. Currently unlocked by default, but planned to be locked by default at a later date.
    * Lowered event frequency significantly and readjusted event weights.
    * Updated Russian translation.
    * Added Ukranian translation.
* Items & Equipment
    * Items can now individually be enabled and disabled via config file. Note that equipment remain unaffected by this change for the moment.
    * Reworked Needles: "Hitting enemies has a 4% chance to prick them, converting all incoming damage to critical hits for 2 seconds (+2s per stack)." Does this feel more balanced? Should the pricked debuff still allow debuffs to critically hit?
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

* Items:
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