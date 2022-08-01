# コミットメッセージについて

このリポジトリでは、コミットメッセージに関して以下のようなルールを設けています。

## 構文

```ebnf
emoji       = ? :sparkles: 等の、GitHub で利用可能な絵文字 ?;
scope       = ? 半角英数字列 ?;
description = ? 任意の文字列 ?;
commit_msg  = emoji , [ ":boom:" ] , [ "(" , scope , ")" ] , description;
```

## 意味

💥 `:boom:` が含まれる場合、そのコミットが破壊的変更を含むことを意味します。

(執筆中)

## 絵文字とコミット種別の対応関係

- 🎉 `:tada:` 初回コミット
- 🆕 `:new:` プロジェクト作成
- ✨ `:sparkles:` 機能追加
- 🏷️ `:label:` 型定義の更新
- 🐛 `:bug:` バグ修正
- ⚡ `:zap:` パフォーマンス改善
- 👽 `:alien:` 外部 API の変更に伴う修正
- ➕ `:heavy_plus_sign:` 依存パッケージの追加
- ➖ `:heavy_minus_sign:` 依存パッケージの削除
- ⬆️ `:arrow_up:` 依存パッケージのアップグレード
- ⬇️ `:arrow_down:` 依存パッケージのダウングレード
- 📄 `:page_facing_up:` ライセンス表記の更新
- 📝 `:memo:` ドキュメントの更新
