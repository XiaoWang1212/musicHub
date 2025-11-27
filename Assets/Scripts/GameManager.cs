using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("系統管理器")]
    public DialogueManager dialogueManager;
    public CharacterDisplay characterDisplay;
    public BackgroundManager backgroundManager;
    
    [Header("遊戲設定")]
    public List<DialogueSequence> gameDialogues = new List<DialogueSequence>();
    public int currentSequenceIndex = 0;
    
    [Header("遊戲狀態")]
    public bool isGamePaused = false;
    public bool isInDialogue = false;
    
    [Header("輸入設定")]
    public KeyCode skipKey = KeyCode.Escape;
    public KeyCode autoModeKey = KeyCode.A;
    public KeyCode saveKey = KeyCode.S;
    public KeyCode loadKey = KeyCode.L;
    
    // 遊戲狀態
    private GameState currentGameState = GameState.MainMenu;
    
    public enum GameState
    {
        MainMenu,
        InGame,
        Paused,
        Settings,
        Dialogue
    }
    
    void Start()
    {
        InitializeGame();
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void InitializeGame()
    {
        // 註冊對話系統事件
        DialogueManager.OnDialogueStart += OnDialogueStart;
        DialogueManager.OnDialogueEnd += OnDialogueEnd;
        DialogueManager.OnChoiceSelected += OnChoiceSelected;
        
        // 設置初始遊戲狀態
        SetGameState(GameState.MainMenu);
        
        Debug.Log("劇情向遊戲系統初始化完成");
    }
    
    void HandleInput()
    {
        // 跳過/快進
        if (Input.GetKeyDown(skipKey))
        {
            HandleSkipInput();
        }
        
        // 自動模式切換
        if (Input.GetKeyDown(autoModeKey))
        {
            ToggleAutoMode();
        }
        
        // 存檔
        if (Input.GetKeyDown(saveKey))
        {
            SaveGame();
        }
        
        // 讀檔
        if (Input.GetKeyDown(loadKey))
        {
            LoadGame();
        }
        
        // 暫停遊戲
        if (Input.GetKeyDown(KeyCode.Escape) && currentGameState == GameState.InGame)
        {
            TogglePause();
        }
    }
    
    // 遊戲控制方法
    public void StartGame()
    {
        SetGameState(GameState.InGame);
        
        if (gameDialogues.Count > 0)
        {
            StartDialogueSequence(0);
        }
        else
        {
            Debug.LogWarning("沒有找到對話序列！");
        }
    }
    
    public void StartDialogueSequence(int sequenceIndex)
    {
        if (sequenceIndex >= 0 && sequenceIndex < gameDialogues.Count)
        {
            currentSequenceIndex = sequenceIndex;
            dialogueManager.StartDialogue(gameDialogues[sequenceIndex]);
        }
        else
        {
            Debug.LogError($"對話序列索引 {sequenceIndex} 超出範圍！");
        }
    }
    
    public void SetGameState(GameState newState)
    {
        GameState previousState = currentGameState;
        currentGameState = newState;
        
        switch (newState)
        {
            case GameState.MainMenu:
                isGamePaused = false;
                isInDialogue = false;
                break;
            case GameState.InGame:
                isGamePaused = false;
                break;
            case GameState.Paused:
                isGamePaused = true;
                break;
            case GameState.Dialogue:
                isInDialogue = true;
                break;
        }
        
        Debug.Log($"遊戲狀態從 {previousState} 變更為 {newState}");
    }
    
    void HandleSkipInput()
    {
        switch (currentGameState)
        {
            case GameState.Dialogue:
                break;
            case GameState.Paused:
                TogglePause();
                break;
        }
    }
    
    void ToggleAutoMode()
    {
        if (dialogueManager != null && currentGameState == GameState.Dialogue)
        {
            // 切換自動模式（這裡需要在 DialogueManager 中實現獲取當前狀態的方法）
            bool currentAutoMode = false; // 這裡應該從 DialogueManager 獲取當前狀態
            dialogueManager.SetAutoMode(!currentAutoMode);
            Debug.Log($"自動模式：{!currentAutoMode}");
        }
    }
    
    void TogglePause()
    {
        if (currentGameState == GameState.Paused)
        {
            SetGameState(GameState.InGame);
        }
        else if (currentGameState == GameState.InGame)
        {
            SetGameState(GameState.Paused);
        }
    }
    
    // 對話系統事件處理
    void OnDialogueStart(DialogueData dialogueData)
    {
        SetGameState(GameState.Dialogue);
        Debug.Log($"對話開始：{dialogueData.characterName}: {dialogueData.dialogueText}");
    }
    
    void OnDialogueEnd()
    {
        SetGameState(GameState.InGame);
        
        // 檢查是否有下一個對話序列
        if (currentSequenceIndex + 1 < gameDialogues.Count)
        {
            // 可以選擇自動開始下一個序列或等待玩家操作
            Debug.Log("對話序列結束，準備下一個序列");
        }
        else
        {
            Debug.Log("所有對話序列完成");
            // 遊戲結束處理
            OnGameComplete();
        }
    }
    
    void OnChoiceSelected(int choiceId)
    {
        Debug.Log($"玩家選擇了選項：{choiceId}");
        // 這裡可以根據選擇進行分支處理
    }
    
    void OnGameComplete()
    {
        Debug.Log("遊戲完成！");
        // 顯示結束畫面或回到主選單
        SetGameState(GameState.MainMenu);
    }
    
    // 存檔/讀檔系統（基礎實現）
    void SaveGame()
    {
        if (currentGameState == GameState.InGame || currentGameState == GameState.Dialogue)
        {
            GameSaveData saveData = new GameSaveData
            {
                currentSequenceIndex = this.currentSequenceIndex,
                currentDialogueIndex = 0, // 這裡需要從 DialogueManager 獲取
                gameState = this.currentGameState
            };
            
            string saveDataJson = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString("GameSaveData", saveDataJson);
            PlayerPrefs.Save();
            
            Debug.Log("遊戲已存檔");
        }
    }
    
    void LoadGame()
    {
        if (PlayerPrefs.HasKey("GameSaveData"))
        {
            string saveDataJson = PlayerPrefs.GetString("GameSaveData");
            GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(saveDataJson);
            
            currentSequenceIndex = saveData.currentSequenceIndex;
            SetGameState(saveData.gameState);
            
            // 載入對應的對話序列
            if (saveData.gameState == GameState.Dialogue)
            {
                StartDialogueSequence(saveData.currentSequenceIndex);
            }
            
            Debug.Log("遊戲已讀檔");
        }
        else
        {
            Debug.LogWarning("沒有找到存檔資料");
        }
    }
    
    // 清理資源
    void OnDestroy()
    {
        DialogueManager.OnDialogueStart -= OnDialogueStart;
        DialogueManager.OnDialogueEnd -= OnDialogueEnd;
        DialogueManager.OnChoiceSelected -= OnChoiceSelected;
    }
}

[System.Serializable]
public class GameSaveData
{
    public int currentSequenceIndex;
    public int currentDialogueIndex;
    public GameManager.GameState gameState;
}
