
namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("SmartPool")]
    [Tooltip("Prepares a SmartPool")]
    public class SmartPoolPrepare : FsmStateAction
    {
        [Tooltip("The SmartPool being adressed")]
        [RequiredField]
        public FsmString smartPool;

        public override void OnEnter()
        {
            if (!smartPool.IsNone) {
                SmartPool.Prepare(smartPool.Value);
            }
            Finish();
        }

        public override void Reset()
        {
            smartPool = new FsmString() { UseVariable = true };
        }
    }
}