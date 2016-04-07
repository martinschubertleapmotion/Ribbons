using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("SmartPool")]
    [Tooltip("Gets the SmartPool an item belongs to")]
    public class SmartPoolGetPoolByItem : FsmStateAction
    {
        [RequiredField, Tooltip("A GameObject managed by a SmartPool")]
        public FsmOwnerDefault gameObject;
        
        [UIHint(UIHint.Variable), Tooltip("Stores the SmartPool object")] 
        public FsmObject storeObject;

        public override void OnEnter()
        {
            GameObject go = gameObject.OwnerOption == OwnerDefaultOption.UseOwner ? Owner : gameObject.GameObject.Value;
            if (go)
                storeObject.Value = SmartPool.GetPoolByItem(go);
            Finish();
        }

        public override void Reset()
        {
            gameObject = null;
            storeObject=new FsmObject(){UseVariable=true};
        }
    }
}