using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Moonstorm.Starstorm2.Components
{
    [RequireComponent(typeof(ObjectScaleCurve))]
    public class AuguryEffectController : MonoBehaviour
    {
        [Serializable]
        public class AugurySphere
        {
            public GameObject sphereObject;

            private Color OriginalColor { get; set; }
            private Vector3 OriginalScale { get; set; }
            public Color GetColor() => sphereObject.GetComponent<MeshRenderer>().material.GetColor("_TintColor");
            public void SetColor(Color color) => sphereObject.GetComponent<MeshRenderer>().material.SetColor("_TintColor", color);

            public void Awake()
            {
                OriginalColor = GetColor();
                OriginalScale = sphereObject.transform.localScale;
            }

            public void RestoreDefault()
            {
                SetColor(OriginalColor);
                sphereObject.transform.localScale = OriginalScale;
            }
        }
        public ObjectScaleCurve explosionScaleCurve;

        public List<AugurySphere> spheres;

        public Vector3 scale;

        [HideInInspector]
        public bool fadeOut;

        private void Awake()
        {
            foreach (AugurySphere sphere in spheres)
            {
                sphere.Awake();
            }
        }
        private void Update()
        {
            if (fadeOut)
            {
                foreach (AugurySphere sphere in spheres)
                {
                    var previousColor = sphere.GetColor();
                    float alpha = previousColor.a - (0.5f * Time.deltaTime) < 0 ? 0 : previousColor.a - (0.5f * Time.deltaTime);
                    var newColor = new Color(previousColor.r, previousColor.g, previousColor.b, alpha);
                    sphere.SetColor(newColor);
                }
                if (AllSpheresFaded())
                {
                    fadeOut = false;
                }
            }
        }

        private bool AllSpheresFaded()
        {
            foreach (var sphere in spheres)
            {
                if (sphere.GetColor().a <= 0)
                    continue;
                else
                    return false;
            }
            return true;
        }

        public void UpdateSize(Vector3 newScale)
        {
            if (gameObject.transform.localScale.x < newScale.x)
            {
                gameObject.transform.localScale = Vector3.Lerp(gameObject.transform.localScale, newScale, Mathf.Clamp01(newScale.x - gameObject.transform.localScale.x) * Time.deltaTime);
            }
            else if (gameObject.transform.localScale.x > newScale.x)
            {
                gameObject.transform.localScale = newScale;
            }
        }

        public void BeginScale(Vector3 scale, float duration)
        {
            explosionScaleCurve.baseScale = scale;
            explosionScaleCurve.timeMax = duration;
            explosionScaleCurve.enabled = true;
        }

        public void RestoreDefaults()
        {
            UpdateSize(Vector3.zero);
            foreach (AugurySphere sphere in spheres)
            {
                sphere.RestoreDefault();
            }
        }
    }
}
