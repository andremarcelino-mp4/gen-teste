using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace WhiteRoom
{
    [RequireComponent(typeof(HingeJoint))]
    [RequireComponent(typeof(Rigidbody))]
    public class HingeGrabDoor : MonoBehaviour
    {
        [SerializeField] float minAngle;
        [SerializeField] float maxAngle = 90f;

        void Awake()
        {
            var body = GetComponent<Rigidbody>();
            body.mass = 8f;
            body.linearDamping = 1.5f;
            body.angularDamping = 2.5f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            var joint = GetComponent<HingeJoint>();
            joint.axis = Vector3.up;
            joint.useLimits = true;
            joint.limits = new JointLimits
            {
                min = minAngle,
                max = maxAngle,
                bounciness = 0f,
                bounceMinVelocity = 0.1f
            };
            joint.useSpring = true;
            joint.spring = new JointSpring { spring = 8f, damper = 4f, targetPosition = 0f };

            var grab = GetComponent<XRGrabInteractable>();
            if (grab == null)
                grab = gameObject.AddComponent<XRGrabInteractable>();

            grab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
            grab.throwOnDetach = false;
            grab.useDynamicAttach = true;
            grab.selectMode = InteractableSelectMode.Single;
        }
    }
}
