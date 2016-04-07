

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("SmartPool")]
    [Tooltip("Culls a SmartPool")]
    public class SmartPoolCull : FsmStateAction
    {
        [Tooltip("The SmartPool being adressed")]
        [RequiredField]
        public FsmString smartPool;
        public FsmBool smartCull;

        public override void OnEnter()
        {
            if (!smartPool.IsNone) {
                if (smartCull.IsNone)
                    SmartPool.Cull(smartPool.Value);
                else
                    SmartPool.Cull(smartPool.Value,smartCull.Value);
            }
            Finish();
        }

        public override void Reset()
        {
            smartPool = new FsmString() { UseVariable = true };
            smartCull = new FsmBool() { UseVariable = true };
        }
        
    }
}