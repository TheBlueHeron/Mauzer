using System.Drawing;
using System.Runtime.InteropServices;

namespace Mauzer;

internal partial class Program
{
    #region Imports

    [Flags]
    public enum EXECUTION_STATE : uint
    {
        ES_AWAYMODE_REQUIRED = 0x00000040,
        ES_CONTINUOUS = 0x80000000,
        ES_DISPLAY_REQUIRED = 0x00000002,
        ES_SYSTEM_REQUIRED = 0x00000001
    }

    [LibraryImport("kernel32.dll", EntryPoint = "GetConsoleWindow")]
    private static partial IntPtr GetConsoleWindow();

    [LibraryImport("user32.dll", EntryPoint = "GetCursorPos")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetCursorPos(ref Point lpPoint);

    [LibraryImport("user32.dll", EntryPoint = "SetCursorPos")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetCursorPos(int x, int y);

    [LibraryImport("kernel32.dll", EntryPoint = "SetThreadExecutionState", SetLastError = true)]
    private static partial EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

    [LibraryImport("user32.dll", EntryPoint = "ShowWindow")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

    #endregion

    private static void Main()
    {
        var moveTimer = new Timer(new Mover().Move, null, 3000, 10000);

        SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_DISPLAY_REQUIRED); // prevent Idle-to-Sleep

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();

        moveTimer.Dispose();
    }

    private class Mover()
    {
        #region Objects and variables

        private readonly Random r = new(Environment.TickCount);
        private Point s = new();

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Needed to function as a TimerCallback")]
        internal void Move(object? state)
        {
            if (s.X == 0 && s.Y == 0) // first trigger
            {
                GetCursorPos(ref s); // store current position around which random moves will be performed
                ShowWindow(GetConsoleWindow(), 6); // minimize console window
            }
            _ = SetCursorPos(s.X + ((r.Next(0, 2) * 2 - 1) * r.Next(1, 51)), s.Y + ((r.Next(0, 2) * 2 - 1) * r.Next(1, 51))); // random positive and negative moves of 1 to 50 pixels in X and Y direction independently
        }
    }
}