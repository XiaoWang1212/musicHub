using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 自定義表情 Sprite 配對
/// </summary>
[System.Serializable]
public class ExpressionSprite
{
    [Tooltip("表情名稱（可自由命名，如：開心、憤怒、我的特殊表情等）")]
    public string expressionName = "新表情";
    
    [Tooltip("對應的 Sprite 圖片")]
    public Sprite sprite;
}

/// <summary>
/// 通用角色管理器 - 支援多角色動態顯示
/// 可以在 Inspector 中配置角色名稱和對應的 SpriteRenderer
/// </summary>
public class CharacterManager : MonoBehaviour
{
    [System.Serializable]
    public class CharacterRenderer
    {
        [Header("角色設定")]
        public string characterName;        // 角色名稱 (要與DialogueData中的characterName一致)
        public SpriteRenderer renderer;     // 對應的SpriteRenderer
        
        [Header("表情 Sprite 設定")]
        [Tooltip("從 SpriteRenderer 讀取當前 sprite 作為預設表情")]
        public bool autoGetDefaultFromRenderer = true;
        
        [Header("自定義表情列表")]
        [Tooltip("可自由設計的表情名稱與對應 Sprite，點擊 + 號新增表情")]
        public List<ExpressionSprite> customExpressions = new List<ExpressionSprite>();
        
        [Header("動畫設定")]
        public bool supportsFade = true;    // 是否支援淡入淡出
        public float fadeInDuration = 0.8f; // 淡入時間
        public float fadeOutDuration = 0.5f;// 淡出時間
        public float colorTransitionDuration = 0.3f; // 顏色過渡時間
        public float expressionChangeSpeed = 0.3f; // 表情切換速度
        
        [Header("位置設定")]
        public Vector3 speakingPosition;    // 說話時的位置
        public Vector3 silentPosition;      // 不說話時的位置 (可選)
        public bool usePositionChange = false; // 是否使用位置變化
        
        /// <summary>
        /// 初始化：從 SpriteRenderer 讀取預設表情
        /// </summary>
        public void Initialize()
        {
            // 如果啟用自動讀取且有 renderer
            if (autoGetDefaultFromRenderer && renderer != null && renderer.sprite != null)
            {
                // 檢查是否已有 "預設" 表情，沒有則自動添加
                var defaultExpression = customExpressions.Find(e => e.expressionName == "預設");
                if (defaultExpression == null)
                {
                    customExpressions.Insert(0, new ExpressionSprite 
                    {
                        expressionName = "預設",
                        sprite = renderer.sprite
                    });
                    Debug.Log($"🎭 {characterName} 自動添加預設表情: {renderer.sprite.name}");
                }
                else if (defaultExpression.sprite == null)
                {
                    defaultExpression.sprite = renderer.sprite;
                    Debug.Log($"🎭 {characterName} 自動設定預設表情: {renderer.sprite.name}");
                }
            }
        }
        
        /// <summary>
        /// 根據表情名稱獲取 Sprite
        /// </summary>
        public Sprite GetExpressionSprite(string expressionName)
        {
            // 在自定義表情列表中尋找
            var expression = customExpressions.Find(e => 
                string.Equals(e.expressionName, expressionName, System.StringComparison.OrdinalIgnoreCase));
            
            if (expression != null && expression.sprite != null)
            {
                return expression.sprite;
            }
            
            Debug.LogWarning($"⚠️ 找不到表情 '{expressionName}'，使用預設表情");
            return GetDefaultSprite();
        }
        
        /// <summary>
        /// 檢查是否有指定表情
        /// </summary>
        public bool HasExpression(string expressionName)
        {
            var expression = customExpressions.Find(e => 
                string.Equals(e.expressionName, expressionName, System.StringComparison.OrdinalIgnoreCase));
            return expression != null && expression.sprite != null;
        }
        
        /// <summary>
        /// 獲取預設表情
        /// </summary>
        public Sprite GetDefaultSprite()
        {
            // 尋找 "預設" 表情
            var defaultExpression = customExpressions.Find(e => 
                e.expressionName == "預設" || e.expressionName == "普通" || e.expressionName == "default");
            
            if (defaultExpression != null && defaultExpression.sprite != null)
                return defaultExpression.sprite;
            
            // 如果沒有預設表情，使用第一個可用的表情
            if (customExpressions.Count > 0 && customExpressions[0].sprite != null)
                return customExpressions[0].sprite;
            
            // 最後嘗試從 renderer 取得
            if (renderer != null && renderer.sprite != null)
                return renderer.sprite;
                
            return null;
        }
        
