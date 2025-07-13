using UnityEngine;
using RoR2;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.AddressableAssets;

namespace SS2.Components
{
    [RequireComponent(typeof(CharacterBody))]
    [RequireComponent(typeof(PurchaseInteraction))]
    public class MimicInventoryManager : MonoBehaviour
    {
        private List<ItemIndex> itemInd = new List<ItemIndex>();
        public Transform pivot;
        public CharacterDirection cdir;
        public Xoroshiro128Plus mimicItemRng;
        MimicDropTable mimicDT;

        public void Start()
        {
            if (!pivot || !cdir)
            {
                FindPivot();
            }

            mimicDT = ScriptableObject.CreateInstance<MimicDropTable>();

            if (mimicItemRng == null)
            {
                mimicItemRng = new Xoroshiro128Plus(Run.instance.seed);
                SS2Log.Warning("rng " + mimicItemRng);
            }

            var item = mimicDT.GenerateDropPreReplacement(mimicItemRng);
            var itemIndex = PickupCatalog.GetPickupDef(item).itemIndex;
            var def = ItemCatalog.GetItemDef(itemIndex);

            if(def.DoesNotContainTag(ItemTag.AIBlacklist))
            {
                SS2Log.Warning("item " + item + " | itemIndex " + def + " | ");
                this.GetComponent<CharacterBody>().inventory.GiveItem(itemIndex);
            }
            else
            {
                SS2Log.Warning("Did Not Add " + def);
            }

             SS2Log.Warning("adding " + item + " | mim ");
             AddItem(itemIndex);
        }

        public void FindPivot()
        {
            var mloc = this.GetComponent<ModelLocator>();
            if (mloc && mloc.modelTransform)
            {
                var cloc = mloc.modelTransform.GetComponent<ChildLocator>();
                cdir = GetComponent<CharacterDirection>();
                if (cloc && cdir)
                {
                    pivot = cloc.FindChild("PickupPivot");
                }
            }
        }

        public void AddItem(ItemIndex ind)
        {
            SS2Log.Warning("Adding item " + ind + " | " + PickupCatalog.FindPickupIndex(ind) + " | " + PickupCatalog.GetPickupDef(PickupCatalog.FindPickupIndex(ind)));
            itemInd.Add(ind);
        }

        public void BeginCountdown()
        {
            StartCoroutine(DropItems());
        }

        IEnumerator DropItems()
        {
            if (!pivot || !cdir)
            {
                FindPivot();
            }
            //SS2Log.Warning("begin waity");
            //string effectName = "RoR2/Base/Chest1/ChestUnzip.prefab";
            //var effect = Addressables.LoadAssetAsync<GameObject>(effectName).WaitForCompletion();
            //UnityEngine.Object.Instantiate<GameObject>(effect, pivot);

            yield return new WaitForSeconds(1.5f);

            //string effectName2 = "RoR2/Base/Chest1/Chest1Starburst.prefab";
            //var effect2 = Addressables.LoadAssetAsync<GameObject>(effectName2).WaitForCompletion();

            //EffectData effectData = new EffectData
            //{
            //    origin = this.transform.position
            //};
            //effectData.SetNetworkedObjectReference(this.gameObject);
            //EffectManager.SimpleEffect(effect, this.transform.position, this.transform.rotation, true);
            //SS2Log.Warning("Simple Effect Just Fired");
            var temp = pivot.position;
            temp.y += .1f;
            EffectData effectData = new EffectData
            {
                origin = temp
            };
            effectData.SetNetworkedObjectReference(pivot.gameObject); //?
            EffectManager.SpawnEffect(SS2.Monsters.Mimic.itemStarburst, effectData, true);
            SS2Log.Warning("SimpleMuzzleFlash Fired");

            //UnityEngine.Object.Instantiate<GameObject>(effect2, pivot);

            SS2Log.Warning("end wait");
            if (itemInd.Count > 0 && pivot && cdir)
            {
                SS2Log.Warning("real,n");
                var dir = cdir.forward;
                var angle = 90 / itemInd.Count;

                Vector3 vec = Vector3.up * 5 + dir * 5;
                Quaternion rot = Quaternion.AngleAxis(angle, Vector3.up);

                foreach (var ind in itemInd)
                {
                    SS2Log.Warning("droppuy");
                    var pind = RoR2.PickupCatalog.FindPickupIndex(ind);
                    PickupDropletController.CreatePickupDroplet(pind, pivot.position, vec);
                    vec = rot * vec;
                }
            }
        }
    }

    public class MimicDropTable : PickupDropTable
    {
        private void Add(List<PickupIndex> sourceDropList, float listWeight)
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

        public override PickupIndex GenerateDropPreReplacement(Xoroshiro128Plus rng)
        {
            int num = 1;
            selector.Clear();
            Add(Run.instance.availableTier1DropList, tier1Weight);
            Add(Run.instance.availableTier2DropList, tier2Weight * (float)num);
            Add(Run.instance.availableTier3DropList, tier3Weight * Mathf.Pow((float)num, 2f));
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

        private float tier1Weight = .70f;
        private float tier2Weight = .25f;
        private float tier3Weight = .05f;
        private readonly WeightedSelection<PickupIndex> selector = new WeightedSelection<PickupIndex>();
    }
}
