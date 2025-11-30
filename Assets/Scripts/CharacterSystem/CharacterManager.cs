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
    public Color dimColor = new Color(0.5f, 0.5f, 0.5f, 0.6f); // 變灰顏色
    public Color normalColor = Color.white; // 正常顏色
    
    // 當前顯示的角色
    private CharacterRenderer currentActiveCharacter;
    
    void Start()
    {
        // 訂閱DialogueManager的角色事件
        DialogueManager.OnCharacterDisplay += OnCharacterDisplay;
        DialogueManager.OnCharacterHide += OnCharacterHide;
        
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
        
        // 如果角色原本隱藏或圖片改變，執行淡入動畫
        if ((wasHidden || spriteChanged) && character.supportsFade)
        {
            StartCoroutine(FadeInCharacter(character));
        }
        

        
        // 設定原角色顏色 (變灰或正常，不包含遮罩)
        Color targetColor = dimCharacter ? dimColor : normalColor;
        
        if (character.supportsFade)
        {
            StartCoroutine(TransitionCharacterColor(renderer, targetColor, character.colorTransitionDuration));
        }
        else
        {
            renderer.color = targetColor;
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
    

}