        /// <summary>
        /// 添加新表情
        /// </summary>
        public void AddExpression(string expressionName, Sprite sprite)
        {
            if (string.IsNullOrEmpty(expressionName) || sprite == null) return;
            
            // 檢查是否已存在
            var existing = customExpressions.Find(e => 
                string.Equals(e.expressionName, expressionName, System.StringComparison.OrdinalIgnoreCase));
            
            if (existing != null)
            {
                existing.sprite = sprite; // 更新現有表情
                Debug.Log($"🎭 更新表情: {expressionName}");
            }
            else
            {
                customExpressions.Add(new ExpressionSprite
                {
                    expressionName = expressionName,
                    sprite = sprite
                });
                Debug.Log($"🎭 新增表情: {expressionName}");
            }
        }
        
        /// <summary>
        /// 移除表情
        /// </summary>
        public void RemoveExpression(string expressionName)
        {
            customExpressions.RemoveAll(e => 
                string.Equals(e.expressionName, expressionName, System.StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// 獲取所有表情名稱
        /// </summary>
        public List<string> GetAllExpressionNames()
        {
            var names = new List<string>();
            foreach (var expression in customExpressions)
            {
                if (!string.IsNullOrEmpty(expression.expressionName))
                    names.Add(expression.expressionName);
            }
            return names;
        }
    }
    

    
    [Header("角色列表")]
    public List<CharacterRenderer> characters = new List<CharacterRenderer>();
    
    [Header("全域設定")]
    public Color dimColor = new Color(0.5f, 0.5f, 0.5f, 1.0f); // 變灰顏色
    public Color normalColor = new Color(1.0f, 1.0f, 1.0f, 1.0f); // 正常顏色
    
    // 當前顯示的角色
    private CharacterRenderer currentActiveCharacter;
    
    [Header("多角色顯示設定")]
    [Tooltip("角色位置陣列，順序必須是：[0]Left, [1]Center, [2]Right")]
    public List<Transform> characterPositions = new List<Transform>(); // 預設角色位置
    
    // 當前顯示的多個角色
    private List<CharacterRenderer> currentDisplayedCharacters = new List<CharacterRenderer>();
    
    void Start()
    {
        // 訂閱DialogueManager的角色事件
        DialogueManager.OnCharacterDisplay += OnCharacterDisplay;
        DialogueManager.OnCharacterHide += OnCharacterHide;
        DialogueManager.OnMultipleCharactersDisplay += OnMultipleCharactersDisplay;
        DialogueManager.OnMultipleCharactersHide += OnMultipleCharactersHide;
        DialogueManager.OnCharacterExpressionChange += ChangeCharacterExpression;
        
        // 驗證位置設定
        ValidateCharacterPositions();
        
        // 初始化所有角色為隱藏狀態
        foreach (var character in characters)
        {
            if (character.renderer != null)
            {
                character.Initialize(); // 初始化角色，自動讀取預設表情
                character.renderer.gameObject.SetActive(false);
            }
        }
    }
    
    void OnDestroy()
    {
        // 取消訂閱
        DialogueManager.OnCharacterDisplay -= OnCharacterDisplay;
        DialogueManager.OnCharacterHide -= OnCharacterHide;
        DialogueManager.OnMultipleCharactersDisplay -= OnMultipleCharactersDisplay;
        DialogueManager.OnMultipleCharactersHide -= OnMultipleCharactersHide;
        DialogueManager.OnCharacterExpressionChange -= ChangeCharacterExpression;
    }
    
    /// <summary>
    /// 根據角色名稱找到對應的CharacterRenderer
    /// </summary>
    CharacterRenderer FindCharacterByName(string characterName)
    {
        return characters.Find(c => c.characterName == characterName);
    }
    
    /// <summary>
    /// 處理角色顯示
    /// </summary>
    void OnCharacterDisplay(string characterName, Sprite characterSprite, bool dimCharacter)
    {
        CharacterRenderer targetCharacter = FindCharacterByName(characterName);
        
        if (targetCharacter != null && targetCharacter.renderer != null)
        {
            // 檢查是否為同一個角色
            bool isSameCharacter = (currentActiveCharacter == targetCharacter);
            
            // 只有在切換角色時才隱藏其他角色
            if (!isSameCharacter)
            {
                // 立即隱藏所有其他角色 (無動畫，避免重疊)
                HideAllCharactersExceptImmediate(targetCharacter);
            }
            
            // 顯示目標角色 (考慮遮罩效果,傳入 isSameCharacter 標記)
            ShowCharacter(targetCharacter, characterSprite, dimCharacter, isSameCharacter);
            
            currentActiveCharacter = targetCharacter;
        }
        else
        {
            // 如果找不到對應角色，隱藏所有角色
            HideAllCharacters();
            currentActiveCharacter = null;
        }
    }
    
    /// <summary>
    /// 隱藏所有角色
    /// </summary>
    void OnCharacterHide()
    {
        HideAllCharacters();
        currentActiveCharacter = null;
    }
    
    /// <summary>
    /// 顯示特定角色
    /// </summary>
    void ShowCharacter(CharacterRenderer character, Sprite sprite, bool dimCharacter, bool isSameCharacter = false)
    {
        var renderer = character.renderer;
        
        // 檢查是否需要 fade 動畫
        bool wasHidden = !renderer.gameObject.activeInHierarchy;
        bool spriteChanged = sprite != null && renderer.sprite != sprite;
        
        // 只有當傳入有效 sprite 時才更新，否則使用 SpriteRenderer 上預設的 sprite
        if (sprite != null)
        {
            renderer.sprite = sprite;
        }
        renderer.gameObject.SetActive(true);
        
        // 設定目標顏色
        Color targetColor = dimCharacter ? dimColor : normalColor;
        
        // 如果是同一個角色連續說話,只做顏色過渡,不要 fade in/out
        if (isSameCharacter && !wasHidden)
        {
            // 同一個角色,只做顏色過渡
            if (character.supportsFade)
            {
                StartCoroutine(TransitionCharacterColor(renderer, targetColor, character.colorTransitionDuration));
            }
            else
            {
                renderer.color = targetColor;
            }
        }
        // 如果角色原本隱藏，執行完整的淡入動畫（從透明到目標顏色）
        else if (wasHidden && character.supportsFade)
        {
            StartCoroutine(FadeInCharacterToColor(character, targetColor));
        }
        else
        {
            // 如果角色已經顯示，只做顏色過渡
            if (character.supportsFade)
            {
                StartCoroutine(TransitionCharacterColor(renderer, targetColor, character.colorTransitionDuration));
            }
            else
            {
                renderer.color = targetColor;
            }
        }
        
        // 設定位置 (如果啟用)
        if (character.usePositionChange)
        {
            Vector3 targetPosition = dimCharacter ? character.silentPosition : character.speakingPosition;
            renderer.transform.position = targetPosition;
        }
    }
    
    /// <summary>
    /// 隱藏所有角色
    /// </summary>
    void HideAllCharacters()
    {
        foreach (var character in characters)
        {
            if (character.renderer != null && character.renderer.gameObject.activeInHierarchy)
            {
                if (character.supportsFade)
                {
                    StartCoroutine(FadeOutCharacter(character));
                }
                else
                {
                    character.renderer.gameObject.SetActive(false);
                }
            }
        }
    }
    
    /// <summary>
    /// 隱藏除了指定角色外的所有角色 (有淡出動畫)
    /// </summary>
    void HideAllCharactersExcept(CharacterRenderer exceptCharacter)
    {
        foreach (var character in characters)
        {
            if (character != exceptCharacter && character.renderer != null && character.renderer.gameObject.activeInHierarchy)
            {
                if (character.supportsFade)
                {
                    StartCoroutine(FadeOutCharacter(character));
                }
                else
                {
                    character.renderer.gameObject.SetActive(false);
                }
            }
        }
    }
    
    /// <summary>
    /// 立即隱藏除了指定角色外的所有角色 (無動畫，避免重疊)
    /// </summary>
    void HideAllCharactersExceptImmediate(CharacterRenderer exceptCharacter)
    {
        foreach (var character in characters)
        {
            if (character != exceptCharacter && character.renderer != null && character.renderer.gameObject.activeInHierarchy)
            {
                character.renderer.gameObject.SetActive(false);

            }
        }
    }
    
    /// <summary>
    /// 角色淡入動畫
    /// </summary>
    System.Collections.IEnumerator FadeInCharacter(CharacterRenderer character)
    {
        var renderer = character.renderer;
        if (renderer == null) yield break;

        float elapsed = 0f;
        Color color = renderer.color;
        Color startColor = color;
        startColor.a = 0f;
        
        renderer.color = startColor;
        
        while (elapsed < character.fadeInDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsed / character.fadeInDuration);
            renderer.color = color;
            yield return null;
        }
        
        color.a = 1f;
        renderer.color = color;
    }

    /// <summary>
    /// 角色淡入到指定顏色的動畫
    /// </summary>
    System.Collections.IEnumerator FadeInCharacterToColor(CharacterRenderer character, Color targetColor)
    {
        var renderer = character.renderer;
        if (renderer == null) yield break;

        float elapsed = 0f;
        Color startColor = targetColor;
        startColor.a = 0f;
        
        renderer.color = startColor;
        
        // 使用目標顏色的原始 alpha 值
        Color finalColor = targetColor;
        
        while (elapsed < character.fadeInDuration)
        {
            elapsed += Time.deltaTime;
            Color currentColor = Color.Lerp(startColor, finalColor, elapsed / character.fadeInDuration);
            renderer.color = currentColor;
            yield return null;
        }
        
        renderer.color = finalColor;
    }
    
    /// <summary>
    /// 角色淡出動畫
    /// </summary>
    System.Collections.IEnumerator FadeOutCharacter(CharacterRenderer character)
    {
        var renderer = character.renderer;
        if (renderer == null) yield break;

        float elapsed = 0f;
        Color color = renderer.color;
        float startAlpha = color.a;
        
        while (elapsed < character.fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, 0f, elapsed / character.fadeOutDuration);
            renderer.color = color;
            yield return null;
        }
        
        color.a = 0f;
        renderer.color = color;
        renderer.gameObject.SetActive(false);
    }

