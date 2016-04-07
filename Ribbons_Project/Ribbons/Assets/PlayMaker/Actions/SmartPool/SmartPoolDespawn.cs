using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("SmartPool")]
    [Tooltip("Despawn an object that is managed by a SmartPool")]
    public class SmartPoolDespawn : FsmStateAction
    {
        [RequiredField]
        public FsmOwnerDefault gameObject;

        public override void OnEnter()
        {
            GameObject go = gameObject.OwnerOption == OwnerDefaultOption.UseOwner ? Owner : gameObject.GameObject.Value;
            SmartPool.Despawn(go);
            Finish();
        }

        public override void Reset()
        {
            gameObject = null;
        }
    }
}