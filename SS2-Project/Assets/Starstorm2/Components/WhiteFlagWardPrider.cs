using SS2.Equipments;
using UnityEngine;
using UnityEngine.Networking;

namespace SS2.Components
{
    public class WhiteFlagWardPrider : NetworkBehaviour
    {
        [SyncVar(hook = "Pridify")]
        public int flagType;
        
        [SerializeField]
        public SkinnedMeshRenderer flagRenderer;
        
        [SerializeField]
        public MeshRenderer indicatorRenderer;
        
        private static readonly int Tint = Shader.PropertyToID("_TintColor");
        private Material wardFlagMat;
        private Material indicatorMat;
        private Color[] indicatorColors;
        private float colorIndex;

        public void Pridify(int flagIndex)
        {
            flagRenderer.material = PrideHelper.flagMaterials[flagIndex];
            
            indicatorMat = Instantiate(indicatorRenderer.material);
            indicatorRenderer.material = indicatorMat;

            indicatorColors = PrideHelper.GetFlagColors(flagIndex);
        }

        private void Update()
        {
            if (!WhiteFlag.usePrideEdits) return;
            if (!indicatorMat) return;
            
            if (indicatorColors.Length < (int)colorIndex)
            {
                colorIndex = 0; 
            }
            
            Color initalColor = (int)colorIndex > 1 ? indicatorColors[(int)colorIndex - 1] : indicatorColors[0];
            indicatorMat.SetColor(Tint, Color.Lerp(initalColor, indicatorColors.Length == (int)colorIndex ? indicatorColors[0] : indicatorColors[(int)colorIndex], colorIndex % 1));
               
            //lasts for duration ,,. cycle through twice !!
            colorIndex += Time.deltaTime * (indicatorColors.Length/WhiteFlag.flagDuration) * 2f;
        }

        private void OnDestroy()
        {
            if (!WhiteFlag.usePrideEdits) return;
            
            Destroy(indicatorMat);
        }
    }
}