using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RoR2;
using RoR2.UI;

namespace SS2.UI
{
    // copypasted DifficultyBarController, we just give it our own scroll value instead of run difficulty
    // added comments to our changes or additions
    [ExecuteAlways]
    public class WeatherBarController : MonoBehaviour
    {
        [Header("Component References")]
        public RectTransform viewPort;
        public RectTransform segmentContainer;
        public Image difficultyBarBackdrop;

        [Header("Layout")]
        [Tooltip("How wide each segment should be.")]
        public float elementWidth;
        public float levelsPerSegment;
        public float debugTime;

        [Header("Segment Parameters")]
        public SegmentDef[] segmentDefs;
        [Tooltip("The prefab to instantiate for each segment.")]
        public GameObject segmentPrefab;

        [Header("Colors")]
        public float pastSaturationMultiplier;
        public float pastValueMultiplier;
        public Color pastLabelColor;
        public float currentSaturationMultiplier;
        public float currentValueMultiplier;
        public Color currentLabelColor;
        public float upcomingSaturationMultiplier;
        public float upcomingValueMultiplier;
        public Color upcomingLabelColor;
        [Tooltip("If the increasedDifficultyBarFX user setting is enabled and the next difficulty color's value is below  flash white instead.")]
        public float backdropFlashWhiteThreshold = 0.43f;

        [Header("Animations")]
        public AnimationCurve fadeAnimationCurve;
        public float fadeAnimationDuration = 1f;
        public AnimationCurve flashAnimationCurve;
        public float flashAnimationDuration = 0.5f;
        private int currentSegmentIndex = -1;
        private static readonly Color labelFadedColor = Color.Lerp(Color.gray, Color.white, 0.5f);

        [SerializeField]
        private string onDifficultyChangeSoundString = "Play_wDifficulty_New";

        // Added noise and weather icon values
        [Header("Noise")]
        public bool enableNoise = true;
        public float perlinNoiseCutoffMax = 1f;
        public float perlinNoiseCutoffMin;
        public float perlinNoiseStrength;
        public float perlinNoiseFrequency;
        public float perlinY = 6.9f;
        [Header("Weather Icon")]
        public Image weatherIcon;

        [Header("Final Segment")]
        public Sprite finalSegmentSprite;
        public float maxFinalScroll = 0.63f; // added max scroll, instead of looping

        private HUD hud;
        private float scrollX;
        private float scrollXRaw;
        private float lastSegmentScroll;

        [Tooltip("Do not set manually. Regenerate the children instead.")]
        public Image[] images;

        [Tooltip("Do not set manually. Regenerate the children instead.")]
        public TextMeshProUGUI[] labels;

        public RawImage[] wormGearImages;

        public float UVScaleToScrollX;

        public float gearUVOffset;

        private readonly List<SegmentImageAnimation> playingAnimations = new List<SegmentImageAnimation>();

        [Serializable]
        public struct SegmentDef
        {
            [Tooltip("The default English string to use for the element at design time.")]
            public string debugString;

            [Tooltip("The final language token to use for element at runtime.")]
            public string token;

            [Tooltip("The color to use for the panel.")]
            public Color color;

            [Tooltip("The icon to use for the panel.")]
            public Sprite iconSprite;
        }
        
        // Added hooks
        public delegate void CalcNoiseDelegate(ref NoiseValues noise);
        public static event CalcNoiseDelegate calcNoise;

        public delegate void CalcScrollDelegate(ref float scroll);
        public static event CalcScrollDelegate calcScroll;

        public delegate void CalcIconLevelDelegate(ref int level);
        public static event CalcIconLevelDelegate calcIconLevel;
        public struct NoiseValues
        {
            public float cutoffMax;
            public float cutoffMin;
            public float strength;
            public float frequency;
        }
        private float noiseAge;


        private class SegmentImageAnimation
        {
            public void Update(float t)
            {
                segmentImage.color = Color.Lerp(color0, color1, colorCurve.Evaluate(t));
            }

