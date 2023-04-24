using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Equipments
{
    //[DisabledContent]
    public sealed class MIDAS : EquipmentBase
    {
        public override EquipmentDef EquipmentDef { get; } = SS2Assets.LoadAsset<EquipmentDef>("MIDAS", SS2Bundle.Equipments);
        public float goldEarned;

        //★ There's probably a way to do this involving an item behavior. Let me know about it.
        //N Nah, this looks good.
        public override bool FireAction(EquipmentSlot slot)
        {
            int playerCount = 0;
            foreach (var player in PlayerCharacterMasterController.instances)
            {
                playerCount++;
            }

            var level = slot.characterBody.level;

            float commandoHealth = .5f * (110 + (33 * (level - 1)));
            float healthLost = slot.characterBody.healthComponent.health * 0.5f;
            float chestFraction = healthLost / commandoHealth;
            //SS2Log.Debug("chest fraction: " + chestFraction);

            goldEarned = Run.instance.GetDifficultyScaledCost((int)(25 * chestFraction));

            goldEarned = Mathf.Max(goldEarned, healthLost);
            //SS2Log.Debug("gold : " + goldEarned);
            //goldEarned = slot.characterBody.healthComponent.health * 0.5f * (1.3f * (playerCount - 1));
            DamageInfo damageInfo = new DamageInfo()
            {
                damage = slot.characterBody.healthComponent.health * 0.5f,
                damageType = DamageType.BypassArmor,
                damageColorIndex = DamageColorIndex.Item,
                inflictor = slot.characterBody.gameObject,
                position = slot.characterBody.healthComponent.gameObject.transform.position,
            };
            slot.characterBody.healthComponent.TakeDamage(damageInfo);
            if (!slot.characterBody.isPlayerControlled && slot.characterBody.teamComponent.teamIndex == TeamIndex.Player)
            {
                //SS2Log.Debug("is not player controled");
                uint splitAmount = (uint)(goldEarned / playerCount);
                //SS2Log.Debug("is not player controlled, giving " + splitAmount + " to all players");
                foreach (var player in PlayerCharacterMasterController.instances)
                {
                    player.master.GiveMoney(splitAmount);
                }
            }
            else
            {
                //SS2Log.Debug("is player controled@!!!!!");
                slot.characterBody.master.GiveMoney((uint)goldEarned);
            }
            
            return true;
        }
    }

}
