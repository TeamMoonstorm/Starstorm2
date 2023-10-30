using UnityEngine;


namespace Moonstorm.Starstorm2.Components
{
    public class EnableRandomOnStart : MonoBehaviour // not networked. just used for visuals (nemmerc hologram poses)
    {
        public Transform[] transforms;
        private void Start()
        {
            int index = Mathf.FloorToInt(Random.Range(0, transforms.Length - 1));
            transforms[index].gameObject.SetActive(true);
        }
    }
}
