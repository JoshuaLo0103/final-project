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
                XRGrabInteractable sword = FindSword();
                XRBaseInteractor rightHandInteractor = FindRightHandInteractor();

                if (TryEquip(sword, rightHandInteractor))
                    yield break;

                yield return new WaitForSeconds(retryDelay);
            }

            Debug.LogWarning("SwordAutoEquip could not find a valid right-hand interactor and sword at startup.");
        }

        private static void EnsureControllerMode()
        {
            XRInputModalityManager modalityManager = FindFirstObjectByType<XRInputModalityManager>();
            if (modalityManager != null && modalityManager.enabled)
                modalityManager.enabled = false;

            ActivateObject(modalityManager != null ? modalityManager.leftController : null);
            ActivateObject(modalityManager != null ? modalityManager.rightController : null);
        }

        private static XRGrabInteractable FindSword()
        {
            XRGrabInteractable[] interactables = FindObjectsByType<XRGrabInteractable>(FindObjectsSortMode.None);
            foreach (XRGrabInteractable interactable in interactables)
            {
                if (interactable == null)
                    continue;

                if (interactable.TryGetComponent<SwordHitScorer>(out _))
                    return interactable;
            }

            return null;
        }

        private static XRBaseInteractor FindRightHandInteractor()
        {
            XRBaseInteractor fallback = null;
            XRBaseInteractor[] interactors = FindObjectsByType<XRBaseInteractor>(FindObjectsSortMode.None);
            foreach (XRBaseInteractor interactor in interactors)
            {
                if (interactor == null || !interactor.isActiveAndEnabled)
                    continue;

                if (interactor.handedness != InteractorHandedness.Right)
                    continue;

                if (interactor is NearFarInteractor)
                    return interactor;

                if (fallback == null && interactor is IXRSelectInteractor)
                    fallback = interactor;
            }

            return fallback;
        }

        private static bool TryEquip(XRGrabInteractable sword, XRBaseInteractor rightHandInteractor)
        {
            if (sword == null || rightHandInteractor == null)
                return false;

            if (sword.isSelected)
                return true;

            if (rightHandInteractor is not IXRSelectInteractor || sword is not IXRSelectInteractable selectInteractable)
                return false;

            rightHandInteractor.StartManualInteraction(selectInteractable);
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
