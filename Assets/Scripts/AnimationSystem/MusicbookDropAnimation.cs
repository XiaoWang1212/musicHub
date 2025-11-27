using UnityEngine;
using System.Collections;

public class MusicbookDropAnimation : MonoBehaviour
{
    [Header("筆記本物件")]
    public GameObject musicbook;                    // 筆記本物件
    public Animator musicbookAnimator;             // 筆記本的Animator組件
    public SpriteRenderer musicbookSpriteRenderer; // 筆記本的SpriteRenderer組件

    [Header("淡入淡出設定")]
    [Range(0f, 2f)]
    public float fadeInDuration = 0.5f;           // 筆記本淡入時間
    [Range(0f, 2f)]
    public float fadeOutDuration = 0.8f;          // 筆記本淡出時間

    [Header("動畫設定")]
    public string animationTriggerName = "Drop";   // 動畫觸發器名稱

    [Header("音效")]
    public AudioSource audioSource;
    public AudioClip dropSound;                   // 掉落音效

    [Header("🎬 場景轉換")]
    public TransitionTrigger transitionTrigger;   // 可重複使用的轉場觸發器

    private bool isAnimating = false;
    private SpriteRenderer[] allSpriteRenderers;

    void Start()
    {
        InitializeMusicbook();
    }

    void InitializeMusicbook()
    {
        if (musicbook != null)
        {
            // 獲取SpriteRenderer組件
            if (musicbookSpriteRenderer == null)
            {
                musicbookSpriteRenderer = musicbook.GetComponent<SpriteRenderer>();
            }

            // 獲取Animator組件
            if (musicbookAnimator == null)
            {
                musicbookAnimator = musicbook.GetComponent<Animator>();
            }

            // 🔥 關鍵修正: 使用 includeInactive = true 確保獲取所有子物件
            allSpriteRenderers = musicbook.GetComponentsInChildren<SpriteRenderer>(true);
            Debug.Log($"📋 找到 {allSpriteRenderers.Length} 個 SpriteRenderer 組件");

            // 列出找到的所有 SpriteRenderer
            for (int i = 0; i < allSpriteRenderers.Length; i++)
            {
                Debug.Log($"   {i}: {allSpriteRenderers[i].gameObject.name} (Alpha: {allSpriteRenderers[i].color.a:F2})");
            }

            // 🔥 強制設置所有物件為完全透明
            foreach (SpriteRenderer renderer in allSpriteRenderers)
            {
                if (renderer != null)
                {
                    Color color = renderer.color;
                    color.a = 0f;
                    renderer.color = color;
                }
            }
            Debug.Log("🎨 已將所有 Sprite 設置為透明");

            // 初始設為不啟用
            musicbook.SetActive(false);
        }

        Debug.Log("📔 筆記本動畫系統已初始化");
    }

    void SetAllSpritesAlpha(float alpha)
    {
        if (allSpriteRenderers != null)
        {
            foreach (SpriteRenderer renderer in allSpriteRenderers)
            {
                if (renderer != null)
                {
                    Color color = renderer.color;
                    color.a = alpha;
                    renderer.color = color;
                }
            }
            Debug.Log($"🎨 設置所有 Sprite 透明度為: {alpha:F2}");
        }
    }

    IEnumerator MusicbookDropSequence()
    {
        isAnimating = true;
        Debug.Log("📔 開始筆記本掉落動畫...");

        // 🔥 關鍵: 先停用 Animator
        if (musicbookAnimator != null)
        {
            musicbookAnimator.enabled = false;
        }

        // 🔥 啟用物件前,再次確保所有 Sprite 都是透明的
        foreach (SpriteRenderer renderer in allSpriteRenderers)
        {
            if (renderer != null)
            {
                Color color = renderer.color;
                color.a = 0f;
                renderer.color = color;
            }
        }
        Debug.Log("🎨 啟用前再次確認所有 Sprite 為透明");

        // 階段1: 淡入效果(包含所有子物件)
        yield return StartCoroutine(FadeIn());

        // 階段2: 播放音效(如果有)
        if (dropSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dropSound);
        }

        // 階段3: 直接播放動畫
        if (musicbookAnimator != null)
        {
            // 啟用 Animator
            musicbookAnimator.enabled = true;
            Debug.Log("🔓 Animator 已啟用");
            
            // 等待幾幀讓 Animator 穩定
            yield return null;
            
            musicbookAnimator.Play("Musicbook_DropAndLand", 0, 0f);
            Debug.Log("🎬 強制播放動畫狀態: Musicbook_DropAndLand (從0開始)");

            // 等待動畫播到最後一幀
            yield return StartCoroutine(WaitForAnimationToLastFrame());
            
            // 立即停止 Animator,防止自動切回 Idle
            musicbookAnimator.enabled = false;
            Debug.Log("🛑 動畫已停在最後一幀,Animator 已停用");
        }

