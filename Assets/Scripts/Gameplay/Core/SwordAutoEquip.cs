using System.Collections;
using BladeFrenzy.Gameplay.Scoring;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace BladeFrenzy.Gameplay.Core
{
    public class SwordAutoEquip : MonoBehaviour
    {
        [SerializeField] private bool forceControllersVisible = true;
        [SerializeField] private int maxAttempts = 20;
        [SerializeField] private float retryDelay = 0.2f;

        private void Start()
        {
            StartCoroutine(AutoEquipRoutine());
        }

        private IEnumerator AutoEquipRoutine()
        {
            if (forceControllersVisible)
                EnsureControllerMode();

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                XRBaseInteractor rightHandInteractor = FindHandInteractor(InteractorHandedness.Right);
                XRBaseInteractor leftHandInteractor = FindHandInteractor(InteractorHandedness.Left);

                XRGrabInteractable rightSword = FindSword(preferLeftNamedSword: false);
                bool rightEquipped = TryEquip(rightSword, rightHandInteractor);
                bool leftEquipped = TryEquip(FindSword(preferLeftNamedSword: true, excludedSword: rightSword), leftHandInteractor);

                if (rightEquipped && leftEquipped)
                    yield break;

                yield return new WaitForSeconds(retryDelay);
            }

            Debug.LogWarning("SwordAutoEquip could not find valid left/right hand interactors and swords at startup.");
        }

        private static void EnsureControllerMode()
        {
            XRInputModalityManager modalityManager = FindFirstObjectByType<XRInputModalityManager>();
            if (modalityManager != null && modalityManager.enabled)
                modalityManager.enabled = false;

            ActivateObject(modalityManager != null ? modalityManager.leftController : null);
            ActivateObject(modalityManager != null ? modalityManager.rightController : null);
        }

        private static XRGrabInteractable FindSword(bool preferLeftNamedSword, XRGrabInteractable excludedSword = null)
        {
            XRGrabInteractable fallback = null;
            XRGrabInteractable[] interactables = FindObjectsByType<XRGrabInteractable>(FindObjectsSortMode.None);
            foreach (XRGrabInteractable interactable in interactables)
            {
                if (interactable == null || interactable == excludedSword)
                    continue;

                if (!interactable.TryGetComponent<SwordHitScorer>(out _) || interactable.isSelected)
                    continue;

                bool isLeftNamedSword = interactable.name.Contains("Left", System.StringComparison.OrdinalIgnoreCase);
                if (isLeftNamedSword == preferLeftNamedSword)
                    return interactable;

                fallback ??= interactable;
            }

            return fallback;
        }

        private static XRBaseInteractor FindHandInteractor(InteractorHandedness handedness)
        {
            XRBaseInteractor fallback = null;
            XRBaseInteractor[] interactors = FindObjectsByType<XRBaseInteractor>(FindObjectsSortMode.None);
            foreach (XRBaseInteractor interactor in interactors)
            {
                if (interactor == null || !interactor.isActiveAndEnabled)
                    continue;

                if (interactor.handedness != handedness)
                    continue;

                if (interactor is NearFarInteractor)
                    return interactor;

                if (fallback == null && interactor is IXRSelectInteractor)
                    fallback = interactor;
            }

            return fallback;
        }

        private static bool TryEquip(XRGrabInteractable sword, XRBaseInteractor handInteractor)
        {
            if (sword == null || handInteractor == null)
                return false;

            if (sword.isSelected)
                return true;

            if (handInteractor is not IXRSelectInteractor || sword is not IXRSelectInteractable selectInteractable)
                return false;

            handInteractor.StartManualInteraction(selectInteractable);
            return true;
        }

        private static void ActivateObject(GameObject target)
        {
            if (target == null)
                return;

            target.SetActive(true);
        }
    }
}
