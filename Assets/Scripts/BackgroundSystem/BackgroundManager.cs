using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BackgroundManager : MonoBehaviour
{
    [Header("背景顯示")]
    public Image backgroundImage;
    public Image overlayImage; // 用於轉場效果的遮罩圖片
    
    [Header("轉場設定")]
    public float fadeTransitionDuration = 1f;
    public float slideTransitionDuration = 1.2f;
    public Color overlayColor = Color.black;
    
    [Header("背景效果")]
    public bool enableParallax = false;
    public float parallaxSpeed = 0.1f;
    
    // 轉場效果枚舉
    public enum TransitionType
    {
        None,
        Fade,
        SlideLeft,
        SlideRight,
        SlideUp,
        SlideDown,
        CrossFade,
        Dissolve
    }
    
    private Sprite currentBackground;
    private bool isTransitioning = false;
    
    void Start()
    {
        InitializeBackground();
    }
    
    void InitializeBackground()
    {
        if (overlayImage != null)
        {
            overlayImage.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0f);
            overlayImage.gameObject.SetActive(false);
        }
    }
    
    public void ChangeBackground(Sprite newBackground, TransitionType transition = TransitionType.Fade)
    {
        if (isTransitioning || newBackground == null) return;
        
        switch (transition)
        {
            case TransitionType.None:
                SetBackgroundImmediate(newBackground);
                break;
            case TransitionType.Fade:
                StartCoroutine(FadeTransition(newBackground));
                break;
            case TransitionType.SlideLeft:
                StartCoroutine(SlideTransition(newBackground, Vector2.left));
                break;
            case TransitionType.SlideRight:
                StartCoroutine(SlideTransition(newBackground, Vector2.right));
                break;
            case TransitionType.SlideUp:
                StartCoroutine(SlideTransition(newBackground, Vector2.up));
                break;
            case TransitionType.SlideDown:
                StartCoroutine(SlideTransition(newBackground, Vector2.down));
                break;
            case TransitionType.CrossFade:
                StartCoroutine(CrossFadeTransition(newBackground));
                break;
            case TransitionType.Dissolve:
                StartCoroutine(DissolveTransition(newBackground));
                break;
        }
    }
    
    void SetBackgroundImmediate(Sprite newBackground)
    {
        if (backgroundImage != null)
        {
            backgroundImage.sprite = newBackground;
            currentBackground = newBackground;
        }
    }
    
    // 淡入淡出轉場
    IEnumerator FadeTransition(Sprite newBackground)
    {
        isTransitioning = true;
        
        if (overlayImage != null)
        {
            overlayImage.gameObject.SetActive(true);
            
            // 淡入遮罩
            float elapsedTime = 0f;
            while (elapsedTime < fadeTransitionDuration / 2)
            {
                float alpha = Mathf.Lerp(0f, 1f, elapsedTime / (fadeTransitionDuration / 2));
                overlayImage.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, alpha);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 更換背景
            SetBackgroundImmediate(newBackground);
            
            // 淡出遮罩
            elapsedTime = 0f;
            while (elapsedTime < fadeTransitionDuration / 2)
            {
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / (fadeTransitionDuration / 2));
                overlayImage.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, alpha);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            overlayImage.gameObject.SetActive(false);
        }
        else
        {
            SetBackgroundImmediate(newBackground);
        }
        
        isTransitioning = false;
    }
    
    // 滑動轉場
    IEnumerator SlideTransition(Sprite newBackground, Vector2 direction)
    {
        isTransitioning = true;
        
        if (backgroundImage != null)
        {
            // 創建臨時的背景圖片用於滑動效果
            GameObject tempBackground = new GameObject("TempBackground");
            tempBackground.transform.SetParent(backgroundImage.transform.parent);
            
            Image tempImage = tempBackground.AddComponent<Image>();
            tempImage.sprite = newBackground;
            
            RectTransform tempRect = tempBackground.GetComponent<RectTransform>();
            RectTransform bgRect = backgroundImage.GetComponent<RectTransform>();
            
            // 設置臨時背景的初始位置和大小
            tempRect.sizeDelta = bgRect.sizeDelta;
            tempRect.anchoredPosition = bgRect.anchoredPosition + direction * bgRect.rect.width;
            
            // 執行滑動動畫
            float elapsedTime = 0f;
            Vector2 originalPos = bgRect.anchoredPosition;
            Vector2 tempOriginalPos = tempRect.anchoredPosition;
            
            while (elapsedTime < slideTransitionDuration)
            {
                float progress = elapsedTime / slideTransitionDuration;
                
                bgRect.anchoredPosition = Vector2.Lerp(originalPos, originalPos - direction * bgRect.rect.width, progress);
                tempRect.anchoredPosition = Vector2.Lerp(tempOriginalPos, originalPos, progress);
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 完成轉場
            backgroundImage.sprite = newBackground;
            bgRect.anchoredPosition = originalPos;
            currentBackground = newBackground;
            
            Destroy(tempBackground);
        }
        
        isTransitioning = false;
    }
    
    // 交叉淡化轉場
    IEnumerator CrossFadeTransition(Sprite newBackground)
    {
        isTransitioning = true;
        
        if (backgroundImage != null)
        {
            // 創建臨時背景用於交叉淡化
            GameObject tempBackground = new GameObject("CrossFadeBackground");
            tempBackground.transform.SetParent(backgroundImage.transform.parent);
            tempBackground.transform.SetSiblingIndex(backgroundImage.transform.GetSiblingIndex() + 1);
            
            Image tempImage = tempBackground.AddComponent<Image>();
            tempImage.sprite = newBackground;
            
            RectTransform tempRect = tempBackground.GetComponent<RectTransform>();
            RectTransform bgRect = backgroundImage.GetComponent<RectTransform>();
            
            // 設置臨時背景與原背景相同的位置和大小
            tempRect.sizeDelta = bgRect.sizeDelta;
            tempRect.anchoredPosition = bgRect.anchoredPosition;
            
            // 初始透明度
            Color tempColor = tempImage.color;
            tempColor.a = 0f;
            tempImage.color = tempColor;
            
            // 執行交叉淡化
            float elapsedTime = 0f;
            while (elapsedTime < fadeTransitionDuration)
            {
                float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeTransitionDuration);
                
                tempColor.a = alpha;
                tempImage.color = tempColor;
                
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // 完成轉場
            backgroundImage.sprite = newBackground;
            currentBackground = newBackground;
            
            Destroy(tempBackground);
        }
        
        isTransitioning = false;
    }
    
    // 溶解轉場（需要特殊 Shader 支援，這裡用簡單的隨機像素效果代替）
    IEnumerator DissolveTransition(Sprite newBackground)
    {
        isTransitioning = true;
        
        // 使用淡化效果作為溶解的替代
        yield return StartCoroutine(FadeTransition(newBackground));
        
        isTransitioning = false;
    }
    
    // 設置背景濾鏡效果
    public void SetBackgroundFilter(Color filterColor, float intensity = 0.5f)
    {
        if (backgroundImage != null)
        {
            Color currentColor = backgroundImage.color;
            backgroundImage.color = Color.Lerp(currentColor, filterColor, intensity);
        }
    }
    
    // 重置背景濾鏡
    public void ResetBackgroundFilter()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = Color.white;
        }
    }
    
    // 獲取當前背景
    public Sprite GetCurrentBackground()
    {
        return currentBackground;
    }
    
    // 檢查是否正在轉場
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
    
    // 視差效果更新（如果啟用）
    void Update()
    {
        if (enableParallax && backgroundImage != null)
        {
            RectTransform bgRect = backgroundImage.GetComponent<RectTransform>();
            float parallaxOffset = Time.time * parallaxSpeed;
            bgRect.anchoredPosition = new Vector2(parallaxOffset % bgRect.rect.width, bgRect.anchoredPosition.y);
        }
    }
}