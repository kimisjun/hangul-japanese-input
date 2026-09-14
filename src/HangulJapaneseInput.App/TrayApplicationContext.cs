using HangulJapaneseInput.Core;

namespace HangulJapaneseInput.App;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly JapanesePhoneticConverter _converter = new();
    private readonly MainForm _form;
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _enabledMenu;
    private readonly HwpTextReplacementService _replacement;
    private readonly GlobalKeyboardHook _hook;

    public TrayApplicationContext()
    {
        var context = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(context);

        _form = new MainForm(_converter);
        _form.ConversionEnabledChanged += (_, _) => SetEnabled(_form.ConversionEnabled);
        _form.FormClosed += (_, _) => ExitThread();

        _replacement = new HwpTextReplacementService(_converter, context);
        _hook = new GlobalKeyboardHook { CommandPressed = _replacement.TryHandleCommand };

        _enabledMenu = new ToolStripMenuItem("자동 변환 사용")
        {
            Checked = true,
            CheckOnClick = true
        };
        _enabledMenu.CheckedChanged += (_, _) => SetEnabled(_enabledMenu.Checked);

        var showMenu = new ToolStripMenuItem("설정 창 열기", null, (_, _) => ShowForm());
        var exitMenu = new ToolStripMenuItem("끝내기", null, (_, _) => ExitApplication());
        var menu = new ContextMenuStrip();
        menu.Items.AddRange([showMenu, _enabledMenu, new ToolStripSeparator(), exitMenu]);

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "한글 발음 → 일본어 입력기 (사용 중)",
            ContextMenuStrip = menu,
            Visible = true
        };
        _notifyIcon.DoubleClick += (_, _) => ShowForm();
        _notifyIcon.ShowBalloonTip(3000, "한글 발음 일본어 입력기", "Space: 히라가나 · F7: 가타카나 · Ctrl+Space: 한자", ToolTipIcon.Info);

        _form.Show();
    }

    private void ShowForm()
    {
        if (!_form.Visible) _form.Show();
        if (_form.WindowState == FormWindowState.Minimized) _form.WindowState = FormWindowState.Normal;
        _form.Activate();
    }

    private void SetEnabled(bool enabled)
    {
        _replacement.Enabled = enabled;
        if (_form.ConversionEnabled != enabled) _form.ConversionEnabled = enabled;
        if (_enabledMenu.Checked != enabled) _enabledMenu.Checked = enabled;
        _notifyIcon.Text = enabled
            ? "한글 발음 → 일본어 입력기 (사용 중)"
            : "한글 발음 → 일본어 입력기 (일시 정지)";
    }

    private void ExitApplication()
    {
        _form.AllowClose();
        _form.Close();
    }

    protected override void ExitThreadCore()
    {
        _hook.Dispose();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _form.Dispose();
        base.ExitThreadCore();
    }
}
