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
                scrollValue = StormController.instance.currentLevel + StormController.instance.levelFractionComplete;
                calcScroll?.Invoke(ref scrollValue);
            }
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
                float num = Mathf.Clamp01(segmentImageAnimation.age / segmentImageAnimation.duration);
                segmentImageAnimation.Update(num);
                if (num >= 1f)
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
            float num = (float)(segmentDefs.Length + 2);
            if (segmentScroll > num)
            {
                segmentScroll = num - 1f + (segmentScroll - Mathf.Floor(segmentScroll));
            }
            scrollXRaw = (segmentScroll - 1f) * -elementWidth;
            scrollX = Mathf.Floor(scrollXRaw);
            int num2 = currentSegmentIndex;
            currentSegmentIndex = Mathf.Clamp(Mathf.FloorToInt(segmentScroll), 0, segmentContainer.childCount - 1);
            if (num2 != currentSegmentIndex)
            {
                OnCurrentSegmentIndexChanged(currentSegmentIndex);
            }
            Vector2 offsetMin = segmentContainer.offsetMin;
            offsetMin.x = scrollX;
            segmentContainer.offsetMin = offsetMin;
            if (segmentContainer && segmentContainer.childCount > 0)
            {
                int num3 = segmentContainer.childCount - 1;
                RectTransform rectTransform = (RectTransform)segmentContainer.GetChild(num3);
                RectTransform rectTransform2 = (RectTransform)rectTransform.Find("Label");
                TextMeshProUGUI textMeshProUGUI = labels[num3];
                if (segmentScroll >= (float)(num3 - 1))
                {
                    float num4 = elementWidth;
                    Vector2 offsetMin2 = rectTransform.offsetMin;
                    offsetMin2.x = CalcSegmentStartX(num3);
                    rectTransform.offsetMin = offsetMin2;
                    Vector2 offsetMax = rectTransform.offsetMax;
                    offsetMax.x = offsetMin2.x + num4;
                    rectTransform.offsetMax = offsetMax;
                    rectTransform2.anchorMin = new Vector2(0f, 0f);
                    rectTransform2.anchorMax = new Vector2(0f, 1f);
                    rectTransform2.offsetMin = new Vector2(0f, 0f);
                    rectTransform2.offsetMax = new Vector2(elementWidth, 0f);
                    return;
                }
                rectTransform.offsetMax = rectTransform.offsetMin + new Vector2(elementWidth, 0f);
                SetLabelDefaultDimensions(rectTransform2);
            }
        }
        private void OnCurrentSegmentIndexChanged(int newSegmentIndex)
        {
            if (!Application.isPlaying)
            {
                return;
            }
            int num = newSegmentIndex - 1;
            float width = viewPort.rect.width;
            int i = 0;
            int num2 = images.Length - 1;
            while (i < num2)
            {
                Image image = images[i];
                TextMeshProUGUI textMeshProUGUI = labels[i];
                RectTransform rectTransform = image.rectTransform;
                bool enabled = rectTransform.offsetMax.x + scrollX >= 0f && rectTransform.offsetMin.x + scrollX <= width;
                image.enabled = enabled;
                textMeshProUGUI.enabled = enabled;
                i++;
            }
            int num3 = images.Length - 1;
            Image image2 = images[num3];
            TextMeshProUGUI textMeshProUGUI2 = labels[num3];
            bool enabled2 = image2.rectTransform.offsetMax.x + scrollX >= 0f;
            image2.enabled = enabled2;
            textMeshProUGUI2.enabled = enabled2;
            for (int j = 0; j <= num; j++)
            {
                images[j].color = ColorMultiplySaturationAndValue(ref segmentDefs[j].color, pastSaturationMultiplier, pastValueMultiplier);
                labels[j].color = pastLabelColor;
            }
            for (int k = newSegmentIndex + 1; k < images.Length; k++)
            {
                images[k].color = ColorMultiplySaturationAndValue(ref segmentDefs[k].color, upcomingSaturationMultiplier, upcomingValueMultiplier);
                labels[k].color = upcomingLabelColor;
            }
            Image image3 = (num != -1) ? images[num] : null;
            Image image4 = (newSegmentIndex != -1) ? images[newSegmentIndex] : null;
            TextMeshProUGUI textMeshProUGUI3 = (newSegmentIndex != -1) ? labels[newSegmentIndex] : null;
            if (image3)
            {
                playingAnimations.Add(new SegmentImageAnimation
                {
                    age = 0f,
                    duration = fadeAnimationDuration,
                    segmentImage = image3,
                    colorCurve = fadeAnimationCurve,
                    color0 = segmentDefs[num].color,
                    color1 = ColorMultiplySaturationAndValue(ref segmentDefs[num].color, pastSaturationMultiplier, pastValueMultiplier)
                });
            }
            Color color = ColorMultiplySaturationAndValue(ref segmentDefs[newSegmentIndex].color, currentSaturationMultiplier, currentValueMultiplier);
            if (image4)
            {
                playingAnimations.Add(new SegmentImageAnimation
                {
                    age = 0f,
                    duration = flashAnimationDuration,
                    segmentImage = image4,
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
            if (textMeshProUGUI3)
            {
                textMeshProUGUI3.color = currentLabelColor;
            }

            // Added weather icon assignment
            if (weatherIcon)
            {
                int level = newSegmentIndex;
                calcIconLevel?.Invoke(ref level);

                if(StormController.instance)
                {
                    weatherIcon.sprite = StormController.instance.GetWeatherIcon(level);
                }
            }
        }

        private float CalcSegmentStartX(int i)
        {
            return (float)i * elementWidth;
        }
        private float CalcSegmentEndX(int i)
        {
            return (float)(i + 1) * elementWidth;
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
            uint num = (uint)segmentContainer.childCount;
            if (images == null || (long)images.Length != (long)((ulong)desiredCount))
            {
                images = new Image[desiredCount];
                labels = new TextMeshProUGUI[desiredCount];
            }
            int i = 0;
            int num2 = Mathf.Min(images.Length, segmentContainer.childCount);
            while (i < num2)
            {
                Transform child = segmentContainer.GetChild(i);
                images[i] = child.GetComponent<Image>();
                labels[i] = child.Find("Label").GetComponent<TextMeshProUGUI>();
                i++;
            }
            while (num > desiredCount)
            {
                UnityEngine.Object.DestroyImmediate(segmentContainer.GetChild((int)(num - 1U)).gameObject);
                num -= 1U;
            }
            while (num < desiredCount)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(segmentPrefab, segmentContainer);
                gameObject.SetActive(true);
                images[i] = gameObject.GetComponent<Image>();
                labels[i] = gameObject.transform.Find("Label").GetComponent<TextMeshProUGUI>();
                i++;
                num += 1U;
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
            TextMeshProUGUI[] array = segmentTransform.GetComponentsInChildren<TextMeshProUGUI>();
            int num = 4;
            if (array.Length < num)
            {
                TextMeshProUGUI[] array2 = new TextMeshProUGUI[num];
                for (int i = 0; i < array.Length; i++)
                {
                    array2[i] = array[i];
                }
                for (int j = array.Length; j < num; j++)
                {
                    array2[j] = UnityEngine.Object.Instantiate<GameObject>(array[0].gameObject, segmentTransform).GetComponent<TextMeshProUGUI>();
                }
                array = array2;
            }
            int k = 0;
            int num2 = array.Length;
            while (k < num2)
            {
                TextMeshProUGUI textMeshProUGUI = array[k];
                textMeshProUGUI.enableWordWrapping = false;
                textMeshProUGUI.overflowMode = TextOverflowModes.Overflow;
                textMeshProUGUI.alignment = TextAlignmentOptions.MidlineLeft;
                textMeshProUGUI.text = Language.GetString(segmentDefs[segmentDefs.Length - 1].token);
                textMeshProUGUI.enableAutoSizing = true;
                Vector3 localPosition = textMeshProUGUI.transform.localPosition;
                localPosition.x = (float)k * elementWidth;
                textMeshProUGUI.transform.localPosition = localPosition;
                k++;
            }
            segmentTransform.GetComponent<Image>().sprite = finalSegmentSprite;
        }
        #endregion
    }
}
