# ハングル発音 → 日本語入力ツール

[![Website](https://img.shields.io/badge/公式サイト-GitHub%20Pages-18a978)](https://kimisjun.github.io/hangul-japanese-input/)
[![Release](https://img.shields.io/github/v/release/kimisjun/hangul-japanese-input)](https://github.com/kimisjun/hangul-japanese-input/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

日本語の発音をハングルで入力し、**Hancom Hangul（HWP）**上でひらがな・カタカナ・漢字へ変換するWindows用ツールです。

- `와타시` + `Space` → `わたし`
- `테레비` + `F7` → `テレビ`
- `니혼` + `Ctrl + Space` → `日本` / `二本`

## ダウンロード

最新版は[GitHub Releases](https://github.com/kimisjun/hangul-japanese-input/releases/latest)から無料でダウンロードできます。

## 必要な環境

- Windows 64-bit
- Hancom Office Hangul（한글）
- 韓国語Microsoft IME（2ボル式を推奨）

> 現在のバージョンはHancom Office Hangulアプリ専用です。Microsoft Word、メモ帳、Webブラウザー、HWP互換エディターでは変換しません。現行版の画面表示は韓国語です。

## 基本操作

| 変換 | キー | 例 |
|---|---|---|
| ひらがな | `Space` | `와타시` → `わたし` |
| カタカナ | `F7` | `테레비` → `テレビ` |
| 漢字候補 | `Ctrl + Space` | `니혼` → `日本` / `二本` |

漢字候補画面では、`↑` / `↓`で移動、`Enter`で確定、`Esc`で取り消します。

## マニュアル

- [公式日本語サイト](https://kimisjun.github.io/hangul-japanese-input/)
- [日本語PDFマニュアル](https://kimisjun.github.io/hangul-japanese-input/manual/HangulJapaneseInput_UserManual_JA.pdf)
- リポジトリの `manual-ja` フォルダーには編集用DOCX・HWP・Markdown原稿もあります。

## プライバシー

本ツールはネットワーク通信、広告、アカウント登録、テレメトリーを使用しません。変換時、直前の語をWindowsクリップボードへ一時的にコピーし、通常は処理後に元の内容へ戻します。クリップボード履歴ソフトを使用している場合は履歴に残ることがあります。動作確認用ログは端末内の `%LOCALAPPDATA%\HangulJapaneseInput\diagnostic.log` に保存されますが、入力した単語そのものは記録しません。

## 現在の制限

- 日本語IMEのような文全体の文脈解析は行いません。
- 漢字候補は内蔵辞書に登録済みの語に限られます。
- 発音表記によっては期待した変換にならないことがあります。
- 実行ファイルは現在コード署名されていないため、Windows SmartScreenが警告する場合があります。配布元とハッシュを確認してください。
- SHA-256: `fbb1f0daa09f0f1fb2c604405df9a8b3aaded2f081886f8118f939b756a68c59`
- [チェックサムファイル](https://github.com/kimisjun/hangul-japanese-input/releases/download/v1.0.0/HangulJapaneseInput-v1.0.0-win-x64.zip.sha256)

## 開発とテスト

```bash
dotnet test -c Release
dotnet publish src/HangulJapaneseInput.App/HangulJapaneseInput.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## ライセンス

[MIT License](LICENSE)。個人・教育・非営利・商用を問わず、利用・複製・変更・再配布できます。著作権表示とライセンス本文を残してください。

HancomおよびHancom Hangulは各権利者の商標です。本プロジェクトはHancomの公式製品ではありません。
