# 🎮 在 Unity Inspector 中設定選擇系統

## 📋 設定步驟

### 步驟 1: 建立選擇序列資產

1. 在 Project 視窗右鍵
2. 選擇 `Create > MusicHub > Choice Sequence`
3. 命名檔案(例如: `Chapter4_Event1_Choices`)

---

### 步驟 2: 設定選項內容

在 Inspector 中:

```
Choice Sequence Asset
├── Choice Description: "第四章事件1 - 大野陽斗練習"
└── Choices (Size = 3)
    ├── Element 0
    │   ├── Choice Text: "我覺得你彈得很好"
    │   ├── Target Character: "大野陽斗"
    │   ├── Relationship Effect: Increase (+1)
    │   ├── Follow Up Dialogue: (可選) 選擇後的對話
    │   └── Character Expression: (可選) "開心"
    ├── Element 1
    │   ├── Choice Text: "我不確定欸"
    │   ├── Target Character: "大野陽斗"
    │   ├── Relationship Effect: None (0)
    │   ├── Follow Up Dialogue: (留空)
    │   └── Character Expression: (留空)
    └── Element 2
        ├── Choice Text: "你可能需要更多練習"
        ├── Target Character: "大野陽斗"
        ├── Relationship Effect: Decrease (-1)
        ├── Follow Up Dialogue: (留空)
        └── Character Expression: "難過"
```

---

### 步驟 3: 在 ActManager 中使用

在你的 Act 場景中:

1. 選擇 ActManager 物件
2. 在 Inspector 找到 **"選擇系統(可選)"** 區域
3. 設定:
   - **Choice Sequence**: 拖曳你建立的 Choice Sequence Asset
   - **After Choice Dialogue**: (可選) 選擇後統一播放的對話

---

## 🎬 使用情境

### 情境 A: 簡單選擇(選完直接轉場)

```
BaseActManager
├── Act Dialogue Sequence: "前置對話"
├── Choice Sequence: "選擇序列" ✅
├── After Choice Dialogue: (留空)
└── Transition To Next Scene: 下一場景
```

**流程**:
1. 播放 Act Dialogue Sequence
2. 對話結束 → 自動顯示 Choice Sequence
3. 玩家選擇 → 好感度變化
4. 轉場到下一場景

---

### 情境 B: 選擇後有統一對話

```
BaseActManager
├── Act Dialogue Sequence: "前置對話"
├── Choice Sequence: "選擇序列" ✅
├── After Choice Dialogue: "選擇後對話" ✅
└── Transition To Next Scene: 下一場景
```

**流程**:
1. 播放 Act Dialogue Sequence
2. 對話結束 → 自動顯示 Choice Sequence
3. 玩家選擇 → 好感度變化
4. 播放 After Choice Dialogue
5. 對話結束 → 轉場

---

### 情境 C: 每個選項有不同後續對話

```
Choice Sequence Asset
└── Choices
    ├── Element 0
    │   ├── Choice Text: "支持他"
    │   ├── Follow Up Dialogue: "支持後的對話" ✅
    │   └── Relationship Effect: Increase
    ├── Element 1
    │   ├── Choice Text: "中立"
    │   ├── Follow Up Dialogue: "中立的對話" ✅
    │   └── Relationship Effect: None
    └── Element 2
        ├── Choice Text: "反對"
        ├── Follow Up Dialogue: "反對後的對話" ✅
        └── Relationship Effect: Decrease
```

**流程**:
1. 播放 Act Dialogue Sequence
2. 對話結束 → 自動顯示 Choice Sequence
3. 玩家選擇選項 0 → 播放 "支持後的對話"
4. 玩家選擇選項 1 → 播放 "中立的對話"
5. 玩家選擇選項 2 → 播放 "反對後的對話"
6. 對話結束 → 轉場

---

## 📝 實際範例

### 第四章事件 1

**建立對話序列**:
1. `Chapter4_Event1_Before` (前置對話)
2. `Chapter4_Event1_Choices` (選擇序列)
3. `Chapter4_Event1_After` (選擇後對話,可選)

**設定 ActManager**:
```
Act2Manager (or BaseActManager)
├── Act Dialogue Sequence: Chapter4_Event1_Before
├── Choice Sequence: Chapter4_Event1_Choices
├── After Choice Dialogue: Chapter4_Event1_After
└── Transition To Next Scene: Act3Scene
```

**設定 Chapter4_Event1_Choices**:
```
Choices (Size = 3)
├── [0] "我覺得你彈得很好"
│   → 大野陽斗 +1
├── [1] "我不確定欸"
│   → 大野陽斗 ±0
└── [2] "你可能需要更多練習"
    → 大野陽斗 -1
```

---

## 🎯 關鍵特性

### 1. 自動流程

- ✅ 對話結束自動檢查是否有選擇
- ✅ 選擇完成自動播放後續對話
- ✅ 好感度自動變化

### 2. 彈性設計

- ✅ 可以不設定選擇(普通 Act)
- ✅ 可以設定統一的選擇後對話
- ✅ 可以每個選項有不同後續對話

### 3. 視覺化編輯

- ✅ 全部在 Inspector 設定
- ✅ 不需要寫程式碼
- ✅ 可以重複使用選擇序列

---

## 🚫 不需要做的事

### ❌ 不需要寫程式碼

以前:
```csharp
void ShowChoices()
{
    ChoiceData[] choices = new ChoiceData[3];
    choices[0] = new ChoiceData { ... };
    choices[1] = new ChoiceData { ... };
    choices[2] = new ChoiceData { ... };
    choiceManager.ShowChoices(choices);
}
```

現在:
- 在 Inspector 拖曳 Choice Sequence Asset ✅

---

### ❌ 不需要建立新的 ActManager

以前:
- 需要寫 `Act2WithChoice.cs`

現在:
- 直接用 `BaseActManager` ✅
- 在 Inspector 設定選擇序列

---

### ❌ 不需要訂閱事件

以前:
```csharp
void Start()
{
    ChoiceManager.OnChoiceMade += HandleChoice;
}
```

現在:
- BaseActManager 自動處理 ✅

---

## 🎉 總結

1. **建立** Choice Sequence Asset
2. **設定** 選項內容
3. **拖曳** 到 ActManager
4. **完成** 🎊

就這麼簡單!
