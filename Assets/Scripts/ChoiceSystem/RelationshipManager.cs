using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 好感度與劇情狀態管理器
/// 追蹤角色好感度、事件標記、用於結局判定
/// 使用單例模式,跨場景保存資料
/// </summary>
public class RelationshipManager : MonoBehaviour
{
    #region 單例模式
    
    private static RelationshipManager instance;
    
    /// <summary>
    /// 單例實例
    /// </summary>
    public static RelationshipManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<RelationshipManager>();
                
                if (instance == null)
                {
                    Debug.LogError("[RelationshipManager] 場景中找不到 RelationshipManager! 請在 MainMenu 場景中新增!");
                }
            }
            return instance;
        }
    }
    
    void Awake()
    {
        // 確保只有一個實例
        if (instance != null && instance != this)
        {
            Debug.LogWarning("[RelationshipManager] 已經存在實例,銷毀重複的物件");
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject); // 跨場景保存
        
        Debug.Log("[RelationshipManager] 初始化完成,已設定為 DontDestroyOnLoad");
    }
    
    #endregion
    
    [Header("角色好感度")]
    [Tooltip("各角色的好感度 (0-100)")]
    public List<CharacterRelationship> characterRelationships = new List<CharacterRelationship>();
    
    [Header("劇情標記")]
    [Tooltip("記錄重要劇情事件")]
    public List<StoryFlag> storyFlags = new List<StoryFlag>();
    
    [Header("好感度變化設定")]
    public int increaseAmount = 10;  // 好感度增加量
    public int decreaseAmount = -5;  // 好感度減少量
    
    [Header("除錯設定")]
    public bool enableDebugLog = true;
    
    // 事件
    public static event Action<string, int, int> OnRelationshipChanged; // 角色名, 舊值, 新值
    
    #region 好感度管理
    
    /// <summary>
    /// 修改角色好感度
    /// </summary>
    /// <param name="characterName">角色名稱</param>
    /// <param name="effect">好感度效果</param>
    public void ModifyRelationship(string characterName, RelationshipEffect effect)
    {
        var character = GetCharacter(characterName);
        if (character == null)
        {
            Debug.LogWarning($"⚠️ 找不到角色: {characterName}");
            return;
        }
        
        int oldValue = character.relationshipValue;
        int changeAmount = 0;
        
        switch (effect)
        {
            case RelationshipEffect.Increase:
                changeAmount = increaseAmount;
                break;
            case RelationshipEffect.Decrease:
                changeAmount = decreaseAmount;
                break;
            case RelationshipEffect.None:
                return;
        }
        
        // 應用變化 (允許負數，用於結局判定)
        character.relationshipValue += changeAmount;
        
        // 觸發事件
        OnRelationshipChanged?.Invoke(characterName, oldValue, character.relationshipValue);
        
        if (enableDebugLog)
        {
            string effectText = effect == RelationshipEffect.Increase ? "增加" : "減少";
            Debug.Log($"💖 {characterName} 好感度{effectText}: {oldValue} → {character.relationshipValue}");
        }
    }
    
    /// <summary>
    /// 直接設定角色好感度
    /// </summary>
    public void SetRelationshipValue(string characterName, int value)
    {
        var character = GetCharacter(characterName);
        if (character == null) return;
        
        int oldValue = character.relationshipValue;
        character.relationshipValue = value;
        
        OnRelationshipChanged?.Invoke(characterName, oldValue, character.relationshipValue);
        
        if (enableDebugLog)
        {
            Debug.Log($"💖 {characterName} 好感度設定為: {character.relationshipValue}");
        }
    }
    
    /// <summary>
    /// 取得角色好感度
    /// </summary>
    public int GetRelationshipValue(string characterName)
    {
        var character = GetCharacter(characterName);
        return character?.relationshipValue ?? 0;
    }
    
    /// <summary>
    /// 取得所有角色好感度總和 (用於結局判定)
    /// </summary>
    public int GetTotalRelationship()
    {
        int total = 0;
        
        // 只計算四位樂團成員的好感度 (不包括白石透羽)
        string[] bandMembers = { "大野陽斗", "久我靜真", "山瀨日向", "高宮芽依" };
        
        foreach (string member in bandMembers)
        {
            total += GetRelationshipValue(member);
        }
        
        return total;
    }
    
    CharacterRelationship GetCharacter(string characterName)
    {
        return characterRelationships.Find(c => c.characterName == characterName);
    }
    
    #endregion
    
    #region 劇情標記系統
    
    /// <summary>
    /// 設定劇情標記
    /// </summary>
    /// <param name="flagId">標記 ID (例如: "hinata_returned")</param>
    /// <param name="value">標記值</param>
    public void SetStoryFlag(string flagId, bool value)
    {
        var flag = storyFlags.Find(f => f.flagId == flagId);
        
        if (flag == null)
        {
            flag = new StoryFlag { flagId = flagId, isSet = value };
            storyFlags.Add(flag);
        }
        else
        {
            flag.isSet = value;
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"🚩 劇情標記: {flagId} = {value}");
        }
    }
    
    /// <summary>
    /// 取得劇情標記
    /// </summary>
    public bool GetStoryFlag(string flagId)
    {
        var flag = storyFlags.Find(f => f.flagId == flagId);
        return flag != null && flag.isSet;
    }
    
    /// <summary>
    /// 檢查多個標記是否全部設定 (AND)
    /// </summary>
    public bool CheckAllFlags(params string[] flagIds)
    {
        foreach (string flagId in flagIds)
        {
            if (!GetStoryFlag(flagId)) return false;
        }
        return true;
    }
    
    #endregion
    
    #region 結局判定
    
    /// <summary>
    /// 判定結局類型
    /// 結局一: totalRelationship >= 90 且日向回歸
    /// 結局二: 回社團但條件不足，或日向未回歸
    /// 結局三: 未回社團
    /// </summary>
    public int DetermineEnding()
    {
        bool tohaReturned = GetStoryFlag("toha_returned_to_club");
        bool hinataReturned = GetStoryFlag("hinata_returned");
        int totalRelationship = GetTotalRelationship();
        
        if (!tohaReturned)
        {
            // 結局三: 分崩離析
            if (enableDebugLog)
            {
                Debug.Log("🎬 結局判定: 結局三 - 分崩離析");
            }
            return 3;
        }
        else if (totalRelationship >= 90 && hinataReturned)
        {
            // 結局一: 完美結局
            if (enableDebugLog)
            {
                Debug.Log($"🎬 結局判定: 結局一 - 完美結局 (好感度總和: {totalRelationship}, 日向回歸: {hinataReturned})");
            }
            return 1;
        }
        else
        {
            // 結局二: 普通結局
            if (enableDebugLog)
            {
                Debug.Log($"🎬 結局判定: 結局二 - 普通結局 (好感度總和: {totalRelationship}, 日向回歸: {hinataReturned})");
            }
            return 2;
        }
    }
    
    /// <summary>
    /// 檢查是否達成完美結局條件
    /// </summary>
    public bool CanAchievePerfectEnding()
    {
        bool tohaReturned = GetStoryFlag("toha_returned_to_club");
        bool hinataReturned = GetStoryFlag("hinata_returned");
        int totalRelationship = GetTotalRelationship();
        
        return tohaReturned && hinataReturned && totalRelationship >= 90;
    }
    
    #endregion
    
    #region 除錯功能
    
    /// <summary>
    /// 顯示當前狀態
    /// </summary>
    [ContextMenu("顯示當前狀態")]
    public void ShowCurrentStatus()
    {
        Debug.Log("💖 === 當前好感度 ===");
        foreach (var character in characterRelationships)
        {
            Debug.Log($"   {character.characterName}: {character.relationshipValue}");
        }
        
        Debug.Log($"\n📊 好感度總和: {GetTotalRelationship()}");
        
        Debug.Log("\n🚩 === 劇情標記 ===");
        foreach (var flag in storyFlags)
        {
            if (flag.isSet)
            {
                Debug.Log($"   ✅ {flag.flagId}");
            }
        }
        
        Debug.Log($"\n🎬 預測結局: {DetermineEnding()}");
    }
    
    /// <summary>
    /// 重置所有數據
    /// </summary>
    [ContextMenu("⚠️ 重置所有數據")]
    public void ResetAllData()
    {
        foreach (var character in characterRelationships)
        {
            character.relationshipValue = 0;
        }
        storyFlags.Clear();
        
        Debug.Log("🔄 已重置所有數據");
    }
    
    #endregion
}

/// <summary>
/// 角色好感度資料
/// </summary>
[System.Serializable]
public class CharacterRelationship
{
    public string characterName = "角色名稱";
    public int relationshipValue = 0;
}

/// <summary>
/// 劇情標記
/// </summary>
[System.Serializable]
public class StoryFlag
{
    public string flagId = "";
    public bool isSet = false;
}
