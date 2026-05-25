using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RoR2;
using RoR2.UI;

namespace SS2.UI
{
    // we want to insert the weather bar between the difficulty bar and objective panel
    // find objective panel (and anything below it), then animate it moving downwards
    public class WeatherBarAnimator : MonoBehaviour
    {
        public static void AddChild(Transform child)
        {
            childrenToMove.Add(child.name);
        }
        public static void AddChild(string childName)
        {
            childrenToMove.Add(childName);
        }
        private static readonly List<string> childrenToMove = new List<string>
        {
            "RightInfoBar",
        };

        private Transform root;
        private ChildLocator rootChildLocator;
        private CanvasGroup juiceCanvasGroup;
        private ChildLocator childLocator;
        public struct ChildInfo
        {
            public Vector3 originalPosition;
            public Vector3 targetPosition;
            public RectTransform rectTransform;
        }
        private List<ChildInfo> targetChildren;

        private RectTransform rectTransform;

        
        public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0.5f, 0f, 1f, 1f);

        public AnimationCurve childMovementCurve = AnimationCurve.EaseInOut(0f, 0f, .5f, 1f);
        private static float defaultRightInfoBarY = -90.5f;
        private static float childTargetY = -150f; // values r from what looked good in editor
        private float childDeltaY => childTargetY - defaultRightInfoBarY;
        public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, .5f, 1f);
        private static float scaleHeightStart = -16f;
        private static float scaleHeightEnd = 18f;

        private static float animDuration = 3f;

        private float endLerpTime;
        private float startLerpTime;
        private float lerpTimeScale;
        private float lerpTime = 1f;
        private float currentTime = -1f;
        private bool barActive;
        private void Awake()
        {
            root = transform.parent;
            rectTransform = (RectTransform)transform;
            rootChildLocator = root.GetComponent<ChildLocator>();
            targetChildren = new List<ChildInfo>();

            for (int i = 0; i < childrenToMove.Count; i++)
            {
                string childName = childrenToMove[i];
                RectTransform child = (RectTransform)rootChildLocator?.FindChild(childName);
                if (!child)
                {
                    child = (RectTransform)root.Find(childName);
                }
                if (child)
                {
                    var childInfo = new ChildInfo
                    {
                        rectTransform = child,
                        originalPosition = child.anchoredPosition,
                        targetPosition = child.anchoredPosition + new Vector2(0, childDeltaY),
                    };
                    targetChildren.Add(childInfo);
                }
            }

            childLocator = GetComponent<ChildLocator>();
            if (childLocator)
            {
                Transform juice = childLocator.FindChild("Juice");
                if (juice)
                {
                    juiceCanvasGroup = juice.GetComponent<CanvasGroup>();
                }
            }

            SetAnimationTime(0f);
        }

        private void OnDisable()
        {
            SetAnimationTime(0f);
        }
        private void FixedUpdate()
        {
            if (StormController.instance)
            {
                // ???? ?
                if (StormController.instance.shouldActivateHud)
                {
                    if (!barActive)
                    {
                        barActive = true;
                        StartLerp(1f, animDuration);
                    }
                }
                else
                {
                    if (barActive)
                    {
                        barActive = false;
                        StartLerp(0f, animDuration);
                    }
                    
                }
            }
        }
        public void StartLerp(float newTargetTime, float lerpDuration)
        {
            endLerpTime = newTargetTime;
            if (lerpDuration > 0)
            {
                startLerpTime = currentTime;
                lerpTimeScale = 1 / lerpDuration;
                lerpTime = 0;
                return;
            }
            lerpTime = 1;
            lerpTimeScale = 0;
        }

        private void Update()
        {
            lerpTime += Time.deltaTime * lerpTimeScale;
            if (lerpTime <= 1)
            {
                SetAnimationTime(Mathf.Lerp(startLerpTime, endLerpTime, lerpTime));
            }
            else
            {
                SetAnimationTime(endLerpTime);
            }
        }

        private void SetAnimationTime(float time)
        {
            if (time == currentTime)
            {
                return;
            }
            currentTime = time;

            if (rectTransform)
            {
                float curvedTime = scaleCurve.Evaluate(time);

                Vector2 sizeDelta = rectTransform.sizeDelta;
                sizeDelta.y = Mathf.Lerp(scaleHeightStart, scaleHeightEnd, curvedTime);
                rectTransform.sizeDelta = sizeDelta;
            }

            for (int i = 0; i < targetChildren.Count; i++)
            {
                var child = targetChildren[i];
                float curvedTime = childMovementCurve.Evaluate(time);
                if (child.rectTransform)
                {
                    child.rectTransform.anchoredPosition = Vector3.Lerp(child.originalPosition, child.targetPosition, curvedTime);
                }
            }

            if (juiceCanvasGroup)
            {
                float curvedTime = alphaCurve.Evaluate(time);
                juiceCanvasGroup.alpha = curvedTime * 1.0f;
            }
        }

    }
}
