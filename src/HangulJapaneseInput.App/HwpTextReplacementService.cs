using System.Diagnostics;
using System.Runtime.InteropServices;
using HangulJapaneseInput.Core;

namespace HangulJapaneseInput.App;

internal sealed class HwpTextReplacementService
{
    private readonly JapanesePhoneticConverter _converter;
    private readonly SynchronizationContext _uiContext;
    private int _busy;

    public HwpTextReplacementService(JapanesePhoneticConverter converter, SynchronizationContext uiContext)
    {
        _converter = converter;
        _uiContext = uiContext;
    }

    public bool Enabled { get; set; } = true;

    public bool TryHandleCommand(ConversionCommand command)
    {
        var foreground = GetForegroundProcessName();
        DiagnosticLog.Write($"command-check command={command} enabled={Enabled} foreground={foreground}");
        if (!Enabled || command == ConversionCommand.None ||
            !foreground.Equals("Hwp", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (Interlocked.CompareExchange(ref _busy, 1, 0) != 0)
        {
            return true;
        }

        _uiContext.Post(async _ => await ReplacePreviousWordAsync(command), null);
        return true;
    }

    private async Task ReplacePreviousWordAsync(ConversionCommand command)
    {
        ClipboardSnapshot? snapshot = null;
        var hwpWindow = GetForegroundWindow();
        try
        {
            snapshot = ClipboardSnapshot.Capture();
            NativeInput.SelectPreviousWord();
            await Task.Delay(55);
            NativeInput.Copy();
            await Task.Delay(80);

            var source = Clipboard.ContainsText(TextDataFormat.UnicodeText)
                ? Clipboard.GetText(TextDataFormat.UnicodeText).Trim()
                : string.Empty;
            DiagnosticLog.Write($"clipboard-read length={source.Length}");

            string? converted = null;
            if (command == ConversionCommand.Hiragana)
            {
                _converter.TryConvert(source, out converted!);
            }
            else if (command == ConversionCommand.Katakana)
            {
                _converter.TryConvertToKatakana(source, out converted!);
            }
            else if (command == ConversionCommand.Kanji &&
                     _converter.TryGetKanjiCandidates(source, out var candidates))
            {
                using var candidateForm = new KanjiCandidateForm(source, candidates);
                candidateForm.ShowDialog();
                converted = candidateForm.SelectedCandidate;
                NativeInput.ActivateWindow(hwpWindow);
                await Task.Delay(110);
            }

            if (!string.IsNullOrEmpty(converted))
            {
                DiagnosticLog.Write($"conversion-match command={command} output-length={converted.Length}");
                Clipboard.SetText(converted, TextDataFormat.UnicodeText);
                NativeInput.Paste();
                await Task.Delay(90);
                if (command == ConversionCommand.Hiragana)
                {
                    NativeInput.Space();
                }
            }
            else
            {
                DiagnosticLog.Write($"conversion-no-match command={command}");
                RestoreOriginalCommand(command);
            }
        }
        catch (Exception exception)
        {
            DiagnosticLog.Write($"replacement-error type={exception.GetType().Name} message={exception.Message}");
            // Never eat the user's shortcut when clipboard or HWP interaction fails.
            RestoreOriginalCommand(command);
        }
        finally
        {
            if (snapshot is not null)
            {
                await Task.Delay(120);
                snapshot.Restore();
            }
            Interlocked.Exchange(ref _busy, 0);
        }
    }

    private static void RestoreOriginalCommand(ConversionCommand command)
    {
        NativeInput.MoveRight();
        if (command == ConversionCommand.Hiragana)
        {
            NativeInput.Space();
        }
        else if (command == ConversionCommand.Katakana)
        {
            NativeInput.F7();
        }
    }

    private static string GetForegroundProcessName()
    {
        var window = GetForegroundWindow();
        if (window == 0) return string.Empty;
        GetWindowThreadProcessId(window, out var processId);
        try
        {
            using var process = Process.GetProcessById((int)processId);
            return process.ProcessName;
        }
        catch
        {
            return string.Empty;
        }
    }

    [DllImport("user32.dll")]
    private static extern nint GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(nint window, out uint processId);
}

internal sealed class ClipboardSnapshot
{
    private readonly DataObject _data;
    private readonly bool _hasData;

    private ClipboardSnapshot(DataObject data, bool hasData)
    {
        _data = data;
        _hasData = hasData;
    }

    public static ClipboardSnapshot Capture()
    {
        var snapshot = new DataObject();
        var source = Clipboard.GetDataObject();
        var hasData = false;
        if (source is not null)
        {
            foreach (var format in source.GetFormats(false))
            {
                try
                {
                    var value = source.GetData(format, false);
                    if (value is null) continue;
                    snapshot.SetData(format, value);
                    hasData = true;
                }
                catch { }
            }
        }
        return new ClipboardSnapshot(snapshot, hasData);
    }

    public void Restore()
    {
        try
        {
            if (_hasData) Clipboard.SetDataObject(_data, true);
            else Clipboard.Clear();
        }
        catch { }
    }
}