            public Image segmentImage;
            public float age;
            public float duration;
            public AnimationCurve colorCurve;
            public Color color0;
            public Color color1;
        }
        private void Awake()
        {
            hud = GetComponentInParent<HUD>();
            SetupSegments();
        }
        private void Update()
        {
            DoBarUpdates(Time.deltaTime);
            UpdateGears();
        }
        private void DoBarUpdates(float deltaTime)
        {
            // copy-paste of DifficultyBarController, except we pass our own value into SetSegmentScroll and add noise
            float scrollValue = debugTime;
            if (StormController.instance)
            {
                scrollValue = StormController.instance.currentLevel + StormController.instance.chargeInCurrentLevel;
                calcScroll?.Invoke(ref scrollValue);
            }
            float maxScrollValue = segmentDefs.Length - 1 + maxFinalScroll;
            scrollValue = Mathf.Min(scrollValue, maxScrollValue);
            if (enableNoise)
            {
                NoiseValues noiseValues = new NoiseValues
                {
                    cutoffMax = perlinNoiseCutoffMax,
                    cutoffMin = perlinNoiseCutoffMin,
                    frequency = perlinNoiseFrequency,
                    strength = perlinNoiseStrength,
                };
                calcNoise?.Invoke(ref noiseValues);

                noiseAge += deltaTime * noiseValues.frequency;
                float noise = Mathf.PerlinNoise(noiseAge, perlinY);
                noise = Util.Remap(noise, noiseValues.cutoffMin, noiseValues.cutoffMax, -1f, 1f);
                noise *= noiseValues.strength;

                scrollValue += noise;
            }

            if (!Application.isPlaying)
            {
                SetupSegments();
            }

            SetSegmentScroll(scrollValue);

            if (Application.isPlaying)
            {
                RunAnimations(deltaTime);
            }
        }

        private void UpdateGears()
        {
            foreach (RawImage rawImage in wormGearImages)
            {
                Rect uvRect = rawImage.uvRect;
                float num = Mathf.Sign(uvRect.width);
                uvRect.x = scrollXRaw * UVScaleToScrollX * num + ((num < 0f) ? gearUVOffset : 0f);
                rawImage.uvRect = uvRect;
            }
        }

