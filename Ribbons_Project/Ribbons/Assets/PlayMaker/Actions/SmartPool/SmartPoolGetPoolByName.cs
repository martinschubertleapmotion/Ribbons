using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory("SmartPool")]
    [Tooltip("Gets a SmartPool by it's name")]
    public class SmartPoolGetPoolByName : FsmStateAction
    {
        [RequiredField, Tooltip("The name of a SmartPool")]
        public FsmString name;

        [UIHint(UIHint.Variable), Tooltip("Stores the SmartPool object")] 
        public FsmObject storeObject;

        public override void OnEnter()
        {
            if (!name.IsNone)
                storeObject.Value = SmartPool.GetPoolByName(name.Value);
            Finish();
        }

        public override void Reset()
        {
            name = new FsmString() { UseVariable = true };
            storeObject = new FsmObject() { UseVariable = true };
        }
    }
}