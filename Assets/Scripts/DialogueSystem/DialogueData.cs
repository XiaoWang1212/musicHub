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
    public bool isNarration = false;     // 勾選後為旁白，不顯示角色名字
    public string characterName;
    public string dialogueText;
    public Sprite characterSprite;
    public Sprite backgroundSprite;
    public List<ChoiceData> choices = new List<ChoiceData>();
    public bool hasChoices => choices.Count > 0;
    
    [Header("多角色顯示")]
    public bool useMultipleCharacters = false;  // 是否使用多角色模式
    public List<CharacterDisplayData> characters = new List<CharacterDisplayData>();
    
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

/// <summary>
/// 多角色顯示數據 - 用於在對話中同時顯示多個角色
/// </summary>
[System.Serializable]
public class CharacterDisplayData
{
    [Header("角色資訊")]
    public string characterName;        // 角色名稱
    public Sprite characterSprite;      // 角色圖片
    
    [Header("顯示設定")]
    public CharacterPosition position = CharacterPosition.Center;  // 角色位置
    public bool dimCharacter = false;   // 是否變暗 (非說話者)
}

/// <summary>
/// 角色在螢幕上的位置
/// </summary>
public enum CharacterPosition
{
    Left,       // 左側
    Center,     // 中央
    Right       // 右側
}