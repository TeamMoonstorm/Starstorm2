using System.Linq;
using TMPro;
using UnityEngine;
using RoR2;
using System.Globalization;
namespace SS2
{
    //ty for permission to use this Mystic :)

    internal static class TMProEffects
    {
        private static bool enabled = true;

        [SystemInitializer]
        private static void Init()
        {
            On.RoR2.UI.ChatBox.Start += ChatBox_Start;
            On.RoR2.UI.HGTextMeshProUGUI.Awake += HGTextMeshProUGUI_Awake;
            On.RoR2.Chat.UserChatMessage.ConstructChatString += UserChatMessage_ConstructChatString;
        }

        // stops vanilla from stopping chat messages from using tmpro rich text
        private static string UserChatMessage_ConstructChatString(On.RoR2.Chat.UserChatMessage.orig_ConstructChatString orig, Chat.UserChatMessage self)
        {
            if (self.sender)
            {
                NetworkUser component = self.sender.GetComponent<NetworkUser>();
                if (component)
                {
                    return string.Format(CultureInfo.InvariantCulture, "<color=#e5eefc>{0}: {1}</color>", Util.EscapeRichTextForTextMeshPro(component.userName), self.text);
                }
            }
            return orig(self);
        }

        [ConCommand(commandName = "debug_toggle_texteffects", flags = ConVarFlags.Cheat, helpText = "Enables/disables TMPro effects. Requires HUD reset. Format: {shouldEnable}")]
        public static void CCDebugToggleHooks(ConCommandArgs args)
        {
            bool shouldEnable = args.GetArgBool(0);
            if (shouldEnable && !enabled)
            {
                enabled = true;
                SS2Log.Debug("Enabled SS2TextEffects. Requires HUD restart.");
                On.RoR2.UI.ChatBox.Start += ChatBox_Start;
                On.RoR2.UI.HGTextMeshProUGUI.Awake += HGTextMeshProUGUI_Awake;
            }
            else if (!shouldEnable)
            {
                enabled = false;
                SS2Log.Debug("Disabled SS2TextEffects. Requires HUD restart.");
                On.RoR2.UI.ChatBox.Start -= ChatBox_Start;
                On.RoR2.UI.HGTextMeshProUGUI.Awake -= HGTextMeshProUGUI_Awake;
            }
        }
        private static void ChatBox_Start(On.RoR2.UI.ChatBox.orig_Start orig, RoR2.UI.ChatBox self)
        {
            orig(self);
            var component = self.messagesText.textComponent.GetComponent<SS2TextEffects>();
            if (!component)
            {
                component = self.messagesText.textComponent.gameObject.AddComponent<SS2TextEffects>();
                component.textComponent = self.messagesText.textComponent; // TODO: TEST THIS idk if it works
            }
        }

        private static void HGTextMeshProUGUI_Awake(On.RoR2.UI.HGTextMeshProUGUI.orig_Awake orig, RoR2.UI.HGTextMeshProUGUI self)
        {
            orig(self);
            var component = self.GetComponent<SS2TextEffects>();
            if (!component)
            {
                component = self.gameObject.AddComponent<SS2TextEffects>();
                component.textComponent = self;
            }
        }

        public class SS2TextEffects : MonoBehaviour
        {
            public TMP_Text textComponent;
            public bool textChanged;
            public TMP_MeshInfo[] cachedMeshInfo;

            public float updateTimer = 0f;
            public float updateFrequency = 0.016f;

            public void Awake()
            {
                textComponent = GetComponent<TMP_Text>();
                textChanged = true;
            }

            public void Start()
            {
                // idk what this does but it was spitting out errors on startup
                //if (textComponent && textComponent.isActiveAndEnabled)
                //{
                //    textComponent.ForceMeshUpdate(true, false);
                //}
                TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
            }
            private void OnDestroy()
            {
                TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
            }

            private void OnDisable()
            {
                
            }

