using UnityEngine;

public class SampleDialogueCreator : MonoBehaviour
{
    [Header("示範對話序列")]
    public DialogueSequenceAsset[] sampleSequences;
    
    [ContextMenu("Create Sample Dialogue")]
    void CreateSampleDialogue()
    {
        // 創建第一個對話序列
        DialogueSequence sequence1 = new DialogueSequence
        {
            id = 0,
            sequenceName = "開場對話",
            dialogues = new System.Collections.Generic.List<DialogueData>()
        };
        
        // 添加開場對話
        DialogueData opening = new DialogueData
        {
            characterName = "敘述者",
            dialogueText = "歡迎來到這個劇情向遊戲的世界...",
            choices = new System.Collections.Generic.List<ChoiceData>()
        };
        sequence1.dialogues.Add(opening);
        
        // 添加角色登場
        DialogueData characterIntro = new DialogueData
        {
            characterName = "主角",
            dialogueText = "這裡是什麼地方？我怎麼會在這裡？",
            choices = new System.Collections.Generic.List<ChoiceData>()
        };
        sequence1.dialogues.Add(characterIntro);
        
        // 添加選擇對話
        DialogueData choiceDialogue = new DialogueData
        {
            characterName = "神秘聲音",
            dialogueText = "你想要了解真相嗎？",
            choices = new System.Collections.Generic.List<ChoiceData>
            {
                new ChoiceData("是的，我想知道真相", 3),
                new ChoiceData("不，我只想離開這裡", 4)
            }
        };
        sequence1.dialogues.Add(choiceDialogue);
        
        // 選擇結果1
        DialogueData choice1Result = new DialogueData
        {
            characterName = "神秘聲音",
            dialogueText = "很好，那麼你的冒險就此開始...",
            choices = new System.Collections.Generic.List<ChoiceData>()
        };
        sequence1.dialogues.Add(choice1Result);
        
        // 選擇結果2
        DialogueData choice2Result = new DialogueData
        {
            characterName = "神秘聲音",
            dialogueText = "逃避並不能解決問題，但你總會回來的...",
            choices = new System.Collections.Generic.List<ChoiceData>()
        };
        sequence1.dialogues.Add(choice2Result);
        
        // 將序列添加到遊戲管理器
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.gameDialogues.Add(sequence1);
            Debug.Log("示範對話序列已創建並添加到遊戲管理器");
        }
        else
        {
            Debug.LogError("找不到遊戲管理器！");
        }
    }
    
    [ContextMenu("Create Multiple Sample Sequences")]
    void CreateMultipleSampleSequences()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("找不到遊戲管理器！");
            return;
        }
        
        // 清空現有序列
        gameManager.gameDialogues.Clear();
        
        // 序列1：開場
        CreateOpeningSequence(gameManager);
        
        // 序列2：第一章
        CreateChapter1Sequence(gameManager);
        
        // 序列3：選擇分支
        CreateBranchSequence(gameManager);
        
        Debug.Log($"已創建 {gameManager.gameDialogues.Count} 個對話序列");
    }
    
    void CreateOpeningSequence(GameManager gameManager)
    {
        DialogueSequence sequence = new DialogueSequence
        {
            id = 0,
            sequenceName = "序章：醒來",
            dialogues = new System.Collections.Generic.List<DialogueData>
            {
                new DialogueData
                {
                    characterName = "",
                    dialogueText = "你從沉睡中醒來，發現自己身處一個陌生的房間...",
                    choices = new System.Collections.Generic.List<ChoiceData>()
                },
                new DialogueData
                {
                    characterName = "主角",
                    dialogueText = "這是哪裡？我的頭好痛...",
                    choices = new System.Collections.Generic.List<ChoiceData>()
                },
                new DialogueData
                {
                    characterName = "",
                    dialogueText = "房間裡很安靜，只有遠處傳來微弱的音樂聲。",
                    choices = new System.Collections.Generic.List<ChoiceData>
                    {
                        new ChoiceData("走向音樂聲的方向", 1),
                        new ChoiceData("檢查房間", 1)
                    }
                }
            }
        };
        
        gameManager.gameDialogues.Add(sequence);
    }
    
    void CreateChapter1Sequence(GameManager gameManager)
    {
        DialogueSequence sequence = new DialogueSequence
        {
            id = 1,
            sequenceName = "第一章：探索",
            dialogues = new System.Collections.Generic.List<DialogueData>
            {
                new DialogueData
                {
                    characterName = "主角",
                    dialogueText = "這個地方給我一種熟悉的感覺...",
                    choices = new System.Collections.Generic.List<ChoiceData>()
                },
                new DialogueData
                {
                    characterName = "神秘女子",
                    dialogueText = "你終於醒了。我等你很久了。",
                    choices = new System.Collections.Generic.List<ChoiceData>()
                },
                new DialogueData
                {
                    characterName = "主角",
                    dialogueText = "你是誰？為什麼在等我？",
                    choices = new System.Collections.Generic.List<ChoiceData>()
                },
                new DialogueData
                {
                    characterName = "神秘女子",
                    dialogueText = "這些問題的答案，需要你自己去尋找。",
                    choices = new System.Collections.Generic.List<ChoiceData>
                    {
                        new ChoiceData("請告訴我真相", 2),
                        new ChoiceData("我會自己找到答案", 2)
                    }
                }
            }
        };
        
        gameManager.gameDialogues.Add(sequence);
    }
    
    void CreateBranchSequence(GameManager gameManager)
    {
        DialogueSequence sequence = new DialogueSequence
        {
            id = 2,
            sequenceName = "第二章：選擇",
            dialogues = new System.Collections.Generic.List<DialogueData>
            {
                new DialogueData
                {
                    characterName = "神秘女子",
                    dialogueText = "不管你選擇什麼，記住一點：每個選擇都會有後果。",
                    choices = new System.Collections.Generic.List<ChoiceData>()
                },
                new DialogueData
                {
                    characterName = "",
                    dialogueText = "她說完便消失了，留下你一個人面對前方的兩扇門。",
                    choices = new System.Collections.Generic.List<ChoiceData>()
                },
                new DialogueData
                {
                    characterName = "",
                    dialogueText = "左邊的門散發著溫暖的光芒，右邊的門則透出神秘的紫光。",
                    choices = new System.Collections.Generic.List<ChoiceData>
                    {
                        new ChoiceData("選擇左邊的溫暖之門", -1),
                        new ChoiceData("選擇右邊的神秘之門", -1),
                        new ChoiceData("都不選擇，尋找其他出路", -1)
                    }
                }
            }
        };
        
        gameManager.gameDialogues.Add(sequence);
    }
}