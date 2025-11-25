using UnityEngine;

public class GameSetupTest : MonoBehaviour
{
    public GameManager gameManager;
    
    void Start()
    {
        // 測試所有管理器是否正確連接
        TestManagerConnections();
        
        // 創建簡單的測試對話
        CreateTestDialogue();
    }
    
    void TestManagerConnections()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManager未設置！");
            return;
        }
        
        if (gameManager.dialogueManager == null)
            Debug.LogError("DialogueManager未連接到GameManager！");
        else
            Debug.Log("✅ DialogueManager已正確連接");
            
        if (gameManager.characterDisplay == null)
            Debug.LogError("CharacterDisplay未連接到GameManager！");
        else
            Debug.Log("✅ CharacterDisplay已正確連接");
            
        if (gameManager.backgroundManager == null)
            Debug.LogError("BackgroundManager未連接到GameManager！");
        else
            Debug.Log("✅ BackgroundManager已正確連接");
    }
    
    void CreateTestDialogue()
    {
        if (gameManager == null || gameManager.dialogueManager == null) return;
        
        // 創建測試對話序列
        DialogueSequence testSequence = new DialogueSequence
        {
            id = 0,
            sequenceName = "測試對話",
            dialogues = new System.Collections.Generic.List<DialogueData>
            {
                new DialogueData
                {
                    characterName = "系統",
                    dialogueText = "歡迎使用劇情向遊戲系統！這是一個測試對話。",
                    choices = new System.Collections.Generic.List<ChoiceData>()
                },
                new DialogueData
                {
                    characterName = "系統",
                    dialogueText = "所有系統都已正確設置並運行。",
                    choices = new System.Collections.Generic.List<ChoiceData>()
                }
            }
        };
        
        gameManager.gameDialogues.Clear();
        gameManager.gameDialogues.Add(testSequence);
        
        Debug.Log("✅ 測試對話已創建，可以按Space鍵開始測試！");
    }
    
    void Update()
    {
        // 按Space開始測試對話
        if (Input.GetKeyDown(KeyCode.Space) && gameManager != null)
        {
            gameManager.StartGame();
        }
    }
}