    /// <summary>
    /// 帶有角色狀態設置的淡入效果
    /// </summary>
    System.Collections.IEnumerator FadeInCharacterWithState(CharacterRenderer character, CharacterDisplayData charData)
    {
        var renderer = character.renderer;
        if (renderer == null) yield break;

        // 設置目標顏色 (根據是否變暗)
        Color targetColor = charData.dimCharacter ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;
        Color startColor = targetColor;
        startColor.a = 0f;
        
        // 從透明開始
        renderer.color = startColor;
        
        // 執行淡入動畫
        float elapsed = 0f;
        while (elapsed < character.fadeInDuration)
        {
            elapsed += Time.deltaTime;
            Color currentColor = targetColor;
            currentColor.a = Mathf.Lerp(0f, 1f, elapsed / character.fadeInDuration);
            renderer.color = currentColor;
            yield return null;
        }
        
        // 最終設置完整顏色
        renderer.color = targetColor;
    }
    
    /// <summary>
    /// 角色顏色過渡動畫
    /// </summary>
    System.Collections.IEnumerator TransitionCharacterColor(SpriteRenderer renderer, Color targetColor, float duration)
    {
        if (renderer == null) yield break;
        
        float elapsed = 0f;
        Color startColor = renderer.color;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            renderer.color = Color.Lerp(startColor, targetColor, progress);
            yield return null;
        }
        