            public void ON_TEXT_CHANGED(Object obj)
            {
                if (obj == textComponent)
                {
                    textChanged = true;
                    // dumb fucking hack. textmeshpro is buggy as shit. wont work if both old and new text have links 
                    // if you make a new effect make sure you also check for it here ----->
                    if(this.enabled && textComponent != null && textComponent.textInfo != null && textComponent.textInfo.linkInfo != null && textComponent.text != null && !textComponent.text.Contains("textWavy") && !textComponent.text.Contains("textShaky"))
                    {
                        this.textComponent.textInfo.linkInfo = new TMPro.TMP_LinkInfo[0]; // fuck tmpro wtf is this. linkinfos never get cleared when text changes
                        this.enabled = false;
                        return;
                    }
                    
                    this.enabled = true;
                }
                    
                
            }

            public void Update()
            {
                updateTimer -= Time.deltaTime;
                while (updateTimer <= 0f)
                {
                    updateTimer += updateFrequency;
                    if (textComponent && textComponent.isActiveAndEnabled)
                    {
                        var textInfo = textComponent.textInfo;

                        if (textInfo == null || textInfo.meshInfo == null || textInfo.meshInfo.Length <= 0 || textInfo.meshInfo[0].vertices == null) return;

                        if (textChanged)
                        {
                            textChanged = false;
                            cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
                        }

                        var anythingChanged = false;

                        foreach(var link in textInfo.linkInfo)
                        {
                            if (link.GetLinkID().Equals("textWavy"))
                            {
                                for (int i = link.linkTextfirstCharacterIndex; i < link.linkTextfirstCharacterIndex + link.linkTextLength; i++)
                                {
                                    var charInfo = textInfo.characterInfo[i];
                                    if (!charInfo.isVisible) continue;

                                    anythingChanged = true;

                                    var waveAmount = 6f * charInfo.scale;
                                    var origVerts = cachedMeshInfo[charInfo.materialReferenceIndex].vertices;
                                    var origColors = cachedMeshInfo[charInfo.materialReferenceIndex].colors32;
                                    var destVerts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                                    var destColors = textInfo.meshInfo[charInfo.materialReferenceIndex].colors32;
                                    var charOffset = new Vector3(0f, Mathf.Sin(Time.time * 8f + 0.22f * i) * waveAmount, 0f);
                                    for (var k = 0; k <= 3; k++)
                                    {
                                        destVerts[charInfo.vertexIndex + k] = origVerts[charInfo.vertexIndex + k] + charOffset;
                                    }
                                }
                            }
                            
                            if (link.GetLinkID().Equals("textShaky"))
                            {
                                for (int i = link.linkTextfirstCharacterIndex; i < link.linkTextfirstCharacterIndex + link.linkTextLength; i++)
                                {
                                    var charInfo = textInfo.characterInfo[i];
                                    if (!charInfo.isVisible) continue;

                                    anythingChanged = true;

                                    var shakeAmount = 3f * charInfo.scale;
                                    var origVerts = cachedMeshInfo[charInfo.materialReferenceIndex].vertices;
                                    var origColors = cachedMeshInfo[charInfo.materialReferenceIndex].colors32;
                                    var destVerts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                                    var destColors = textInfo.meshInfo[charInfo.materialReferenceIndex].colors32;
                                    var charOffset = new Vector3(Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount), 0f);
                                    for (var j = 0; j <= 3; j++)
                                    {
                                        destVerts[charInfo.vertexIndex + j] = origVerts[charInfo.vertexIndex + j] + charOffset;
                                    }
                                }
                            }
                            
                        }

                        if (anythingChanged)
                        {
                            for (var i = 0; i < textInfo.meshInfo.Length; i++)
                            {
                                var meshInfo = textInfo.meshInfo[i];
                                meshInfo.mesh.vertices = meshInfo.vertices;
                                meshInfo.mesh.colors32 = meshInfo.colors32;
                                textComponent.UpdateGeometry(meshInfo.mesh, i);
                                textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                            }
                        }
                        else
                        {
                            this.enabled = false; // for performance 
                        }
                    }
                }
            }
        }
    }
}