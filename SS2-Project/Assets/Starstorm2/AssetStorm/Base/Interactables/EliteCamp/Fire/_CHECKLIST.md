### duplicate all assets in this folder and rename
list of references and values to change. best done in order
dont forget to use Auto Find in the AssetCollection

bdPowerup___
- Buff Color

mat___EliteCrystalSolid
- Main Color

matPickup___CrystalGlow
- Tint

.___EliteCrystalLargeBody
- CharacterBody baseNameToken
- CharacterBody bodyColor
- CharacterDeathBehavior deathState
- EliteCrystalLarge mesh crystal material
- Light color
- mdlEliteCrystal CharacterModel renderer info material and light color
- Swirls particle color

.___EliteCrystalSmallBody
- CharacterBody baseNameToken
- CharacterBody bodyColor
- CharacterDeathBehavior deathState
- EliteCrystalSmall mesh material
- Light color
- mdlEliteCrystal CharacterModel renderer info material and light color
- Swirls particle color

.___EliteCrystalLargeDeath
- IceMesh crystal material
- Chunks start color
- Point Light color
- Flash color

.___EliteCrystalSmallDeath
- IceMesh crystal material
- Chunks start color
- Point Light color
- Flash color

.___EliteCrystalSpawnEffect
- LightShafts start color
- Dust start color
- Point Light color

.___PowerupEffect
- crystal + glow material into BallPrefab mesh
- RingFlash particle start color
- SoundwaveRings particle start color

.___CrystalPickupEffect
- Ring color
- Lightning color
- Flash color

.___CrystalPowerup
- Powerup BuffDef into PackTrigger SS2BuffPickup
- PickupEffect into PackTrigger SS2BuffPickup
- Mesh solid crystal material
- Mesh glow crystal material
- Lightning color
- Point Light color

isc___EliteCrystalSmall
- SmallBody into SpawnCard.prefab

isc___EliteCrystalLarge
- LargeBody into SpawnCard.prefab

dccs___EliteCampCrystals
- Replace small crystal spawncard

.___EliteCamp
- EliteCampController Elite Def Address
- Camp 1: Large crystal body into GenericSceneSpawnPoint
- Camp 2: dccs___EliteCampCrystals into CampDirector.interactableDirectorCards

isc___EliteCamp
- EliteCamp prefab into SpawnCard.prefab

icp___EliteCamp
- EliteCamp spawncard into first spawncard
- small crystal spawncard into second spawncard

EntityStates.Destructible.___CrystalDeathLarge
- Explosion Effect Prefab
- Monster Spawn Effect Prefab
- Pickup prefab

EntityStates.Destructible.___CrystalDeathSmall
- Explosion Effect Prefab
- Monster Spawn Effect Prefab
- Pickup prefab


CODE STUFF
EliteCrystalDeath.cs
- Add Small and Large death states 

SS2Content.cs
- Add bdPowerup___ entry

EliteCamp.cs
- add colors and powerup effect prefab fields
- add powerup buff to GetStatCoefficients
- add powerup check into the AddDamageNumbers hook
- create new PowerupEffectBehavior
