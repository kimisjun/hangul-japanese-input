using System.Diagnostics;
using System.Runtime.InteropServices;
using HangulJapaneseInput.Core;

namespace HangulJapaneseInput.App;

internal sealed class GlobalKeyboardHook : IDisposable
{
    private const int WhKeyboardLl = 13;
    private const int WmKeyDown = 0x0100;
    private const int WmSysKeyDown = 0x0104;
    private const int VkSpace = 0x20;

    private readonly HookProc _callback;
    private nint _hook;

    public GlobalKeyboardHook()
    {
        _callback = HookCallback;
        using var process = Process.GetCurrentProcess();
        using var module = process.MainModule;
        _hook = SetWindowsHookEx(WhKeyboardLl, _callback, GetModuleHandle(module?.ModuleName), 0);
        if (_hook == 0)
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }
        DiagnosticLog.Write($"hook-installed handle={_hook}");
    }

    public Func<ConversionCommand, bool>? CommandPressed { get; set; }

    private nint HookCallback(int code, nint wParam, nint lParam)
    {
        if (code >= 0 && ((int)wParam == WmKeyDown || (int)wParam == WmSysKeyDown))
        {
            var data = Marshal.PtrToStructure<KbdLlHookStruct>(lParam);
            var modifiers = Control.ModifierKeys;
            var command = ConversionShortcut.Resolve(
                data.VirtualKeyCode,
                modifiers.HasFlag(Keys.Control),
                modifiers.HasFlag(Keys.Shift),
                modifiers.HasFlag(Keys.Alt));
            if (command != ConversionCommand.None &&
                data.ExtraInfo != NativeInput.InjectionMarker)
            {
                var handled = CommandPressed?.Invoke(command) == true;
                DiagnosticLog.Write($"shortcut command={command} flags={data.Flags} extra={data.ExtraInfo} handled={handled}");
                if (handled) return 1;
            }
        }
        return CallNextHookEx(_hook, code, wParam, lParam);
    }

    public void Dispose()
    {
        if (_hook != 0)
        {
            UnhookWindowsHookEx(_hook);
            _hook = 0;
        }
        GC.SuppressFinalize(this);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KbdLlHookStruct
    {
        public int VirtualKeyCode;
        public int ScanCode;
        public int Flags;
        public int Time;
        public nint ExtraInfo;
    }

    private delegate nint HookProc(int code, nint wParam, nint lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern nint SetWindowsHookEx(int idHook, HookProc callback, nint module, uint threadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(nint hook);

    [DllImport("user32.dll")]
    private static extern nint CallNextHookEx(nint hook, int code, nint wParam, nint lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern nint GetModuleHandle(string? moduleName);
}
