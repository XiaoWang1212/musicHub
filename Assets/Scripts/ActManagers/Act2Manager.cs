using UnityEngine;
using System.Collections;

/// <summary>
/// 第二幕管理器 - S 的房間（清晨）
/// 簡化版:只處理對話和音效
/// </summary>
public class Act2Manager : MonoBehaviour
{
    [Header("對話系統")]
    public DialogueManager dialogueManager;
    public DialogueSequenceAsset act2DialogueSequence;  // 在 Inspector 中設定

    [Header("場景物件")]
    public SpriteRenderer musicBookRenderer;  // 音樂筆記本
    public SpriteRenderer characterRenderer;  // 角色 (用於抖動)

    [Header("背景音效")]
    public AudioSource bgmSource;
    public AudioClip morningAmbience;      // 清晨氛圍音
    public AudioClip heartbeatSound;       // 心跳聲
    public AudioClip drawerCloseSound;     // 抽屜關閉音效

    [Header("動畫設定")]
    public float bookFadeInDuration = 2f;  // 筆記本淡入時間
    public float bookFadeOutDuration = 0.3f; // 筆記本淡出時間
    public float shakeIntensity = 0.15f;    // 抖動強度
    public int shakeCount = 2;              // 抖動次數

    [Header("時間控制")]
    public float initialDelay = 1f;

    void Start()
    {
        Debug.Log("🎬 第二幕開始 - S 的房間");
        
        // 初始化筆記本為隱藏
        if (musicBookRenderer != null)
        {
            Color color = musicBookRenderer.color;
            color.a = 0f;
            musicBookRenderer.color = color;
            musicBookRenderer.gameObject.SetActive(false);
        }
        
        // 訂閱對話索引事件
        DialogueManager.OnDialogueIndexChanged += OnDialogueIndexChanged;
        
        StartCoroutine(Act2Sequence());
    }
    
    void OnDestroy()
    {
        // 取消訂閱
        DialogueManager.OnDialogueIndexChanged -= OnDialogueIndexChanged;
    }
    
    void OnDialogueIndexChanged(int dialogueIndex)
    {
        // 根據對話索引觸發特定效果
        switch (dialogueIndex)
        {
            case 4:  // 第 5 句話 (索引從 0 開始)
                ShowMusicBook();
                break;
            case 5:  // 第 6 句話
                HideMusicBookAndShake();
                break;
        }
    }

    IEnumerator Act2Sequence()
    {
        yield return new WaitForSeconds(initialDelay);

        // 播放清晨氛圍音
        PlayAmbience();

        // 開始對話
        StartDialogue();
    }

    void PlayAmbience()
    {
        if (bgmSource != null && morningAmbience != null)
        {
            bgmSource.clip = morningAmbience;
            bgmSource.loop = true;
            bgmSource.volume = 0.3f;
            bgmSource.Play();
        }
    }

    void StartDialogue()
    {
        if (act2DialogueSequence != null && dialogueManager != null)
        {
            // 直接傳入 ScriptableObject，讓 DialogueManager 自動觸發事件
            dialogueManager.StartDialogue(act2DialogueSequence);
        }
        else
        {
            Debug.LogWarning("⚠️ 請在 Inspector 中設定 Act2 Dialogue Sequence!");
        }
    }

    // 可以從 UnityEvent 或對話系統呼叫的音效方法
    public void PlayHeartbeat()
    {
        if (bgmSource != null && heartbeatSound != null)
        {
            bgmSource.PlayOneShot(heartbeatSound, 0.8f);
        }
    }

    public void PlayDrawerClose()
    {
        if (bgmSource != null && drawerCloseSound != null)
        {
            bgmSource.PlayOneShot(drawerCloseSound);
        }
    }

    // 第五句話:慢慢顯現筆記本
    public void ShowMusicBook()
    {
        if (musicBookRenderer != null)
        {
            StartCoroutine(FadeInMusicBook());
        }
    }

    // 第六句話:筆記本快速消失 + 角色抖動
    public void HideMusicBookAndShake()
    {
        StartCoroutine(HideBookAndShakeCharacter());
    }

    IEnumerator FadeInMusicBook()
    {
        if (musicBookRenderer == null) yield break;

        musicBookRenderer.gameObject.SetActive(true);
        
        float elapsed = 0f;
        Color color = musicBookRenderer.color;
        
        while (elapsed < bookFadeInDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsed / bookFadeInDuration);
            musicBookRenderer.color = color;
            yield return null;
        }
        
        // 確保完全顯示
        color.a = 1f;
        musicBookRenderer.color = color;
    }

    IEnumerator HideBookAndShakeCharacter()
    {
        // 同時進行淡出和抖動
        Coroutine fadeOut = StartCoroutine(FadeOutMusicBook());
        Coroutine shake = StartCoroutine(ShakeCharacter());
        
        // 等待兩個動畫完成
        yield return fadeOut;
        yield return shake;
        
        // 播放抽屜關閉音效
        PlayDrawerClose();
    }

    IEnumerator FadeOutMusicBook()
    {
        if (musicBookRenderer == null) yield break;

        float elapsed = 0f;
        Color color = musicBookRenderer.color;
        float startAlpha = color.a;
        
        while (elapsed < bookFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, 0f, elapsed / bookFadeOutDuration);
            musicBookRenderer.color = color;
            yield return null;
        }
        
        // 確保完全隱藏
        color.a = 0f;
        musicBookRenderer.color = color;
        musicBookRenderer.gameObject.SetActive(false);
    }

    IEnumerator ShakeCharacter()
    {
        if (characterRenderer == null) yield break;

        Vector3 originalPosition = characterRenderer.transform.position;  // 改用 position 而非 localPosition
        
        for (int i = 0; i < shakeCount; i++)
        {
            // 向左抖
            characterRenderer.transform.position = originalPosition + Vector3.left * shakeIntensity;
            yield return new WaitForSeconds(0.05f);
            
            // 向右抖
            characterRenderer.transform.position = originalPosition + Vector3.right * shakeIntensity;
            yield return new WaitForSeconds(0.05f);
        }
        
        // 恢復原位
        characterRenderer.transform.position = originalPosition;
    }
}
