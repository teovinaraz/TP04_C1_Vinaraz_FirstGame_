using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Movement playerOne;
    [SerializeField] private Movement playerTwo;
    [SerializeField] private BallMovement ball;
    [SerializeField] private Sprite buttonBackground;

    private GameSettings settings;
    private bool pauseMenuOpen;
    private bool roundInProgress;
    private bool matchOver;
    private float goalTimer;
    private int playerOneScore;
    private int playerTwoScore;
    private GameObject runtimeUi;
    private TMP_Text scoreText;
    private TMP_Text timerText;
    private TMP_Text messageText;
    private Button restartButton;
    private Color textColor = new Color(0.9098f, 0.8275f, 0.6392f, 1f);

    public Movement PlayerOne => playerOne;
    public Movement PlayerTwo => playerTwo;
    public GameSettings Settings => settings;

    private void Awake()
    {
        settings = GameSettings.Load();
        playerOne ??= FindPlayer(true);
        playerTwo ??= FindPlayer(false);
        ball ??= FindFirstObjectByType<BallMovement>();
        PrepareGoalColliders();
        CreateEventSystem();
        CreateRuntimeUI();
    }

    private void Start()
    {
        Time.timeScale = 1f;
        playerOneScore = 0;
        playerTwoScore = 0;
        UpdateScoreUI();
        StartCoroutine(StartRoundRoutine());
        CreateAdvancedSystems();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame)) TogglePause();
        if (pauseMenuOpen || matchOver || !roundInProgress || ball == null) return;

        goalTimer -= Time.deltaTime;
        UpdateTimerUI();

        float goalX = settings.goalX;
        if (ball.transform.position.x <= -goalX) ScorePoint(false);
        else if (ball.transform.position.x >= goalX) ScorePoint(true);
        else if (goalTimer <= 0f) ScorePoint(ball.transform.position.x >= 0f);
    }

    private Movement FindPlayer(bool leftSide)
    {
        Movement[] players = FindObjectsByType<Movement>(FindObjectsSortMode.None);
        foreach (Movement movement in players)
        {
            if ((movement.transform.position.x < 0f) == leftSide) return movement;
        }
        return null;
    }

    private void PrepareGoalColliders()
    {
        SetTrigger("Left");
        SetTrigger("Right");
    }

    private void SetTrigger(string objectName)
    {
        GameObject target = GameObject.Find(objectName);
        Collider2D collider = target != null ? target.GetComponent<Collider2D>() : null;
        if (collider != null) collider.isTrigger = true;
    }

    private void CreateEventSystem()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem != null) return;

        GameObject go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>();
    }

    private IEnumerator StartRoundRoutine()
    {
        roundInProgress = false;
        matchOver = false;
        SetPlayersEnabled(false);
        ball.ResetBall(Vector2.zero);
        goalTimer = settings.goalTimeLimit;
        messageText.text = "GET READY";
        yield return new WaitForSeconds(settings.roundStartDelay);
        messageText.text = string.Empty;
        roundInProgress = true;
        SetPlayersEnabled(true);
        ball.LaunchRandom();
    }

    private void ScorePoint(bool playerOneGetsPoint)
    {
        if (!roundInProgress || matchOver) return;

        roundInProgress = false;
        if (playerOneGetsPoint) playerOneScore++;
        else playerTwoScore++;
        UpdateScoreUI();

        if (playerOneScore >= settings.pointsToWin || playerTwoScore >= settings.pointsToWin)
        {
            StartCoroutine(EndMatchRoutine(playerOneGetsPoint));
            return;
        }

        StartCoroutine(ResetAfterPointRoutine());
    }

    private IEnumerator ResetAfterPointRoutine()
    {
        SetPlayersEnabled(false);
        ball.ResetBall(Vector2.zero);
        goalTimer = settings.goalTimeLimit;
        messageText.text = "POINT!";
        yield return new WaitForSeconds(settings.roundResetDelay);
        messageText.text = string.Empty;
        SetPlayersEnabled(true);
        roundInProgress = true;
        ball.LaunchRandom();
    }

    private IEnumerator EndMatchRoutine(bool playerOneWon)
    {
        SetPlayersEnabled(false);
        ball.ResetBall(Vector2.zero);
        matchOver = true;
        messageText.text = playerOneWon ? "PLAYER 1 WINS!" : "PLAYER 2 WINS!";
        restartButton.gameObject.SetActive(true);
        yield return null;
    }

    private void SetPlayersEnabled(bool enabled)
    {
        playerOne?.SetControlsEnabled(enabled);
        playerTwo?.SetControlsEnabled(enabled);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = $"{playerOneScore}   -   {playerTwoScore}";
    }

    private void UpdateTimerUI()
    {
        if (timerText != null) timerText.text = Mathf.CeilToInt(Mathf.Max(0f, goalTimer)).ToString();
    }

    private void CreateRuntimeUI()
    {
        runtimeUi = new GameObject("Runtime Game UI");
        Canvas canvas = runtimeUi.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        CanvasScaler scaler = runtimeUi.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        runtimeUi.AddComponent<GraphicRaycaster>();

        scoreText = CreateText(canvas.transform, "Score", new Vector2(0.5f, 0.86f), 64);
        timerText = CreateText(canvas.transform, "Timer", new Vector2(0.5f, 0.76f), 42);
        messageText = CreateText(canvas.transform, "Message", new Vector2(0.5f, 0.5f), 56);
        TMP_Text controls = CreateText(canvas.transform, "Controls", new Vector2(0.5f, 0.05f), 22);
        controls.text = "P1: WASD     P2: ARROWS     ESC / P: PAUSE";

        restartButton = CreateButton(canvas.transform, "PLAY AGAIN", new Vector2(0.5f, 0.37f));
        restartButton.gameObject.SetActive(false);
        restartButton.onClick.AddListener(RestartMatch);
    }

    private TMP_Text CreateText(Transform parent, string objectName, Vector2 anchor, float fontSize)
    {
        GameObject go = new GameObject(objectName);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(700, 100);

        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = textColor;
        text.outlineWidth = 0.15f;
        text.outlineColor = Color.black;
        return text;
    }

    private Button CreateButton(Transform parent, string label, Vector2 anchor)
    {
        GameObject go = new GameObject("Restart Button");
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(550, 160);

        Image image = go.AddComponent<Image>();
        image.sprite = buttonBackground;
        image.color = Color.white;
        image.raycastTarget = true;

        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;

        TMP_Text text = CreateText(go.transform, "Label", new Vector2(0.5f, 0.5f), 36);
        text.rectTransform.sizeDelta = new Vector2(550, 160);
        text.text = label;
        return button;
    }

    private void CreateAdvancedSystems()
    {
        GameObject systems = new GameObject("Advanced Systems");
        ArenaSpawner spawner = systems.AddComponent<ArenaSpawner>();
        spawner.Initialize(settings);
    }

    private void RestartMatch()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void TogglePause()
    {
        if (matchOver) return;
        if (pauseMenuOpen) ContinueGame();
        else OpenPauseMenu();
    }

    private void OpenPauseMenu()
    {
        pauseMenuOpen = true;
        if (runtimeUi != null) runtimeUi.SetActive(false);
        Time.timeScale = 0f;
        SceneManager.LoadSceneAsync("PauseMenu", LoadSceneMode.Additive);
    }

    public void ContinueGame()
    {
        pauseMenuOpen = false;
        Time.timeScale = 1f;
        if (runtimeUi != null) runtimeUi.SetActive(true);
        if (SceneManager.GetSceneByName("PauseMenu").isLoaded) SceneManager.UnloadSceneAsync("PauseMenu");
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
