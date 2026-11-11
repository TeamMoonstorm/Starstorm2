using System;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using RoR2.UI;
using UnityEngine.Networking;
using MSU;

namespace SS2
{
    public class EliteCampController : MonoBehaviour
    {
        [SerializeField]
        public EliteDef eliteDef;
        [SerializeField]
        public string eliteDefAddress;
        [SerializeField]
        public CampDirectorInfo[] campDirectors;

        [Serializable]
        public struct CampDirectorInfo
        {
            public CampDirector director;
            public int minInteractableCredit;
            public int maxInteractableCredit;
        }

        private Xoroshiro128Plus rng;
        private void Awake()
        {
            if (NetworkServer.active)
            {
                rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUint);

                if (!eliteDef && !string.IsNullOrEmpty(eliteDefAddress))
                {
                    eliteDef = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<EliteDef>(eliteDefAddress).WaitForCompletion();
                }

                if (eliteDef && campDirectors != null)
                {
                    for (int i = 0; i < campDirectors.Length; i++)
                    {
                        var info = campDirectors[i];
                        if (info.director)
                        {
                            info.director.eliteDef = eliteDef;
                            if (info.maxInteractableCredit > info.minInteractableCredit)
                                info.director.baseInteractableCredit = rng.RangeInt(info.minInteractableCredit, info.maxInteractableCredit);
                        }
                    }
                }
            }
            
        }
    }
}