        renderer.color = targetColor;
    }
    
    /// <summary>
    /// 公開方法：根據名稱獲取角色 (供其他腳本使用，如抖動動畫)
    /// </summary>
    public SpriteRenderer GetCharacterRenderer(string characterName)
    {
        CharacterRenderer character = FindCharacterByName(characterName);
        return character?.renderer;
    }
    
    /// <summary>
    /// 公開方法：獲取當前活躍角色
    /// </summary>
    public CharacterRenderer GetCurrentActiveCharacter()
    {
        return currentActiveCharacter;
    }
    
    /// <summary>
    /// 處理多角色同時顯示
    /// </summary>
    void OnMultipleCharactersDisplay(List<CharacterDisplayData> charactersToDisplay)
    {
        Debug.Log($"👥 開始顯示 {charactersToDisplay.Count} 個角色");
        
        // 列出所有可用角色名稱用於除錯
        Debug.Log($"📋 可用角色列表: {string.Join(", ", characters.ConvertAll(c => c.characterName))}");
        
        // 記錄成功顯示的角色
        List<CharacterRenderer> successfullyDisplayed = new List<CharacterRenderer>();
        
        // 先嘗試顯示所有指定角色
        foreach (var charData in charactersToDisplay)
        {
            CharacterRenderer character = FindCharacterByName(charData.characterName);
            if (character != null && character.renderer != null)
            {
                // 檢查角色是否原本就已顯示
                bool wasAlreadyVisible = character.renderer.gameObject.activeSelf;
                
                // 顯示角色
                character.renderer.gameObject.SetActive(true);
                
                // 設置角色位置
                SetCharacterPosition(character, charData.position);
                
                // 設置角色狀態和動畫
                if (!wasAlreadyVisible && character.supportsFade)
                {
                    // 新出現的角色：淡入效果
                    Color targetColor = charData.dimCharacter ? dimColor : normalColor;
                    StartCoroutine(FadeInCharacterToColor(character, targetColor));
                }
                else
                {
                    // 已經顯示的角色：顏色過渡
                    Color targetColor = charData.dimCharacter ? dimColor : normalColor;
                    
                    if (character.supportsFade)
                    {
                        StartCoroutine(TransitionCharacterColor(character.renderer, targetColor, character.colorTransitionDuration));
                    }
                    else
                    {
                        character.renderer.color = targetColor;
                    }
                }
                
                // 記錄成功顯示的角色
                successfullyDisplayed.Add(character);
                
                Debug.Log($"👤 顯示角色: {charData.characterName} 在 {charData.position} 位置 (變暗: {charData.dimCharacter}, 淡入: {!wasAlreadyVisible})");
            }
            else
            {
                Debug.LogError($"❌ 找不到角色: '{charData.characterName}'");
                Debug.LogError($"💡 請檢查 CharacterManager.characters 列表中是否有名稱完全一致的角色");
                Debug.LogError($"📝 可用角色: {string.Join(", ", characters.ConvertAll(c => $"'{c.characterName}'"))}");
                
                // 提供相似名稱建議
                var similarNames = characters.FindAll(c => c.characterName.Contains(charData.characterName) || charData.characterName.Contains(c.characterName));
                if (similarNames.Count > 0)
                {
                    Debug.LogWarning($"🔍 相似的角色名稱: {string.Join(", ", similarNames.ConvertAll(c => $"'{c.characterName}'"))}");
                }
            }
        }
        
        // 隱藏未在此次顯示列表中的其他角色
        foreach (var character in characters)
        {
            if (!successfullyDisplayed.Contains(character))
            {
                if (character.renderer != null && character.renderer.gameObject.activeSelf)
                {
                    character.renderer.gameObject.SetActive(false);
                    Debug.Log($"🙈 隱藏角色: {character.characterName}");
                }
            }
        }
        
        // 更新當前顯示列表
        currentDisplayedCharacters = successfullyDisplayed;
    }
    
