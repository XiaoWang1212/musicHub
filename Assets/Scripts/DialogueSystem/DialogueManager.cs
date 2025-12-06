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
    public SpriteRenderer backgroundRenderer; // 背景
    public GameObject choiceButtonPrefab;
    public Transform choiceButtonContainer;
    
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
    
    // 多角色顯示事件
    public static event Action<List<CharacterDisplayData>> OnMultipleCharactersDisplay;  // 多角色顯示
    public static event Action OnMultipleCharactersHide;  // 隱藏所有角色
    
    // 表情切換事件
    public static event Action<string, string, bool> OnCharacterExpressionChange;  // 角色名稱, 表情名稱, 是否使用動畫
    
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
        
        // 設定角色名稱 (根據 isNarration 決定是否顯示)
        if (characterNameText != null)
        {
            if (currentDialogue.isNarration)
            {
                characterNameText.text = "";  // 旁白不顯示角色名稱
            }
            else
            {
                characterNameText.text = currentDialogue.characterName;  // 正常對話顯示角色名稱
            }
        }
        
        // 處理角色顯示 (支援多角色模式)
        if (currentDialogue.useMultipleCharacters && currentDialogue.characters.Count > 0)
        {
            // 多角色顯示模式
            OnMultipleCharactersDisplay?.Invoke(currentDialogue.characters);
            Debug.Log($"👥 多角色顯示: {currentDialogue.characters.Count} 個角色");
            
            // 處理每個角色的表情
            var characterManager = FindFirstObjectByType<CharacterManager>();
            if (characterManager != null)
            {
                foreach (var charData in currentDialogue.characters)
                {
                    if (!string.IsNullOrEmpty(charData.expression))
                    {
                        characterManager.ChangeCharacterExpression(charData.characterName, charData.expression);
                        Debug.Log($"😊 {charData.characterName} 表情切換: {charData.expression}");
                    }
                }
            }
        }
        else
        {
            // 傳統單角色顯示模式
            if (currentDialogue.isNarration)
            {
                // 旁白模式：如果有指定角色名稱，顯示該角色並變暗
                if (!string.IsNullOrEmpty(currentDialogue.characterName))
                {
                    OnCharacterDisplay?.Invoke(currentDialogue.characterName, 
                                             null,  // 不再傳入 sprite，由 CharacterManager 使用預設 sprite
                                             true);  // 旁白時角色一律變暗
                }
                // 如果沒有指定角色名稱，保持現有顯示狀態
            }
            else
            {
                // 正常對話模式
                if (string.IsNullOrEmpty(currentDialogue.characterName))
                {
                    OnCharacterHide?.Invoke();
                }
                else
                {
                    OnCharacterDisplay?.Invoke(currentDialogue.characterName, 
                                             null,  // 不再傳入 sprite,由 CharacterManager 使用預設 sprite
                                             currentDialogue.dimCharacter);
                    
                    // 處理單人對話的表情切換
                    if (!string.IsNullOrEmpty(currentDialogue.expression))
                    {
                        var characterManager = FindFirstObjectByType<CharacterManager>();
                        if (characterManager != null)
                        {
                            characterManager.ChangeCharacterExpression(currentDialogue.characterName, currentDialogue.expression);
                            Debug.Log($"😊 {currentDialogue.characterName} 表情切換: {currentDialogue.expression}");
                        }
                    }
                }
            }
        }
        // 背景管理已移至 ActManager，此處不再處理背景設定
        
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
    


    // 背景管理已移至 ActManager，移除背景淡入方法

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
        // 同時淡出對話 UI 元素
        Coroutine fadeText = StartCoroutine(FadeOutDialogueText());
        Coroutine fadeName = StartCoroutine(FadeOutCharacterName());
        
        // 等待淡出動畫完成
        yield return fadeText;
        yield return fadeName;
        
        // 隱藏對話面板
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        
        Debug.Log("✅ 對話面板淡出完成");
    }

}