![Starstorm 2](https://github.com/swuff-star/Starstorm2/blob/main/SS2-Project/Assets/Starstorm2/ReadmeAssets/ss2logored.gif?raw=true)

Starstorm 2 is a work-in-progress sequel / adaptation of the  Risk of Rain 1 mod "Starstorm". It currently features two new survivors, over 20 new items and equipment, and numerous new challenges to shake up your runs.

![New Survivors...](https://github.com/swuff-star/Starstorm2/blob/main/SS2-Project/Assets/Starstorm2/ReadmeAssets/ss2survivorpromo.gif?raw=true)


* **Executioner**  - An aggressive, versatile survivor with an arsenal made for picking off targets and chaining kills. His powerful Ion Manipulators gain charges with every kill.

* **???** - A survivor of unknown origin, familiar yet unfamiliar. Under the right conditions, you may meet him on the Planet, but he certainly won't be on your side.

## ITEMS

* **New items** - 20 new items and 5 new equipment to support new and existing playstyles. Watch your six with an Armed Backpack, rush through teleporter events with a Field Accelerator, all while keeping on the move with a Swift Skateboard - and more! 

![New Challenges...](https://github.com/swuff-star/Starstorm2/blob/main/SS2-Project/Assets/Starstorm2/ReadmeAssets/ss2challengepromo.gif?raw=true)

* **Events** - Between severe weather, magical buildups, and mysterious invaders, learn to expect the unexpected as forces both natural and supernatural interfere with your runs.

* **Typhoon** - The Planet grows restless, with both more fearsome events and monsters coming in greater numbers than ever before. Unlock new skins for survivors* by proving yourself in this new challenge exceeding even Monsoon!
    *currently includes Commando, MUL-T, Acrid, Captain, and Commando's Vestige.



## Credits
Starstorm 2 is currently being developed and maintained by...
* Nebby, prod, swuff★, Zenithrium

With previous and one-off code contributions by...
* Anreol, bread, Flan, Gaforb, GrooveSalad, HIFU, KevinFromHPCustomerService, malfaisant, Moffein, Nebby, Noop, Phreel, rob, swuff★, sebacame, Shared, TheTimesweeper, Vale-X, xpcybic, Xubas

With Art/Modelling/Animation contributions by...
* Anreol, Bolty, bread, bruh, Cexeub, dotflare, Draymarc, Domi, JestAnotherAnimator, Gem, goodguy, GrooveSalad, LucidInceptor, Neik, KevinFromHPCustomerService, PapaZach, Plexus, prodzpod, QuietAnon, redacted, rob, SkeletorChampion, SOM, Spenny, swuff★, xpcybic, Reithierion, UnknownGlaze, Zenithrium

Additional thanks to... 
* Writing - Blimblam, Lyrical Endymion, swuff★, T_Dimensional, Zenithrium
* Translations - Anreol (Spanish), MysticSword (Russian), Damglador (Ukrainian), Dying_Twilight (Chinese)
* Sound - KevinFromHPCustomerService, Neik, SOM, UnknownGlaze
* Special thanks - Altzeus, DestroyedClone, Dee'd, don, Gnome, JesusPxP, KomradeSpectre, MinimalEffort, Moshinate, MysticSword, Riskka, Ruxbieno, SalvadorBunny, SlipSkip, Twiner, valerie ♥ 

## Feedback, Bug Reporting & Known Issues

Feedback can be best made via our Discord server, found at https://discord.com/invite/SgFxwKT7nY.  

Bugs can be reported at our GitHub page: https://github.com/TeamMoonstorm/Starstorm2/issues. A list of known issues can also be found here. Please include an Output Log and detailed steps to help recreate your error!

The most important issues you may encounter are listed below, to help expect things that may seem off:

* Known Issues
	* Item Displays for the mod's items on survivors are absent. Starstorm 2 survivors may lack item displays.
    * Event effects may not be networked.

## Changelog

**Warning: content spoilers below!**

### 0.3.51
* General
    * Added Chinese translation. 
    * Adjusted Cloaking Headband's Russian translation.
    * Commando now correctly fires phase round projectiles in multiplayer with the Stealth skin equipped.
    * Fixed "Key Not Found" exception being thrown after ally death.
* Items & Equipment
    * Added Equipment disabling.
    * Reworked Relic of Termination: "Every 30 seconds, gain a buff which marks the next spawning enemy. If defeated in time, they drop an item; if not, they drop nothing and delay the next mark. Marked enemies have significantly increased stats. (Rarity of the dropped item increases with stacks)."
    * Reworked Needles: "Non-critical strikes add +1% critical chance against the struck enemy. Additional critical chance is cleared after 1 (+1 per stack) critical strikes against them." 
    * Reworked Coffee Bag: "Using interactables increases movement speed by 21% and attack speed by 22.5% for up to 15 (+15 per stack) seconds."
    * Adjusted Detritive Trematode - The debuff no longer expires, but the low health threshold was lowered to 25% for consistency with other "low health" items.
    * Adjusted MIDAS - When given to an equipment drone, the drone now splits the gold it would recieve amongst all players.
    * Adjusted Greater Warbanner - No longer directly gives CDR, but instead makes cooldowns tick faster.
    * Greater Warbanner's buff now has a visual on the player, like Warbanner.
    * Fixed Coffee Bag behaving strangely in multiplayer.
    * Blacklisted Coffee Bag from enemies.
    * Fixed Relic of Force giving primary skills cooldowns.
    * Should have fixed Greater Warbanner's cooldown breaking.
    * Relic of Termination now has a blacklist for enemies that are not fit for being marked (specific bosses, destructible props, etc), and can no longer mark Cognation ghosts.
    * Added R2API TempVFX dependency.
* Artifacts
    * Cognation's icon now better matches with vanilla artifact icons.
    * Cognation's unlock code now works, and the artifact properly spawns ghosts. Cognation ghosts now have the "Cognate" prefix.
    * Metamorphosis no longer makes the game unplayable. 
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