        // 階段4: 淡出(包含所有子物件)
        yield return StartCoroutine(FadeOut());

        // 階段5: 重置 Animator 並隱藏物件
        if (musicbookAnimator != null)
        {
            // 重新啟用 Animator
            musicbookAnimator.enabled = true;
            yield return null;
            
            // 重置到 Idle 空狀態
            musicbookAnimator.Play("Idle", 0, 0f);
            Debug.Log("🔄 Animator 已重置到 Idle 狀態");
            
            yield return null;
        }
        musicbook.SetActive(false);

        Debug.Log("✅ 筆記本掉落動畫完成");

        // 🆕 階段6: 執行場景轉換（如果有設置）
        if (transitionTrigger != null)
        {
            transitionTrigger.TriggerTransition();
            Debug.Log("🎬 已觸發場景轉換");
        }

        isAnimating = false;
    }

    // 🆕 檢查動畫參數是否存在
    bool HasAnimationParameter(string parameterName)
    {
        if (musicbookAnimator == null) return false;

        for (int i = 0; i < musicbookAnimator.parameterCount; i++)
        {
            if (musicbookAnimator.GetParameter(i).name == parameterName)
            {
                return true;
            }
        }
        return false;
    }

    // 🆕 列出可用參數
    void ListAnimationParameters()
    {
        Debug.Log("📋 可用的動畫參數:");
        for (int i = 0; i < musicbookAnimator.parameterCount; i++)
        {
            var param = musicbookAnimator.GetParameter(i);
            Debug.Log($"  - {param.name} ({param.type})");
        }
    }

    // 🆕 等待動畫播放到最後一幀
    IEnumerator WaitForAnimationToLastFrame()
    {
        Debug.Log("⏱️ 等待動畫播放到最後一幀...");

        yield return new WaitForSeconds(0.02f); // 極短等待讓動畫開始

        int safetyCounter = 0;
        int targetHash = Animator.StringToHash("Musicbook_DropAndLand");

        while (safetyCounter < 200) // 最多等待200幀
        {
            if (musicbookAnimator == null) break;

            AnimatorStateInfo state = musicbookAnimator.GetCurrentAnimatorStateInfo(0);

            // 檢查是否在目標動畫狀態
            if (state.shortNameHash == targetHash)
            {
                // 🔥 等待動畫播放到 95% 以上(確保已到最後一幀)
                if (state.normalizedTime >= 0.95f)
                {
                    Debug.Log($"✅ 動畫已到達最後一幀 - 進度: {state.normalizedTime:F3}");
                    yield break; // 立即返回
                }

                // 每20幀輸出一次進度
                if (safetyCounter % 20 == 0)
                {
                    Debug.Log($"🎬 播放進度: {state.normalizedTime:F3}");
                }
            }

            safetyCounter++;
            yield return null;
        }

        Debug.Log("⚠️ 動畫監控超時");
    }

    // 🔧 修改 FadeIn 使用所有子物件
    IEnumerator FadeIn()
    {
        foreach (SpriteRenderer renderer in allSpriteRenderers)
        {
            if (renderer != null)
            {
                Color color = renderer.color;
                color.a = 0f;
                renderer.color = color;
            }
        }

        SpriteRenderer musicbookRenderer = null;
        System.Collections.Generic.List<SpriteRenderer> musicsheetRenderers = new System.Collections.Generic.List<SpriteRenderer>();

        foreach (SpriteRenderer renderer in allSpriteRenderers)
        {
            if (renderer != null)
            {
                // 判斷是 musicbook 還是 musicsheet
                if (renderer.gameObject.name.ToLower().Contains("musicsheet"))
                {
                    musicsheetRenderers.Add(renderer);
                    Debug.Log($"   識別為 musicsheet: {renderer.gameObject.name}");
                }
                else if (renderer.gameObject == musicbook)
                {
                    musicbookRenderer = renderer;
                    Debug.Log($"   識別為 musicbook: {renderer.gameObject.name}");
                }
            }
        }

        // 🔥 階段1: 先淡入 musicbook
        Debug.Log("📘 階段1: 淡入 musicbook...");
        float elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);

            // 只對 musicbook 應用透明度
            if (musicbookRenderer != null)
            {
                Color color = musicbookRenderer.color;
                color.a = alpha;
                musicbookRenderer.color = color;
            }

            yield return null;
        }

        // 確保 musicbook 完全不透明
        if (musicbookRenderer != null)
        {
            Color color = musicbookRenderer.color;
            color.a = 1f;
            musicbookRenderer.color = color;
        }

        Debug.Log("✅ musicbook 淡入完成");

        yield return null;

        // 🔥 階段2: 淡入 musicsheet
        Debug.Log("📄 階段2: 淡入 musicsheet...");
        elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);

            // 對所有 musicsheet 應用透明度
            foreach (SpriteRenderer renderer in musicsheetRenderers)
            {
                if (renderer != null)
                {
                    Color color = renderer.color;
                    color.a = alpha;
                    renderer.color = color;
                }
            }

            yield return null;
        }

        // 確保所有 musicsheet 完全不透明
        foreach (SpriteRenderer renderer in musicsheetRenderers)
        {
            if (renderer != null)
            {
                Color color = renderer.color;
                color.a = 1f;
                renderer.color = color;
            }
        }

        Debug.Log("✅ 所有 musicsheet 淡入完成");

        // 🔥 驗證結果
        Debug.Log("📊 淡入完成驗證:");
        for (int i = 0; i < allSpriteRenderers.Length; i++)
        {
            Debug.Log($"   {allSpriteRenderers[i].gameObject.name} - Alpha: {allSpriteRenderers[i].color.a:F2}");
        }
    }

    // 🔧 修改 FadeOut 使用所有子物件
    IEnumerator FadeOut()
    {
        Debug.Log("🌄 開始淡出效果（包含所有子物件）...");

        if (allSpriteRenderers == null || allSpriteRenderers.Length == 0)
        {
            Debug.LogError("❌ 沒有找到 SpriteRenderer 組件！");
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);

            // 🔧 對所有 SpriteRenderer 應用透明度
            SetAllSpritesAlpha(alpha);

            yield return null;
        }

        // 確保完全透明
        SetAllSpritesAlpha(0f);

        Debug.Log("✅ 所有物件淡出完成");
    }

    [ContextMenu("播放筆記本掉落動畫")]
    public void PlayMusicbookDropAnimation()
    {
        if (isAnimating)
        {
            Debug.Log("⚠️ 動畫正在播放中，跳過");
            return;
        }

        if (musicbook != null)
        {
            // 🔥 重新掃描所有 SpriteRenderer,確保沒有遺漏
            allSpriteRenderers = musicbook.GetComponentsInChildren<SpriteRenderer>(true);
            Debug.Log($"📋 重新掃描: 找到 {allSpriteRenderers.Length} 個 SpriteRenderer");
            
            for (int i = 0; i < allSpriteRenderers.Length; i++)
            {
                Debug.Log($"   {i}: {allSpriteRenderers[i].gameObject.name}");
            }
            
            // 🔥 確保所有物件都是透明的
            foreach (SpriteRenderer renderer in allSpriteRenderers)
            {
                if (renderer != null)
                {
                    Color color = renderer.color;
                    color.a = 0f;
                    renderer.color = color;
                }
            }
            
            musicbook.SetActive(true);
            StartCoroutine(MusicbookDropSequence());
        }
        else
        {
            Debug.LogError("❌ Musicbook 物件未設置！");
        }
    }

    [ContextMenu("重置筆記本")]
    public void ResetMusicbook()
    {
        StopAllCoroutines();
        isAnimating = false;

        if (musicbook != null)
        {
            musicbook.SetActive(false);

            // 🔧 重置所有 SpriteRenderer 的透明度
            SetAllSpritesAlpha(0f);

            // 重置動畫狀態
            if (musicbookAnimator != null)
            {
                musicbookAnimator.ResetTrigger(animationTriggerName);
            }
        }

        Debug.Log("🔄 筆記本動畫已重置");
    }

    // 🆕 調試方法：檢查子物件狀態
    [ContextMenu("🔍 檢查子物件狀態")]
    public void CheckChildrenStatus()
    {
        Debug.Log("🔍 檢查所有子物件狀態...");

        if (musicbook == null)
        {
            Debug.LogError("❌ musicbook 未設置");
            return;
        }

        Debug.Log($"📔 主物件: {musicbook.name} (Active: {musicbook.activeSelf})");

        // 重新掃描所有 SpriteRenderer
        allSpriteRenderers = musicbook.GetComponentsInChildren<SpriteRenderer>();
        Debug.Log($"📋 找到 {allSpriteRenderers.Length} 個 SpriteRenderer");

        for (int i = 0; i < allSpriteRenderers.Length; i++)
        {
            var renderer = allSpriteRenderers[i];
            Debug.Log($"   {i}: {renderer.gameObject.name} - Alpha: {renderer.color.a:F2} (Active: {renderer.gameObject.activeSelf})");
        }
    }

    // 🆕 測試所有子物件淡入
    [ContextMenu("🧪 測試子物件淡入")]
    public void TestChildrenFadeIn()
    {
        if (musicbook != null)
        {
            musicbook.SetActive(true);
            StartCoroutine(TestFadeSequence());
        }
    }

    IEnumerator TestFadeSequence()
    {
        Debug.Log("🧪 測試淡入效果...");
        yield return StartCoroutine(FadeIn());

        yield return new WaitForSeconds(2f);

        Debug.Log("🧪 測試淡出效果...");
        yield return StartCoroutine(FadeOut());

        musicbook.SetActive(false);
        Debug.Log("🧪 測試完成");
    }

    public bool IsAnimating()
    {
        return isAnimating;
    }
}