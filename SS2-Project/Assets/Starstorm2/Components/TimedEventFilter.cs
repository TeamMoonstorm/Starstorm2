using UnityEngine;
namespace SS2.Components
{
    //make this real later
    public class TimedEventFilter : MonoBehaviour
    {
        private void Awake()
        {
            base.GetComponent<Renderer>().enabled = SS2Main.ChristmasTime;
        }
    }
}
