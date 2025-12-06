using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "New Dialogue Sequence", menuName = "Narrative Game/Dialogue Sequence")]
public class DialogueSequenceAsset : ScriptableObject
{
    [Header("序列資訊")]
    public string sequenceName;
    public int sequenceId;
    
    [Header("對話內容")]
    public List<DialogueData> dialogues = new List<DialogueData>();
    
    [Header("序列設定")]
    public bool isMainStoryline = true;
    public bool canSkip = true;
    public bool autoSave = false;
    
    // 編輯器輔助方法
    [ContextMenu("Add Sample Dialogue")]
    void AddSampleDialogue()
    {
        DialogueData sampleDialogue = new DialogueData
        {
            characterName = "角色名稱",
            dialogueText = "這是一段示範對話文字。",
            choices = new List<ChoiceData>()
        };
        
        dialogues.Add(sampleDialogue);
    }
    
    [ContextMenu("Add Choice Dialogue")]
    void AddChoiceDialogue()
    {
        DialogueData choiceDialogue = new DialogueData
        {
            characterName = "角色名稱",
            dialogueText = "請選擇一個選項：",
            choices = new List<ChoiceData>
            {
                new ChoiceData("選項 1", dialogues.Count + 1),
                new ChoiceData("選項 2", dialogues.Count + 2)
            }
        };
        
        dialogues.Add(choiceDialogue);
    }
    
    [ContextMenu("Add Narration Dialogue")]
    void AddNarrationDialogue()
    {
        DialogueData narrationDialogue = new DialogueData
        {
            isNarration = true,              // 勾選旁白選項
            characterName = "",              // 旁白不需要角色名字
            dialogueText = "這是旁白敘述文字。",
            dimCharacter = true,             // 讓現有角色變淡
            choices = new List<ChoiceData>()
        };
        
        dialogues.Add(narrationDialogue);
    }
    
    [ContextMenu("Add Act4 Narration Scene")]
    void AddAct4NarrationScene()
    {
        // 為 Act4 出門場景添加完整的旁白序列
        List<DialogueData> act4Dialogues = new List<DialogueData>
        {
            new DialogueData
            {
                isNarration = true,
                characterName = "",
                dialogueText = "白石 透羽提起書包，走出家門。",
                dimCharacter = true
            },
            new DialogueData
            {
                isNarration = true,
                characterName = "",
                dialogueText = "清晨的光線照在她臉上，卻照不進她眼裡。",
                dimCharacter = true
            },
            new DialogueData
            {
                isNarration = true,
                characterName = "",
                dialogueText = "她的背影像是從昨天的世界逃走，卻還沒準備好面對今天。",
                dimCharacter = true
            },
            new DialogueData
            {
                isNarration = true,
                characterName = "",
                dialogueText = "風輕輕掀起她外套的下擺。",
                dimCharacter = true
            },
            new DialogueData
            {
                isNarration = true,
                characterName = "",
                dialogueText = "遠處傳來不知名的校園廣播聲，象徵著她將走入的陌生環境。",
                dimCharacter = true
            }
        };
        
        dialogues.AddRange(act4Dialogues);
    }
    
    public DialogueSequence ToDialogueSequence()
    {
        DialogueSequence sequence = new DialogueSequence
        {
            id = sequenceId,
            sequenceName = sequenceName,
            dialogues = new List<DialogueData>(dialogues)
        };
        
        return sequence;
    }
}