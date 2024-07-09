using UnityEngine;
using RoR2;
using RoR2.Skills;
using UnityEngine.Networking;

namespace EntityStates.Knight
{
public class BannerSpecial : BaseState
{
    public static SkillDef buffedSkillRef;
    public static GameObject powerBuffWard;
    public static GameObject slowBuffWard;
            
    private GameObject knightBannerWard;
    private GameObject slowBuffWardInstance;

    public override void OnEnter()
    {
        base.OnEnter();

        if (isAuthority & NetworkServer.active)
        {
            Vector3 position = inputBank.aimOrigin - (inputBank.aimDirection);
            GameObject bannerObject = UnityEngine.Object.Instantiate(knightBannerWard, position, Quaternion.identity);

            bannerObject.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
            NetworkServer.Spawn(bannerObject);

            slowBuffWardInstance = UnityEngine.Object.Instantiate(slowBuffWard, position, Quaternion.identity);
            slowBuffWardInstance.GetComponent<TeamFilter>().teamIndex = characterBody.teamComponent.teamIndex;
            slowBuffWardInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(bannerObject);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        outer.SetNextStateToMain();
    }

    public override void OnExit()
    {
        outer.SetNextStateToMain();
        base.OnExit();
    }

    public override InterruptPriority GetMinimumInterruptPriority()
    {
        return InterruptPriority.PrioritySkill;
    }
}

}