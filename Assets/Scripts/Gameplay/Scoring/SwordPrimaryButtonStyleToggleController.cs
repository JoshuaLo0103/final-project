using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace BladeFrenzy.Gameplay.Scoring
{
    [RequireComponent(typeof(XRGrabInteractable))]
    [RequireComponent(typeof(SwordComboGlowController))]
    public class SwordPrimaryButtonStyleToggleController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private bool useRightControllerAButton = true;
        [SerializeField] private bool alsoAllowLeftPrimaryButton = false;
        [SerializeField] private float toggleCooldown = 0.18f;

        private XRGrabInteractable _grabInteractable;
        private SwordComboGlowController _glowController;
        private bool _isHeld;
        private bool _wasPressed;
        private bool _ignoreUntilReleased;
        private float _lastToggleTime = float.NegativeInfinity;

        private void Awake()
        {
            _grabInteractable = GetComponent<XRGrabInteractable>();
            _glowController = GetComponent<SwordComboGlowController>();
        }

        private void OnEnable()
        {
            if (_grabInteractable == null)
                return;

            _grabInteractable.selectEntered.AddListener(HandleSelectEntered);
            _grabInteractable.selectExited.AddListener(HandleSelectExited);
        }

        private void OnDisable()
        {
            if (_grabInteractable != null)
            {
                _grabInteractable.selectEntered.RemoveListener(HandleSelectEntered);
                _grabInteractable.selectExited.RemoveListener(HandleSelectExited);
            }

            _isHeld = false;
            _wasPressed = false;
            _ignoreUntilReleased = false;
        }

        private void Update()
        {
            if (!_isHeld || _glowController == null)
                return;

            bool isPressed = IsPrimaryButtonPressed();
            if (_ignoreUntilReleased)
            {
                if (!isPressed)
                    _ignoreUntilReleased = false;

                _wasPressed = isPressed;
                return;
            }

            if (isPressed && !_wasPressed && Time.time >= _lastToggleTime + toggleCooldown)
            {
                _lastToggleTime = Time.time;
                _glowController.ToggleColorStyle();
            }

            _wasPressed = isPressed;
        }

        private void HandleSelectEntered(SelectEnterEventArgs _)
        {
            _isHeld = true;
            _wasPressed = IsPrimaryButtonPressed();
            _ignoreUntilReleased = _wasPressed;
        }

        private void HandleSelectExited(SelectExitEventArgs _)
        {
            _isHeld = false;
            _wasPressed = false;
            _ignoreUntilReleased = false;
        }

        private bool IsPrimaryButtonPressed()
        {
            if (useRightControllerAButton && TryReadPrimaryButton(InputDeviceCharacteristics.Right, out bool rightPressed) && rightPressed)
                return true;

            return alsoAllowLeftPrimaryButton &&
                   TryReadPrimaryButton(InputDeviceCharacteristics.Left, out bool leftPressed) &&
                   leftPressed;
        }

        private static bool TryReadPrimaryButton(InputDeviceCharacteristics handedness, out bool isPressed)
        {
            isPressed = false;

            InputDevice device = InputDevices.GetDeviceAtXRNode(
                handedness == InputDeviceCharacteristics.Left ? XRNode.LeftHand : XRNode.RightHand);
            if (!device.isValid)
                return false;

            return device.TryGetFeatureValue(CommonUsages.primaryButton, out isPressed);
        }
    }
}
