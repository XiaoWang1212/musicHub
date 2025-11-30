using UnityEngine;

/// <summary>
/// 第三幕管理器 - 繼承基礎 Act 管理器
/// 使用基礎功能，無需額外自定義
/// </summary>
public class Act3Manager : BaseActManager
{
    protected override string GetActName()
    {
        return "Act3";
    }
}
