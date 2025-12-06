using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 第三幕第二章: 天台偶遇
/// 展示巢狀選項（選擇觸發選擇）
/// </summary>
public class Chapter3Act2Manager : MonoBehaviour
{
    [Header("管理器引用")]
    public ChoiceManager choiceManager;
    public RelationshipManager relationshipManager;
    public DialogueManager dialogueManager;
    
    [Header("對話資源")]
    public DialogueSequenceAsset initialDialogue;
    public DialogueSequenceAsset hinataExplanationDialogue;
    public DialogueSequenceAsset supportDialogue;
    public DialogueSequenceAsset neutralDialogue;
    public DialogueSequenceAsset dontAskDialogue;
    public DialogueSequenceAsset endingDialogue;
    
    private bool askedReason = false;
    
    void Start()
    {
        ChoiceManager.OnChoiceMade += HandleChoiceMade;
        StartCoroutine(StartEventSequence());
    }
    
    void OnDestroy()
    {
        ChoiceManager.OnChoiceMade -= HandleChoiceMade;
    }
    
    IEnumerator StartEventSequence()
    {
        // 播放初始對話
        if (initialDialogue != null)
        {
            dialogueManager.StartDialogue(initialDialogue);
            yield return new WaitUntil(() => !dialogueManager.IsDialogueActive());
        }
        
        // 顯示第一層選項
        ShowFirstChoice();
    }
    
    /// <summary>
    /// 第一層選擇: 是否詢問退團理由
    /// </summary>
    void ShowFirstChoice()
    {
        ChoiceData[] choices = new ChoiceData[]
        {
            // 選項 A1: 詢問理由
            new ChoiceData
            {
                choiceText = "為什麼……你會退出？",
                targetCharacter = "",  // 不影響好感度
                relationshipEffect = RelationshipEffect.None
            },
            
            // 選項 A2: 不詢問
            new ChoiceData
            {
                choiceText = "（不詢問退團理由）",
                targetCharacter = "",
                relationshipEffect = RelationshipEffect.None
            }
        };
        
        choiceManager.ShowChoices(choices);
    }
    
    /// <summary>
    /// 第二層選擇: 是否支持日向
    /// </summary>
    void ShowSecondChoice()
    {
        ChoiceData[] choices = new ChoiceData[]
        {
            // 選項 B1: 支持 (+10 好感，日向會回來)
            new ChoiceData
            {
                choiceText = "我……嗯，我覺得你可以回去的。你喜歡的，應該去做。",
                targetCharacter = "山瀨日向",
                relationshipEffect = RelationshipEffect.Increase,
                characterExpression = "開心"
            },
            
            // 選項 B2: 保持觀望 (±0 好感，日向不回來)
            new ChoiceData
            {
                choiceText = "嗯……那就……隨你吧。我是覺得，決定還是要靠你自己……",
                targetCharacter = "山瀨日向",
                relationshipEffect = RelationshipEffect.None,
                characterExpression = "正常"
            }
        };
        
        choiceManager.ShowChoices(choices);
    }
    
    /// <summary>
    /// 處理選擇結果
    /// </summary>
    void HandleChoiceMade(int choiceIndex)
    {
        if (!askedReason)
        {
            // 處理第一層選擇
            if (choiceIndex == 0)
            {
                // 選擇詢問
                askedReason = true;
                relationshipManager.SetStoryFlag("asked_hinata_reason", true);
                
                StartCoroutine(ContinueToSecondChoice());
            }
            else
            {
                // 選擇不詢問 → 日向不會回來
                relationshipManager.SetStoryFlag("asked_hinata_reason", false);
                relationshipManager.SetStoryFlag("hinata_returned", false);
                
                StartCoroutine(PlayEndingDialogue());
            }
        }
        else
        {
            // 處理第二層選擇
            if (choiceIndex == 0)
            {
                // 支持日向 → 日向會回來
                relationshipManager.SetStoryFlag("hinata_returned", true);
                
                StartCoroutine(PlaySupportDialogue());
            }
            else
            {
                // 保持觀望 → 日向不回來
                relationshipManager.SetStoryFlag("hinata_returned", false);
                
                StartCoroutine(PlayNeutralDialogue());
            }
        }
    }
    
    IEnumerator ContinueToSecondChoice()
    {
        // 播放日向解釋對話
        if (hinataExplanationDialogue != null)
        {
            dialogueManager.StartDialogue(hinataExplanationDialogue);
            yield return new WaitUntil(() => !dialogueManager.IsDialogueActive());
        }
        
        // 顯示第二層選項
        ShowSecondChoice();
    }
    
    IEnumerator PlaySupportDialogue()
    {
        if (supportDialogue != null)
        {
            dialogueManager.StartDialogue(supportDialogue);
            yield return new WaitUntil(() => !dialogueManager.IsDialogueActive());
        }
        
        StartCoroutine(PlayEndingDialogue());
    }
    
    IEnumerator PlayNeutralDialogue()
    {
        if (neutralDialogue != null)
        {
            dialogueManager.StartDialogue(neutralDialogue);
            yield return new WaitUntil(() => !dialogueManager.IsDialogueActive());
        }
        
        StartCoroutine(PlayEndingDialogue());
    }
    
    IEnumerator PlayEndingDialogue()
    {
        if (endingDialogue != null)
        {
            dialogueManager.StartDialogue(endingDialogue);
            yield return new WaitUntil(() => !dialogueManager.IsDialogueActive());
        }
        
        // 事件結束，可以轉場或繼續劇情
        Debug.Log("天台事件結束");
    }
}