        private void RunAnimations(float deltaTime)
        {
            for (int i = playingAnimations.Count - 1; i >= 0; i--)
            {
                SegmentImageAnimation segmentImageAnimation = playingAnimations[i];
                segmentImageAnimation.age += deltaTime;
                float t = Mathf.Clamp01(segmentImageAnimation.age / segmentImageAnimation.duration);
                segmentImageAnimation.Update(t);
                if (t >= 1f)
                {
                    playingAnimations.RemoveAt(i);
                }
            }
        }
        private void SetSegmentScroll(float segmentScroll)
        {
            if (segmentScroll == lastSegmentScroll)
            {
                return;
            }
            lastSegmentScroll = segmentScroll;

            // this handles looping the final segment. we need to do something else because we dont want "Heavy Storm" looping forever
            //float maxScroll = segmentDefs.Length + 2;
            //if (segmentScroll > maxScroll)
            //{
            //    segmentScroll = maxScroll - 1f + (segmentScroll - Mathf.Floor(segmentScroll));
            //}
            //

            scrollXRaw = (segmentScroll - 1f) * -elementWidth;
            scrollX = Mathf.Floor(scrollXRaw);

            int oldSegmentIndex = currentSegmentIndex;
            currentSegmentIndex = Mathf.Clamp(Mathf.FloorToInt(segmentScroll), 0, segmentContainer.childCount - 1);
            if (oldSegmentIndex != currentSegmentIndex)
            {
                OnCurrentSegmentIndexChanged(currentSegmentIndex);
            }
            Vector2 offsetMin = segmentContainer.offsetMin;
            offsetMin.x = scrollX;
            segmentContainer.offsetMin = offsetMin;
            if (segmentContainer && segmentContainer.childCount > 0)
            {
                int finalSegmentIndex = segmentContainer.childCount - 1;
                RectTransform rectTransform = (RectTransform)segmentContainer.GetChild(finalSegmentIndex);
                RectTransform rectTransform2 = (RectTransform)rectTransform.Find("Label");
                TextMeshProUGUI textMeshProUGUI = labels[finalSegmentIndex];
                if (segmentScroll >= finalSegmentIndex - 1)
                {
                    float num4 = elementWidth;
                    Vector2 offsetMin2 = rectTransform.offsetMin;
                    offsetMin2.x = CalcSegmentStartX(finalSegmentIndex);
                    rectTransform.offsetMin = offsetMin2;
                    Vector2 offsetMax = rectTransform.offsetMax;
                    offsetMax.x = offsetMin2.x + num4;
                    rectTransform.offsetMax = offsetMax;
                    rectTransform2.anchorMin = new Vector2(0f, 0f);
                    rectTransform2.anchorMax = new Vector2(0f, 1f);
                    rectTransform2.offsetMin = new Vector2(0f, 0f);
                    rectTransform2.offsetMax = new Vector2(elementWidth, 0f);
                }
                else
                {
                    rectTransform.offsetMax = rectTransform.offsetMin + new Vector2(elementWidth, 0f);
                    SetLabelDefaultDimensions(rectTransform2);
                }
            }
        }
        private void OnCurrentSegmentIndexChanged(int newSegmentIndex)
        {
            if (!Application.isPlaying)
            {
                return;
            }
            int oldSegmentIndex = newSegmentIndex - 1;
            float width = viewPort.rect.width;

            for (int i = 0, iEnd = images.Length - 1; i < iEnd; i++)
            {
                Image image = images[i];
                TextMeshProUGUI label = labels[i];
                RectTransform rectTransform = image.rectTransform;
                bool enabled = rectTransform.offsetMax.x + scrollX >= 0f && rectTransform.offsetMin.x + scrollX <= width;
                //image.enabled = enabled;
                //label.enabled = enabled;
                image.enabled = true; // vanilla disables segments if the scroll isnt far enough. but we do some weird offset stuff for the weather bar so that looks funky.
                                      // cba to figure out the math for this. making UI is fucked without playmode in editor.
                                      // i dont know why vanilla even does this anyways, segments are behind a ui mask already
                label.enabled = true;
            }
            {
                int finalSegment = images.Length - 1;
                Image finalImage = images[finalSegment];
                TextMeshProUGUI finalLabel = labels[finalSegment];
                bool enabled = finalImage.rectTransform.offsetMax.x + scrollX >= 0f;
                //finalImage.enabled = enabled;
                //finalLabel.enabled = enabled;
                finalImage.enabled = true; // same as above
                finalLabel.enabled = true;
            }
            
            for (int i = 0; i <= oldSegmentIndex; i++)
            {
                images[i].color = ColorMultiplySaturationAndValue(ref segmentDefs[i].color, pastSaturationMultiplier, pastValueMultiplier);
                labels[i].color = pastLabelColor;
            }
            for (int i = newSegmentIndex + 1; i < images.Length; i++)
            {
                images[i].color = ColorMultiplySaturationAndValue(ref segmentDefs[i].color, upcomingSaturationMultiplier, upcomingValueMultiplier);
                labels[i].color = upcomingLabelColor;
            }
            Image oldImage = (oldSegmentIndex != -1) ? images[oldSegmentIndex] : null;
            Image newImage = (newSegmentIndex != -1) ? images[newSegmentIndex] : null;
            TextMeshProUGUI newLabel = (newSegmentIndex != -1) ? labels[newSegmentIndex] : null;
            if (oldImage)
            {
                playingAnimations.Add(new SegmentImageAnimation
                {
                    age = 0f,
                    duration = fadeAnimationDuration,
                    segmentImage = oldImage,
                    colorCurve = fadeAnimationCurve,
                    color0 = segmentDefs[oldSegmentIndex].color,
                    color1 = ColorMultiplySaturationAndValue(ref segmentDefs[oldSegmentIndex].color, pastSaturationMultiplier, pastValueMultiplier)
                });
            }
            Color color = ColorMultiplySaturationAndValue(ref segmentDefs[newSegmentIndex].color, currentSaturationMultiplier, currentValueMultiplier);
            if (newImage)
            {
                playingAnimations.Add(new SegmentImageAnimation
                {
                    age = 0f,
                    duration = flashAnimationDuration,
                    segmentImage = newImage,
                    colorCurve = flashAnimationCurve,
                    color0 = color,
                    color1 = Color.white
                });
            }
            if (hud && difficultyBarBackdrop)
            {
                // removed this check
                //UserProfile userProfile = hud.localUserViewer.userProfile;
                //if (userProfile != null && userProfile.increasedDifficultyBarFX)
                {
                    Color.RGBToHSV(segmentDefs[newSegmentIndex].color, out float hue, out float saturation, out float value);
                    playingAnimations.Add(new SegmentImageAnimation
                    {
                        age = 0f,
                        duration = flashAnimationDuration,
                        segmentImage = difficultyBarBackdrop,
                        colorCurve = flashAnimationCurve,
                        color0 = difficultyBarBackdrop.color,
                        color1 = ((value < backdropFlashWhiteThreshold) ? Color.white : color)
                    });
                    if (newSegmentIndex > 0)
                    {
                        Util.PlaySound(onDifficultyChangeSoundString, RoR2Application.instance.gameObject);
                    }
                }
            }
            if (newLabel)
            {
                newLabel.color = currentLabelColor;
            }

            // Added weather icon assignment
            if (weatherIcon)
            {
                int level = newSegmentIndex;
                calcIconLevel?.Invoke(ref level);

                weatherIcon.sprite = segmentDefs[level].iconSprite;
            }
        }

