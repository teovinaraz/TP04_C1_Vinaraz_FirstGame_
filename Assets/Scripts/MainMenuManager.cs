using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private Button buttonPlay;
    [SerializeField] private Button buttonSettings;
    [SerializeField] private Button buttonCredits;
    [SerializeField] private Button buttonExit;
    [SerializeField] private Button buttonSettingsBack;
    [SerializeField] private Slider sliderPlayerOneSpeed;
    [SerializeField] private TMP_Text sliderValuePlayerOneSpeed;
    [SerializeField] private Slider sliderPlayerTwoSpeed;
    [SerializeField] private TMP_Text sliderValuePlayerTwoSpeed;
    [SerializeField] private Button buttonCreditsBack;

    private GameSettings settings;
    private Slider sliderPaddleSize;
    private Slider sliderRounds;
    private TMP_Text valuePaddleSize;
    private TMP_Text valueRounds;

    private void Awake()
    {
        settings = GameSettings.Load();
        EnsureEventSystem();

        buttonPlay.onClick.AddListener(OnPlayClicked);
        buttonSettings.onClick.AddListener(OnSettingsClicked);
        buttonCredits.onClick.AddListener(OnCreditsClicked);
        buttonExit.onClick.AddListener(OnExitClicked);
        buttonSettingsBack.onClick.AddListener(OnSettingsBackClicked);
        buttonCreditsBack.onClick.AddListener(OnCreditsBackClicked);
        sliderPlayerOneSpeed.onValueChanged.AddListener(OnPlayerOneSpeed);
        sliderPlayerTwoSpeed.onValueChanged.AddListener(OnPlayerTwoSpeed);
    }

    private void Start()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        SetupSpeedSliders();
        CreateExtraSettings();
        buttonSettingsBack.transform.SetAsLastSibling();
    }

    private void SetupSpeedSliders()
    {
        SetupSlider(sliderPlayerOneSpeed, 5f, 40f, settings.playerOneSpeed, false);
        SetupSlider(sliderPlayerTwoSpeed, 5f, 40f, settings.playerTwoSpeed, false);
        UpdateSpeedLabels();
    }

    private void CreateExtraSettings()
    {
        GameObject root = new GameObject("Extra Settings");
        root.transform.SetParent(settingsPanel.transform, false);
        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = new Vector2(1f, 1f);
        LayoutElement rootLayout = root.AddComponent<LayoutElement>();
        rootLayout.ignoreLayout = true;

        CreateSettingRow(root.transform, "PADDLE SIZE", 0.5f, 2f, settings.playerPaddleScale, false, 660f, out sliderPaddleSize, out valuePaddleSize);
        CreateSettingRow(root.transform, "ROUNDS TO WIN", 1f, 5f, settings.pointsToWin, true, 745f, out sliderRounds, out valueRounds);

        sliderPaddleSize.onValueChanged.AddListener(OnPaddleSize);
        sliderRounds.onValueChanged.AddListener(OnRounds);
    }

    private void CreateSettingRow(Transform parent, string labelText, float min, float max, float value, bool wholeNumbers, float y,
        out Slider slider, out TMP_Text valueText)
    {
        GameObject row = new GameObject(labelText);
        row.transform.SetParent(parent, false);

        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.5f, 0.5f);
        rowRect.anchorMax = new Vector2(0.5f, 0.5f);
        rowRect.pivot = new Vector2(0.5f, 0.5f);
        rowRect.anchoredPosition = new Vector2(700f, -y);
        rowRect.sizeDelta = new Vector2(520f, 70f);

        TMP_Text label = CreateText(row.transform, labelText, 30f, new Vector2(-150f, 0f), new Vector2(230f, 55f));
        label.alignment = TextAlignmentOptions.Center;

        slider = CreateSlider(row.transform, new Vector2(60f, 0f), new Vector2(240f, 32f));
        SetupSlider(slider, min, max, value, wholeNumbers);

        valueText = CreateText(row.transform, wholeNumbers ? value.ToString("F0") : value.ToString("F1"), 30f, new Vector2(235f, 0f), new Vector2(70f, 55f));
    }

    private Slider CreateSlider(Transform parent, Vector2 position, Vector2 size)
    {
        GameObject go = new GameObject("Slider");
        go.transform.SetParent(parent, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Slider slider = go.AddComponent<Slider>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.interactable = true;

        GameObject background = new GameObject("Background");
        background.transform.SetParent(go.transform, false);
        RectTransform backgroundRect = background.AddComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.35f);
        backgroundRect.anchorMax = new Vector2(1f, 0.65f);
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.sprite = CreateUiSprite(new Color(0.12f, 0.09f, 0.07f, 1f));
        backgroundImage.raycastTarget = true;

        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0f);
        fillAreaRect.anchorMax = new Vector2(1f, 1f);
        fillAreaRect.offsetMin = new Vector2(0f, 0f);
        fillAreaRect.offsetMax = new Vector2(0f, 0f);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0.35f);
        fillRect.anchorMax = new Vector2(0f, 0.65f);
        fillRect.offsetMin = new Vector2(0f, 0f);
        fillRect.offsetMax = new Vector2(0f, 0f);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.sprite = CreateUiSprite(new Color(0.9098f, 0.8275f, 0.6392f, 1f));
        fillImage.raycastTarget = false;

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(go.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0f, 0.5f);
        handleRect.anchorMax = new Vector2(0f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(24f, 24f);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.sprite = CreateUiSprite(new Color(0.9098f, 0.8275f, 0.6392f, 1f));
        handleImage.raycastTarget = true;

        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        slider.onValueChanged.AddListener(value => { });

        return slider;
    }

    private Sprite CreateUiSprite(Color color)
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    private void SetupSlider(Slider slider, float min, float max, float value, bool wholeNumbers)
    {
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = wholeNumbers;
        slider.interactable = true;
        slider.SetValueWithoutNotify(Mathf.Clamp(value, min, max));
    }

    private TMP_Text CreateText(Transform parent, string textValue, float size, Vector2 position, Vector2 dimensions)
    {
        GameObject go = new GameObject(textValue);
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = dimensions;

        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.font = sliderValuePlayerOneSpeed.font;
        text.fontSize = size;
        text.color = new Color(0.9098f, 0.8275f, 0.6392f, 1f);
        text.alignment = TextAlignmentOptions.Center;
        text.text = textValue;
        return text;
    }

    private void OnPlayClicked() => SceneManager.LoadScene("Gameplay");

    private void OnSettingsClicked()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    private void OnCreditsClicked()
    {
        mainMenuPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    private void OnSettingsBackClicked()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    private void OnCreditsBackClicked()
    {
        creditsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    private void OnPlayerOneSpeed(float value)
    {
        settings.playerOneSpeed = value;
        UpdateSpeedLabels();
    }

    private void OnPlayerTwoSpeed(float value)
    {
        settings.playerTwoSpeed = value;
        UpdateSpeedLabels();
    }

    private void OnPaddleSize(float value)
    {
        settings.playerPaddleScale = value;
        valuePaddleSize.text = value.ToString("F1");
    }

    private void OnRounds(float value)
    {
        settings.pointsToWin = Mathf.RoundToInt(value);
        valueRounds.text = settings.pointsToWin.ToString();
    }

    private void UpdateSpeedLabels()
    {
        sliderValuePlayerOneSpeed.text = settings.playerOneSpeed.ToString("F1");
        sliderValuePlayerTwoSpeed.text = settings.playerTwoSpeed.ToString("F1");
    }

    private void OnExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;
        GameObject go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
    }

    private void OnDestroy()
    {
        buttonPlay.onClick.RemoveListener(OnPlayClicked);
        buttonSettings.onClick.RemoveListener(OnSettingsClicked);
        buttonCredits.onClick.RemoveListener(OnCreditsClicked);
        buttonExit.onClick.RemoveListener(OnExitClicked);
        buttonSettingsBack.onClick.RemoveListener(OnSettingsBackClicked);
        buttonCreditsBack.onClick.RemoveListener(OnCreditsBackClicked);
        sliderPlayerOneSpeed.onValueChanged.RemoveListener(OnPlayerOneSpeed);
        sliderPlayerTwoSpeed.onValueChanged.RemoveListener(OnPlayerTwoSpeed);
        if (sliderPaddleSize != null) sliderPaddleSize.onValueChanged.RemoveListener(OnPaddleSize);
        if (sliderRounds != null) sliderRounds.onValueChanged.RemoveListener(OnRounds);
    }
}
