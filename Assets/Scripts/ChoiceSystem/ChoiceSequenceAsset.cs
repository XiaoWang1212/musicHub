using UnityEngine;
using System;

/// <summary>
/// 選擇序列資產 - 可在 Inspector 中設定選項
/// 類似 DialogueSequenceAsset 的設計
/// </summary>
[CreateAssetMenu(fileName = "NewChoiceSequence", menuName = "MusicHub/Choice Sequence")]
public class ChoiceSequenceAsset : ScriptableObject
{
    [Header("選擇設定")]
    [Tooltip("這組選擇的描述(僅用於編輯器識別)")]
    public string choiceDescription = "新的選擇";
    
    [Tooltip("選項列表")]
    public ChoiceOption[] choices;
}

/// <summary>
/// 單一選項資料
/// </summary>
[System.Serializable]
public class ChoiceOption
{
    [Header("選項文字")]
    [TextArea(2, 4)]
    public string choiceText = "選項文字";
    
    [Header("好感度影響")]
    [Tooltip("影響的角色名稱")]
    public string targetCharacter = "大野陽斗";
    
    [Tooltip("好感度效果")]
    public RelationshipEffect relationshipEffect = RelationshipEffect.None;
    
    [Header("後續對話(可選)")]
    [Tooltip("選擇後要播放的對話序列")]
    public DialogueSequenceAsset branchDialogue;
    
    [Header("角色表情(可選)")]
    [Tooltip("選擇後角色的表情變化")]
    public string characterExpression = "";
    
    /// <summary>
    /// 轉換為 ChoiceData
    /// </summary>
    public ChoiceData ToChoiceData()
    {
        return new ChoiceData
        {
            choiceText = this.choiceText,
            targetCharacter = this.targetCharacter,
            relationshipEffect = this.relationshipEffect,
            branchDialogue = this.branchDialogue,
            characterExpression = this.characterExpression
        };
    }
}
