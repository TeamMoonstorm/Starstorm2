namespace Moonstorm.Starstorm2.Components
{
    /*public class BorgController : MonoBehaviour
    {
        private CharacterBody characterBody;
        private CharacterModel model;
        private ChildLocator childLocator;//gonna store this for when it's needed (if it's ever needed kek)


        private void Awake()
        {
            characterBody = gameObject.GetComponent<CharacterBody>();
            childLocator = gameObject.GetComponentInChildren<ChildLocator>();
            model = GetComponentInChildren<CharacterModel>();


            if (characterBody)
            {
                Transform modelTransform = characterBody.GetComponent<ModelLocator>().modelTransform;
                /*if (modelTransform)
                {
                    gunMat = modelTransform.GetComponent<ModelSkinController>().skins[characterBody.skinIndex].rendererInfos[1].defaultMaterial;
                }
            }

            //InitItemDisplays();
        }

        private void FixedUpdate()
        {
            /*currentEmission = Mathf.Lerp(currentEmission, characterBody.skillLocator.secondary.stock, 1.5f * Time.fixedDeltaTime);

            if (gunMat)
            {
                gunMat.SetFloat("_EmPower", Util.Remap(currentEmission, 0, characterBody.skillLocator.secondary.maxStock, 0, maxEmission));
                float colorValue = Util.Remap(currentEmission, 0, characterBody.skillLocator.secondary.maxStock, 0f, 1f);
                Color emColor = emColor = new Color(colorValue, colorValue, colorValue);
                gunMat.SetColor("_EmColor", emColor);
            }
        }

        //private void InitItemDisplays()
        //{
        //    // i really don't know why this is necessary but just deal with it for now
        //    Starstorm2.Cores.ItemDisplays.BorgItemDisplays.RegisterModdedDisplays();

        //    ItemDisplayRuleSet newRuleset = Instantiate(Starstorm2.Cores.ItemDisplays.BorgItemDisplays.itemDisplayRuleSet);
        //    model.itemDisplayRuleSet = newRuleset;
        //}
    }*/
}