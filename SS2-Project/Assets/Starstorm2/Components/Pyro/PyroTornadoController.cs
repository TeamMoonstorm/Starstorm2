using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using RoR2.Projectile;

public class PyroTornadoController : NetworkBehaviour
{
    private List<CharacterBody> processedVictims = new List<CharacterBody>();
    private CharacterBody ownerBody;
    private ProjectileDamage projDmg;
    private int burnStacks;
    private Vector3 baseScale;

    private float timer;
    private float timeBetweenRefreshes = 0.5f;

    public void Awake()
    {
        baseScale = transform.localScale;

        if (TryGetComponent(out ProjectileController pc) && pc.TryGetComponent(out CharacterBody body))
        {
            ownerBody = body;
        }

        if (TryGetComponent(out ProjectileDamage dmg))
        {
            projDmg = dmg;
        }
    }

    //public void FixedUpdate()
    //{
    //    timer += Time.fixedDeltaTime;
          // to-do: itd be cool if the growing happened between ticks, rather than instantly.
          // just stagger the additions - in TryProcessVictim, we set a new target to reach, and we add 1 to burnStacks using a timer, until its caught up.
          // do math to figure out how often to do it per half second to catch up with the new goal, trigger a visual burst with each, etc.
    //}

    public void TryProcessVictim(HurtBox victim)
    {
        if (NetworkServer.active)
        {
            // we have a victim body and haven't already checked them out:
            if (victim != null && victim.healthComponent != null && victim.healthComponent.body != null && !processedVictims.Contains(victim.healthComponent.body))
            {
                CharacterBody victimBody = victim.healthComponent.body;
                processedVictims.Add(victimBody);

                // same counting logic as passive for bounses:
                // count burn debuffs. red elite (blazing) counts as burning (since theyre always hot)
                // we dont count bigger guys as 2x more though because that feels maybe absurd
                int burnCount = victimBody.GetBuffCount(RoR2Content.Buffs.OnFire) + victimBody.GetBuffCount(DLC1Content.Buffs.StrongerBurn) + victimBody.GetBuffCount(RoR2Content.Buffs.AffixRed);
                int oldBurn = burnStacks;
                burnStacks += burnCount;

                if (oldBurn != burnStacks)
                {
                    // SS2.SS2Log.Info("PyroTornadoController.TryProcessVictim : Adding " + (burnStacks - oldBurn) + " burnstacks");
                    RpcAdjustScale(burnStacks);
                }
            }

            // ignite.
            // we do this manually afterwards, so that we don't count ourselves every time. would make it insanely easy to grow.

            // to-do: this half doesn't work because why?
            // not really needed, but just would be nice
            if (ownerBody != null)
            {
                var dotInfo = new InflictDotInfo()
                {
                    attackerObject = ownerBody.gameObject,
                    victimObject = victim.healthComponent.gameObject,
                    dotIndex = DotController.DotIndex.Burn,
                    duration = 4f,
                    damageMultiplier = projDmg.damage,
                };
                StrengthenBurnUtils.CheckDotForUpgrade(ownerBody.inventory, ref dotInfo);
                DotController.InflictDot(ref dotInfo);
            }
        }
    }

    [ClientRpc]
    public void RpcAdjustScale(int burn)
    {
        Vector3 scale = new Vector3(baseScale.x + (burn * 0.2f), baseScale.y + (burn * 0.2f), baseScale.z + (burn * 0.2f));
        transform.localScale = scale;
        // SS2.SS2Log.Info("PyroTornadoController : Adjusted scale based on " + burn);
    }
}
