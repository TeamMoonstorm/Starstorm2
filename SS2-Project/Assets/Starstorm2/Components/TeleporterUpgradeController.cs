using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Moonstorm.Starstorm2
{
    public class TeleporterUpgradeController : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnIsEtherealChanged))]
        public bool isEthereal = false;
        [SyncVar]
        private bool hasUpgraded = false;

        private HoldoutZoneController hzc;
        private TeleporterInteraction ti;
        private ParticleSystem telePassiveParticles;
        private ParticleSystem teleCenterParticles;
        private ParticleSystem lightningParticles;
        private Light lightningLightRef;
        private Renderer rangeIndicator;

        private void Start()
        {
            Debug.Log("TELEPORTER UPGRADE CONTROLLER AWAKE");
            hzc = GetComponent<HoldoutZoneController>();
            ti = GetComponent<TeleporterInteraction>();
            telePassiveParticles = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/PassiveParticle, Sphere").GetComponent<ParticleSystem>();
            teleCenterParticles = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/PassiveParticle, Center").GetComponent<ParticleSystem>();
            lightningParticles = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/ChargedEffect/LightningAlongProngs").GetComponent<ParticleSystem>();
            lightningLightRef = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/ChargedEffect/LightningAlongProngs/ReferencePointLight").GetComponent<Light>();
            rangeIndicator = hzc.radiusIndicator;
        }

        public void FixedUpdate()
        {
            if (isEthereal && !hasUpgraded)
            {
                hasUpgraded = true;

                UpgradeTeleporter();
            }
        }

        [Command]
        public void CmdUpdateIsEthereal(bool value)
        {
            isEthereal = value;
        }

        [ClientRpc]
        public void RpcUpdateIsEthereal(bool value)
        {
            isEthereal = value;
        }

        private void OnIsEtherealChanged(bool value)
        {
            isEthereal = value;
            //UpgradeTeleporter();
        }

        private void UpgradeTeleporter()
        {
            Debug.Log("UPGRADING TELEPORTER");
            //check for sky meadows (lunar tp)
            var currStage = SceneManager.GetActiveScene().name;

            //update teleporter holdout zone / directors / etc.
            hzc.baseRadius *= 1.5f;
            hzc.baseChargeDuration *= 0.25f;
            var cd = ti.bonusDirector;
            cd.maxConsecutiveCheapSkips = 2; //:slight_smile:
            cd.maximumNumberToSpawnBeforeSkipping = 10;


            //set a bunch of variables we'll be using for teleporter modifications:
            var newTeleMat = SS2Assets.LoadAsset<Material>("matEtherealFresnelOverlay", SS2Bundle.Indev);
            var teleBase = GameObject.Find("TeleporterBaseMesh").gameObject;
            var teleProngs = teleBase.transform.Find("TeleporterProngMesh").gameObject;
            var teleBeacon = teleBase.transform.Find("SurfaceHeight/TeleporterBeacon").gameObject;

            //update the fresnel material from red to green
            if (currStage != "skymeadow")
            {
                teleBase.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
                teleProngs.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
                teleBeacon.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
            }

            //resize & reposition the teleporter
            teleBase.transform.localScale *= 1.5f;
            teleBase.transform.position = new Vector3(teleBase.transform.position.x, teleBase.transform.position.y + 1f, teleBase.transform.position.z);
            //teleProngs.transform.localScale *= 2f;
            teleBeacon.transform.localScale *= 0.5f;

            //resize & recolor large teleporter particles
            var tppMain = telePassiveParticles.main;
            telePassiveParticles.transform.localScale *= 1.5f;
            tppMain.startColor = new Color(0f, .89f, .39f);

            //resize & recolor particles above teleporter well
            var tcpMain = teleCenterParticles.main;
            tcpMain.startColor = new Color(.0f, .89f, .39f);

            //re-material the range indicator
            rangeIndicator.material = SS2Assets.LoadAsset<Material>("matEthTeleporterRangeIndicator", SS2Bundle.Indev);

            //lightning effects
            var lightningPSR = lightningParticles.GetComponent<ParticleSystemRenderer>();
            lightningPSR.material = SS2Assets.LoadAsset<Material>("matEthTPLightning", SS2Bundle.Indev);
            lightningLightRef.color = new Color(.0f, .89f, .39f);

            //there's so many vfx to replace and recolor.
            //it'd maybe be easier to make a new prefab entirely although i can't help but feel it makes more sense to just edit the existing...
            //maybe particle system prefabs in unity & code to adjust scale / pos of the objects?
            //hell either way.

        }
    }
}
