using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 第四章事件 1: 大野陽斗練習場景
/// 展示如何使用 ChoiceManager 和 RelationshipManager
/// </summary>
public class Chapter4Event1Manager : MonoBehaviour
{
    [Header("管理器引用")]
    public ChoiceManager choiceManager;
    public RelationshipManager relationshipManager;
    public DialogueManager dialogueManager;
    
    [Header("對話資源")]
    public DialogueSequenceAsset introDialogue;      // 初始對話
    public DialogueSequenceAsset helpDialogue;       // 選擇幫忙後的對話
    public DialogueSequenceAsset restDialogue;       // 選擇休息後的對話
    public DialogueSequenceAsset mentionStuckDialogue; // 提及卡住後的對話
    
    void Start()
    {
        // 訂閱選擇事件
        ChoiceManager.OnChoiceMade += HandleChoiceMade;
        
        // 開始初始對話
        StartCoroutine(StartEventSequence());
    }
    
    void OnDestroy()
    {
        ChoiceManager.OnChoiceMade -= HandleChoiceMade;
    }
    
    IEnumerator StartEventSequence()
    {
        // 播放初始對話
        if (introDialogue != null)
        {
            dialogueManager.StartDialogue(introDialogue);
            
            // 等待對話結束
            yield return new WaitUntil(() => !dialogueManager.IsDialogueActive());
        }
        
        // 顯示選項
        ShowChoices();
    }
    
    /// <summary>
    /// 顯示選項
    /// </summary>
    void ShowChoices()
    {
        // 創建選項資料
        ChoiceData[] choices = new ChoiceData[]
        {
            // 選項 A: 幫忙 (+10 好感)
            new ChoiceData
            {
                choiceText = "想一起試一下？我聽聽看哪裡卡。",
                targetCharacter = "大野陽斗",
                relationshipEffect = RelationshipEffect.Increase,
                characterExpression = "喜",
                followUpDialogue = helpDialogue
            },
            
            // 選項 B: 休息 (±0 好感)
            new ChoiceData
            {
                choiceText = "先休息一下吧，你彈太久了。",
                targetCharacter = "大野陽斗",
                relationshipEffect = RelationshipEffect.None,
                characterExpression = "平常",
                followUpDialogue = restDialogue
            },
            
            // 選項 C: 提及卡住 (-5 好感)
            new ChoiceData
            {
                choiceText = "你這段已經卡三天了吧。",
                targetCharacter = "大野陽斗",
                relationshipEffect = RelationshipEffect.Decrease,
                characterExpression = "不爽",
                followUpDialogue = mentionStuckDialogue
            }
        };
        
        // 顯示選項
        choiceManager.ShowChoices(choices);
    }
    
    /// <summary>
    /// 處理選擇結果
    /// </summary>
    void HandleChoiceMade(int choiceIndex)
    {
        Debug.Log($"玩家選擇了選項 {choiceIndex}");
        
        // ChoiceManager 會自動處理好感度變化
        // 這裡可以處理其他邏輯，例如播放音效、特效等
        
        // 繼續劇情...
    }
    
    /// <summary>
    /// 測試用：顯示當前好感度
    /// </summary>
    [ContextMenu("顯示大野陽斗好感度")]
    void ShowOhnoRelationship()
    {
        int relationship = relationshipManager.GetRelationshipValue("大野陽斗");
        Debug.Log($"大野陽斗好感度: {relationship}");
    }
}
