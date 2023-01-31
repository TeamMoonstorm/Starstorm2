using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.API
{
    //If youre wondering why this class is emptier now, its because most of these have been moved into MSUDebug.cs.
    //You can enable MSUDebug's debug features in its config file
    public class SS2DebugUtil : MonoBehaviour
    {
        private void Update()
        {
            var input0 = Input.GetKeyDown(KeyCode.Home);
            var input1 = Input.GetKeyDown(KeyCode.PageUp);
            var input2 = Input.GetKeyDown(KeyCode.Delete);
            if (input0)
            {
                var inputBank = PlayerCharacterMasterController.instances[0].master.GetBodyObject().GetComponent<InputBankTest>();
                var position = inputBank.aimOrigin + inputBank.aimDirection * 5;
                var effect = SS2Assets.LoadAsset<GameObject>("Center", SS2Bundle.All);
                Instantiate(effect, position, Quaternion.identity);
            }
            if (input1)
            {
                /*var inputBank = PlayerCharacterMasterController.instances[0].master.GetBodyObject().GetComponent<InputBankTest>();
                var position = inputBank.aimOrigin + inputBank.aimDirection * 5;
                var effect = Assets.Instance.MainAssetBundle.LoadAsset<GameObject>("MusicTester");
                Instantiate(effect, position, Quaternion.identity);*/
            }
            if (input2)
            {
                /*var inputBank = PlayerCharacterMasterController.instances[0].master.GetBodyObject().GetComponent<InputBankTest>();
                var position = inputBank.aimOrigin + inputBank.aimDirection * 5;
                var effect = Assets.Instance.MainAssetBundle.LoadAsset<GameObject>("CognationCode");
                Instantiate(effect, position, Quaternion.identity);*/
            }
        }
    }
}
