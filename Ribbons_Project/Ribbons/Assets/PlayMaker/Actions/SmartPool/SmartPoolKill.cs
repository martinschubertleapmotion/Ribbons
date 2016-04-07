using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("SmartPool")]
    [Tooltip("Kills a spawned item instead of despawning it")]
    public class SmartPoolKill : FsmStateAction
    {
        [RequiredField]
        public FsmOwnerDefault gameObject;

        public override void OnEnter()
        {
            GameObject go = gameObject.OwnerOption == OwnerDefaultOption.UseOwner ? Owner : gameObject.GameObject.Value;
            SmartPool.Kill(go);
            Finish();
        }

        public override void Reset()
        {
            gameObject = null;
        }
    }
}