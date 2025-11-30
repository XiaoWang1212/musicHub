using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DialogueManager : MonoBehaviour
{
    [Header("UI 元件")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI dialogueText;
    public SpriteRenderer characterRenderer;  // 改用 SpriteRenderer
    public SpriteRenderer backgroundRenderer; // 改用 SpriteRenderer
    public GameObject choiceButtonPrefab;
    public Transform choiceButtonContainer;
    
    [Header("特殊效果")]
    public SpriteRenderer characterMask;     // 角色遮罩 (用於隔門效果)
    
    [Header("音效管理")]
    public AudioSource voiceAudioSource;
    public AudioSource musicAudioSource;
    
    [Header("打字效果設定")]
    public float typingSpeed = 0.05f;
    public bool isAutoMode = false;
    public float autoModeWaitTime = 2f;
    
    [Header("輸入設定")]
    public KeyCode continueKey = KeyCode.Space;  // 繼續鍵
    public bool allowMouseClick = true;          // 允許滑鼠左鍵
    
    // 私有變量
    private DialogueSequence currentSequence;
    private DialogueSequenceAsset currentSequenceAsset;  // 新增:保存 ScriptableObject 以觸發事件
    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private string fullText = "";
    private List<Button> currentChoiceButtons = new List<Button>();
    
    // 事件
    public static event Action<DialogueData> OnDialogueStart;
    public static event Action<int> OnDialogueIndexChanged;  // 新增:傳遞對話索引
    public static event Action OnDialogueEnd;
    public static event Action<int> OnChoiceSelected;
    
    // 角色管理事件
    public static event Action<string, Sprite, bool> OnCharacterDisplay;  // 角色名稱, 圖片, 是否變灰
    public static event Action OnCharacterHide;  // 隱藏角色
    
    void Start()
    {
        InitializeDialogueSystem();
    }
    
    void Update()
    {
        // 空白鍵繼續
        if (Input.GetKeyDown(continueKey))
        {
            ContinueDialogue();
        }
        
        // 滑鼠左鍵繼續
        if (allowMouseClick && Input.GetMouseButtonDown(0))
        {
            ContinueDialogue();
        }
    }
    
    void InitializeDialogueSystem()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
    
    public void StartDialogue(DialogueSequence sequence)
    {
        currentSequence = sequence;
        currentSequenceAsset = null;  // 直接使用 DialogueSequence 時無法觸發事件
        currentDialogueIndex = 0;
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
            
        ShowCurrentDialogue();
        OnDialogueStart?.Invoke(currentSequence.dialogues[currentDialogueIndex]);
    }
    
    // 新增:支持直接傳入 DialogueSequenceAsset
    public void StartDialogue(DialogueSequenceAsset sequenceAsset)
    {
        if (sequenceAsset == null) return;
        
        currentSequenceAsset = sequenceAsset;
        currentSequence = sequenceAsset.ToDialogueSequence();
        currentDialogueIndex = 0;
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
            
        ShowCurrentDialogue();
        OnDialogueStart?.Invoke(currentSequence.dialogues[currentDialogueIndex]);
    }
    
    void ShowCurrentDialogue()
    {
        if (currentSequence == null || currentDialogueIndex >= currentSequence.dialogues.Count)
        {
            EndDialogue();
            return;
        }
        
        DialogueData currentDialogue = currentSequence.dialogues[currentDialogueIndex];
        
        // 觸發對話索引事件
        OnDialogueIndexChanged?.Invoke(currentDialogueIndex);
        
        // 觸發對話開始事件
        currentDialogue.onDialogueStart?.Invoke();
        
        // 設定角色名稱
        if (characterNameText != null)
            characterNameText.text = currentDialogue.characterName;
        
        // 觸發角色顯示事件 (讓其他Manager處理角色顯示)
        if (currentDialogue.characterSprite == null)
        {
            OnCharacterHide?.Invoke();
        }
        else
        {
            OnCharacterDisplay?.Invoke(currentDialogue.characterName, 
                                     currentDialogue.characterSprite, 
                                     currentDialogue.dimCharacter);
        }
        
        // 角色顯示完全由 CharacterManager 統一管理
        // characterRenderer 僅作為後備，通常不會執行到這裡
        

        

        
        // 設定背景 (帶淡入效果)
        if (backgroundRenderer != null && currentDialogue.backgroundSprite != null)
        {
            // 如果背景改變了或是第一次設定背景,執行淡入動畫
            if (backgroundRenderer.sprite != currentDialogue.backgroundSprite || 
                (backgroundRenderer.sprite == null && currentDialogue.backgroundSprite != null))
            {
                StartCoroutine(FadeInBackground(currentDialogue.backgroundSprite));
            }
        }
        
        // 播放語音
        if (voiceAudioSource != null && currentDialogue.voiceClip != null)
        {
            voiceAudioSource.clip = currentDialogue.voiceClip;
            voiceAudioSource.Play();
        }
        
        // 播放背景音樂
        if (musicAudioSource != null && currentDialogue.backgroundMusic != null)
        {
            if (musicAudioSource.clip != currentDialogue.backgroundMusic)
            {
                musicAudioSource.clip = currentDialogue.backgroundMusic;
                musicAudioSource.Play();
            }
        }
        
        // 顯示對話文字（打字效果）
        StartCoroutine(TypeText(currentDialogue.dialogueText));
        
        // 處理選擇按鈕
        if (currentDialogue.hasChoices)
        {
            ShowChoices(currentDialogue.choices);
        }
        else
        {
            ClearChoices();
        }
    }
    
    IEnumerator TypeText(string text)
    {
        isTyping = true;
        fullText = text;
        dialogueText.text = "";
        
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        isTyping = false;
        
        // 自動模式
        if (isAutoMode && !currentSequence.dialogues[currentDialogueIndex].hasChoices)
        {
            yield return new WaitForSeconds(autoModeWaitTime);
            ContinueDialogue();
        }
    }
    
    void ShowChoices(List<ChoiceData> choices)
    {
        ClearChoices();
        
        foreach (ChoiceData choice in choices)
        {
            if (choice.isAvailable)
            {
                GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceButtonContainer);
                Button button = choiceButton.GetComponent<Button>();
                TextMeshProUGUI buttonText = choiceButton.GetComponentInChildren<TextMeshProUGUI>();
                
                if (buttonText != null)
                    buttonText.text = choice.choiceText;
                
                int choiceIndex = choice.nextDialogueId;
                button.onClick.AddListener(() => HandleChoiceSelected(choiceIndex));
                
                currentChoiceButtons.Add(button);
            }
        }
    }
    
    void ClearChoices()
    {
        foreach (Button button in currentChoiceButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }
        currentChoiceButtons.Clear();
    }
    
    void HandleChoiceSelected(int nextDialogueId)
    {
        OnChoiceSelected?.Invoke(nextDialogueId);
        
        // 根據選擇跳轉到對應對話
        if (nextDialogueId >= 0 && nextDialogueId < currentSequence.dialogues.Count)
        {
            currentDialogueIndex = nextDialogueId;
        }
        else
        {
            currentDialogueIndex++;
        }
        
        ShowCurrentDialogue();
    }
    
    public void ContinueDialogue()
    {
        if (isTyping)
        {
            // 如果正在打字，立即顯示完整文字
            StopAllCoroutines();
            dialogueText.text = fullText;
            isTyping = false;
        }
        else
        {
            // 前進到下一個對話
            currentDialogueIndex++;
            ShowCurrentDialogue();
        }
    }
    
    void EndDialogue()
    {
        // 觸發對話結束事件 (索引 -1)
        OnDialogueIndexChanged?.Invoke(-1);
        
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
            
        ClearChoices();
        OnDialogueEnd?.Invoke();
    }
    
    // 公用方法
    public void SetAutoMode(bool autoMode)
    {
        isAutoMode = autoMode;
    }
    
    public void SetTypingSpeed(float speed)
    {
        typingSpeed = speed;
    }
    
    public void SkipCurrentDialogue()
    {
        if (currentSequence != null)
        {
            EndDialogue();
        }
    }
    
    // GameManager需要的額外方法
    public bool GetAutoMode()
    {
        return isAutoMode;
    }
    
    public int GetCurrentDialogueIndex()
    {
        return currentDialogueIndex;
    }
    
    // 外部調用方法 - 用於直接顯示文字（如恐怖序列）
    public void DisplayText(string text, string characterName = "")
    {
        // 啟動對話面板
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
        
        // 設定角色名稱
        if (characterNameText != null)
            characterNameText.text = characterName;
        
        // 直接顯示文字
        if (dialogueText != null)
        {
            StopAllCoroutines(); // 停止之前的打字效果
            StartCoroutine(TypeText(text));
        }
        
        Debug.Log($"💬 顯示文字: {text}");
    }

    // 背景淡入動畫 (針對 SpriteRenderer)
    IEnumerator FadeInBackground(Sprite newBackgroundSprite)
    {
        if (backgroundRenderer == null || newBackgroundSprite == null) yield break;

        // 保存當前透明度
        Color originalColor = backgroundRenderer.color;
        
        // 設定新背景
        backgroundRenderer.sprite = newBackgroundSprite;
        
        // 從透明開始淡入
        Color fadeColor = originalColor;
        fadeColor.a = 0f;
        backgroundRenderer.color = fadeColor;
        
        float elapsed = 0f;
        float fadeDuration = 1f; // 1秒淡入時間
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeColor.a = Mathf.Lerp(0f, originalColor.a, elapsed / fadeDuration);
            backgroundRenderer.color = fadeColor;
            yield return null;
        }
        
        // 確保完全顯示
        backgroundRenderer.color = originalColor;
    }

    // 角色第一次出現的淡入動畫
    IEnumerator FadeInCharacter()
    {
        if (characterRenderer == null) yield break;

        float elapsed = 0f;
        float fadeDuration = 0.8f; // 0.8秒淡入
        Color color = characterRenderer.color;
        Color startColor = color;
        startColor.a = 0f; // 從透明開始
        
        characterRenderer.color = startColor;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            characterRenderer.color = color;
            yield return null;
        }
        
        // 確保完全顯示
        color.a = 1f;
        characterRenderer.color = color;
    }

    // 角色顏色平滑過渡 (說話/旁白切換)
    IEnumerator TransitionCharacterColor(Color targetColor)
    {
        if (characterRenderer == null) yield break;

        float elapsed = 0f;
        float transitionDuration = 0.3f; // 0.3秒過渡
        Color startColor = characterRenderer.color;
        
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / transitionDuration;
            characterRenderer.color = Color.Lerp(startColor, targetColor, progress);
            yield return null;
        }
        
        // 確保到達目標顏色
        characterRenderer.color = targetColor;
    }

    // 角色淡出動畫
    IEnumerator FadeOutCharacter()
    {
        if (characterRenderer == null) yield break;

        float elapsed = 0f;
        float fadeDuration = 0.5f; // 0.5秒淡出
        Color color = characterRenderer.color;
        float startAlpha = color.a;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            characterRenderer.color = color;
            yield return null;
        }
        
        // 確保完全透明並隱藏
        color.a = 0f;
        characterRenderer.color = color;
        characterRenderer.gameObject.SetActive(false);
    }

    // 對話文字淡出
    IEnumerator FadeOutDialogueText()
    {
        if (dialogueText == null) yield break;

        float elapsed = 0f;
        float fadeDuration = 0.5f;
        Color color = dialogueText.color;
        float startAlpha = color.a;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            dialogueText.color = color;
            yield return null;
        }
        
        color.a = 0f;
        dialogueText.color = color;
    }

    // 角色名字淡出
    IEnumerator FadeOutCharacterName()
    {
        if (characterNameText == null) yield break;

        float elapsed = 0f;
        float fadeDuration = 0.5f;
        Color color = characterNameText.color;
        float startAlpha = color.a;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            characterNameText.color = color;
            yield return null;
        }
        
        color.a = 0f;
        characterNameText.color = color;
    }

    // 對話面板整體淡出 (Act2 結束用)
    public IEnumerator FadeOutDialoguePanel()
    {
        // 同時淡出所有對話 UI 元素
        Coroutine fadeText = StartCoroutine(FadeOutDialogueText());
        Coroutine fadeName = StartCoroutine(FadeOutCharacterName());
        Coroutine fadeCharacter = StartCoroutine(FadeOutCharacter());
        
        // 等待所有淡出動畫完成
        yield return fadeText;
        yield return fadeName;
        yield return fadeCharacter;
        
        // 隱藏對話面板
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        Debug.Log("✅ 對話面板淡出完成");
    }

}