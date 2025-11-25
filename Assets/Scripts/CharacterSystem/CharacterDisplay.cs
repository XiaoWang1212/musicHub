using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CharacterDisplay : MonoBehaviour
{
    [Header("角色顯示設定")]
    public Image characterImageLeft;
    public Image characterImageRight;
    public Image characterImageCenter;
    
    [Header("動畫設定")]
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 0.3f;
    public float slideDistance = 100f;
    
    [Header("表情變化")]
    public bool enableExpressionChange = true;
    public float expressionChangeSpeed = 0.2f;
    
    // 角色位置枚舉
    public enum CharacterPosition
    {
        Left,
        Right,
        Center
    }
    
    // 角色顯示效果
    public enum DisplayEffect
    {
        None,
        FadeIn,
        SlideIn,
        PopIn,
        FadeOut,
        SlideOut
    }
    
    void Start()
    {
        InitializeCharacterImages();
    }
    
    void InitializeCharacterImages()
    {
        // 初始化所有角色圖片為隱藏狀態
        if (characterImageLeft != null)
        {
            SetImageAlpha(characterImageLeft, 0f);
            characterImageLeft.gameObject.SetActive(false);
        }
        
        if (characterImageRight != null)
        {
            SetImageAlpha(characterImageRight, 0f);
            characterImageRight.gameObject.SetActive(false);
        }
        
        if (characterImageCenter != null)
        {
            SetImageAlpha(characterImageCenter, 0f);
            characterImageCenter.gameObject.SetActive(false);
        }
    }
    
    public void ShowCharacter(Sprite characterSprite, CharacterPosition position, DisplayEffect effect = DisplayEffect.FadeIn)
    {
        Image targetImage = GetImageByPosition(position);
        if (targetImage == null || characterSprite == null) return;
        
        targetImage.sprite = characterSprite;
        targetImage.gameObject.SetActive(true);
        
        switch (effect)
        {
            case DisplayEffect.FadeIn:
                StartCoroutine(FadeInCharacter(targetImage));
                break;
            case DisplayEffect.SlideIn:
                StartCoroutine(SlideInCharacter(targetImage, position));
                break;
            case DisplayEffect.PopIn:
                StartCoroutine(PopInCharacter(targetImage));
                break;
            default:
                SetImageAlpha(targetImage, 1f);
                break;
        }
    }
    
    public void HideCharacter(CharacterPosition position, DisplayEffect effect = DisplayEffect.FadeOut)
    {
        Image targetImage = GetImageByPosition(position);
        if (targetImage == null) return;
        
        switch (effect)
        {
            case DisplayEffect.FadeOut:
                StartCoroutine(FadeOutCharacter(targetImage));
                break;
            case DisplayEffect.SlideOut:
                StartCoroutine(SlideOutCharacter(targetImage, position));
                break;
            default:
                targetImage.gameObject.SetActive(false);
                break;
        }
    }
    
    public void ChangeCharacterExpression(Sprite newExpression, CharacterPosition position)
    {
        if (!enableExpressionChange) return;
        
        Image targetImage = GetImageByPosition(position);
        if (targetImage == null || newExpression == null) return;
        
        StartCoroutine(ChangeExpressionSmooth(targetImage, newExpression));
    }
    
    public void HighlightCharacter(CharacterPosition position, bool highlight = true)
    {
        Image targetImage = GetImageByPosition(position);
        if (targetImage == null) return;
        
        float targetAlpha = highlight ? 1f : 0.5f;
        StartCoroutine(FadeToAlpha(targetImage, targetAlpha, 0.2f));
    }
    
    public void ClearAllCharacters()
    {
        HideCharacter(CharacterPosition.Left, DisplayEffect.FadeOut);
        HideCharacter(CharacterPosition.Right, DisplayEffect.FadeOut);
        HideCharacter(CharacterPosition.Center, DisplayEffect.FadeOut);
    }
    
    // 私有輔助方法
    Image GetImageByPosition(CharacterPosition position)
    {
        switch (position)
        {
            case CharacterPosition.Left:
                return characterImageLeft;
            case CharacterPosition.Right:
                return characterImageRight;
            case CharacterPosition.Center:
                return characterImageCenter;
            default:
                return null;
        }
    }
    
    void SetImageAlpha(Image image, float alpha)
    {
        if (image == null) return;
        
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
    
    // 動畫協程
    IEnumerator FadeInCharacter(Image image)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            SetImageAlpha(image, alpha);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        SetImageAlpha(image, 1f);
    }
    
    IEnumerator FadeOutCharacter(Image image)
    {
        float elapsedTime = 0f;
        float startAlpha = image.color.a;
        
        while (elapsedTime < fadeOutDuration)
        {
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeOutDuration);
            SetImageAlpha(image, alpha);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        SetImageAlpha(image, 0f);
        image.gameObject.SetActive(false);
    }
    
    IEnumerator SlideInCharacter(Image image, CharacterPosition position)
    {
        RectTransform rectTransform = image.GetComponent<RectTransform>();
        Vector3 originalPosition = rectTransform.localPosition;
        Vector3 startPosition = originalPosition;
        
        // 根據位置決定滑入方向
        switch (position)
        {
            case CharacterPosition.Left:
                startPosition.x -= slideDistance;
                break;
            case CharacterPosition.Right:
                startPosition.x += slideDistance;
                break;
            case CharacterPosition.Center:
                startPosition.y -= slideDistance;
                break;
        }
        
        rectTransform.localPosition = startPosition;
        SetImageAlpha(image, 0f);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            float progress = elapsedTime / fadeInDuration;
            rectTransform.localPosition = Vector3.Lerp(startPosition, originalPosition, progress);
            SetImageAlpha(image, progress);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        rectTransform.localPosition = originalPosition;
        SetImageAlpha(image, 1f);
    }
    
    IEnumerator SlideOutCharacter(Image image, CharacterPosition position)
    {
        RectTransform rectTransform = image.GetComponent<RectTransform>();
        Vector3 originalPosition = rectTransform.localPosition;
        Vector3 endPosition = originalPosition;
        
        // 根據位置決定滑出方向
        switch (position)
        {
            case CharacterPosition.Left:
                endPosition.x -= slideDistance;
                break;
            case CharacterPosition.Right:
                endPosition.x += slideDistance;
                break;
            case CharacterPosition.Center:
                endPosition.y += slideDistance;
                break;
        }
        
        float elapsedTime = 0f;
        float startAlpha = image.color.a;
        
        while (elapsedTime < fadeOutDuration)
        {
            float progress = elapsedTime / fadeOutDuration;
            rectTransform.localPosition = Vector3.Lerp(originalPosition, endPosition, progress);
            SetImageAlpha(image, Mathf.Lerp(startAlpha, 0f, progress));
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        rectTransform.localPosition = originalPosition;
        SetImageAlpha(image, 0f);
        image.gameObject.SetActive(false);
    }
    
    IEnumerator PopInCharacter(Image image)
    {
        RectTransform rectTransform = image.GetComponent<RectTransform>();
        Vector3 originalScale = rectTransform.localScale;
        
        rectTransform.localScale = Vector3.zero;
        SetImageAlpha(image, 1f);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            float progress = elapsedTime / fadeInDuration;
            // 使用彈性效果
            float scale = Mathf.Lerp(0f, 1.1f, progress);
            if (progress > 0.8f)
            {
                scale = Mathf.Lerp(1.1f, 1f, (progress - 0.8f) / 0.2f);
            }
            
            rectTransform.localScale = originalScale * scale;
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        rectTransform.localScale = originalScale;
    }
    
    IEnumerator ChangeExpressionSmooth(Image image, Sprite newExpression)
    {
        float elapsedTime = 0f;
        
        // 淡出當前表情
        while (elapsedTime < expressionChangeSpeed / 2)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / (expressionChangeSpeed / 2));
            SetImageAlpha(image, alpha);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 更換精靈
        image.sprite = newExpression;
        
        elapsedTime = 0f;
        
        // 淡入新表情
        while (elapsedTime < expressionChangeSpeed / 2)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / (expressionChangeSpeed / 2));
            SetImageAlpha(image, alpha);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        SetImageAlpha(image, 1f);
    }
    
    IEnumerator FadeToAlpha(Image image, float targetAlpha, float duration)
    {
        float startAlpha = image.color.a;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            SetImageAlpha(image, alpha);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        SetImageAlpha(image, targetAlpha);
    }
}