using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using RoR2.UI;
using SS2.Components;
namespace SS2.UI
{
    public class PickupCarouselPanel : MonoBehaviour
    {
        public RectTransform carousel;
        public float elementDistance = 250f;
        public GameObject elementPrefab;
        public float totalRotation = 180f;
        public AnimationCurve rotationSpeedCurve;
        public float duration = 2f;
        private UIElementAllocator<ChildLocator> allocator;
        private PickupIndex[] pickupOptions;
        [NonSerialized]
        public PickupCarouselController controller;
        private int winner;
        private float stopwatch;
        private float rotationSpeed;
        private bool hasFaded;
        public UnityEvent onWinnerSelected;
        public float fadeDuration = 1.3f;
        private void Awake()
        {
            allocator = new UIElementAllocator<ChildLocator>(carousel, this.elementPrefab, true, false);  // ?
        }
        private void Start()
        {
            float baseRps = totalRotation / duration;
            float thing = 1 / SS2Util.IntegrateCurve(rotationSpeedCurve, 0, 1, 100);
            rotationSpeed = baseRps * thing;
        }
        public void SetOptions(PickupIndex[] options, int rewardIndex)
        {
            this.pickupOptions = options;
            winner = rewardIndex;
            this.allocator.AllocateElements(options.Length);
            ReadOnlyCollection<ChildLocator> elements = allocator.elements;
            float angleBetweenElements = 360f / elements.Count;
            for(int i = 0; i < elements.Count; i++)
            {
                PickupDef pickupDef = PickupCatalog.GetPickupDef(options[i]);
                ChildLocator element = elements[i];
                float angle = Mathf.Deg2Rad * angleBetweenElements * i;
                element.transform.localPosition = new Vector3(Mathf.Cos(angle) * elementDistance, Mathf.Sin(angle) * elementDistance, 0); ///////////////
                Image visual = element.FindChild("Icon").GetComponent<Image>();
                visual.sprite = pickupDef.iconSprite;
            }
        }
        private void Update()
        {
            stopwatch += Time.deltaTime;
            if (!hasFaded && stopwatch < duration)
            {                
                AddRotation(rotationSpeedCurve.Evaluate(stopwatch / duration) * rotationSpeed * Time.deltaTime);
            }
            else
            {
                FadeOut();
                allocator.elements[winner].transform.localScale = Vector3.one * Mathf.Lerp(stopwatch / fadeDuration, 1, 1.25f);
                if(stopwatch > fadeDuration)
                {
                    Destroy(base.gameObject);
                }
            }
        }

        public void AddRotation(float rotation)
        {
            carousel.transform.Rotate(0, 0, rotation);
            for(int i = 0; i < allocator.elements.Count; i++)
            {
                allocator.elements[i].transform.rotation = Quaternion.identity; // im over it
            }
        }

        public void FadeOut()
        {
            if(!hasFaded)
            {
                stopwatch = 0;
                hasFaded = true;
                for (int i = 0; i < allocator.elements.Count; i++)
                {
                    if (i != winner)
                        allocator.elements[i].GetComponent<UIJuice>().TransitionAlphaFadeOut();
                    else
                        allocator.elements[i].FindChild("HoverOutline").gameObject.SetActive(true);
                }
                onWinnerSelected?.Invoke();
            }           
        }

        public void OnSkip()
        {
            if(!hasFaded)
            {
                FadeOut();
            }
            else
            {
                Destroy(base.gameObject);
            }

        }
    }
}
