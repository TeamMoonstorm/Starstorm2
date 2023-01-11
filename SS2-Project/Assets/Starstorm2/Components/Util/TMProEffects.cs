using System.Linq;
using TMPro;
using UnityEngine;

namespace Moonstorm.Starstorm2
{
    //ty for permission to use this Mystic :)
    public static class TMProEffects
    {
        public static void Init()
        {
            On.RoR2.UI.ChatBox.Start += ChatBox_Start;
            On.RoR2.UI.HGTextMeshProUGUI.Awake += HGTextMeshProUGUI_Awake;
        }

        private static void ChatBox_Start(On.RoR2.UI.ChatBox.orig_Start orig, RoR2.UI.ChatBox self)
        {
            orig(self);
            var component = self.messagesText.textComponent.GetComponent<SS2TextEffects>();
            if (!component)
            {
                component = self.messagesText.textComponent.gameObject.AddComponent<SS2TextEffects>();
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
                if (textComponent && textComponent.isActiveAndEnabled)
                {
                    textComponent.ForceMeshUpdate();
                }
            }

            public void OnEnable()
            {
                TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
            }

            public void OnDisable()
            {
                TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
            }

            public void ON_TEXT_CHANGED(Object obj)
            {
                if (obj == textComponent)
                    textChanged = true;
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

                        foreach (var link in textInfo.linkInfo.Where(x => x.GetLinkID() == "textWavy"))
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
                                for (var j = 0; j <= 3; j++)
                                {
                                    destVerts[charInfo.vertexIndex + j] = origVerts[charInfo.vertexIndex + j] + charOffset;
                                }
                            }
                        }

                        foreach (var link in textInfo.linkInfo.Where(x => x.GetLinkID() == "textShaky"))
                        {
                            for (int i = link.linkTextfirstCharacterIndex; i < link.linkTextfirstCharacterIndex + link.linkTextLength; i++)
                            {
                                var charInfo = textInfo.characterInfo[i];
                                if (!charInfo.isVisible) continue;

                                anythingChanged = true;

                                var shakeAmount = 6f * charInfo.scale;
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
                    }
                }
            }
        }
    }
}