---
applyTo: "src/ProjectAdoAssistant.Core/**"
---

# Core 層のコーディング規約

## 依存関係の制約

- **インフラ・フレームワーク依存の NuGet パッケージを追加しない。**
  - 禁止例: `Azure.*`, `Microsoft.AspNetCore.*`, `System.Net.Http.Json` 等
  - 許可例: `System.*`（BCL）のみ
- Core プロジェクトは他の 2 プロジェクト（Api・Web）から参照されるため、依存が増えると全体に波及する。

## DTO

- DTO は `record` で定義する。`class` は使わない。
- プロパティは `init` セッターを使い、不変にする。
- 名前は `～Dto` サフィックスをつける。

```csharp
// OK
public sealed record ChatRequestDto(string Message, string? ThreadId);

// NG
public class ChatRequest { public string Message { get; set; } = ""; }
```

## インターフェース

- インターフェースは `I` プレフィックスで命名する（`IFoundryAgentClient` 等）。
- インターフェースの実装クラスは Core に置かない。実装は Api または Web に配置する。
- 非同期メソッドには必ず `CancellationToken cancellationToken = default` を引数に含める。

## モデル

- ドメインモデルは `Models/` フォルダに配置する。
- エンティティ基底クラスが必要な場合は `BaseEntity` を継承する。
- `enum` は対応するモデルと同じファイルまたは同じフォルダに配置する。

## 命名規則

| 種別 | 例 |
|---|---|
| DTO | `ChatRequestDto`, `ChatResponseDto` |
| インターフェース | `IFoundryAgentClient`, `IClock` |
| モデル | `ChatMessage`, `BaseEntity` |
| enum | `ChatRole` |
