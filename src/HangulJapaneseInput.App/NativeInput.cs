using System.Runtime.InteropServices;

namespace HangulJapaneseInput.App;

internal static class NativeInput
{
    internal static readonly nint InjectionMarker = 0x484A494E;
    private const int InputKeyboard = 1;
    private const uint KeyUp = 0x0002;
    private const ushort VkControl = 0x11;
    private const ushort VkShift = 0x10;
    private const ushort VkLeft = 0x25;
    private const ushort VkRight = 0x27;
    private const ushort VkC = 0x43;
    private const ushort VkV = 0x56;
    private const ushort VkSpace = 0x20;
    private const ushort VkF7 = 0x76;

    public static void SelectPreviousWord() => SendChord([VkControl, VkShift], VkLeft);
    public static void Copy() => SendChord([VkControl], VkC);
    public static void Paste() => SendChord([VkControl], VkV);
    public static void MoveRight() => SendKey(VkRight);
    public static void Space() => SendKey(VkSpace);
    public static void F7() => SendKey(VkF7);
    public static void ActivateWindow(nint window) => SetForegroundWindow(window);

    private static void SendChord(ushort[] modifiers, ushort key)
    {
        var inputs = new List<Input>();
        inputs.AddRange(modifiers.Select(KeyDown));
        inputs.Add(KeyDown(key));
        inputs.Add(KeyUpInput(key));
        inputs.AddRange(modifiers.Reverse().Select(KeyUpInput));
        Send(inputs);
    }

    private static void SendKey(ushort key) => Send([KeyDown(key), KeyUpInput(key)]);

    private static Input KeyDown(ushort key) => new()
    {
        Type = InputKeyboard,
        Union = new InputUnion { Keyboard = new KeyboardInput { VirtualKey = key, ExtraInfo = InjectionMarker } }
    };

    private static Input KeyUpInput(ushort key) => new()
    {
        Type = InputKeyboard,
        Union = new InputUnion { Keyboard = new KeyboardInput { VirtualKey = key, Flags = KeyUp, ExtraInfo = InjectionMarker } }
    };

    private static void Send(IReadOnlyCollection<Input> inputs)
    {
        var array = inputs.ToArray();
        if (SendInput((uint)array.Length, array, Marshal.SizeOf<Input>()) != array.Length)
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public int Type;
        public InputUnion Union;
    }

    // Win32 INPUT's union is 32 bytes on x64 (MOUSEINPUT is its largest member).
    // Declaring the full union size keeps sizeof(INPUT) at the required 40 bytes.
    [StructLayout(LayoutKind.Explicit, Size = 32)]
    private struct InputUnion
    {
        [FieldOffset(0)] public KeyboardInput Keyboard;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInput
    {
        public ushort VirtualKey;
        public ushort ScanCode;
        public uint Flags;
        public uint Time;
        public nint ExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint count, Input[] inputs, int size);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(nint window);
}
