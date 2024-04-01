//using static SS2.Interactables.DroneTable;

namespace EntityStates.DroneTable
{
    public class Idle : DroneTableBaseState
    {

        protected override bool enableInteraction
        {
            get
            {
                return true;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            var scrapRoot = this.gameObject.transform.Find("ScrapRoot").gameObject;
            scrapRoot.SetActive(false);
        }
    }
}
