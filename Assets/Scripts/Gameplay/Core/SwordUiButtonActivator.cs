using BladeFrenzy.Gameplay.Scoring;
using UnityEngine;
using UnityEngine.UI;

namespace BladeFrenzy.Gameplay.Core
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(BoxCollider))]
    public class SwordUiButtonActivator : MonoBehaviour
    {
        [SerializeField] private float triggerDepth = 22f;
        [SerializeField] private float activationCooldown = 0.45f;

        private Button _button;
        private BoxCollider _collider;
        private float _nextActivationTime;

        private void Awake()
        {
            ResolveReferences();
            FitColliderToRect();
        }

        private void OnEnable()
        {
            ResolveReferences();
            FitColliderToRect();
        }

        public void Configure(Button button)
        {
            _button = button;
            ResolveReferences();
            FitColliderToRect();
        }

        public void SuppressActivationFor(float seconds)
        {
            _nextActivationTime = Mathf.Max(_nextActivationTime, Time.time + Mathf.Max(0f, seconds));
        }

        private void OnTriggerEnter(Collider other)
        {
            TryActivate(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null)
                return;

            TryActivate(collision.collider);
        }

        private void TryActivate(Collider other)
        {
            if (!isActiveAndEnabled || Time.time < _nextActivationTime)
                return;

            ResolveReferences();
            if (_button == null || !_button.interactable || other == null)
                return;

            if (!IsSwordCollider(other))
                return;

            _nextActivationTime = Time.time + activationCooldown;
            _button.onClick.Invoke();
        }

        private void ResolveReferences()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            if (_collider == null)
                _collider = GetComponent<BoxCollider>();

            if (_collider != null)
                _collider.isTrigger = true;
        }

        private void FitColliderToRect()
        {
            if (_collider == null)
                return;

            RectTransform rectTransform = transform as RectTransform;
            if (rectTransform == null)
                return;

            Rect rect = rectTransform.rect;
            _collider.center = new Vector3(
                (0.5f - rectTransform.pivot.x) * rect.width,
                (0.5f - rectTransform.pivot.y) * rect.height,
                0f);
            _collider.size = new Vector3(
                Mathf.Max(1f, rect.width),
                Mathf.Max(1f, rect.height),
                Mathf.Max(1f, triggerDepth));
        }

        private static bool IsSwordCollider(Collider other)
        {
            if (other.attachedRigidbody != null && other.attachedRigidbody.GetComponent<SwordHitScorer>() != null)
                return true;

            if (other.GetComponent<SwordHitScorer>() != null)
                return true;

            return other.GetComponentInParent<SwordHitScorer>() != null;
        }
    }
}
