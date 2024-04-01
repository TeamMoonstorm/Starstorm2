namespace EntityStates.Events
{
    public class StageEvent : EventState
    {
        public override void FixedUpdate()
        {
            //if (Run.) to-do: end after teleporter?
            if (!HasWarned && fixedAge >= warningDur)
                StartEvent();
        }
    }
}
