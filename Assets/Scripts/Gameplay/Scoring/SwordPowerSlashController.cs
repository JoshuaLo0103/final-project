using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace BladeFrenzy.Gameplay.Scoring
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class SwordPowerSlashController : MonoBehaviour
    {
        [Header("Power Slash")]
        [SerializeField] private float activeDuration = 1f;
        [SerializeField] private float scaleMultiplier = 1.65f;
        [SerializeField] private float growEaseDuration = 0.08f;
        [SerializeField] private float shrinkEaseDuration = 0.16f;
        [SerializeField] private float cooldownDuration = 0.15f;

        private XRGrabInteractable _grabInteractable;
        private Coroutine _powerSlashRoutine;
        private Vector3 _baseScale;
        private float _lastActivatedTime = float.NegativeInfinity;

        public bool IsPowerSlashActive { get; private set; }

        private void Awake()
        {
            _grabInteractable = GetComponent<XRGrabInteractable>();
            _baseScale = transform.localScale;
        }

        private void OnEnable()
        {
            if (_grabInteractable != null)
                _grabInteractable.activated.AddListener(HandleActivated);
        }

        private void OnDisable()
        {
            if (_grabInteractable != null)
                _grabInteractable.activated.RemoveListener(HandleActivated);

            if (_powerSlashRoutine != null)
            {
                StopCoroutine(_powerSlashRoutine);
                _powerSlashRoutine = null;
            }

            IsPowerSlashActive = false;
            transform.localScale = _baseScale;
        }

        private void HandleActivated(ActivateEventArgs eventArgs)
        {
            if (_grabInteractable == null || !_grabInteractable.isSelected)
                return;

            if (Time.time < _lastActivatedTime + cooldownDuration)
                return;

            _lastActivatedTime = Time.time;

            if (_powerSlashRoutine != null)
                StopCoroutine(_powerSlashRoutine);

            _powerSlashRoutine = StartCoroutine(PowerSlashRoutine());
        }

        private IEnumerator PowerSlashRoutine()
        {
            IsPowerSlashActive = true;

            Vector3 enlargedScale = _baseScale * Mathf.Max(1f, scaleMultiplier);
            yield return EaseScale(transform.localScale, enlargedScale, growEaseDuration);

            float holdEndTime = Time.time + Mathf.Max(0f, activeDuration);
            while (Time.time < holdEndTime)
                yield return null;

            IsPowerSlashActive = false;
            yield return EaseScale(transform.localScale, _baseScale, shrinkEaseDuration);

            _powerSlashRoutine = null;
        }

        private IEnumerator EaseScale(Vector3 fromScale, Vector3 toScale, float duration)
        {
            if (duration <= 0f)
            {
                transform.localScale = toScale;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = Mathf.SmoothStep(0f, 1f, t);
                transform.localScale = Vector3.LerpUnclamped(fromScale, toScale, t);
                yield return null;
            }

            transform.localScale = toScale;
        }
    }
}
