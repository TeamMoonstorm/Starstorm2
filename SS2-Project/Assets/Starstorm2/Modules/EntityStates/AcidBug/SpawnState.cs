using UnityEngine;

namespace EntityStates.AcidBug
{
    public class SpawnState : GenericCharacterSpawnState
    {
        public override void OnEnter()
        {
            base.OnEnter();

            Transform wingFx = FindModelChild("WingFX");
            if (wingFx)
            {
                wingFx.gameObject.SetActive(true);
            }

            Transform wingMesh = FindModelChild("WingMesh");
            if (wingMesh)
            {
                wingMesh.gameObject.SetActive(false);
            }
        }
    }
}