    /// <summary>
    /// 隱藏所有多角色顯示
    /// </summary>
    void OnMultipleCharactersHide()
    {
        Debug.Log("👥 隱藏所有多角色顯示");
        HideAllCharacters();
        currentDisplayedCharacters.Clear();
    }
    
    /// <summary>
    /// 設置角色位置
    /// </summary>
    void SetCharacterPosition(CharacterRenderer character, CharacterPosition position)
    {
        int positionIndex = (int)position;
        Debug.Log($"🎯 設定 {character.characterName} 到 {position} 位置 (索引: {positionIndex})");
        Debug.Log($"📍 characterPositions 陣列長度: {characterPositions.Count}");
        
        if (characterPositions.Count > positionIndex && characterPositions[positionIndex] != null)
        {
            Vector3 targetPosition = characterPositions[positionIndex].position;
            character.renderer.transform.position = targetPosition;
            Debug.Log($"✅ 成功設定 {character.characterName} 位置為: {targetPosition}");
        }
        else
        {
            // 使用預設位置
            Vector3 defaultPos = GetDefaultPosition(position);
            character.renderer.transform.position = defaultPos;
            Debug.LogWarning($"⚠️ 未設定 {position} 位置 (索引 {positionIndex})，使用預設位置 {defaultPos}");
            Debug.LogWarning($"💡 請確保 characterPositions 陣列有 {positionIndex + 1} 個元素");
        }
    }
    
