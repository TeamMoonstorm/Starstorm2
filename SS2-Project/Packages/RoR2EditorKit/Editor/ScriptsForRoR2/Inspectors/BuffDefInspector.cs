using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using RoR2EditorKit.Core.Inspectors;
using RoR2EditorKit.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static RoR2EditorKit.Utilities.AssetDatabaseUtils;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(RoR2.BuffDef))]
    public class BuffDefInspector : ScriptableObjectInspector<BuffDef>
    {
        private EliteDef eliteDef;
        private List<IMGUIContainer> eliteDefMessages = new List<IMGUIContainer>();

        private NetworkSoundEventDef networkSoundEventDef;
        private IMGUIContainer networkSoundEventdefMessage = null;

        VisualElement header = null;
        VisualElement inspectorData = null;
        VisualElement messages = null;

        VisualElement buffColor = null;

        private Button objectNameSetter = null;
        protected override void OnEnable()
        {
            base.OnEnable();
            eliteDef = TargetType.eliteDef;
            networkSoundEventDef = TargetType.startSfx;
            prefix = Settings.GetPrefix1stUpperRestLower();
            prefixUsesTokenPrefix = true;

            OnVisualTreeCopy += () =>
            {
                header = Find<VisualElement>("Header");
                inspectorData = Find<VisualElement>("InspectorData");
                messages = Find<VisualElement>("Messages");
                buffColor = Find<ColorField>(inspectorData, "buffColor");
                Find<Button>(buffColor, "colorSetter").clicked += () => TargetType.buffColor = eliteDef.color;
            };
        }
        protected override void DrawInspectorGUI()
        {
            var label = Find<Label>(header, "m_Name");
            label.RegisterValueChangedCallback((cb) => EnsureNamingConventions(cb));

            Find<ObjectField>(inspectorData, "iconSprite").SetObjectType<Sprite>();

            var eliteDef = Find<ObjectField>(inspectorData, "eliteDef");
            eliteDef.SetObjectType<EliteDef>();
            eliteDef.RegisterValueChangedCallback(CheckEliteDef);
            CheckEliteDef();

            var startSfx = Find<ObjectField>(inspectorData, "startSfx");
            startSfx.SetObjectType<NetworkSoundEventDef>();
            startSfx.RegisterValueChangedCallback(CheckSoundEvent);
            CheckSoundEvent();
        }

        private void CheckSoundEvent(ChangeEvent<UnityEngine.Object> evt = null)
        {
            if(networkSoundEventdefMessage != null)
            {
                networkSoundEventdefMessage.TryRemoveFromParent();
            }

            if (!networkSoundEventDef)
                return;

            if(networkSoundEventDef.eventName.IsNullOrEmptyOrWhitespace())
            {
                networkSoundEventdefMessage = CreateHelpBox($"You've associated a NetworkSoundEventDef ({networkSoundEventDef.name}) to this buff, but the EventDef's eventName is Null, Empty or Whitespace!", MessageType.Warning);
                messages.Add(networkSoundEventdefMessage);
            }
        }

        private void CheckEliteDef(ChangeEvent<UnityEngine.Object> evt = null)
        {
            var button = Find<Button>(buffColor, "colorSetter");
            foreach (IMGUIContainer container in eliteDefMessages)
            {
                if (container != null)
                    container.TryRemoveFromParent();
            }
            eliteDefMessages.Clear();

            if (!eliteDef)
            {
                button.style.display = DisplayStyle.None;
                return;
            }
            button.style.display = DisplayStyle.Flex;

            IMGUIContainer msg = null;
            if(!eliteDef.eliteEquipmentDef)
            {
                msg = CreateHelpBox($"You've associated an EliteDef ({eliteDef.name}) to this buff, but the EliteDef has no EquipmentDef assigned!", MessageType.Warning);
                messages.Add(msg);
                eliteDefMessages.Add(msg);
            }

            if(eliteDef.eliteEquipmentDef && !eliteDef.eliteEquipmentDef.passiveBuffDef)
            {
                msg = CreateHelpBox($"You've associated an EliteDef ({eliteDef.name}) to this buff, but the assigned EliteDef's EquipmentDef ({eliteDef.eliteEquipmentDef.name})'s \"passiveBuffDef\" is not asigned!", MessageType.Warning);
                messages.Add(msg);
                eliteDefMessages.Add(msg);
            }

            if(eliteDef.eliteEquipmentDef && eliteDef.eliteEquipmentDef.passiveBuffDef != TargetType)
            {
                msg = CreateHelpBox($"You've associated an EliteDef ({eliteDef.name}) to this buff, but the assigned EliteDef's EquipmentDef ({eliteDef.eliteEquipmentDef.name})'s \"passiveBuffDef\" is not the inspected BuffDef!", MessageType.Warning);
                messages.Add(msg);
                eliteDefMessages.Add(msg);
            }
        }

        protected override IMGUIContainer EnsureNamingConventions(ChangeEvent<string> evt = null)
        {
            IMGUIContainer container = base.EnsureNamingConventions();
            if (container != null)
            {
                container.style.alignItems = Align.FlexEnd;
                container.style.paddingBottom = 20;
                container.name += "_NamingConvention";
                if(InspectorEnabled)
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
            UpdateNameOfObject(TargetType);
        }
    }
}