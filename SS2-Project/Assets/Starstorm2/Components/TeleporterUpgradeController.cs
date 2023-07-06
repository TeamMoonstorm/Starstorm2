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
        private ParticleSystem betweenProngsCore;
        private Light coreLightRef;
        private ParticleSystem debris;
        private ParticleSystem chargingRing;
        private Light loopLight;
        private ParticleSystem core;
        private ParticleSystem beam;

        //private SpriteRenderer teleporterSprite;

        private void Awake()
        {
            Debug.Log("TELEPORTER UPGRADE CONTROLLER AWAKE");
            hzc = GetComponent<HoldoutZoneController>();
            ti = GetComponent<TeleporterInteraction>();

            //passive vfx
            telePassiveParticles = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/PassiveParticle, Sphere").GetComponent<ParticleSystem>();
            teleCenterParticles = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/PassiveParticle, Center").GetComponent<ParticleSystem>();
            lightningParticles = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/ChargedEffect/LightningAlongProngs").GetComponent<ParticleSystem>();
            lightningLightRef = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/ChargedEffect/LightningAlongProngs/ReferencePointLight").GetComponent<Light>();
            betweenProngsCore = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/IdleToChargingEffect/BetweenProngs/Core").GetComponent<ParticleSystem>();
            coreLightRef = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/IdleToChargingEffect/BetweenProngs/Point light").GetComponent<Light>();
            debris = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/IdleToChargingEffect/3DDebris").GetComponent<ParticleSystem>();
            chargingRing = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/ChargingEffect/Ring").GetComponent<ParticleSystem>();
            loopLight = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/ChargingEffect/BetweenProngs/Loop/Point light").GetComponent<Light>();
            core = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/ChargingEffect/BetweenProngs/Loop/Core").GetComponent<ParticleSystem>();
            beam = GameObject.Find("TeleporterBaseMesh/BuiltInEffects/ChargingEffect/BetweenProngs/Loop/Beam").GetComponent<ParticleSystem>();

            //teleporterSprite = GameObject.Find("TeleporterChargingPositionIndicator(Clone)/TeleporterSprite").GetComponent<SpriteRenderer>();

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

        public void UpgradeTeleporter()
        {
            Debug.Log("UPGRADING TELEPORTER");
            //check for sky meadows (lunar tp)
            var currStage = SceneManager.GetActiveScene().name;

            Ethereal.teleIsEthereal = true;

            //update teleporter holdout zone / directors / etc.
            hzc.baseRadius *= 1.5f;
            hzc.baseChargeDuration *= 1.25f;
            var cd = ti.bonusDirector;
            cd.maxConsecutiveCheapSkips = 2; //:slight_smile:
            cd.maximumNumberToSpawnBeforeSkipping = 10;


            //set a bunch of variables we'll be using for teleporter modifications:
            var newTeleMat = SS2Assets.LoadAsset<Material>("matEtherealFresnelOverlay", SS2Bundle.Indev);
            var teleBase = GameObject.Find("TeleporterBaseMesh").gameObject;
            var teleProngs = teleBase.transform.Find("TeleporterProngMesh").gameObject;
            var teleBeacon = teleBase.transform.Find("SurfaceHeight/TeleporterBeacon").gameObject;
            var teleBuiltInEffects = teleBase.transform.Find("BuiltInEffects").gameObject;
            var radiusScaler = teleBuiltInEffects.transform.Find("ChargingEffect/RadiusScaler").gameObject;

            //update the fresnel material from red to green
            if (currStage != "skymeadow")
            {
                teleBase.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
                teleProngs.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
                teleBeacon.GetComponent<MeshRenderer>().sharedMaterials[1].CopyPropertiesFromMaterial(newTeleMat);
            }

            //resize & reposition the teleporter
            teleBase.transform.localScale *= 1f;
            teleBase.transform.position = new Vector3(teleBase.transform.position.x, teleBase.transform.position.y, teleBase.transform.position.z);
            //teleProngs.transform.localScale *= 2f;
            //teleBeacon.transform.localScale *= 0.5f;
            //radiusScaler.transform.localScale *= 0.67f;

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

            //idletocharging
            var corePSR = betweenProngsCore.GetComponent<ParticleSystemRenderer>();
            corePSR.material = SS2Assets.LoadAsset<Material>("matEthTPFire", SS2Bundle.Indev);
            coreLightRef.color = new Color(.0f, .89f, .39f);

            //3DDebris
            var debrisPSR = debris.GetComponent<ParticleSystemRenderer>();
            debrisPSR.trailMaterial = SS2Assets.LoadAsset<Material>("matEthExplosion", SS2Bundle.Indev);

            //charging effects
            var ringPSR = chargingRing.GetComponent<ParticleSystemRenderer>();
            ringPSR.material = SS2Assets.LoadAsset<Material>("matEthTPShockwave", SS2Bundle.Indev);

            //center beam
            loopLight.color = new Color(.0f, .89f, .39f);
            var beamCorePSR = core.GetComponent<ParticleSystemRenderer>();
            beamCorePSR.material = SS2Assets.LoadAsset<Material>("matEthTPFire", SS2Bundle.Indev);
            var beamPSR = beam.GetComponent<ParticleSystemRenderer>();
            beamPSR.material = SS2Assets.LoadAsset<Material>("matEthTPLaser", SS2Bundle.Indev);

            //indicator sprite
            //teleporterSprite.color = new Color(.0f, .89f, .39f);

            //there's so many vfx to replace and recolor.
            //it'd maybe be easier to make a new prefab entirely although i can't help but feel it makes more sense to just edit the existing...
            //maybe particle system prefabs in unity & code to adjust scale / pos of the objects?
            //hell either way.

        }
    }
}