        private float CalcSegmentStartX(int i)
        {
            return i * elementWidth;
        }
        private float CalcSegmentEndX(int i)
        {
            return (i + 1) * elementWidth;
        }
        private void SetLabelDefaultDimensions(RectTransform labelRectTransform)
        {
            labelRectTransform.anchorMin = new Vector2(0f, 0f);
            labelRectTransform.anchorMax = new Vector2(1f, 1f);
            labelRectTransform.pivot = new Vector2(0.5f, 0.5f);
            labelRectTransform.offsetMin = new Vector2(0f, 0f);
            labelRectTransform.offsetMax = new Vector2(0f, 0f);
        }
        private static Color ColorMultiplySaturationAndValue(ref Color col, float saturationMultiplier, float valueMultiplier)
        {
            float h;
            float num;
            float num2;
            Color.RGBToHSV(col, out h, out num, out num2);
            return Color.HSVToRGB(h, num * saturationMultiplier, num2 * valueMultiplier);
        }

        // TODO: need to derive segments from StormController, since it will change based on the map and difficulty 
        #region Segment Setup
        private void SetupSegments()
        {
            if (!segmentContainer || !segmentPrefab)
            {
                return;
            }
            SetSegmentCount((uint)segmentDefs.Length);
            for (int i = 0; i < segmentContainer.childCount; i++)
            {
                SetupSegment((RectTransform)segmentContainer.GetChild(i), ref segmentDefs[i], i);
            }
            SetupFinalSegment((RectTransform)segmentContainer.GetChild(segmentContainer.childCount - 1));
        }
        private void SetSegmentCount(uint desiredCount)
        {
            if (!segmentContainer || !segmentPrefab)
            {
                return;
            }
            uint childCount = (uint)segmentContainer.childCount;

            if (!Application.isPlaying)
            {
                images = null;
                labels = null;
            }

            if (images == null || images.Length != desiredCount)
            {
                images = new Image[desiredCount];
                labels = new TextMeshProUGUI[desiredCount];
            }
            int imageIndex = 0;
            int imageCount = Mathf.Min(images.Length, segmentContainer.childCount);
            for (; imageIndex < imageCount; ++imageIndex)
            {
                Transform child = segmentContainer.GetChild(imageIndex);
                images[imageIndex] = child.GetComponent<Image>();
                labels[imageIndex] = child.Find("Label").GetComponent<TextMeshProUGUI>();
            }
            while (childCount > desiredCount)
            {
                UnityEngine.Object.DestroyImmediate(segmentContainer.GetChild((int)(childCount - 1)).gameObject);
                childCount--;
            }
            while (childCount < desiredCount)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(segmentPrefab, segmentContainer);
                gameObject.SetActive(true);
                images[imageIndex] = gameObject.GetComponent<Image>();
                labels[imageIndex] = gameObject.transform.Find("Label").GetComponent<TextMeshProUGUI>();
                imageIndex++;
                childCount++;
            }
        }
        private void SetupSegment(RectTransform segmentTransform, ref SegmentDef segmentDef, int i)
        {
            Vector2 offsetMin = segmentTransform.offsetMin;
            Vector2 offsetMax = segmentTransform.offsetMax;
            offsetMin.x = CalcSegmentStartX(i);
            offsetMax.x = CalcSegmentEndX(i);
            segmentTransform.offsetMin = offsetMin;
            segmentTransform.offsetMax = offsetMax;
            segmentTransform.GetComponent<Image>().color = segmentDef.color;


            // added debug string usage
            string labelToken = segmentDef.token;
            if (!Application.isPlaying)
            {
                labelToken = segmentDef.debugString;
            }
            ((RectTransform)segmentTransform.Find("Label")).GetComponent<LanguageTextMeshController>().token = labelToken;
        }

