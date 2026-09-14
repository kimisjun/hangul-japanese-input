namespace HangulJapaneseInput.App;

internal sealed class KanjiCandidateForm : Form
{
    private readonly ListBox _candidateList = new();

    public KanjiCandidateForm(string reading, IReadOnlyList<string> candidates)
    {
        Text = "한자 후보 선택";
        Font = new Font("Yu Gothic UI", 11F);
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        TopMost = true;
        KeyPreview = true;
        ClientSize = new Size(300, Math.Min(260, 78 + candidates.Count * 36));

        var guide = new Label
        {
            Text = $"{reading}  —  ↑↓ 선택, Enter 확정, Esc 취소",
            Dock = DockStyle.Top,
            Height = 42,
            Padding = new Padding(10, 10, 6, 4),
            Font = new Font("맑은 고딕", 9.5F)
        };

        _candidateList.Dock = DockStyle.Fill;
        _candidateList.IntegralHeight = false;
        _candidateList.Items.AddRange(candidates.Cast<object>().ToArray());
        _candidateList.SelectedIndex = 0;
        _candidateList.DoubleClick += (_, _) => ConfirmSelection();

        Controls.Add(_candidateList);
        Controls.Add(guide);
        KeyDown += HandleKeyDown;
        Shown += (_, _) => _candidateList.Focus();

        var screen = Screen.FromPoint(Cursor.Position).WorkingArea;
        var x = Math.Min(Cursor.Position.X + 12, screen.Right - Width);
        var y = Math.Min(Cursor.Position.Y + 18, screen.Bottom - Height);
        Location = new Point(Math.Max(screen.Left, x), Math.Max(screen.Top, y));
    }

    public string? SelectedCandidate =>
        DialogResult == DialogResult.OK ? _candidateList.SelectedItem as string : null;

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            ConfirmSelection();
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Escape)
        {
            DialogResult = DialogResult.Cancel;
            Close();
            e.Handled = true;
        }
    }

    private void ConfirmSelection()
    {
        if (_candidateList.SelectedItem is null) return;
        DialogResult = DialogResult.OK;
        Close();
    }
}
