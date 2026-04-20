namespace EntityStates.NemMage
{
    public class NemMageMain : GenericCharacterMain
    {
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (inputBank.jump.down)
            {
                characterMotor.isAirControlForced = true;
                characterMotor.airControl = 0.0833333f; // 0.3 x normal air control. yes that's all it took
            }
            else
            {
                characterMotor.isAirControlForced = false;
                characterMotor.airControl = 0.25f;
            }
        }
    }
}