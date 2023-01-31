using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Survivors
{
    [DisabledContent]
    public sealed class Borg : SurvivorBase
    {
        public override SurvivorDef SurvivorDef { get; } = SS2Assets.LoadAsset<SurvivorDef>("SurvivorBorg", SS2Bundle.Indev);
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("BorgBody", SS2Bundle.Indev);
        public override GameObject MasterPrefab { get; } = null;

        /*public override void ModifyPrefab()
        {
            var charBody = BodyPrefab.GetComponent<CharacterBody>();
            charBody._defaultCrosshairPrefab = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair");
            charBody.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
            var footstepHandler = BodyPrefab.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>();
            footstepHandler.footstepDustPrefab = Resources.Load<GameObject>("Prefabs/GenericFootstepDust");
        }*/

        public override void Hook()
        {

        }
    }
}

/*namespace Moonstorm.Starstorm2.Survivors
{
    internal class Borg : SurvivorBase
    {
        /*internal override string bodyName { get; set; } = "Borg";

        internal override GameObject bodyPrefab { get; set; }
        internal override GameObject displayPrefab { get; set; }

        internal override float sortPosition { get; set; } = 6.001f;

        internal override ConfigEntry<bool> characterEnabled { get; set; }

        internal override StarstormBodyInfo bodyInfo { get; set; } = new StarstormBodyInfo
        {
            armor = 0f,
            armorGrowth = 0f,
            bodyName = "BorgBody",
            bodyNameToken = "Borg_NAME",
            characterPortrait = Assets.Instance.MainAssetBundle.LoadAsset<Texture2D>("Borgicon"),
            bodyColor = new Color32(138, 183, 168, 255),
            crosshair = Resources.Load<GameObject>("Prefabs/Crosshair/StandardCrosshair"),
            damage = 12f,
            healthGrowth = 33f,
            healthRegen = 1.5f,
            jumpCount = 1,
            maxHealth = 110f,
            subtitleNameToken = "Borg_SUBTITLE",
            podPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod")
        };

        internal override int mainRendererIndex { get; set; } = 0;

        internal override Type characterMainState { get; set; } = typeof(BorgMain);

        internal override ItemDisplayRuleSet itemDisplayRuleSet { get; set; }
        internal override List<ItemDisplayRuleSet.KeyAssetRuleGroup> itemDisplayRules { get; set; }

        internal override UnlockableDef characterUnlockableDef { get; set; } = null;//Unlockables_Deprecated.AddUnlockable<Cores.Unlockables.Achievements.BorgUnlockAchievement>(true);
        private static UnlockableDef masterySkinUnlockableDef;
        private static UnlockableDef grandMasterySkinUnlockableDef;

        public static GameObject bfgProjectile;
        public static GameObject BorgPylon;

        public static SkillDef specialDef1;
        public static SkillDef specialDef2;

        internal override void InitializeCharacter()
        {
            base.InitializeCharacter();

            RegisterProjectiles();
            bodyPrefab.AddComponent<Components.BorgController>();
            bodyPrefab.AddComponent<BorgInfoComponent>();
        }

        private void RegisterProjectiles()
        {
            bfgProjectile = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/FMJ"), "Prefabs/Projectiles/BorgbfgProjectile", true, "C:\\Users\\test\\Documents\\ror2mods\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor.cs", "RegisterCharacter", 155);
            bfgProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
            //bfgProjectile.GetComponent<ProjectileController>().catalogIndex = 53;
            bfgProjectile.GetComponent<ProjectileController>().ghostPrefab = Resources.Load<GameObject>("Prefabs/ProjectileGhosts/BeamSphereGhost");
            bfgProjectile.GetComponent<ProjectileDamage>().damage = 1f;
            bfgProjectile.GetComponent<ProjectileDamage>().damageType = DamageType.Generic;
            bfgProjectile.GetComponent<ProjectileDamage>().damageColorIndex = DamageColorIndex.Default;
            bfgProjectile.GetComponent<ProjectileSimple>().desiredForwardSpeed = 13;
            bfgProjectile.GetComponent<ProjectileSimple>().lifetime = 4;
            bfgProjectile.AddComponent<ProjectileProximityBeamController>();
            bfgProjectile.GetComponent<ProjectileProximityBeamController>().attackRange = 8;
            bfgProjectile.GetComponent<ProjectileProximityBeamController>().listClearInterval = .2f;
            bfgProjectile.GetComponent<ProjectileProximityBeamController>().attackInterval = .2f;
            bfgProjectile.GetComponent<ProjectileProximityBeamController>().damageCoefficient = 0.8f;
            bfgProjectile.GetComponent<ProjectileProximityBeamController>().procCoefficient = .2f;
            bfgProjectile.GetComponent<ProjectileProximityBeamController>().inheritDamageType = true;
            bfgProjectile.AddComponent<RadialForce>();
            bfgProjectile.GetComponent<RadialForce>().radius = 18;
            bfgProjectile.GetComponent<RadialForce>().damping = 0.5f;
            bfgProjectile.GetComponent<RadialForce>().forceMagnitude = -1500;
            bfgProjectile.GetComponent<RadialForce>().forceCoefficientAtEdge = 0.5f;
            bfgProjectile.AddComponent<ProjectileImpactExplosion>();
            bfgProjectile.GetComponent<ProjectileImpactExplosion>().impactEffect = Resources.Load<GameObject>("Prefabs/Effects/BeamSphereExplosion");
            bfgProjectile.GetComponent<ProjectileImpactExplosion>().destroyOnEnemy = true;
            bfgProjectile.GetComponent<ProjectileImpactExplosion>().destroyOnWorld = true;
            bfgProjectile.GetComponent<ProjectileImpactExplosion>().timerAfterImpact = false;
            bfgProjectile.GetComponent<ProjectileImpactExplosion>().falloffModel = BlastAttack.FalloffModel.SweetSpot;
            bfgProjectile.GetComponent<ProjectileImpactExplosion>().lifetime = 3;
            bfgProjectile.GetComponent<ProjectileImpactExplosion>().lifetimeAfterImpact = 0;
            bfgProjectile.GetComponent<ProjectileImpactExplosion>().lifetimeRandomOffset = 0;
            bfgProjectile.GetComponent<ProjectileImpactExplosion>().blastRadius = 20;
            bfgProjectile.GetComponent<ProjectileImpactExplosion>().blastDamageCoefficient = 12f;
            bfgProjectile.GetComponent<ProjectileImpactExplosion>().blastProcCoefficient = 1;
            bfgProjectile.GetComponent<ProjectileImpactExplosion>().blastAttackerFiltering = AttackerFiltering.Default;
            bfgProjectile.GetComponent<ProjectileOverlapAttack>().enabled = false;

            //bfgProjectile.GetComponent<ProjectileProximityBeamController>().enabled = false;

            BorgPylon = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Projectiles/EngiGrenadeProjectile"), "Prefabs/Projectiles/BorgTPPylon", true, "C:\\Users\\test\\Documents\\ror2mods\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor\\ExampleSurvivor.cs", "RegisterCharacter", 156);

            GameObject ghost = Assets.Instance.MainAssetBundle.LoadAsset<GameObject>("BorgTeleGhost2");
            ghost.AddComponent<ProjectileGhostController>();
            BorgPylon.GetComponent<ProjectileController>().ghostPrefab = ghost;
            BorgPylon.GetComponent<ProjectileSimple>().lifetime = 2147483646;
            BorgPylon.GetComponent<ProjectileImpactExplosion>().lifetime = 2147483646;
            BorgPylon.GetComponent<ProjectileImpactExplosion>().lifetimeAfterImpact = 2147483646;
            BorgPylon.GetComponent<ProjectileImpactExplosion>().destroyOnEnemy = false;
            //BorgPylon.GetComponent<ProjectileImpactExplosion>().falloffModel = BlastAttack.FalloffModel.SweetSpot;
            BorgPylon.GetComponent<Rigidbody>().drag = 2;
            BorgPylon.GetComponent<Rigidbody>().angularDrag = 2f;
            //BorgPylon.AddComponent<AntiGravityForce>();
            BorgPylon.GetComponent<AntiGravityForce>().rb = BorgPylon.GetComponent<Rigidbody>();
            BorgPylon.GetComponent<AntiGravityForce>().antiGravityCoefficient = 1;

            // register it for networking
            if (bfgProjectile) PrefabAPI.RegisterNetworkPrefab(bfgProjectile);
            if (BorgPylon) PrefabAPI.RegisterNetworkPrefab(BorgPylon);

            // add it to the projectile catalog or it won't work in multiplayer
            //Prefabs.projectilePrefabs.Add(bfgProjectile);
            //Prefabs.projectilePrefabs.Add(BorgPylon);
        }

        internal override void InitializeSkills()
        {
            foreach (GenericSkill sk in bodyPrefab.GetComponentsInChildren<GenericSkill>())
            {
                UnityEngine.Object.DestroyImmediate(sk);
            }

            SkillLocator skillLocator = bodyPrefab.GetComponent<SkillLocator>();

            SetUpPrimaries(skillLocator);
            SetUpSecondaries(skillLocator);
            SetUpUtilities(skillLocator);
            SetUpSpecials(skillLocator);
        }

        private void SetUpPrimaries(SkillLocator skillLocator)
        {
            SkillDef primaryDef1 = ScriptableObject.CreateInstance<SkillDef>();
            primaryDef1.activationState = new EntityStates.SerializableEntityStateType(typeof(BorgFireBaseShot));
            primaryDef1.activationStateMachineName = "Weapon";
            primaryDef1.skillName = "Borg_PRIMARY_GUN_NAME";
            primaryDef1.skillNameToken = "Borg_PRIMARY_GUN_NAME";
            primaryDef1.skillDescriptionToken = "Borg_PRIMARY_GUN_DESCRIPTION";
            primaryDef1.icon = Assets.Instance.MainAssetBundle.LoadAsset<Sprite>("Borgprimary");
            primaryDef1.baseMaxStock = 1;
            primaryDef1.baseRechargeInterval = 0f;
            primaryDef1.beginSkillCooldownOnSkillEnd = false;
            primaryDef1.canceledFromSprinting = false;
            primaryDef1.fullRestockOnAssign = true;
            primaryDef1.interruptPriority = EntityStates.InterruptPriority.Any;
            primaryDef1.isCombatSkill = true;
            primaryDef1.mustKeyPress = false;
            primaryDef1.cancelSprintingOnActivation = true;
            primaryDef1.rechargeStock = 1;
            primaryDef1.requiredStock = 1;
            primaryDef1.stockToConsume = 1;
        }

        private void SetUpSecondaries(SkillLocator skillLocator)
        {
            SkillDef secondaryDef1 = ScriptableObject.CreateInstance<SkillDef>();
            secondaryDef1.activationState = new EntityStates.SerializableEntityStateType(typeof(BorgFireTrackshot));
            secondaryDef1.activationStateMachineName = "Weapon";
            secondaryDef1.skillName = "Borg_SECONDARY_AIMBOT_NAME";
            secondaryDef1.skillNameToken = "Borg_SECONDARY_AIMBOT_NAME";
            secondaryDef1.skillDescriptionToken = "Borg_SECONDARY_AIMBOT_DESCRIPTION";
            secondaryDef1.icon = Assets.Instance.MainAssetBundle.LoadAsset<Sprite>("Borgsecondary");
            secondaryDef1.baseMaxStock = 1;
            secondaryDef1.baseRechargeInterval = 3f;
            secondaryDef1.beginSkillCooldownOnSkillEnd = false;
            secondaryDef1.canceledFromSprinting = false;
            secondaryDef1.fullRestockOnAssign = true;
            secondaryDef1.interruptPriority = EntityStates.InterruptPriority.Skill;
            secondaryDef1.isCombatSkill = true;
            secondaryDef1.mustKeyPress = false;
            secondaryDef1.cancelSprintingOnActivation = true;
            secondaryDef1.rechargeStock = 1;
            secondaryDef1.requiredStock = 1;
            secondaryDef1.stockToConsume = 1;
        }

        private void SetUpUtilities(SkillLocator skillLocator)
        {
            SkillDef utilityDef1 = ScriptableObject.CreateInstance<SkillDef>();
            utilityDef1.activationState = new EntityStates.SerializableEntityStateType(typeof(BorgFireBFG));
            utilityDef1.activationStateMachineName = "Weapon";
            utilityDef1.skillName = "Borg_SPECIAL_NOTPREON_NAME";
            utilityDef1.skillNameToken = "Borg_SPECIAL_NOTPREON_NAME";
            utilityDef1.skillDescriptionToken = "Borg_SPECIAL_NOTPREON_DESCRIPTION";
            utilityDef1.icon = Assets.Instance.MainAssetBundle.LoadAsset<Sprite>("Borgutility");
            utilityDef1.baseMaxStock = 1;
            utilityDef1.baseRechargeInterval = 10f;
            utilityDef1.beginSkillCooldownOnSkillEnd = false;
            utilityDef1.canceledFromSprinting = false;
            utilityDef1.fullRestockOnAssign = true;
            utilityDef1.interruptPriority = EntityStates.InterruptPriority.Skill;
            utilityDef1.isCombatSkill = true;
            utilityDef1.mustKeyPress = false;
            utilityDef1.cancelSprintingOnActivation = false;
            utilityDef1.rechargeStock = 1;
            utilityDef1.requiredStock = 1;
            utilityDef1.stockToConsume = 1;
        }

        private void SetUpSpecials(SkillLocator skillLocator)
        {
            specialDef1 = ScriptableObject.CreateInstance<SkillDef>();
            specialDef1.activationState = new EntityStates.SerializableEntityStateType(typeof(BorgTeleport));
            specialDef1.activationStateMachineName = "Weapon";
            specialDef1.skillName = "Borg_SPECIAL_TELEPORT_NAME";
            specialDef1.skillNameToken = "Borg_SPECIAL_TELEPORT_NAME";
            specialDef1.skillDescriptionToken = "Borg_SPECIAL_TELEPORT_DESCRIPTION";
            specialDef1.icon = Assets.Instance.MainAssetBundle.LoadAsset<Sprite>("Borgspecial");
            specialDef1.baseMaxStock = 1;
            specialDef1.baseRechargeInterval = 3f;
            specialDef1.beginSkillCooldownOnSkillEnd = false;
            specialDef1.canceledFromSprinting = false;
            specialDef1.fullRestockOnAssign = true;
            specialDef1.interruptPriority = EntityStates.InterruptPriority.Pain;
            specialDef1.isCombatSkill = false;
            specialDef1.mustKeyPress = false;
            specialDef1.cancelSprintingOnActivation = false;
            specialDef1.rechargeStock = 1;
            specialDef1.requiredStock = 1;
            specialDef1.stockToConsume = 1;
            specialDef1.keywordTokens = new string[] { "KEYWORD_TELEFRAG" };

            specialDef2 = ScriptableObject.CreateInstance<SkillDef>();
            specialDef2.activationState = new EntityStates.SerializableEntityStateType(typeof(BorgTeleport));
            specialDef2.activationStateMachineName = "Weapon";
            specialDef2.skillName = "Borg_SPECIAL_TELEPORTB_NAME";
            specialDef2.skillNameToken = "Borg_SPECIAL_TELEPORTB_NAME";
            specialDef2.skillDescriptionToken = "Borg_SPECIAL_TELEPORTB_DESCRIPTION";
            specialDef2.icon = Assets.Instance.MainAssetBundle.LoadAsset<Sprite>("Borgspecial2");
            specialDef2.baseMaxStock = 1;
            specialDef2.baseRechargeInterval = 2f;
            specialDef2.beginSkillCooldownOnSkillEnd = false;
            specialDef2.canceledFromSprinting = false;
            specialDef2.fullRestockOnAssign = false;
            specialDef2.interruptPriority = EntityStates.InterruptPriority.Pain;
            specialDef2.isCombatSkill = false;
            specialDef2.mustKeyPress = false;
            specialDef2.cancelSprintingOnActivation = false;
            specialDef2.rechargeStock = 1;
            specialDef2.requiredStock = 1;
            specialDef2.stockToConsume = 1;
        }

        internal override void RegisterTokens()
        {
            LanguageAPI.Add("Borg_NAME", "Borg");
            LanguageAPI.Add("Borg_SUBTITLE", "Man Made Monstrosity");
            LanguageAPI.Add("Borg_DESCRIPTION", "The Borg is a well-rounded survivor, prepared for any situation.<color=#CCD3E0> \n\n" +
                " < ! > Unmaker is instant and very accurate, making it useful against enemies both at close and long ranges. \n\n" +
                " < ! > Rising Star shines when dealing with several hard to hit enemies, as it can hit several targets in a short amount of time. \n\n" +
                " < ! > Overheat Redress vacuums up nearby enemies before exploding - it also provides a quick burst of movement in the opposite direction from which it's fired. \n\n" +
                " < ! > Recall can be a very handy asset while exploring the map, to backtrack to the teleporter or to an interactable. Teleporting also deals heavy damage to any enemies you appear inside of.</color>\n\n");
            LanguageAPI.Add("Borg_OUTRO_FLAVOR", "..and so he left, with a broken arm and no spare parts.");
            LanguageAPI.Add("Borg_OUTRO_FAILURE", "..and so he vanished, teleportation beacon left with no signal.");
            LanguageAPI.Add("Borg_LORE", "<style=cMono>Audio file found under \"Dr. Rayell's Testimony\". Playing...</style>\n\n" +
                "\"Recording started. Begin testing, Dr. Rayell.\"\n\n" +
                "\"Thank you. After the EOD's smashing success with biomechanical augmentations, producing some of the finest combat engineers to ever come out of the department, they proceeded further with slightly more... shall we say, experimental designs.\"\n\n" +
                "\"How so?\"\n\n" +
                "\"The department began looking into mental augmentation. It was definitely a leap, to be sure. Augmentation was already very cutting edge, but supplementing - or even replacing a damaged brain? It was a tall order. But the heads thought we could do it, so we obliged.\"\n\n" +
                "\"Hm. What came of your project?\"\n\n" +
                "\"Once prototyping was complete, we began the human trials. We got all sorts of folks as test subjects, they were vegetative, mentally unstable, broken people. We were told to fix them as best would could.\"\n\n" +
                "\"And how did that go?\"\n\n" +
                "\"It was... It was a travesty. Some of them were simply too far gone. There was nothing we could do. Those were the best cases. Worse was when they rejected the replacements. Some died in a swift immune response. The more cognisant ones begged for us to end the headaches they were having. Some of them just started screaming. They'd scream for hours and hours, sometimes for days even. And then they'd stop. They'd literally scream themselves to death.\"\n\n" +
                "\"That's, um, a bit unsettling. Is that all?\"\n\n" +
                "\"There was one. One who made it through the replacement. And it worked wonderfully. We got him in a vegetative state, no hope whatsoever, and we took him from that, and we gave him - maybe not his full life back, but perhaps something resembling it. You have no idea how thankful I was to hear that he was doing well.\"\n\n" +
                "\"Well, I suppose that's good to hear. So, what's happened to him?\"\n\n" +
                "\"You know? I'm not sure. Other than some behavioral changes, which is to be expected, he was able to return to society rather gracefully. We've done check-ups on him every three months to analyze long-term effects, but... Actually? Now that I'm thinking about it, he didn't show up for it last month. Oh, well. I'm sure he's doing fine. I'll have to contact him about that.\"\n\n" +
                "\"Um, alright, then. Thank you for your time, Dr. Rayell. Your testimony is... Well, it's certainly something. I hope to hear more about your projects in the future.\"\n\n" +
                "\"Very well. I hope you find what you're looking for.\"\n\n" +
                "<style=cMono>End of file.</style>");

            var dmg = BorgFireBaseShot.damageCoefficient * 100f;

            LanguageAPI.Add("Borg_PRIMARY_GUN_NAME", "Unmaker");
            LanguageAPI.Add("Borg_PRIMARY_GUN_DESCRIPTION", $"Shoot an enemy for <style=cIsDamage>{dmg}% damage</style>.");

            dmg = BorgFireTrackshot.damageCoefficient * 100f;

            LanguageAPI.Add("Borg_SECONDARY_AIMBOT_NAME", "Rising Star");
            LanguageAPI.Add("Borg_SECONDARY_AIMBOT_DESCRIPTION", $"Quickly fire three seeking shots at contenders in front for <style=cIsDamage>3x{dmg}% damage</style>. Stunning.");

            var zapDmg = 0.8f * 100f * 5f;
            var explosionDmg = 12f * 100f;

            LanguageAPI.Add("Borg_SPECIAL_NOTPREON_NAME", "Overheat Redress");
            LanguageAPI.Add("Borg_SPECIAL_NOTPREON_DESCRIPTION", $"Blast yourself backwards, firing a greater energy bullet, dealing up to <style=cIsDamage>{zapDmg}%</style> damage per second. " +
                $"Explodes at the end dealing <style=cIsDamage>{explosionDmg}%</style> in an area.");

            LanguageAPI.Add("KEYWORD_TELEFRAG", $"<style=cKeywordName>Telefragging</style><style=cSub>Deals heavy damage to enemies when teleporting inside of them.</style>");
            LanguageAPI.Add("Borg_SPECIAL_TELEPORT_NAME", "Recall");
            LanguageAPI.Add("Borg_SPECIAL_TELEPORT_DESCRIPTION", "Create a warp point. Once a warp point is set, teleport to its location. Teleporting <style=cIsDamage>reduces skill cooldowns by 4 seconds</style>. " +
                $"Telefragging.");
            LanguageAPI.Add("Borg_SPECIAL_TELEPORTB_NAME", "Recall");
            LanguageAPI.Add("Borg_SPECIAL_TELEPORTB_DESCRIPTION", "Tap to teleport to the beacon, or hold to remove it.");
        }

        internal override void InitializeSkins()
        {
            GameObject model = bodyPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            ModelSkinController skinController = model.AddComponent<ModelSkinController>();
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            SkinnedMeshRenderer mainRenderer = characterModel.mainSkinnedMeshRenderer;

            CharacterModel.RendererInfo[] defaultRenderers = characterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            //TODO: add skins
        }

        /*
        public static void CreateDoppelganger()
        {
            doppelganger = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterMasters/CommandoMonsterMaster"), "BorgMonsterMaster", true);
            doppelganger.GetComponent<CharacterMaster>().bodyPrefab = bodyPrefab;

            Prefabs.masterPrefabs.Add(doppelganger);
        }
        
    }
    public sealed class BorgInfoComponent : NetworkBehaviour
    {
        public bool tpReady;
        public Vector3 tpPos;
        public bool isHooked;

        /*[ClientRpc]
         * I could need some RPC later, so keeping it handy
        public void RpcAddIonCharge()
        {
            SkillLocator skillLoc = gameObject.GetComponent<SkillLocator>();
            GenericSkill ionGunSkill = skillLoc?.secondary;
            if (hasAuthority && ionGunSkill && ionGunSkill.stock < ionGunSkill.maxStock)
                ionGunSkill.AddOneStock();
        }
    }

    public sealed class BorgPylonComponent : NetworkBehaviour
    {

    }

    public sealed class DeadBorgBehavior : NetworkBehaviour
    {
        public int maxPurchaseCount = 1;
        public int purchaseCount;
        private float refreshTimer;
        private bool waitingForRefresh;
        public static event Action OnRepairBorg;

        public PurchaseInteraction purchaseInteraction;

        private void Start()
        {
            if (purchaseInteraction)
                purchaseInteraction.onPurchase.AddListener(RepairBorg);
        }

        public void FixedUpdate()
        {
            if (waitingForRefresh)
            {
                refreshTimer -= Time.fixedDeltaTime;
                if (refreshTimer <= 0 && purchaseCount < maxPurchaseCount)
                {
                    purchaseInteraction.SetAvailable(true);
                    waitingForRefresh = false;
                }
            }
        }

        [Server]
        public void RepairBorg(Interactor interactor)
        {
            if (!NetworkServer.active)
                return;

            purchaseInteraction.SetAvailable(false);
            waitingForRefresh = true;

            List<PickupIndex> droplist = Run.instance.availableTier3DropList;
            if (droplist != null && droplist.Count > 0)
            {
                PickupIndex item = Run.instance.treasureRng.NextElementUniform(droplist);
                PickupDropletController.CreatePickupDroplet(item, transform.position + new Vector3(0, 5, 0), new Vector3(0, 1, 0));
            }

            purchaseCount++;
            refreshTimer = 2;

            OnRepairBorg();

            if (purchaseCount >= maxPurchaseCount)
            {
                //spawn in a fixed Borg or beam him out or whatever
            }
        }
    }
}*/
