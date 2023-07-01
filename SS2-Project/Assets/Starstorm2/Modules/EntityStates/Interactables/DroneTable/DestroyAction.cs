using EntityStates.DroneTable;
using Moonstorm.Starstorm2;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace EntityStates.DroneTable
{
    public class DestroyAction : DroneTableBaseState
    {

        public static float duration;

        public GameObject droneObject;

        public static Xoroshiro128Plus droneTableRNG;

        public float value;

        protected override bool enableInteraction
        {
            get
            {
                return false;
            }
        }

        // Start is called before the first frame update
        public override void OnEnter()
        {
            SS2Log.Info("entered destroy action");
            PlayCrossfade("Main", "Action", "Action.playbackRate", duration, 0.05f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge > duration)
                outer.SetNextStateToMain();
        }

        // Update is called once per frame
        public override void OnExit()
        {
            SS2Log.Info("destroy action finished");
            base.OnExit();
            Vector3 vector = Quaternion.AngleAxis(0, Vector3.up) * (Vector3.up * 20f);
            DroneTableDropTable dropTable = new DroneTableDropTable();
            if (droneTableRNG == null)
            {
                droneTableRNG = new Xoroshiro128Plus(Run.instance.seed);
            }
            PickupIndex ind = dropTable.GenerateDropPreReplacement(droneTableRNG, (int)Mathf.Sqrt(value));
            PickupDropletController.CreatePickupDroplet(ind, this.gameObject.transform.position, vector);
            //outer.SetNextStateToMain();
        }


    }

    public class DroneTableDropTable : BasicPickupDropTable
    {
        private void AddNew(List<PickupIndex> sourceDropList, float listWeight)
        {
            if (listWeight <= 0f || sourceDropList.Count == 0)
            {
                return;
            }
            float weight = listWeight / (float)sourceDropList.Count;
            foreach (PickupIndex value in sourceDropList)
            {
                selector.AddChoice(value, weight);
            }
        }

        public PickupIndex GenerateDropPreReplacement(Xoroshiro128Plus rng, int count)
        {
            selector.Clear();
            AddNew(Run.instance.availableTier1DropList, tier1Weight);
            AddNew(Run.instance.availableTier2DropList, tier2Weight * (float)count);
            AddNew(Run.instance.availableTier3DropList, tier3Weight * Mathf.Pow((float)count, 1.75f)); //this is basically the shipping request code but with a slightly lower red weight scaling

            return PickupDropTable.GenerateDropFromWeightedSelection(rng, selector);
        }

        public override int GetPickupCount()
        {
            return selector.Count;
        }

        public override PickupIndex[] GenerateUniqueDropsPreReplacement(int maxDrops, Xoroshiro128Plus rng)
        {
            return PickupDropTable.GenerateUniqueDropsFromWeightedSelection(maxDrops, rng, selector);
        }

        new private float tier1Weight = .79f; //.316f;

        new private float tier2Weight = .20f; //.08f;

        new private float tier3Weight = .01f; //.004f;

        new private readonly WeightedSelection<PickupIndex> selector = new WeightedSelection<PickupIndex>(8);
    }

}