    /// <summary>
    /// 獲取預設角色位置
    /// </summary>
    Vector3 GetDefaultPosition(CharacterPosition position)
    {
        switch (position)
        {
            case CharacterPosition.Left: return new Vector3(-3f, 0f, 0f);
            case CharacterPosition.Center: return new Vector3(0f, 0f, 0f);
            case CharacterPosition.Right: return new Vector3(3f, 0f, 0f);
            default: return Vector3.zero;
        }
    }
    
    /// <summary>
    /// 設置角色狀態 (自動處理透明度和色調)
    /// </summary>
    void SetCharacterState(CharacterRenderer character, CharacterDisplayData charData)
    {
        SpriteRenderer renderer = character.renderer;
        
        // 注意：不再從 charData 設定 sprite，使用 SpriteRenderer 預設的 sprite
        
        // 自動設定顏色：根據 dimCharacter 決定
        Color finalColor = charData.dimCharacter ? dimColor : normalColor;
        
        renderer.color = finalColor;
        
        Debug.Log($"🎨 設定 {charData.characterName} 狀態: 變暗={charData.dimCharacter}");
    }
    
    System.Collections.IEnumerator ShakeCharacter(SpriteRenderer renderer)
    {
        Vector3 originalPos = renderer.transform.position;
        float shakeIntensity = 0.1f;
        int shakeCount = 3;
        
        for (int i = 0; i < shakeCount; i++)
        {
            renderer.transform.position = originalPos + new Vector3(
                UnityEngine.Random.Range(-shakeIntensity, shakeIntensity),
                UnityEngine.Random.Range(-shakeIntensity, shakeIntensity),
                0f
            );
            yield return new WaitForSeconds(0.05f);
        }
        
        renderer.transform.position = originalPos;
    }
    
    /// <summary>
    /// 驗證角色位置設定是否正確
    /// </summary>
    void ValidateCharacterPositions()
    {
        Debug.Log("🔍 驗證角色位置設定...");
        
        string[] positionNames = { "Left", "Center", "Right" };
        
        for (int i = 0; i < 3; i++)
        {
            if (i < characterPositions.Count && characterPositions[i] != null)
            {
                Debug.Log($"✅ 位置 [{i}] {positionNames[i]}: {characterPositions[i].name} at {characterPositions[i].position}");
            }
            else
            {
                Debug.LogWarning($"❌ 位置 [{i}] {positionNames[i]}: 未設定或為空");
            }
        }
        
        if (characterPositions.Count < 3)
        {
            Debug.LogError($"🚨 characterPositions 陣列不完整！當前長度: {characterPositions.Count}，需要: 3");
            Debug.LogError("💡 請在 Inspector 中添加 3 個 Transform 位置：Left, Center, Right");
        }
    }
    
    [ContextMenu("檢查角色位置設定")]
    public void CheckPositionSetup()
    {
        ValidateCharacterPositions();
    }
    
    [ContextMenu("測試多角色顯示")]
    public void TestMultiCharacterDisplay()
    {
        List<CharacterDisplayData> testCharacters = new List<CharacterDisplayData>
        {
            new CharacterDisplayData
            {
                characterName = "白石 透羽",  // 確保這個名字與 characters 列表中的 characterName 一致
                position = CharacterPosition.Left,
                dimCharacter = true  // 變暗 (不是說話者)
            },
            new CharacterDisplayData
            {
                characterName = "母親",  // 確保這個名字與 characters 列表中的 characterName 一致
                position = CharacterPosition.Right,
                dimCharacter = false  // 正常亮度 (說話者)
            }
        };
        
        OnMultipleCharactersDisplay(testCharacters);
    }
    
    // ==================== 表情切換功能 ====================
    
