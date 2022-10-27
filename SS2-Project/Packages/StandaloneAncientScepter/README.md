# Standalone Ancient Scepter

[![github issues/request link](https://raw.githubusercontent.com/DestroyedClone/PoseHelper/master/PoseHelper/github_link.webp)](https://github.com/DestroyedClone/PoseHelper/issues) [![discord invite](https://raw.githubusercontent.com/DestroyedClone/PoseHelper/master/PoseHelper/discord_link.webp)](https://discord.gg/DpHu3qXMHK)
[![github issues/request link](https://raw.githubusercontent.com/DestroyedClone/PoseHelper/master/PoseHelper/github_link.webp)](https://github.com/DestroyedClone/AncientScepter/issues) [![discord invite](https://raw.githubusercontent.com/DestroyedClone/PoseHelper/master/PoseHelper/discord_link.webp)](https://discord.gg/DpHu3qXMHK)

- Adds the Ancient Scepter from Risk of Rain 1 as a standalone mod
- Code is almost entirely by ThinkInvisible, this is merely a port of that, without the TILER2 dependency and with a custom model

[![](https://media.discordapp.net/attachments/849798075001864214/893641536443674634/unknown.png?width=1144&height=591)
]()

| ![](https://cdn.discordapp.com/attachments/755273690131988522/814607241532407818/unknown.png) | ![](https://media.discordapp.net/attachments/849798075001864214/889274013493387284/Scepter.png) |
|--|--|

## Upgrades
    - Commando: Suppressive Fire > Death Blossom (2x shots, fire rate, and accuracy. 2x autoaim range, hold primary fire to fire normally)  
	    - -OR-  Grenade > Carpet Bomb (0.5x damage, throw a spread of 8 at once)
    - Huntress: Arrow Rain > Burning Rain (1.5x duration and radius, burns)  
	    - -OR-  Ballista > Rabauld (5 extra weaker projectiles per shot, for 2.5x TOTAL damage)
    - Bandit: Lights Out > Assassinate (2x bullets)
	    - -OR-  Desperado > Renegade (25% (+0.35% per token) chance to ricochet to another enemy on hit up to 8 times within 30m. -10% distance & damage per bounce. Unaffected by luck.) 
    - MUL-T: Transport Mode > Breach Mode (0.5x incoming damage, 2x duration; after stopping, retaliate with a stunning explosion for 100% of unmodified damage taken)
    - Engineer: TR12 Gauss Auto-Turret > TR12-C Gauss Compact (+1 stock, +1 placed turret cap)  
	    - -OR-  TR58 Carbonizer Turret > TR58-C Carbonizer Mini (+2 stock, +2 placed turret cap)
    - Artificer: Flamethrower > Dragon's Breath (hits leave lingering fire clouds)  
	    - -OR-  Ion Surge > Antimatter Surge (2x damage, 4x radius)
    - Mercenary: Eviscerate > Massacre (2x duration, kills refresh duration [hold special to leave early])  
	    - -OR-  Slicing Winds > Gale-Force (4x stock and recharge speed, fires all charges at once)
    - REX: Tangling Growth > Chaotic Growth (2x radius, pulses additional random debuffs)  
	    - -OR-  DIRECTIVE: Harvest > COMMAND: Reap (Spawns extra fruit that gives random buffs)
    - Loader: Charged Gauntlet > Megaton Gauntlet (2x damage and lunge speed, 7x knockback)  
	    - -OR-  Thunder Gauntlet > Thundercrash (3x lightning bolts fired, cone AoE becomes sphere)
    - Acrid: Epidemic > Plague (victims become walking sources of Plague, chains infinitely)
    - Captain: Orbital Probe > 21-Probe Salute (1/3 damage, 7x shots, hold primary to fire continuously)
	    - -OR-  OGM-72 'DIABLO' Strike > PHN-8300 'Lilith' Strike (30s, 2x blast radius, 100,000% damage, blights everyone in line of sight)
    - Heretic: Nevermore (Special) > Perish Song (After 30 seconds, deals 5000% fatal damage to you and nearby enemies.)
	- Railgunner: Supercharge (Special) > Hypercharge (-20 armor on hit, +0.5 proc coefficient)
	    - -OR-  Cryocharge > Permafrosted Cryocharge (Explodes on contact with a frost blast, dealing 200% damage to all enemies within 6m. Slows on hit by 80% for 20 seconds.)
	- Void Fiend: Crush/Corrupted Crush -> Reclaimed Crush/Forfeited Crush (Also affects nearby enemies or allies within 25m. Corrupted Supress has +2 charges.)

    - Alloy Vulture: Windblade > Repeated Windblade (50% chance to fire twice)
    - Aurelionite: Eye Laser > Piercing Eye Laser (Additional 35% of damage dealt pierces walls)

## To-Do/Issues
* Mithrix Scepter
* CaptainBustedAirstrike support:
	* Currently it uses the reduced cooldown from the standard alt airstrike.
* Alternate Replacement for Diablo Strike
* Further improvements to scepters(?)
* Railgunner, Void Fiend
* Fix Void Fiend's Scepter desyncing overrides if the Scepter is lost in corrupt mode

## For Devs
- Adding a ScepterReplacer to a character that already has an existing ScepterReplacer for that slot and variant will replace it.
	- This change allows developers that modify or outright change existing skills to have their own Scepter skill for existing characters.
- All icons for Scepter Skills in this mod are now publicly accessible. See the static class `Assets.SpriteAssets`.

## Credits
* **ThinkInvisible** - Original code and implementation
* **DestroyedClone** - Porter, Maintainer
* **rob** - Fixed code
* **bruh** and **redacted** - Made the Scepter model
* **swuff** - Updated Textures, Consultation
* **QandQuestion** - Lore
* **Moffein** - Consultation
* **Mico27** - Orb creation help
* **TheTimeSweeper** - UnusedMode, Alternate Item Model, Item Displays, Other help
* **/vm/** - Idea for Captain's Lilith Strike


## Changelog
`1.1.1`
- 🛠️fixed BetterUI compat having issues when BetterUI isn't installed

`1.1.0`
- 🛠️Updated for SOTV
	- Flamethrower: Copied updated IL code from ClassicItems
- ➕New Config:
	- "Remove Classic Items' Ancient Scepter From Droplist": Enabled by default.
- ➕Added Item Displays for Heretic, Railgunner, and Void FIend
- ➕New Scepters (WIP)
	- Railgunner:
		- Supercharge -> Hypercharge: Permanently removes 20 armor, +0.5 proc coeff
		- Cryocharge -> Permafrosted Cryocharge: Slows on hit, ice explosion
	- Void Fiend
		- Crush -> Reclaimed Crush: Heals nearby allies within 25m
		- Corrupted Crush -> Reclaimed Crush (Corrupted): Damages nearby enemies within 25m. +2 stocks
- BetterUI Compat: Proc Coefficients
- 🛠️Captain: PHN-8300 'Lilith' Strike
	- Reduced blight duration (30s -> 20s) and stacks (10 -> 5)

`1.0.9`
- ➕Added new config setting: UnusedMode
	-   Keep: Non-sceptered characters keep scepter when picked up
	-   Reroll: Characters reroll according to the Reroll on pickup config
	-   Metamorphosis: characters without scepter upgrades will not reroll if metamorphosis is active  
		- Allowing you to play metamorphosis runs with sceptered characters
- ➕Added alternate model
- 🛠️Fixed the weird itemdef.modelpickupprefab warning thing
- ➕Added item displays to [Enforcer and Nemforcer](https://thunderstore.io/package/EnforcerGang/Enforcer/)

`1.0.8`
- 🛠️Fixed Captain's Diablo Scepter Skill still inflicting Blight on allies when Captain Nuke Friendly Fire is disabled.
- 🛠️Recategorized the config. Requires refreshing your config.

`1.0.7`
- 🛠️Temporarily replaced the texture used for the glow on the Ancient Scepter to the purple fire texture used by the Ancient Wisp, while waiting on the next material fix.

`1.0.6`
- 🛠️Config setting `StridesTakesPrecedence` changed to `HeresyTakesPrecedence` in accordance of RoR2 possessing more than one Heresy item
- ➕Added configuration setting to blacklist AncientScepter from turrets, incorrectly causing the turrets to receive a reroll due to having no skill coded. (Disabled by default to maintain original behavior)
- 🛠️Fixed Unity Error complaining about trying to register it to network despite missing a networkidentity
- ➕Added configuration setting to Captain's Nuke skill whether or not it blights allies (Disabled by default)
- 🛠️(For Devs) Adding a ScepterReplacer to a character that already has an existing ScepterReplacer for that slot and variant will replace it, instead of throwing an error.
	- This change allows developers that modify or outright change existing skills to have their own Scepter skill for existing characters.
- 🛠️(For Devs) All icons for Scepter Skills in this mod are now publicly accessible. See the static class `Assets.SpriteAssets`.

`1.0.5`
 - 🛠️Restored the property accessor on the ItemDef of the ItemBase, which was preventing other mods from accessing the item properly.

`1.0.4`
- ❌Blacklisted from being copied by TinkersSatchel's "Mostly-Tame Mimic" to prevent accidental rerolls.
- 🛠️Merged config option to reroll duplicates into: Disabled, Random, and Scrap.
	- Scrap option allows extra to reroll into red scrap.
- 🛠️Increased radius of Commando's "Death Blossom".
	- Holding down primary fire while using the skill switches back to the vanilla aim radius.
- 🛠️Added option to Mercernary's "Massacre", allowing the user to exit early to prevent softlocks; especially with with SkillsPlusPlus.
	- Hold down your special ability to exit early.
- ➕Added missing scepter skills for Captain's "Diablo Strike", Bandit's "Lights Out" and "Desperado", and Heretic's "Nevermore".
- ➕Added new skills for Aurelionite's Laser, and Alloy Vulture's Windblade. 
- ➕Updated Assets and Lore
- ➕Updated Readme
- 🛠️Updated internal names to be more independent of ClassicItems

`1.0.3`
- 🛠️Made ItemDef public so other mods can access it for their display rules

`1.0.2`
- 🛠️Fixed for latest RoR2 version

`1.0.1`
- 🛠️Fixed REX's upgrade not applying debuffs
- 🛠️Fixed Captain's upgrade not having a skill icon
- ➕Added item display for Bandit

`1.0.0`
- Initial release