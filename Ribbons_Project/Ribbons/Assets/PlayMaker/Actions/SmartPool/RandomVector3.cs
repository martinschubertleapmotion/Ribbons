using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(ActionCategory.Math)]
    [Tooltip("Creates a random Vector3 inside a unit sphere")]
    public class RandomVector3 : FsmStateAction
    {
        [UIHint(UIHint.Variable)]
        public FsmVector3 storeResult;

        public override void Reset()
        {
            storeResult = null;
        }

        public override void OnEnter()
        {
            storeResult.Value = Random.insideUnitSphere;
            Finish();
        }
    }
}