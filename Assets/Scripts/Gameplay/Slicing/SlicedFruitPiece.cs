using UnityEngine;

namespace BladeFrenzy.Gameplay.Slicing
{
    [RequireComponent(typeof(MeshFilter))]
    public class SlicedFruitPiece : MonoBehaviour
    {
        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();
        }

        private void OnDestroy()
        {
            if (_meshFilter != null && _meshFilter.sharedMesh != null)
            {
                if (_meshCollider != null)
                    _meshCollider.sharedMesh = null;

                Destroy(_meshFilter.sharedMesh);
                _meshFilter.sharedMesh = null;
            }
        }
    }
}
