using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace WhiteRoom
{
    public enum RoomTransformDirection
    {
        ToHouse,
        ToWhiteRoom,
        Toggle
    }

    /// <summary>
    /// XR poke/select button that triggers a procedural room metamorphosis.
    /// </summary>
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class RoomTransformButton : MonoBehaviour
    {
        [SerializeField] RoomTransformController controller;
        [SerializeField] RoomTransformDirection direction = RoomTransformDirection.ToHouse;

        void Awake()
        {
            if (controller == null)
                controller = FindFirstObjectByType<RoomTransformController>();

            var interactable = GetComponent<XRSimpleInteractable>();
            interactable.selectEntered.AddListener(OnSelect);
        }

        void OnDestroy()
        {
            var interactable = GetComponent<XRSimpleInteractable>();
            if (interactable != null)
                interactable.selectEntered.RemoveListener(OnSelect);
        }

        void OnSelect(SelectEnterEventArgs _)
        {
            if (controller == null || controller.IsMorphing)
                return;

            switch (direction)
            {
                case RoomTransformDirection.ToHouse:
                    controller.TransformToHouse();
                    break;
                case RoomTransformDirection.ToWhiteRoom:
                    controller.TransformToWhiteRoom();
                    break;
                default:
                    controller.ToggleTransform();
                    break;
            }
        }
    }
}