        private void SetupFinalSegment(RectTransform segmentTransform)
        {
            segmentTransform.GetComponent<Image>().sprite = finalSegmentSprite;
        }

        // copied from vanilla. this loops the text in the final segment, which we dont want
        //private void SetupFinalSegment(RectTransform segmentTransform)
        //{
        //    TextMeshProUGUI[] labels = segmentTransform.GetComponentsInChildren<TextMeshProUGUI>();
        //    int maxLabels = 4;
        //    if (labels.Length < maxLabels)
        //    {
        //        TextMeshProUGUI[] newLabels = new TextMeshProUGUI[maxLabels];
        //        for (int i = 0; i < labels.Length; i++)
        //        {
        //            newLabels[i] = labels[i];
        //        }
        //        for (int j = labels.Length; j < maxLabels; j++)
        //        {
        //            newLabels[j] = UnityEngine.Object.Instantiate<GameObject>(labels[0].gameObject, segmentTransform).GetComponent<TextMeshProUGUI>();
        //        }
        //        labels = newLabels;
        //    }
        //    for (int i = 0, iEnd = labels.Length; i < iEnd; ++i)
        //    {
        //        TextMeshProUGUI textMeshProUGUI = labels[i];
        //        textMeshProUGUI.enableWordWrapping = false;
        //        textMeshProUGUI.overflowMode = TextOverflowModes.Overflow;
        //        textMeshProUGUI.alignment = TextAlignmentOptions.MidlineLeft;
        //        textMeshProUGUI.text = Language.GetString(segmentDefs[segmentDefs.Length - 1].token);
        //        textMeshProUGUI.enableAutoSizing = true;
        //        Vector3 localPosition = textMeshProUGUI.transform.localPosition;
        //        localPosition.x = i * elementWidth;
        //        textMeshProUGUI.transform.localPosition = localPosition;
        //        k++;
        //    }
        //    segmentTransform.GetComponent<Image>().sprite = finalSegmentSprite;
        //}
        #endregion
    }
}
