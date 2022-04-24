using RoR2;
using RoR2EditorKit.Core.Inspectors;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System;
using System.Collections;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using RoR2EditorKit.Utilities;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(EquipmentDef))]
    public class EquipmentDefInspector : ScriptableObjectInspector<EquipmentDef>
    {
        IMGUIContainer cooldownMsg;

        FloatField field;

        BuffDef passiveBuffDef;
        IMGUIContainer buffMessage;

        bool DoesNotAppear => (!TargetType.appearsInMultiPlayer && !TargetType.appearsInSinglePlayer);
        IMGUIContainer notAppearMessage;

        VisualElement header = null;
        VisualElement inspectorData = null;
        VisualElement messages = null;
        VisualElement tokenHolder = null;

        Slider slider = null;

        Button objectNameSetter;
        protected override void OnEnable()
        {
            base.OnEnable();
            passiveBuffDef = TargetType.passiveBuffDef;
            prefix = Settings.GetPrefix1stUpperRestLower();
            prefixUsesTokenPrefix = true;

            OnVisualTreeCopy += () =>
            {
                header = Find<VisualElement>("Header");
                inspectorData = Find<VisualElement>("InspectorData");
                slider = Find<Slider>(inspectorData, "dropOnDeathChance");
                field = Find<FloatField>(slider, "floatField");
                Find<Button>(slider, "chanceSetter").clicked += () => TargetType.dropOnDeathChance = 0.00025f;
                tokenHolder = Find<Foldout>(inspectorData, "tokenFoldout").Q<VisualElement>("tokenHolder");

                messages = Find<VisualElement>("Messages");
            };
        }
        protected override void DrawInspectorGUI()
        {
            var label = Find<Label>(header, "m_Name");
            label.RegisterValueChangedCallback((cb) => EnsureNamingConventions(cb));

            Find<PropertyField>(inspectorData, "cooldown").RegisterCallback<ChangeEvent<float>>(OnCooldownSet);
            OnCooldownSet();

            Find<PropertyField>(inspectorData, "passiveBuffDef").RegisterCallback<ChangeEvent<BuffDef>>(OnBuffDefSet);
            OnBuffDefSet();

            slider.RegisterValueChangedCallback(OnDropChanceSet);
            Find<FloatField>(slider, "floatField").RegisterValueChangedCallback(OnDropChanceSet);

            Find<PropertyField>(inspectorData, "appearsInSinglePlayer").RegisterCallback<ChangeEvent<bool>>(OnXPlayerToggle);
            Find<PropertyField>(inspectorData, "appearsInMultiPlayer").RegisterCallback<ChangeEvent<bool>>(OnXPlayerToggle);
            OnXPlayerToggle();

            Find<Button>(tokenHolder, "tokenSetter").clicked += SetTokens;
        }

        private void OnDropChanceSet(ChangeEvent<float> evt = null)
        {
            float value = Mathf.Clamp01(evt.newValue);
            TargetType.dropOnDeathChance = value;
            field.value = (float)Math.Round((decimal)value, 5);
        }

        private void OnXPlayerToggle(ChangeEvent<bool> evt = null)
        {
            if(notAppearMessage != null)
            {
                notAppearMessage.TryRemoveFromParent();
            }

            if(DoesNotAppear)
            {
                notAppearMessage = CreateHelpBox("You've set both \"Appears in SinglePlayer\" and \"Appears in MultiPlayer\" false! The equipment will not appear!", MessageType.Warning);
                messages.Add(notAppearMessage);
            }
        }

        private void OnCooldownSet(ChangeEvent<float> evt = null)
        {
            if(cooldownMsg != null)
            {
                cooldownMsg.TryRemoveFromParent();
            }
            if(TargetType.cooldown < 0)
            {
                cooldownMsg = CreateHelpBox("You've set cooldown to a negative value! this is not allowed!", MessageType.Error);
                messages.Add(cooldownMsg);
            }
        }

        private void OnBuffDefSet(ChangeEvent<BuffDef> evt = null)
        {
            var button = Find<Button>(slider, "chanceSetter");
            buffMessage?.TryRemoveFromParent();

            if (!passiveBuffDef)
                return;

            if(passiveBuffDef.eliteDef && passiveBuffDef.eliteDef.eliteEquipmentDef != TargetType)
            {
                buffMessage = CreateHelpBox($"You've assigned a BuffDef ({TargetType.passiveBuffDef.name}) to this EquipmentDef, And the BuffDef has an EliteDef ({TargetType.passiveBuffDef.eliteDef.name}), But said EliteDef's Elite Equipment Def is not the inspected EquipmentDef!", MessageType.Warning);
                messages.Add(buffMessage);
                Find<Button>(slider, "chanceSetter").style.display = DisplayStyle.None;
            }
            else
            {
                Find<Button>(slider, "chanceSetter").style.display = DisplayStyle.Flex;
            }
        }

        private void SetTokens()
        {
            if (Settings.TokenPrefix.IsNullOrEmptyOrWhitespace())
                throw ErrorShorthands.ThrowNullTokenPrefix();

            string objName = TargetType.name.ToLowerInvariant();
            if (objName.Contains(prefix.ToLowerInvariant()))
            {
                objName = objName.Replace(prefix.ToLowerInvariant(), "");
            }
            string tokenBase = $"{Settings.GetPrefixUppercase()}_EQUIP_{objName.ToUpperInvariant()}_";
            TargetType.nameToken = $"{tokenBase}NAME";
            TargetType.pickupToken = $"{tokenBase}PICKUP";
            TargetType.descriptionToken = $"{tokenBase}DESC";
            TargetType.loreToken = $"{tokenBase}LORE";
        }

        protected override IMGUIContainer EnsureNamingConventions(ChangeEvent<string> evt = null)
        {
            IMGUIContainer container = base.EnsureNamingConventions(evt);
            if (container != null)
            {
                container.style.alignItems = Align.FlexEnd;
                container.style.paddingBottom = 20;
                container.name += "_NamingConvention";
                if (InspectorEnabled)
                {
                    objectNameSetter = new Button(SetObjectName);
                    objectNameSetter.name = "objectNameSetter";
                    objectNameSetter.text = "Fix Naming Convention";
                    container.Add(objectNameSetter);
                    RootVisualElement.Add(container);
                    container.SendToBack();
                }
                else
                {
                    RootVisualElement.Add(container);
                    container.SendToBack();
                }
            }
            else if (objectNameSetter != null)
            {
                objectNameSetter.TryRemoveFromParent();
            }

            return null;
        }

        private void SetObjectName()
        {
            var origName = TargetType.name;
            TargetType.name = prefix + origName;
            AssetDatabaseUtils.UpdateNameOfObject(TargetType);
        }
    }
}