/*using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class HeatComponent : NetworkBehaviour
{
    public bool IsHighHeat()
    {
        return heatPercent > 0.75f ? true : false;
    }

    public float GetHeat()
    {
        return heatPercent;
    }

    [ClientRpc]
    public void RpcAddHeatServer(float heat)
    {
        if (hasAuthority)
        {
            AddHeat(heat);
        }
    }

    public void AddHeat(float heat)
    {
        if (heatPercent <= 0f)
        {
            EnableGauge();
        }
        heatPercent += heat;
        if (heatPercent > 1f)
        {
            this.heatPercent = 1f;
        }
        this.heatDecayStopwatch = 0f;
    }

    public void ConsumeHeat(float heat)
    {
        heatPercent -= heat;
        if (heatPercent <= 0f)
        {
            this.heatPercent = 0f;
            gaugeEnabled = false;
        }
        this.heatDecayStopwatch = 0f;
    }

    private void FixedUpdate()
    {
        Chat.AddMessage("Heat: " + heatPercent);
        if (heatPercent > 0f)
        {
            if (heatDecayStopwatch < heatDecayDelay)
            {
                this.heatDecayStopwatch += Time.fixedDeltaTime;
            }
            else if (!this.pauseDecay)
            {
                heatPercent -= Time.fixedDeltaTime / HeatComponent.baseHeatDecayDuration;
                if (heatPercent < 0f)
                {
                    heatPercent = 0f;
                    gaugeEnabled = false;
                }
            }
        }
        if (skillLocator.secondary.skillDef.skillName == "Airblast")
        {
            if (heatPercent < EntityStates.Pyro.Airblast.heatCost)
            {
                skillLocator.secondary.enabled = false;
                skillLocator.secondary.stock = 0;
                skillLocator.secondary.rechargeStopwatch = 0f;
            }
            else
            {
                skillLocator.secondary.enabled = true;
                skillLocator.secondary.stock = skillLocator.secondary.maxStock;
            }
        }
        else if (skillLocator.secondary.skillDef.skillName == "SuppressiveFire")
        {
            if (heatPercent <= 0f)
            {
                skillLocator.secondary.enabled = false;
                skillLocator.secondary.stock = 0;
                skillLocator.secondary.rechargeStopwatch = 0f;
            }
            else
            {
                skillLocator.secondary.enabled = true;
                skillLocator.secondary.stock = skillLocator.secondary.maxStock;
            }
        }
    }

    private void EnableGauge()
    {
        gaugeEnabled = true;
        rectGauge.width = Screen.height / 1080f * gaugeScale * heatGauge.width;
        rectGauge.height = Screen.height / 1080f * gaugeScale * heatGauge.height;
        rectGauge.position = new Vector2(Screen.width / 2f - rectGauge.width / 2f, Screen.height / 2f + rectGauge.height * 2f);

        rectBar.width = Screen.height / 1080f * gaugeScale * heatBar.width;
        barWidth = rectBar.width;
        rectBar.height = Screen.height / 1080f * gaugeScale * heatBar.height;
        rectBar.position = new Vector2(rectGauge.position.x + Screen.height / 1080f * gaugeScale * 2f, rectGauge.position.y + Screen.height / 1080f * gaugeScale * 2f);
    }

    private void OnGUI()
    {
        if (this.hasAuthority && gaugeEnabled && !RoR2.PauseManager.isPaused && healthComponent && healthComponent.alive)
        {
            rectBar.width = barWidth * heatPercent;
            GUI.DrawTexture(rectBar, IsHighHeat() ? heatBar_Heated : heatBar, ScaleMode.ScaleAndCrop, true, 0f);
            GUI.DrawTexture(rectGauge, IsHighHeat() ? heatGauge_Heated : heatGauge, ScaleMode.StretchToFill, true, 0f);
        }
    }

    private void Awake()
    {
        characterBody = base.GetComponent<CharacterBody>();
        healthComponent = characterBody.healthComponent;
        skillLocator = characterBody.skillLocator;

        rectGauge = new Rect();
        rectBar = new Rect();
        gaugeEnabled = false;
    }

    public static float baseHeatDecayDuration = 20f;
    public static float heatDecayDelay = 1f; //how long before heat starts draining
    public static float gaugeScale = 4f;

    public static Texture2D heatGauge = Resources.Load<Texture2D>("texHeatgauge.png");
    public static Texture2D heatGauge_Heated = Resources.Load<Texture2D>("texHeatgauge_heated.png");
    public static Texture2D heatBar = Resources.Load<Texture2D>("texHeatbar.png");
    public static Texture2D heatBar_Heated = Resources.Load<Texture2D>("texHeatbar_heated.png");

    public bool pauseDecay = false;

    private bool gaugeEnabled;
    private float heatPercent = 0f;
    private float heatDecayStopwatch = 0f;
    private CharacterBody characterBody;
    private SkillLocator skillLocator;
    private HealthComponent healthComponent;
    private Rect rectGauge;
    private Rect rectBar;
    private float barWidth;
}

*/