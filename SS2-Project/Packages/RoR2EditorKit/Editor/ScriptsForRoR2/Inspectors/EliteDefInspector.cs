using RoR2;
using RoR2EditorKit.Core.Inspectors;
using RoR2EditorKit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(EliteDef))]
    public class EliteDefInspector : ScriptableObjectInspector<EliteDef>
    {
        private EquipmentDef equipmentDef;
        private List<IMGUIContainer> equipDefMessages = new List<IMGUIContainer>();

        VisualElement header = null;
        VisualElement inspectorData = null;
        VisualElement messages = null;

        ColorField color = null;

        Button objectNameSetter = null;

        protected override void OnEnable()
        {
            base.OnEnable();
            equipmentDef = TargetType.eliteEquipmentDef;
            prefix = Settings.GetPrefix1stUpperRestLower() + "Affix";
            prefixUsesTokenPrefix = true;

            OnVisualTreeCopy += () =>
            {
                header = Find<VisualElement>("Header");
                inspectorData = Find<VisualElement>("InspectorData");
                color = Find<ColorField>(inspectorData, "color");
                messages = Find<VisualElement>("Messages");
            };
        }

        protected override void DrawInspectorGUI()
        {
            var label = Find<Label>(header, "m_Name");
            label.RegisterValueChangedCallback((cb) => EnsureNamingConventions(cb));

            Find<Button>(inspectorData, "tokenSetter").clicked += SetTokens;

            color = Find<ColorField>(inspectorData, "color");
            Find<Button>(color, "colorSetter").clicked += SetColor;

            var equipDef = Find<PropertyField>(inspectorData, "eliteEquipmentDef");
            equipDef.RegisterCallback<ChangeEvent<EquipmentDef>>(CheckEquipDef);
            CheckEquipDef();
        }

        private void CheckEquipDef(ChangeEvent<EquipmentDef> evt = null)
        {
            var button = Find<Button>(color, "colorSetter");
            foreach(IMGUIContainer container in equipDefMessages)
            {
                if (container != null)
                    container.TryRemoveFromParent();
            }
            equipDefMessages.Clear();

            IMGUIContainer msg = null;
            if(!equipmentDef)
            {
                msg = CreateHelpBox("This EliteDef has no EquipmentDef assigned! Is this intentional?", MessageType.Info);
                messages.Add(msg);
                equipDefMessages.Add(msg);
                return;
            }


            if(!equipmentDef.passiveBuffDef)
            {
                msg = CreateHelpBox($"You've assigned an EquipmentDef ({equipmentDef.name}) to this Elite, but the assigned Equipment's has no passiveBuffDef assigned!", MessageType.Warning);
                messages.Add(msg);
                equipDefMessages.Add(msg);
            }


            if(equipmentDef.passiveBuffDef && equipmentDef.passiveBuffDef.eliteDef != TargetType)
            {
                msg = CreateHelpBox($"You've associated an EquipmentDef ({equipmentDef.name}) to this Elite, but the assigned EquipmentDef's \"passiveBuffDef\" ({equipmentDef.passiveBuffDef.name})'s EliteDef is not the inspected EliteDef!", MessageType.Warning);
                messages.Add(msg);
                equipDefMessages.Add(msg);
            }

            button.style.display = (equipmentDef.passiveBuffDef && equipmentDef.passiveBuffDef.eliteDef == TargetType) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SetColor()
        {
            if(equipmentDef && equipmentDef.passiveBuffDef && equipmentDef.passiveBuffDef.eliteDef == TargetType)
            {
                TargetType.color = equipmentDef.passiveBuffDef.buffColor;
            }
        }

        private void SetTokens()
        {
            if(Settings.TokenPrefix.IsNullOrEmptyOrWhitespace())
                throw ErrorShorthands.ThrowNullTokenPrefix();

            string objName = TargetType.name.ToLowerInvariant();
            if(objName.Contains(prefix.ToLowerInvariant()))
            {
                objName = objName.Replace(prefix.ToLowerInvariant(), "");
            }
            TargetType.modifierToken = $"{Settings.GetPrefixUppercase()}_AFFIX_{objName.ToUpperInvariant()}";
        }

        protected override IMGUIContainer EnsureNamingConventions(ChangeEvent<string> evt = null)
        {
            IMGUIContainer container =  base.EnsureNamingConventions(evt);
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
