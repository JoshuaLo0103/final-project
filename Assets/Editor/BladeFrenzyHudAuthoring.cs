using System.IO;
using BladeFrenzy.Gameplay.Core;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BladeFrenzy.EditorTools
{
    public static class BladeFrenzyHudAuthoring
    {
        private const string PrefabDirectory = "Assets/Gameplay/Prefabs/UI";
        private const string PrefabPath = PrefabDirectory + "/BladeFrenzyRuntime.prefab";

        [MenuItem("Blade Frenzy/Create Runtime HUD Prefab")]
        public static string CreateRuntimeHudPrefab()
        {
            GameObject gameplayRoot = GameObject.Find("Gameplay");
            GameObject existing = GameObject.Find("BladeFrenzyRuntime");
            if (existing != null)
                Object.DestroyImmediate(existing);

            GameObject runtimeRoot = new("BladeFrenzyRuntime");
            if (gameplayRoot != null)
                runtimeRoot.transform.SetParent(gameplayRoot.transform, false);

            runtimeRoot.AddComponent<ScoreManager>();
            runtimeRoot.AddComponent<DifficultyManager>();
            runtimeRoot.AddComponent<GameManager>();
            ScoreboardUI scoreboardUi = runtimeRoot.AddComponent<ScoreboardUI>();

            Canvas canvas = CreateCanvas(runtimeRoot.transform);
            TMP_Text statusText;
            TMP_Text scoreText;
            TMP_Text comboText;
            TMP_Text multiplierText;
            TMP_Text highScoreText;
            TMP_Text timerText;
            TMP_Text difficultyText;
            TMP_Text finalScoreText;
            TMP_Text finalComboText;
            GameObject gameOverPanel = BuildHudLayout(
                canvas.transform,
                out statusText,
                out scoreText,
                out comboText,
                out multiplierText,
                out highScoreText,
                out timerText,
                out difficultyText,
                out finalScoreText,
                out finalComboText);

            PlaceCanvas(canvas);
            EnsureEventSystem();
            WireReferences(
                scoreboardUi,
                canvas,
                statusText,
                scoreText,
                comboText,
                multiplierText,
                highScoreText,
                timerText,
                difficultyText,
                gameOverPanel,
                finalScoreText,
                finalComboText);

            Directory.CreateDirectory(PrefabDirectory);
            PrefabUtility.SaveAsPrefabAssetAndConnect(runtimeRoot, PrefabPath, InteractionMode.AutomatedAction);
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();

            return PrefabPath;
        }

        private static Canvas CreateCanvas(Transform parent)
        {
            GameObject canvasObject = new("ScoreboardCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(parent, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);

            RectTransform rect = canvasObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(920f, 420f);

            return canvas;
        }

        private static GameObject BuildHudLayout(
            Transform canvasTransform,
            out TMP_Text statusText,
            out TMP_Text scoreText,
            out TMP_Text comboText,
            out TMP_Text multiplierText,
            out TMP_Text highScoreText,
            out TMP_Text timerText,
            out TMP_Text difficultyText,
            out TMP_Text finalScoreText,
            out TMP_Text finalComboText)
        {
            Image panel = CreatePanel("Panel", canvasTransform, new Color(0.03f, 0.05f, 0.08f, 0.78f));
            RectTransform panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = new Vector2(18f, 18f);
            panelRect.offsetMax = new Vector2(-18f, -18f);

            CreateText(
                "TitleText",
                panel.transform,
                "BLADE FRENZY",
                42,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -30f),
                new Vector2(820f, 54f),
                out _,
                new Color(0.97f, 0.98f, 1f));

            CreateText(
                "StatusText",
                panel.transform,
                "Slice to survive.",
                22,
                FontStyles.Normal,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -68f),
                new Vector2(820f, 36f),
                out statusText,
                new Color(0.7f, 0.84f, 0.93f));

            CreateMetricCard("ScoreCard", panel.transform, new Vector2(235f, 86f), new Vector2(-210f, -152f), "SCORE", out scoreText, Color.white);
            CreateMetricCard("ComboCard", panel.transform, new Vector2(235f, 86f), new Vector2(0f, -152f), "COMBO", out comboText, new Color(1f, 0.84f, 0.28f));
            CreateMetricCard("MultiplierCard", panel.transform, new Vector2(235f, 86f), new Vector2(210f, -152f), "MULTIPLIER", out multiplierText, new Color(1f, 0.5f, 0.25f));
            CreateMetricCard("HighScoreCard", panel.transform, new Vector2(235f, 86f), new Vector2(-210f, -258f), "HIGH SCORE", out highScoreText, new Color(0.76f, 0.98f, 0.86f));
            CreateMetricCard("TimerCard", panel.transform, new Vector2(235f, 86f), new Vector2(0f, -258f), "TIME", out timerText, new Color(0.8f, 0.91f, 1f));
            CreateMetricCard("DifficultyCard", panel.transform, new Vector2(235f, 86f), new Vector2(210f, -258f), "DIFFICULTY", out difficultyText, new Color(1f, 0.67f, 0.34f));

            return BuildGameOverPanel(panel.transform, out finalScoreText, out finalComboText);
        }

        private static GameObject BuildGameOverPanel(Transform parent, out TMP_Text finalScoreText, out TMP_Text finalComboText)
        {
            GameObject panelObject = new("GameOverPanel", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(parent, false);

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(680f, 210f);
            panelRect.anchoredPosition = new Vector2(0f, 70f);

            Image image = panelObject.GetComponent<Image>();
            image.color = new Color(0.11f, 0.04f, 0.04f, 0.92f);

            CreateText(
                "GameOverTitle",
                panelObject.transform,
                "RUN OVER",
                38,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -30f),
                new Vector2(560f, 48f),
                out _);

            CreateText(
                "FinalScoreText",
                panelObject.transform,
                string.Empty,
                26,
                FontStyles.Normal,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -82f),
                new Vector2(540f, 34f),
                out finalScoreText);

            CreateText(
                "FinalComboText",
                panelObject.transform,
                string.Empty,
                26,
                FontStyles.Normal,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -116f),
                new Vector2(540f, 34f),
                out finalComboText);

            CreateButton("RestartButton", panelObject.transform, "Restart", new Vector2(-110f, -162f));
            CreateButton("QuitButton", panelObject.transform, "Quit", new Vector2(110f, -162f));

            panelObject.SetActive(false);
            return panelObject;
        }

        private static void PlaceCanvas(Canvas canvas)
        {
            Transform viewer = Camera.main != null
                ? Camera.main.transform
                : GameObject.Find("XR Origin (XR Rig)")?.GetComponentInChildren<Camera>(true)?.transform;

            if (viewer == null)
            {
                canvas.transform.position = new Vector3(0f, 1.6f, 5f);
                canvas.transform.rotation = Quaternion.identity;
                return;
            }

            Vector3 forward = Vector3.ProjectOnPlane(viewer.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude <= 0.0001f)
                forward = Vector3.forward;

            canvas.transform.position = viewer.position + forward * 5f + Vector3.up * 0.05f;
            canvas.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
                return;

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static void WireReferences(
            ScoreboardUI scoreboardUi,
            Canvas canvas,
            TMP_Text statusText,
            TMP_Text scoreText,
            TMP_Text comboText,
            TMP_Text multiplierText,
            TMP_Text highScoreText,
            TMP_Text timerText,
            TMP_Text difficultyText,
            GameObject gameOverPanel,
            TMP_Text finalScoreText,
            TMP_Text finalComboText)
        {
            SerializedObject serializedObject = new(scoreboardUi);
            serializedObject.FindProperty("scoreboardCanvas").objectReferenceValue = canvas;
            serializedObject.FindProperty("statusText").objectReferenceValue = statusText;
            serializedObject.FindProperty("scoreText").objectReferenceValue = scoreText;
            serializedObject.FindProperty("comboText").objectReferenceValue = comboText;
            serializedObject.FindProperty("multiplierText").objectReferenceValue = multiplierText;
            serializedObject.FindProperty("highScoreText").objectReferenceValue = highScoreText;
            serializedObject.FindProperty("timerText").objectReferenceValue = timerText;
            serializedObject.FindProperty("difficultyText").objectReferenceValue = difficultyText;
            serializedObject.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
            serializedObject.FindProperty("finalScoreText").objectReferenceValue = finalScoreText;
            serializedObject.FindProperty("finalComboText").objectReferenceValue = finalComboText;
            serializedObject.FindProperty("buildRuntimeHudIfMissing").boolValue = false;
            serializedObject.FindProperty("followViewer").boolValue = false;
            serializedObject.FindProperty("placeFromViewerOnStart").boolValue = false;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Image CreatePanel(string objectName, Transform parent, Color color)
        {
            GameObject panelObject = new(objectName, typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(parent, false);

            Image image = panelObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static void CreateMetricCard(
            string objectName,
            Transform parent,
            Vector2 size,
            Vector2 anchoredPosition,
            string label,
            out TMP_Text valueText,
            Color accentColor)
        {
            Image card = CreatePanel(objectName, parent, new Color(0.09f, 0.12f, 0.16f, 0.9f));
            RectTransform cardRect = (RectTransform)card.transform;
            cardRect.anchorMin = new Vector2(0.5f, 1f);
            cardRect.anchorMax = new Vector2(0.5f, 1f);
            cardRect.sizeDelta = size;
            cardRect.anchoredPosition = anchoredPosition;

            CreateText(
                objectName + "_Label",
                card.transform,
                label,
                18,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -18f),
                new Vector2(size.x - 20f, 24f),
                out _,
                new Color(0.63f, 0.72f, 0.82f));

            CreateText(
                objectName + "_Value",
                card.transform,
                "0",
                34,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, -6f),
                new Vector2(size.x - 24f, 40f),
                out valueText,
                accentColor);
        }

        private static void CreateText(
            string objectName,
            Transform parent,
            string text,
            float fontSize,
            FontStyles fontStyle,
            TextAlignmentOptions alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 size,
            out TMP_Text label,
            Color? color = null)
        {
            GameObject textObject = new(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            TextMeshProUGUI textLabel = textObject.GetComponent<TextMeshProUGUI>();
            textLabel.text = text;
            textLabel.fontSize = fontSize;
            textLabel.fontStyle = fontStyle;
            textLabel.alignment = alignment;
            textLabel.color = color ?? Color.white;
            label = textLabel;
        }

        private static void CreateButton(string objectName, Transform parent, string label, Vector2 anchoredPosition)
        {
            GameObject buttonObject = new(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 1f);
            buttonRect.anchorMax = new Vector2(0.5f, 1f);
            buttonRect.sizeDelta = new Vector2(180f, 54f);
            buttonRect.anchoredPosition = anchoredPosition;

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0.86f, 0.31f, 0.19f, 0.96f);

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = buttonImage;

            CreateText(
                "Label",
                buttonObject.transform,
                label,
                28,
                FontStyles.Bold,
                TextAlignmentOptions.Center,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(160f, 40f),
                out _);
        }
    }
}
