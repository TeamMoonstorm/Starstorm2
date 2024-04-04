using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using MSU;
using System.Collections;
using RoR2.ContentManagement;

namespace SS2.Equipments
{
    public sealed class PressurizedCanister : SS2Equipment, IContentPackModifier
    {
        public override EquipmentDef EquipmentDef => _equipmentDef;
        private EquipmentDef _equipmentDef;
        public BuffDef _canJumpBuffDef;

        public override NullableRef<GameObject> ItemDisplayPrefab => throw new System.NotImplementedException();

        public override bool Execute(EquipmentSlot slot)
        {
            var characterMotor = slot.characterBody.characterMotor;
            if (characterMotor)
            {
                slot.characterBody.AddItemBehavior<Behavior>(1);
                Debug.Log($"Adding behavior");
                //if (slot.hasAuthority)
                {
                } //this is so fucking true
                return true;
            }
            return false;
        }

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            ParallelAssetLoadCoroutineHelper helper = new ParallelAssetLoadCoroutineHelper();

            helper.AddAssetToLoad<EquipmentDef>("PressurizedCanister", SS2Bundle.Equipments);
            helper.AddAssetToLoad<BuffDef>("bdCanJump", SS2Bundle.Equipments);

            helper.Start();
            while (!helper.IsDone()) yield return null;

            _equipmentDef = helper.GetLoadedAsset<EquipmentDef>("PressurizedCanister");
            _canJumpBuffDef = helper.GetLoadedAsset<BuffDef>("BuffDef");
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.buffDefs.AddSingle(_canJumpBuffDef);
        }

        public sealed class PressurizedCanisterBehavior : BuffBehaviour

        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdCanJump;
            public void Start()
            {
                CharacterBody.baseJumpCount++;
                CharacterBody.characterMotor.onHitGroundAuthority += RemoveBuff;
            }

            public void FixedUpdate()
            {
                if (CharacterBody.inputBank.jump.justPressed) // server doesnt see client inputs so the buff cant be removed by the server. not gonna bother. too lazy. nemmerc must release.
                {
                    EffectManager.SimpleEffect(SS2Assets.LoadAsset<GameObject>("canExhaust", SS2Bundle.Equipments), CharacterBody.transform.position, CharacterBody.transform.rotation, false);
                    CharacterBody.RemoveBuff(SS2Content.Buffs.bdCanJump.buffIndex);
                }
            }

            private void RemoveBuff(ref CharacterMotor.HitGroundInfo hitGroundInfo)
            {
                CharacterBody.RemoveBuff(SS2Content.Buffs.bdCanJump.buffIndex);
            }

            public void OnDestroy()
            {
                CharacterBody.characterMotor.onHitGroundAuthority -= RemoveBuff;
                CharacterBody.baseJumpCount--;
            }
        }


        //notice how this is not added by this.AddItemBehavior() but the Fire Action
        public sealed class Behavior : CharacterBody.ItemBehavior
        {
            private static float duration = 0.8f;
            private static float thrustForce = 90f;
            private static int effectSpawnTotal = 10;


            private float stopwatch;
            private int effectInterval;
            private CharacterMotor characterMotor;

            private void Start()
            {
                characterMotor = body.characterMotor;
                if (characterMotor.isGrounded)
                    EffectManager.SimpleEffect(Resources.Load<GameObject>("prefabs/effects/SmokescreenEffect"), body.footPosition, Quaternion.identity, true);
                if (NetworkServer.active)
                {
                    characterMotor.Motor.ForceUnground();
                    EffectManager.SimpleEffect(SS2Assets.LoadAsset<GameObject>("canExhaust", SS2Bundle.Equipments), body.transform.position, body.transform.rotation, true);
                    body.AddBuff(SS2Content.Buffs.bdCanJump.buffIndex);
                    body.characterMotor.Jump(1.2f, 2.2f, false); //it would be so awesome 
                }
            }

            private void FixedUpdate()
            {
                stopwatch += Time.fixedDeltaTime;

                //Gets removed if you switch equipments
                if (stopwatch > duration || body.equipmentSlot.equipmentIndex != EquipmentCatalog.FindEquipmentIndex("PressurizedCanister"))
                    Destroy(this);
            }
        }
    }
}
