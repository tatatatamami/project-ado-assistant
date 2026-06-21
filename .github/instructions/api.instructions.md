---
applyTo: "src/ProjectAdoAssistant.Api/**"
---

# API 層のコーディング規約

## エンドポイント定義

- エンドポイントは Minimal API（`app.MapGet` / `app.MapPost`）で定義する。Controller クラスは使わない。
- ルートは `/api/` プレフィックスで統一する。

## レスポンス形式

- **すべてのレスポンスを `ApiResponse<T>` でラップする。** 直接 DTO を返さない。

```csharp
// 成功
return Results.Ok(ApiResponse<MyDto>.Ok(data));

// 失敗（バリデーションエラー）
return Results.BadRequest(ApiResponse<object?>.Failure(
    "validation_error", "Invalid input.", context.TraceIdentifier));

// 失敗（リソース未存在）
return Results.NotFound(ApiResponse<object?>.Failure(
    "not_found", "Resource not found.", context.TraceIdentifier));
```

## 設定値

- 設定値は `IOptions<T>` 経由で取得する。`IConfiguration` を直接使わない。
- オプションクラスは `[Required]` や `[Range]` でバリデーションを付け、`ValidateDataAnnotations().ValidateOnStart()` を登録する。

```csharp
builder.Services.AddOptions<MyOptions>()
    .BindConfiguration(MyOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();
```

## CancellationToken

- エンドポイントハンドラーの引数に `CancellationToken cancellationToken` を追加し、外部サービス呼び出しに必ず伝搬する。

## ロギング

- 構造化ログを使う。文字列補間（`$"..."`）で直接メッセージを組み立てない。
- 外部入力をログに含める場合は無害化してから渡す（改行・制御文字を除去）。

```csharp
// OK
logger.LogInformation("Request received for agent {AgentId}", Sanitize(agentId));

// NG
logger.LogInformation($"Request received for agent {agentId}");
```

## 例外処理

- エンドポイント内でキャッチすべき例外を個別に処理する。
- 予期しない例外はグローバル例外ハンドラー（`UseExceptionHandler`）に委譲し、エンドポイント内で握りつぶさない。

## Foundry エージェント連携

- `IFoundryAgentClient` インターフェース経由でエージェントを呼び出す。実装クラスを直接使わない。
- エージェントへのリクエストには `threadId`（会話継続 ID）を引き回す。`null` の場合は新規会話として扱われる。
