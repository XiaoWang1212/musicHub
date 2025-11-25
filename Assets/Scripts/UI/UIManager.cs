using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("主選單UI")]
    public GameObject mainMenuPanel;
    public Button startButton;
    public Button loadButton;
    public Button settingsButton;
    public Button exitButton;
    
    [Header("遊戲內UI")]
    public GameObject gamePanel;
    public Button menuButton;
    public Button saveButton;
    public Button autoButton;
    public TextMeshProUGUI gameStateText;
    
    [Header("暫停選單")]
    public GameObject pauseMenuPanel;
    public Button resumeButton;
    public Button saveGameButton;
    public Button mainMenuButton;
    
    [Header("設定選單")]
    public GameObject settingsPanel;
    public Slider typingSpeedSlider;
    public Slider musicVolumeSlider;
    public Slider soundEffectVolumeSlider;
    public Button backButton;
    
    private GameManager gameManager;
    
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        InitializeUI();
        SetupButtonEvents();
    }
    
    void InitializeUI()
    {
        // 初始化時顯示主選單
        ShowMainMenu();
        
        // 設置滑桿初始值
        if (typingSpeedSlider != null)
            typingSpeedSlider.value = 0.05f;
        
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = 0.7f;
            
        if (soundEffectVolumeSlider != null)
            soundEffectVolumeSlider.value = 0.8f;
    }
    
    void SetupButtonEvents()
    {
        // 主選單按鈕
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClick);
            
        if (loadButton != null)
            loadButton.onClick.AddListener(OnLoadButtonClick);
            
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsButtonClick);
            
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitButtonClick);
        
        // 遊戲內按鈕
        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenuButtonClick);
            
        if (saveButton != null)
            saveButton.onClick.AddListener(OnSaveButtonClick);
            
        if (autoButton != null)
            autoButton.onClick.AddListener(OnAutoButtonClick);
        
        // 暫停選單按鈕
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeButtonClick);
            
        if (saveGameButton != null)
            saveGameButton.onClick.AddListener(OnSaveGameButtonClick);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClick);
        
        // 設定選單按鈕
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClick);
            
        // 滑桿事件
        if (typingSpeedSlider != null)
            typingSpeedSlider.onValueChanged.AddListener(OnTypingSpeedChanged);
            
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            
        if (soundEffectVolumeSlider != null)
            soundEffectVolumeSlider.onValueChanged.AddListener(OnSoundEffectVolumeChanged);
    }
    
    // UI 顯示方法
    public void ShowMainMenu()
    {
        SetPanelActive(mainMenuPanel, true);
        SetPanelActive(gamePanel, false);
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(settingsPanel, false);
    }
    
    public void ShowGameUI()
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(gamePanel, true);
        SetPanelActive(pauseMenuPanel, false);
        SetPanelActive(settingsPanel, false);
    }
    
    public void ShowPauseMenu()
    {
        SetPanelActive(pauseMenuPanel, true);
        SetPanelActive(gamePanel, false);
    }
    
    public void ShowSettings()
    {
        SetPanelActive(settingsPanel, true);
        SetPanelActive(mainMenuPanel, false);
    }
    
    void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }
    
    // 按鈕事件處理
    void OnStartButtonClick()
    {
        Debug.Log("開始遊戲");
        ShowGameUI();
        
        if (gameManager != null)
            gameManager.StartGame();
    }
    
    void OnLoadButtonClick()
    {
        Debug.Log("載入遊戲");
        ShowGameUI();
        
        if (gameManager != null)
        {
            // 這裡應該調用 gameManager 的載入方法
            // gameManager.LoadGame();
        }
    }
    
    void OnSettingsButtonClick()
    {
        Debug.Log("開啟設定");
        ShowSettings();
    }
    
    void OnExitButtonClick()
    {
        Debug.Log("退出遊戲");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    void OnMenuButtonClick()
    {
        Debug.Log("開啟暫停選單");
        ShowPauseMenu();
    }
    
    void OnSaveButtonClick()
    {
        Debug.Log("快速存檔");
        
        if (gameManager != null)
        {
            // gameManager.SaveGame();
        }
    }
    
    void OnAutoButtonClick()
    {
        Debug.Log("切換自動模式");
        
        if (gameManager != null && gameManager.dialogueManager != null)
        {
            // 切換自動模式
            // gameManager.dialogueManager.ToggleAutoMode();
        }
    }
    
    void OnResumeButtonClick()
    {
        Debug.Log("繼續遊戲");
        ShowGameUI();
        
        if (gameManager != null)
        {
            gameManager.SetGameState(GameManager.GameState.InGame);
        }
    }
    
    void OnSaveGameButtonClick()
    {
        Debug.Log("存檔遊戲");
        
        if (gameManager != null)
        {
            // gameManager.SaveGame();
        }
    }
    
    void OnMainMenuButtonClick()
    {
        Debug.Log("返回主選單");
        ShowMainMenu();
        
        if (gameManager != null)
        {
            gameManager.SetGameState(GameManager.GameState.MainMenu);
        }
    }
    
    void OnBackButtonClick()
    {
        Debug.Log("返回上一頁");
        ShowMainMenu();
    }
    
    // 滑桿事件處理
    void OnTypingSpeedChanged(float value)
    {
        Debug.Log($"打字速度變更：{value}");
        
        if (gameManager != null && gameManager.dialogueManager != null)
        {
            gameManager.dialogueManager.SetTypingSpeed(value);
        }
    }
    
    void OnMusicVolumeChanged(float value)
    {
        Debug.Log($"音樂音量變更：{value}");
        
        // 設定音樂音量
        AudioListener.volume = value;
    }
    
    void OnSoundEffectVolumeChanged(float value)
    {
        Debug.Log($"音效音量變更：{value}");
        
        // 這裡可以設定音效音量
    }
    
    // 更新遊戲狀態顯示
    void Update()
    {
        if (gameStateText != null && gameManager != null)
        {
            var gameState = gameManager.GetType().GetField("currentGameState", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(gameManager);
            gameStateText.text = $"遊戲狀態: {gameState}";
        }
    }
}