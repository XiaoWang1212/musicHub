using UnityEngine;
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