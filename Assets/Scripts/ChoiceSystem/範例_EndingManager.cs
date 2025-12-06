using UnityEngine;

/// <summary>
/// 結局判定管理器
/// 根據好感度和劇情標記決定結局
/// </summary>
public class EndingManager : MonoBehaviour
{
    [Header("結局場景名稱")]
    public string ending1SceneName = "Ending1_Perfect";
    public string ending2WithHinataSceneName = "Ending2_Normal_WithHinata";
    public string ending2WithoutHinataSceneName = "Ending2_Normal_WithoutHinata";
    public string ending3SceneName = "Ending3_Disbandment";
    
    /// <summary>
    /// 判定並載入結局
    /// </summary>
    public void DetermineAndLoadEnding()
    {
        if (RelationshipManager.Instance == null)
        {
            Debug.LogError("找不到 RelationshipManager!");
            return;
        }
        
        int endingType = RelationshipManager.Instance.DetermineEnding();
        
        switch (endingType)
        {
            case 1:
                // 完美結局
                LoadEnding(ending1SceneName);
                break;
                
            case 2:
                // 普通結局(檢查日向是否回歸)
                bool hinataReturned = RelationshipManager.Instance.GetStoryFlag("hinata_returned");
                
                if (hinataReturned)
                {
                    LoadEnding(ending2WithHinataSceneName);
                }
                else
                {
                    LoadEnding(ending2WithoutHinataSceneName);
                }
                break;
                
            case 3:
                // 壞結局
                LoadEnding(ending3SceneName);
                break;
        }
    }
    
    /// <summary>
    /// 載入結局場景
    /// </summary>
    void LoadEnding(string sceneName)
    {
        Debug.Log($"🎬 載入結局場景: {sceneName}");
        
        // 使用 Unity 場景管理器載入場景
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// 檢查結局條件（用於提示玩家）
    /// </summary>
    [ContextMenu("檢查結局條件")]
    public void CheckEndingConditions()
    {
        Debug.Log("=== 結局條件檢查 ===");
        
        bool tohaReturned = RelationshipManager.Instance.GetStoryFlag("toha_returned_to_club");
        bool hinataReturned = RelationshipManager.Instance.GetStoryFlag("hinata_returned");
        int totalRelationship = relationshipManager.GetTotalRelationship();
        
        Debug.Log($"白石透羽回歸: {tohaReturned}");
        Debug.Log($"山瀨日向回歸: {hinataReturned}");
        Debug.Log($"好感度總和: {totalRelationship} / 90");
        
        Debug.Log("\n=== 結局判定 ===");
        
        if (!tohaReturned)
        {
            Debug.Log("❌ 結局三: 分崩離析 - 白石透羽未回到音樂社");
        }
        else if (totalRelationship >= 90 && hinataReturned)
        {
            Debug.Log("✅ 結局一: 完美結局 - 好感度達標且日向回歸");
        }
        else
        {
            Debug.Log($"⚠️ 結局二: 普通結局");
            
            if (!hinataReturned)
            {
                Debug.Log("   - 山瀨日向未回歸");
            }
            
            if (totalRelationship < 90)
            {
                Debug.Log($"   - 好感度不足 (需要 90，目前 {totalRelationship})");
                Debug.Log($"   - 還差 {90 - totalRelationship} 點");
            }
        }
    }
    
    /// <summary>
    /// 顯示達成完美結局的路徑
    /// </summary>
    [ContextMenu("顯示完美結局攻略")]
    public void ShowPerfectEndingGuide()
    {
        Debug.Log("=== 完美結局達成條件 ===");
        Debug.Log("1. 白石透羽必須回到音樂社");
        Debug.Log("2. 山瀨日向必須回歸");
        Debug.Log("3. 四位樂團成員好感度總和 >= 90");
        Debug.Log("");
        Debug.Log("=== 推薦選擇 ===");
        Debug.Log("第四章事件1 (大野陽斗): 選擇 A - 幫忙 (+10)");
        Debug.Log("第四章事件2 (久我靜真): 選擇 A - 關心 (+10)");
        Debug.Log("第四章事件3 (山瀨日向): 選擇 A - 鼓勵 (+10)");
        Debug.Log("第四章事件4 (高宮芽依): 選擇 A - 幫助 (+10)");
        Debug.Log("第五章事件1 (節奏衝突): 選擇 A - 折衷 (大野+10, 久我+10, 山瀨+10)");
        Debug.Log("第五章事件2 (舞台動作): 選擇 A - 平衡 (大野+10, 山瀨+10)");
        Debug.Log("第三幕第二章 (天台): 必須詢問理由並支持日向");
        Debug.Log("");
        Debug.Log("最佳情況總和: 40 + 30 + 20 = 90 分");
    }
}
