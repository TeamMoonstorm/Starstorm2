using UnityEngine;

namespace EntityStates.Runshroom
{
    public class RushroomDeath : GenericCharacterDeath
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Transform trail = FindModelChild("TrailEffect");
            if(trail)
            {
                trail.gameObject.SetActive(false);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        public override void OnExit()
        {
            
            base.OnExit();
        }
    }
}
