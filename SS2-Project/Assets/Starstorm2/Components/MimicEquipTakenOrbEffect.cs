using System.Collections;
using RoR2;
using SS2;
using UnityEngine;

namespace Starstorm2.Components
{
    [RequireComponent(typeof(EffectComponent))]
    public class MimicEquipTakenOrbEffect : MonoBehaviour
    {
        public TrailRenderer trailToColor;

        public ParticleSystem[] particlesToColor;

        public SpriteRenderer[] spritesToColor;

        public SpriteRenderer iconSpriteRenderer;

        private void OnEnable()
        {
            StartCoroutine(DelayedUpdateSprite());
        }

        private IEnumerator DelayedUpdateSprite()
        {
            yield return 0;
            EquipmentDef equipDef = EquipmentCatalog.GetEquipmentDef((EquipmentIndex)Util.UintToIntMinusOne(GetComponent<EffectComponent>().effectData.genericUInt));
            SS2Log.Debug($"equip def null ? {equipDef == null} {GetComponent<EffectComponent>()}");
            SS2Log.Debug($"equip def null ? {equipDef == null} {GetComponent<EffectComponent>().effectData}");
            SS2Log.Debug($"equip def null ? {equipDef.pickupIconSprite} {GetComponent<EffectComponent>().effectData.genericUInt}");
            ColorCatalog.ColorIndex colorIndex = ColorCatalog.ColorIndex.Error;
            Sprite sprite = null;
            if (equipDef != null)
            {
                colorIndex = equipDef.colorIndex;
                sprite = equipDef.pickupIconSprite;
            }
            Color color = ColorCatalog.GetColor(colorIndex);
            if (trailToColor != null)
            {
                trailToColor.startColor *= color;
                trailToColor.endColor *= color;
            }
            for (int i = 0; i < particlesToColor.Length; i++)
            {
                ParticleSystem obj = particlesToColor[i];
                ParticleSystem.MainModule main = obj.main;
                main.startColor = color;
                obj.Play();
            }
            for (int j = 0; j < spritesToColor.Length; j++)
            {
                spritesToColor[j].color = color;
            }
            iconSpriteRenderer.sprite = equipDef.pickupIconSprite;
        }
    }
}