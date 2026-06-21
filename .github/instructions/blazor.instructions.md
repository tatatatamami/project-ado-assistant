---
applyTo: "**/*.razor"
---

# Blazor コーディング規約

## レンダリングモード

- 対話が必要なページには必ず `@rendermode InteractiveServer` を宣言する。
- 静的な表示のみのコンポーネントにはレンダリングモード指定を省略してよい。

## JavaScript 連携

- DOM 操作・ブラウザ API の呼び出しは `IJSRuntime` 経由で行う。
- JS の実装は `wwwroot/*.js` に集約し、Razor ファイル内に `<script>` タグを直接書かない。
- `IJSRuntime` は `@inject IJSRuntime JSRuntime` で注入する。
- `OnAfterRenderAsync(bool firstRender)` の `firstRender == true` のタイミングで初期化処理を実行する。

```razor
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        await JSRuntime.InvokeVoidAsync("myNamespace.initialize", _elementRef);
    }
}
```

## HTTP 通信

- `HttpClient` を直接 `@inject` せず、型付きクライアントインターフェース（例: `IChatApiClient`）を使う。
- `@inject ProjectAdoAssistant.Web.Services.IChatApiClient ChatApiClient` の形で注入する。

## 状態管理

- コンポーネントのフィールドは `_camelCase` で命名する（例: `_isLoading`, `_errorMessage`）。
- ローディング状態は `bool _isLoading` で管理し、UI の `disabled` 属性に連動させる。
- エラーメッセージは `string? _errorMessage` で管理し、Bootstrap の `alert alert-danger` で表示する。

## フォームとキーボード操作

- `Enter` キー送信など、デフォルトのキーボード挙動を変更する場合は JS 側で `preventDefault` を呼び出す。
- `@onkeydown` イベントは `KeyboardEventArgs` を受け取る非同期メソッドで処理する。

```razor
private async Task HandleKeyDown(KeyboardEventArgs e)
{
    if (e.Key == "Enter" && !e.ShiftKey)
    {
        await SendAsync();
    }
}
```

## アクセシビリティ

- インタラクティブな要素には `aria-label` を付与する。
- ローディング中のスピナーには `role="status"` と `aria-label` を設定する。
- フォームの入力欄には `aria-label` または関連する `<label>` を必ず設定する。
