using System.Collections.Generic;
using UnityEngine;
using System;

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
        
        [Header("動畫設定")]
        public bool supportsFade = true;    // 是否支援淡入淡出
        public float fadeInDuration = 0.8f; // 淡入時間
        public float fadeOutDuration = 0.5f;// 淡出時間
        public float colorTransitionDuration = 0.3f; // 顏色過渡時間
        
        [Header("位置設定")]
        public Vector3 speakingPosition;    // 說話時的位置
        public Vector3 silentPosition;      // 不說話時的位置 (可選)
        public bool usePositionChange = false; // 是否使用位置變化
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
        
        // 驗證位置設定
        ValidateCharacterPositions();
        
        // 初始化所有角色為隱藏狀態
        foreach (var character in characters)
        {
            if (character.renderer != null)
            {
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
            // 立即隱藏所有其他角色 (無動畫，避免重疊)
            HideAllCharactersExceptImmediate(targetCharacter);
            
            // 顯示目標角色 (考慮遮罩效果)
            ShowCharacter(targetCharacter, characterSprite, dimCharacter);
            
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
    void ShowCharacter(CharacterRenderer character, Sprite sprite, bool dimCharacter)
    {
        var renderer = character.renderer;
        
        // 檢查是否需要 fade 動畫
        bool wasHidden = !renderer.gameObject.activeInHierarchy;
        bool spriteChanged = renderer.sprite != sprite;
        
        // 設定圖片
        renderer.sprite = sprite;
        renderer.gameObject.SetActive(true);
        
        // 設定目標顏色
        Color targetColor = dimCharacter ? dimColor : normalColor;
        
        // 如果角色原本隱藏，執行完整的淡入動畫（從透明到目標顏色）
        if (wasHidden && character.supportsFade)
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
        
        // 設置精靈圖片
        if (charData.characterSprite != null)
        {
            renderer.sprite = charData.characterSprite;
        }
        
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
    


}