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
    public Image backgroundImage;
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
        
        // 設定角色圖片
        if (characterRenderer != null)
        {
            // 如果有指定新的 sprite,就更換
            if (currentDialogue.characterSprite != null)
            {
                characterRenderer.sprite = currentDialogue.characterSprite;
            }
            
            // 角色永遠顯示,只改變亮度
            characterRenderer.gameObject.SetActive(true);
            
            // 根據是否有角色名稱決定亮度 (沒有名稱=旁白,角色變暗)
            if (string.IsNullOrEmpty(currentDialogue.characterName))
            {
                // 旁白時,角色變暗 (灰色半透明)
                characterRenderer.color = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            }
            else
            {
                // 角色說話時,恢復正常亮度
                characterRenderer.color = Color.white;
            }
        }
        
        // 設定背景
        if (backgroundImage != null && currentDialogue.backgroundSprite != null)
        {
            backgroundImage.sprite = currentDialogue.backgroundSprite;
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
}