    /// <summary>
    /// 切換角色表情
    /// </summary>
    /// <param name="characterName">角色名稱</param>
    /// <param name="expressionName">表情名稱</param>
    /// <param name="useAnimation">是否使用切換動畫</param>
    public void ChangeCharacterExpression(string characterName, string expressionName, bool useAnimation = true)
    {
        CharacterRenderer character = FindCharacterByName(characterName);
        if (character == null)
        {
            Debug.LogWarning($"⚠️ 找不到角色: {characterName}");
            return;
        }
        
        if (character.renderer == null)
        {
            Debug.LogWarning($"⚠️ 角色 {characterName} 的 SpriteRenderer 未設定");
            return;
        }
        
        Sprite targetSprite = character.GetExpressionSprite(expressionName);
        if (targetSprite == null)
        {
            Debug.LogWarning($"⚠️ 角色 {characterName} 沒有表情: {expressionName}，使用預設表情");
            targetSprite = character.GetDefaultSprite();
        }
        
        if (targetSprite == null)
        {
            Debug.LogWarning($"⚠️ 角色 {characterName} 沒有可用的表情 Sprite");
            return;
        }
        
        if (useAnimation)
        {
            StartCoroutine(ChangeExpressionWithAnimation(character, targetSprite, expressionName));
        }
        else
        {
            character.renderer.sprite = targetSprite;
            Debug.Log($"😊 {characterName} 表情立即切換為: {expressionName}");
        }
    }
    
    /// <summary>
    /// 帶動畫的表情切換
    /// </summary>
    System.Collections.IEnumerator ChangeExpressionWithAnimation(CharacterRenderer character, Sprite targetSprite, string expressionName)
    {
        SpriteRenderer renderer = character.renderer;
        Color originalColor = renderer.color;
        
        Debug.Log($"😊 {character.characterName} 開始表情切換動畫: {expressionName}");
        
        // 淡出當前表情
        float elapsed = 0f;
        float halfDuration = character.expressionChangeSpeed / 2f;
        
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0f, elapsed / halfDuration);
            Color currentColor = originalColor;
            currentColor.a = alpha;
            renderer.color = currentColor;
            yield return null;
        }
        
        // 更換 Sprite
        renderer.sprite = targetSprite;
        
        // 淡入新表情
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, originalColor.a, elapsed / halfDuration);
            Color currentColor = originalColor;
            currentColor.a = alpha;
            renderer.color = currentColor;
            yield return null;
        }
        
        // 確保顏色完全恢復
        renderer.color = originalColor;
        Debug.Log($"✅ {character.characterName} 表情切換完成: {expressionName}");
    }
    
    /// <summary>
    /// 設定角色預設表情
    /// </summary>
    public void SetCharacterDefaultExpression(string characterName)
    {
        CharacterRenderer character = FindCharacterByName(characterName);
        if (character != null)
        {
            Sprite defaultSprite = character.GetDefaultSprite();
            if (defaultSprite != null)
            {
                character.renderer.sprite = defaultSprite;
                Debug.Log($"🎭 {characterName} 恢復預設表情");
            }
            else
            {
                Debug.LogWarning($"⚠️ {characterName} 沒有設定預設表情");
            }
        }
    }
    
    /// <summary>
    /// 檢查角色表情設定
    /// </summary>
    [ContextMenu("檢查角色表情設定")]
    public void CheckExpressionSetup()
    {
        Debug.Log("🎭 === 角色表情設定檢查 ===");
        
        foreach (var character in characters)
        {
            Debug.Log($"📋 角色: {character.characterName}");
            
            // 檢查自定義表情列表
            if (character.customExpressions.Count > 0)
            {
                foreach (var expression in character.customExpressions)
                {
                    string status = expression.sprite != null ? "✅" : "❌";
                    Debug.Log($"   - {expression.expressionName}: {status}");
                }
            }
            else
            {
                Debug.Log($"   ⚠️ 沒有設定任何表情");
            }
            
            // 檢查預設表情
            Sprite defaultSprite = character.GetDefaultSprite();
            Debug.Log($"   預設表情: {(defaultSprite != null ? "✅ 已設定" : "❌ 未設定")}");
        }
    }
    
    /// <summary>
    /// 測試表情切換
    /// </summary>
    [ContextMenu("測試表情切換")]
    public void TestExpressionChange()
    {
        if (characters.Count > 0)
        {
            var testCharacter = characters[0];
            if (testCharacter.customExpressions.Count > 1)
            {
                // 使用第二個表情進行測試（跳過預設表情）
                string testExpression = testCharacter.customExpressions[1].expressionName;
                ChangeCharacterExpression(testCharacter.characterName, testExpression, true);
                Debug.Log($"🧪 測試切換 {testCharacter.characterName} 的表情為 {testExpression}");
            }
            else
            {
                Debug.LogWarning("⚠️ 角色沒有足夠的表情可供測試");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ 沒有角色可供測試");
        }
    }

}