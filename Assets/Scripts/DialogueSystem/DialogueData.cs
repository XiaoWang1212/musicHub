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
    
    [TextArea(3, 10)]
    public string dialogueText;
    
    [Header("😊 表情設定 (單人對話)")]
    [Tooltip("角色的表情名稱(例如: 開心, 難過, 不爽, 平常). 留空則不改變表情")]
    public string expression = "";
    
    [Header("選擇系統")]
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
}

[System.Serializable]
public class ChoiceData
{
    [Header("選項內容")]
    [TextArea(1, 3)]
    public string choiceText;
    
    [Header("🔀 對話分支")]
    [Tooltip("選擇後接續的對話序列(優先)")]
    public DialogueSequenceAsset branchDialogue;
    
    [Tooltip("如果沒有設定分支對話,跳到這個對話 ID")]
    public int nextDialogueId = -1;
    
    public bool isAvailable = true;
    
    [Header("💝 好感度系統")]
    [Tooltip("影響的角色")]
    public string targetCharacter = "";
    
    [Tooltip("好感度變化效果")]
    public RelationshipEffect relationshipEffect = RelationshipEffect.None;
    
    [Header("😊 角色反應")]
    [Tooltip("選擇後角色的表情")]
    public string characterExpression = "";
    
    public ChoiceData(string text, int nextId)
    {
        choiceText = text;
        nextDialogueId = nextId;
    }
    
    public ChoiceData()
    {
        choiceText = "";
        nextDialogueId = -1;
    }
}

/// <summary>
/// 好感度效果類型
/// </summary>
public enum RelationshipEffect
{
    None,       // 無變化
    Increase,   // 增加
    Decrease    // 減少
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
    
    [Header("顯示設定")]
    public CharacterPosition position = CharacterPosition.Center;  // 角色位置
    public bool dimCharacter = false;   // 是否變暗 (非說話者)
    
    [Header("表情設定")]
    [Tooltip("角色的表情名稱(例如: 開心, 難過, 不爽, 平常)")]
    public string expression = "";      // 角色表情
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