using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
    [ActionCategory(ActionCategory.Physics)]
    [Tooltip("Sets the Angular Velocity of a Game Object. To leave any axis unchanged, set variable to 'None'. NOTE: Game object must have a rigidbody.")]
    public class SetAngularVelocity : FsmStateAction
    {
        [RequiredField]
        [CheckForComponent(typeof(Rigidbody))]
        public FsmOwnerDefault gameObject;

        [UIHint(UIHint.Variable)]
        public FsmVector3 vector;

        public FsmFloat x;
        public FsmFloat y;
        public FsmFloat z;

        public Space space;

        public bool everyFrame;

        public override void Reset()
        {
            gameObject = null;
            vector = null;
            // default axis to variable dropdown with None selected.
            x = new FsmFloat { UseVariable = true };
            y = new FsmFloat { UseVariable = true };
            z = new FsmFloat { UseVariable = true };
            space = Space.Self;
            everyFrame = false;
        }

        public override void OnEnter()
        {
            DoSetAngularVelocity();

            if (!everyFrame)
            {
                Finish();
            }
        }

        public override void OnFixedUpdate()
        {
            DoSetAngularVelocity();

            if (!everyFrame)
                Finish();
        }

        void DoSetAngularVelocity()
        {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if (go == null || go.GetComponent<UnityEngine.Rigidbody>() == null)
            {
                return;
            }

            // init position

            Vector3 angularVelocity;

            if (vector.IsNone)
            {
                angularVelocity = space == Space.World ?
                    go.GetComponent<UnityEngine.Rigidbody>().angularVelocity :
                    go.transform.InverseTransformDirection(go.GetComponent<UnityEngine.Rigidbody>().angularVelocity);
            }
            else
            {
                angularVelocity = vector.Value;
            }

            // override any axis

            if (!x.IsNone) angularVelocity.x = x.Value;
            if (!y.IsNone) angularVelocity.y = y.Value;
            if (!z.IsNone) angularVelocity.z = z.Value;

            // apply

            go.GetComponent<UnityEngine.Rigidbody>().angularVelocity = space == Space.World ? angularVelocity : go.transform.TransformDirection(angularVelocity);
        }

    }
}