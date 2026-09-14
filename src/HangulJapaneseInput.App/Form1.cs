using HangulJapaneseInput.Core;

namespace HangulJapaneseInput.App;

public sealed class MainForm : Form
{
    private readonly CheckBox _enabledCheckBox = new();
    private readonly TextBox _previewInput = new();
    private readonly Label _previewResult = new();
    private readonly JapanesePhoneticConverter _converter;
    private bool _allowClose;

    public MainForm(JapanesePhoneticConverter converter)
    {
        _converter = converter;
        BuildUi();
    }

    public bool ConversionEnabled
    {
        get => _enabledCheckBox.Checked;
        set => _enabledCheckBox.Checked = value;
    }

    public event EventHandler? ConversionEnabledChanged;

    public void AllowClose() => _allowClose = true;

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!_allowClose && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            return;
        }
        base.OnFormClosing(e);
    }

    private void BuildUi()
    {
        Text = "한글 발음 → 일본어 입력기";
        ClientSize = new Size(520, 490);
        MinimumSize = new Size(480, 460);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("맑은 고딕", 10F);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;

        var title = new Label
        {
            Text = "한글 발음 → 일본어 입력기",
            Font = new Font("맑은 고딕", 17F, FontStyle.Bold),
            ForeColor = Color.FromArgb(36, 62, 99),
            AutoSize = true,
            Location = new Point(24, 22)
        };

        var subtitle = new Label
        {
            Text = "한컴 한글(HWP)에서 일본어 발음을 한글로 입력한 뒤 단축키를 누르세요.",
            AutoSize = true,
            Location = new Point(27, 66),
            ForeColor = Color.DimGray
        };

        _enabledCheckBox.Text = "HWP 자동 변환 사용";
        _enabledCheckBox.Checked = true;
        _enabledCheckBox.AutoSize = true;
        _enabledCheckBox.Font = new Font("맑은 고딕", 11F, FontStyle.Bold);
        _enabledCheckBox.Location = new Point(28, 105);
        _enabledCheckBox.CheckedChanged += (_, _) => ConversionEnabledChanged?.Invoke(this, EventArgs.Empty);

        var guide = new Label
        {
            Text = string.Join(Environment.NewLine,
            [
                "사용 방법",
                "  와타시 + Space          → わたし       (히라가나)",
                "  테레비 + F7             → テレビ       (가타카나)",
                "  니혼 + Ctrl+Space       → 日本 / 二本  (한자 후보)",
                "  후보 창: ↑↓ 선택 · Enter 확정 · Esc 취소"
            ]),
            Location = new Point(28, 148),
            Size = new Size(455, 140),
            BackColor = Color.FromArgb(244, 247, 251),
            Padding = new Padding(12, 9, 8, 8)
        };

        var previewLabel = new Label
        {
            Text = "변환 미리보기",
            AutoSize = true,
            Font = new Font("맑은 고딕", 10F, FontStyle.Bold),
            Location = new Point(28, 310)
        };

        _previewInput.Location = new Point(28, 338);
        _previewInput.Size = new Size(215, 28);
        _previewInput.PlaceholderText = "예: 와타시와";
        _previewInput.TextChanged += (_, _) => UpdatePreview();

        _previewResult.Location = new Point(260, 337);
        _previewResult.Size = new Size(225, 84);
        _previewResult.Font = new Font("Yu Gothic UI", 10.5F, FontStyle.Bold);
        _previewResult.Text = "→";

        var trayHint = new Label
        {
            Text = "창을 닫아도 작업표시줄 알림 영역에서 계속 실행됩니다.",
            AutoSize = true,
            Location = new Point(28, 448),
            ForeColor = Color.Gray
        };

        Controls.AddRange([title, subtitle, _enabledCheckBox, guide, previewLabel, _previewInput, _previewResult, trayHint]);
    }

    private void UpdatePreview()
    {
        var source = _previewInput.Text.Trim();
        var hasHiragana = _converter.TryConvert(source, out var hiragana);
        var hasKatakana = _converter.TryConvertToKatakana(source, out var katakana);
        var hasKanji = _converter.TryGetKanjiCandidates(source, out var kanji);

        _previewResult.Text = hasHiragana
            ? string.Join(Environment.NewLine,
            [
                $"히라가나  {hiragana}",
                $"가타카나  {(hasKatakana ? katakana : "-")}",
                $"한자 후보  {(hasKanji ? string.Join(" / ", kanji) : "-")}"
            ])
            : hasKanji
                ? string.Join(Environment.NewLine,
                [
                    "히라가나  -",
                    "가타카나  -",
                    $"한자 후보  {string.Join(" / ", kanji)}"
                ])
                : "→  (등록되지 않은 발음)";
    }
}
