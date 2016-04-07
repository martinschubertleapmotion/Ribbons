
namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("SmartPool")]
    [Tooltip("Spawn an object managed by a SmartPool")]
    public class SmartPoolSpawn : FsmStateAction
    {
        [Tooltip("The SmartPool being adressed")]
        [RequiredField]
        public FsmString smartPool;

        [UIHint(UIHint.Variable)]
        [Tooltip("Optionally store the created object.")]
        public FsmGameObject storeObject;

        public override void OnEnter()
        {
            if (!smartPool.IsNone) {
                storeObject.Value=SmartPool.Spawn(smartPool.Value);
            }
            Finish();
        }

        public override void Reset()
        {
            smartPool = new FsmString() { UseVariable = true };
            storeObject = new FsmGameObject() { UseVariable = true };
        }
    }
}