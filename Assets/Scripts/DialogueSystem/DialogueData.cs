using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class DialogueData
{
    [Header("📍 對話索引")]
    [HideInInspector]
    public int dialogueIndex = -1;
    
    [Header("對話內容")]
    public string characterName;
    public string dialogueText;
    public Sprite characterSprite;
    public Sprite backgroundSprite;
    public List<ChoiceData> choices = new List<ChoiceData>();
    public bool hasChoices => choices.Count > 0;
    
    [Header("音效設定")]
    public AudioClip voiceClip;
    public AudioClip backgroundMusic;
    
    [Header("特殊效果")]
    public DialogueEffect effect = DialogueEffect.None;
    public bool dimCharacter = false;        // 角色變灰 (不在說話狀態)
    
    [Header("對話觸發事件")]
    public UnityEvent onDialogueStart;  // 對話開始時觸發
}

[System.Serializable]
public class ChoiceData
{
    public string choiceText;
    public int nextDialogueId;
    public bool isAvailable = true;
    
    public ChoiceData(string text, int nextId)
    {
        choiceText = text;
        nextDialogueId = nextId;
    }
}

[System.Serializable]
public class DialogueSequence
{
    public int id;
    public List<DialogueData> dialogues = new List<DialogueData>();
    public string sequenceName;
}

public enum DialogueEffect
{
    None,
    Shake,
    Flash,
    SlowText,
    FastText
}