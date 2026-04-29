using BladeFrenzy.Gameplay.Scoring;
using UnityEngine;

namespace BladeFrenzy.Gameplay.Spawning
{
    public class StarCopierOrbTrigger : MonoBehaviour
    {
        private StarCopierShrine _shrine;

        private void Awake()
        {
            ResolveShrine();
        }

        private void OnEnable()
        {
            ResolveShrine();
        }

        public void Initialize(StarCopierShrine shrine)
        {
            _shrine = shrine;
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
            ResolveShrine();
            if (_shrine == null || other == null)
                return;

            if (!ResolveSword(other))
                return;

            Vector3 hitPoint = other.ClosestPoint(transform.position);
            _shrine.TryActivate(hitPoint);
        }

        private void ResolveShrine()
        {
            if (_shrine == null)
                _shrine = GetComponentInParent<StarCopierShrine>();
        }

        private static bool ResolveSword(Collider other)
        {
            if (other.attachedRigidbody != null && other.attachedRigidbody.GetComponent<SwordHitScorer>() != null)
                return true;

            if (other.GetComponent<SwordHitScorer>() != null)
                return true;

            return other.GetComponentInParent<SwordHitScorer>() != null;
        }
    }
}
