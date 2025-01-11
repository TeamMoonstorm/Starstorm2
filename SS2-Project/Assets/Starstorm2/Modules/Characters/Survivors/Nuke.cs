using MSU;
using R2API;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SS2.Survivors
{
    /// <summary>
    /// Nucleator my beloved...
    /// 
    /// <br></br>
    /// Normally we dont document our classes but since Nebby (nucleator stan) has an actual job now he cant afford to work on him anymore, this is for whomever takes the flag.
    /// </summary>
    public class Nuke : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acNuke", SS2Bundle.Indev);

        /// <summary>
        /// Reference to the Nuclear damage type, this damage type inflicts either the regular <see cref="SS2Content.Buffs.bdIrradiated"/> or <see cref="SS2Content.Buffs.dbdNuclearSickness"/> DOT. depending if nucleator has his special active.
        /// </summary>
        public static DamageAPI.ModdedDamageType NuclearDamageType { get; private set; }

        /// <summary>
        /// The DotIndex tied to NuclearSickness
        /// </summary>
        public static DotController.DotIndex NuclearSicknessDotIndex => _dbdNuclearSickness.dotIndex;
        private static DotBuffDef _dbdNuclearSickness;

        public override void Initialize()
        {
            FindAndInitAssets();
            NuclearDamageType = DamageAPI.ReserveDamageType();
            GlobalEventManager.onServerDamageDealt += InflictNuclearSickness;
            R2API.RecalculateStatsAPI.GetStatCoefficients += StatChanges;
            ModifyPrefabs();
        }

        /// <summary>
        /// Implementation of both Irradiated and Nuclear Sickness stat modifications. both add to the same "finalCount" float, which is used for the actual stat modifiers
        /// </summary>
        private void StatChanges(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            var irradiatedBuffCount = (float)sender.GetBuffCount(SS2Content.Buffs.bdIrradiated);
            var sicknessBuffCount = (float)sender.GetBuffCount(SS2Content.Buffs.dbdNuclearSickness);

            float finalCount = 0;

            //If the enemy has both, add them and divide by 2
            if (irradiatedBuffCount > 0 && sicknessBuffCount > 0)
            {
                finalCount = (irradiatedBuffCount + sicknessBuffCount) / 2;
            }
            else
            {
                //othertwise just add them
                finalCount += sicknessBuffCount;
                finalCount += irradiatedBuffCount;
            }

            //Weaken high level enemies
            args.levelMultAdd -= finalCount / 10;

            //reduce armor
            args.armorAdd -= sicknessBuffCount * 5;

            // The buff behavior for Nuke's surge special
            // Lets give Nuke a small little treat for the special
            if (sender.HasBuff(SS2Content.Buffs.bdNukeSpecial))
            {
                args.attackSpeedMultAdd += 0.1f;
                args.damageMultAdd += 0.1f;
                args.moveSpeedMultAdd += 0.1f;
            }
        }

        private void FindAndInitAssets()
        {
            //Add our colours and initialize nuclear sickness itself
            var damageColor = AssetCollection.FindAsset<SerializableDamageColor>("NukeDamageColor");
            ColorsAPI.AddSerializableDamageColor(damageColor);
            damageColor = AssetCollection.FindAsset<SerializableDamageColor>("NukeSelfDamageColor");
            ColorsAPI.AddSerializableDamageColor(damageColor);

            _dbdNuclearSickness = AssetCollection.FindAsset<DotBuffDef>("dbdNuclearSickness");
            _dbdNuclearSickness.Init();
        }

        private void ModifyPrefabs()
        {
            var cb = CharacterPrefab.GetComponent<CharacterBody>();
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
            var ctp = CharacterPrefab.GetComponent<CameraTargetParams>();
            ctp.cameraParams = Addressables.LoadAssetAsync<CharacterCameraParams>("RoR2/Base/Croco/ccpCroco.asset").WaitForCompletion();

            //FIXME: Do what this says
            AssetCollection.FindAsset<GameObject>("NukeSludgeProjectile").AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(NuclearDamageType);
            AssetCollection.FindAsset<GameObject>("NukePoolDOT").AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(NuclearDamageType);
        }

        /// <summary>
        /// Method that actually inflicts nuclear sickness to a victim.
        /// </summary>
        private void InflictNuclearSickness(DamageReport report)
        {
            var victimBody = report.victimBody;
            var attackerBody = report.attackerBody;
            var damageInfo = report.damageInfo;

            if (victimBody && attackerBody && damageInfo.HasModdedDamageType(NuclearDamageType))
            {
                //As long as nucleator has his special active, inflict the DOT version instead of the non DOT version
                if (attackerBody.HasBuff(SS2Content.Buffs.bdNukeSpecial))
                {
                    var dotInfo = new InflictDotInfo
                    {
                        attackerObject = attackerBody.gameObject,
                        damageMultiplier = 1,
                        dotIndex = _dbdNuclearSickness.dotIndex,
                        duration = 10,
                        victimObject = victimBody.gameObject
                    };
                    DotController.InflictDot(ref dotInfo);
                }
                else
                {
                    victimBody.AddTimedBuff(SS2Content.Buffs.bdIrradiated, 2, 10);
                }
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        /// <summary>
        /// Represents an <see cref="EntityStates.EntityState"/> that can be Charged up by Nucleator's systems
        /// </summary>
        public interface IChargeableState
        {
            /// <summary>
            /// The Current charge of the entity state, this will be given to the <see cref="IChargedState"/>
            /// </summary>
            float currentCharge { get; }
            /// <summary>
            /// The starting charge coefficient, this is also the starting damage coefficient of the skill.
            /// </summary>
            float startingChargeCoefficient { get; }
            /// <summary>
            /// The soft-cap charge coefficient, if nucleator keeps charging, he'll start taking damage.
            /// </summary>
            float chargeCoefficientSoftCap { get; }
            /// <summary>
            /// The hard-cap charge coefficient, nucleator will be forced to fire if he reaches this threshold
            /// </summary>
            float chargeCoefficientHardCap { get; }
        }

        /// <summary>
        /// Represents an <see cref="EntityStates.EntityState"/> that's a fired, charged state of Nucleator.
        /// </summary>
        public interface IChargedState
        {
            /// <summary>
            /// The final charge of the state, this is also the damage coefficient.
            /// </summary>
            float charge { get; set; }
        }
    }
}