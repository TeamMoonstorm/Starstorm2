using UnityEngine;
namespace SS2.Components
{
    public class ResetRotation :MonoBehaviour
    {
        [SerializeField]
        private bool onupdate;

        private void Start()
        {
            transform.rotation = Quaternion.identity;
        }

        private void Update()
        {
            if (onupdate)
            {
                transform.rotation = Quaternion.identity;
            }
        }
    }
}