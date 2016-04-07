
namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("SmartPool")]
    [Tooltip("Despawn all objects managed by a SmartPool")]
    public class SmartPoolDespawnAll : FsmStateAction
    {
        [Tooltip("The SmartPool being adressed")]
        [RequiredField]
        public FsmString smartPool;

        public override void OnEnter()
        {
            if (!smartPool.IsNone) {
                SmartPool.DespawnAllItems(smartPool.Value);
            }
            Finish();
        }

        public override void Reset()
        {
            smartPool = new FsmString() { UseVariable = true };
        }
    }
}