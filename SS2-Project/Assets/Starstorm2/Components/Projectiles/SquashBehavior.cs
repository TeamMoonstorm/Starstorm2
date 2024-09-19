using RoR2;
using RoR2.Projectile;
using System.Collections;
using UnityEngine;

namespace SS2.Components
{
    [RequireComponent(typeof(ProjectileController))]
    public class SquashBehavior : MonoBehaviour, IOnDamageInflictedServerReceiver
    {

        public void OnDamageInflictedServer(DamageReport damageReport)
        {
            CharacterBody victim = damageReport.victimBody;

            if (victim)
            {
                victim.gameObject.AddComponent<SquashedComponent>().speed = 5f;
            }
        }

        public class SquashedComponent : MonoBehaviour
        {
            public float speed = 5f;
            private Vector3 originalScale;

            private void Awake()
            {
                originalScale = transform.localScale;
                transform.localScale = new Vector3(1.25f * transform.localScale.x, 0.05f * transform.localScale.y, 1.25f * transform.localScale.z);

                StartCoroutine("EndSquash");
            }

            IEnumerator EndSquash()
            {
                yield return new WaitForSeconds(2f);

                float t = 0f;
                while (t < 1f)
                {
                    t += speed * Time.deltaTime;
                    transform.localScale = Vector3.Lerp(transform.localScale, originalScale, t);

                    yield return 0;
                }

                transform.localScale = originalScale;
                Destroy(this);

                yield return null;
            }
        }
    }
}
