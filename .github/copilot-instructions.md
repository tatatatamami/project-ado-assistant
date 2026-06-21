# GitHub Copilot Instructions

## プロジェクト概要

**project-ado-assistant** は Azure DevOps 操作を支援するチャット UI アプリケーションです。

### アーキテクチャ

```
ProjectAdoAssistant.Web  (Blazor Server)
    └─ HTTP (HttpClient) ──▶  ProjectAdoAssistant.Api  (ASP.NET Core Minimal API)
                                    └─ Azure AI Foundry Responses API
                                            └─ Foundry エージェント (MCP ツール)
                                                    └─ Azure DevOps (ADO)
```

- **ADO 操作は .NET 側では実装しない。** Azure AI Foundry エージェントが MCP ツール経由で処理する。
- **ProjectAdoAssistant.Core** は共有ライブラリ。インフラ依存（HTTP クライアント・Azure SDK 等）を持たない。

### プロジェクト構成

| プロジェクト | 役割 |
|---|---|
| `ProjectAdoAssistant.Api` | ASP.NET Core Minimal API。Foundry エージェントへのプロキシ。 |
| `ProjectAdoAssistant.Web` | Blazor Server フロントエンド。ユーザー向けチャット UI。 |
| `ProjectAdoAssistant.Core` | DTOs・インターフェース・ドメインモデルの共有ライブラリ。 |

---

## コーディング規約

### 全般

- **C# 12 / .NET 8** を使用する。最新の言語機能（`record`・パターンマッチ・コレクション式 `[]`）を積極的に使う。
- `nullable enable` が有効。null 非許容型に対して null を代入・返却しない。
- クラスは原則 `sealed` にする。継承が必要な場合のみ外す。
- `string.Empty` より `""` は使わず、どちらかに統一する場合は `string.Empty` を使う。
- ログメッセージは構造化ログ（`logger.LogXxx("Message {Key}", value)`）で書く。ログに外部入力を直接埋め込む前に無害化する。

### API 層（ProjectAdoAssistant.Api）

- レスポンスは必ず `ApiResponse<T>` でラップする。
  - 成功: `ApiResponse<T>.Ok(data)` → `Results.Ok(...)`
  - 失敗: `ApiResponse<object?>.Failure(code, message, traceId)` → 適切な HTTP ステータス
- エンドポイントは Minimal API（`app.MapGet` / `app.MapPost`）で定義する。Controller クラスは使わない。
- 設定値は `IOptions<T>` 経由で取得する。コンストラクタインジェクションで注入する。
- 外部サービス呼び出しには `CancellationToken` を必ず伝搬する。

### Core 層（ProjectAdoAssistant.Core）

- インフラ・フレームワーク依存の NuGet パッケージを追加しない。
- DTO は `record` で定義する。
- インターフェースは `I` プレフィックスで命名する（`IFoundryAgentClient` 等）。

### Web 層（ProjectAdoAssistant.Web）

- Blazor の対話モードは **InteractiveServer** を使用する（`@rendermode InteractiveServer`）。
- JavaScript 呼び出しは `IJSRuntime` 経由で行う。直接的な DOM 操作は JS ファイル（`wwwroot/*.js`）に集約する。
- `HttpClient` は `IHttpClientFactory` 経由の型付きクライアント（`IChatApiClient`）を使用する。

---

## 認証

- API 側の Azure AI Foundry 接続は `DefaultAzureCredential` を使用する。
- ローカル開発時は `az login` で認証する。
- シークレット（API キー・PAT 等）をソースコードや `appsettings.json` にコミットしない。
