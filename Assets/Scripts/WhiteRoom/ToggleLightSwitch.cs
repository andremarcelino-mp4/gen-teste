using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace WhiteRoom
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class ToggleLightSwitch : MonoBehaviour
    {
        [SerializeField] Light[] lights;
        [SerializeField] Renderer led;
        [SerializeField] Color onColor = new Color(0.2f, 1.4f, 0.35f);
        [SerializeField] Color offColor = new Color(0.08f, 0.12f, 0.08f);

        bool isOn = true;

        void Awake()
        {
            var interactable = GetComponent<XRSimpleInteractable>();
            interactable.selectEntered.AddListener(OnSelect);
            Apply();
        }

        void OnDestroy()
        {
            var interactable = GetComponent<XRSimpleInteractable>();
            if (interactable != null)
                interactable.selectEntered.RemoveListener(OnSelect);
        }

        void OnSelect(SelectEnterEventArgs _)
        {
            isOn = !isOn;
            Apply();
        }

        void Apply()
        {
            if (lights != null)
            {
                for (int i = 0; i < lights.Length; i++)
                {
                    if (lights[i] != null)
                        lights[i].enabled = isOn;
                }
            }

            if (led != null)
            {
                var block = new MaterialPropertyBlock();
                led.GetPropertyBlock(block);
                Color c = isOn ? onColor : offColor;
                if (led.sharedMaterial != null && led.sharedMaterial.HasProperty("_BaseColor"))
                    block.SetColor("_BaseColor", c);
                if (led.sharedMaterial != null && led.sharedMaterial.HasProperty("_EmissionColor"))
                    block.SetColor("_EmissionColor", isOn ? onColor : Color.black);
                led.SetPropertyBlock(block);
            }
        }
    }
}
