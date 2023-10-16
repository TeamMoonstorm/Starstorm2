using UnityEngine;


namespace Moonstorm.Starstorm2.Components
{
    public class EnableRandomOnStart : MonoBehaviour
    {
        public Transform[] transforms;
        private void Start()
        {
            int index = Mathf.FloorToInt(Random.Range(0, transforms.Length - 1));
            transforms[index].gameObject.SetActive(true);
        }
    }
}
