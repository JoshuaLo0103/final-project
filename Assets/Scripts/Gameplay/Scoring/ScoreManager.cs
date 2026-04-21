using UnityEngine;

namespace BladeFrenzy.Gameplay.Scoring
{
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] private int startingScore;
        [SerializeField] private bool drawDebugOverlay = true;

        private int _currentScore;
        private GUIStyle _labelStyle;

        public int CurrentScore => _currentScore;

        private void Awake()
        {
            ResetScore();
        }

        public void ResetScore()
        {
            _currentScore = startingScore;
        }

        public void AddScore(int amount)
        {
            _currentScore += amount;
        }

        private void OnGUI()
        {
            if (!drawDebugOverlay)
                return;

            _labelStyle ??= new GUIStyle(GUI.skin.box)
            {
                fontSize = 28,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(16, 16, 10, 10)
            };

            GUI.Box(new Rect(20f, 20f, 180f, 52f), $"Score: {_currentScore}", _labelStyle);
        }
    }
}
