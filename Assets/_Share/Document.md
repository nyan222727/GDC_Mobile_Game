# GDC_Mobile_Game 專案架構說明

## 資料夾結構

```
Assets/
├── _David/                  # David 的個人工作區
│   └── David_Scene          # David 的場景
├── _Little_Eagle/           # Little Eagle 的個人工作區
│   └── Little_Eagle_Scene   # Little Eagle 的場景
├── _Nyan/                   # Nyan 的個人工作區
│   └── Nyan_Scene           # Nyan 的場景
└── _Share/                  # 全團隊共用資源
    ├── Settings/            # 專案設定（URP、Quality 等）
    ├── InputSystem_Actions/ # Input System 輸入設定
    └── Document.md          # 本文件
```

## 資料夾說明

### 個人工作區（`_David` / `_Little_Eagle` / `_Nyan`）
- 各成員的個人開發區域，放自己正在開發的場景、腳本、素材等
- 他人的資料夾請先溝通再動

### 共用資源（`_Share`）
- 全團隊共用的資源與設定，修改前記得跟大家說一聲
- `Settings/` — URP Renderer、Quality Settings 等渲染管線設定
- `InputSystem_Actions/` — 統一的輸入設定（鍵盤、觸控、手把）

## Git 開發流程
每個人都在自己的分支上開發，完成後再一起合併回 main。