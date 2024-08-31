using System.Drawing;
using System.Runtime.InteropServices;

namespace Mauzer;

internal partial class Program
{
    #region Imports

    [Flags]
    public enum MouseFlags
    {
        Absolute = 0x8000,
        Move = 0x0001,
    }

    public enum DeviceCap
    {
        /// <summary>
        /// Vertical height of entire desktop in pixels
        /// </summary>
        DESKTOPVERTRES = 117,
        /// <summary>
        /// Horizontal width of entire desktop in pixels
        /// </summary>
        DESKTOPHORZRES = 118,
    }

    [Flags]
    public enum EXECUTION_STATE : uint
    {
        ES_AWAYMODE_REQUIRED = 0x00000040,
        ES_CONTINUOUS = 0x80000000,
        ES_DISPLAY_REQUIRED = 0x00000002,
        ES_SYSTEM_REQUIRED = 0x00000001
    }

    [LibraryImport("kernel32.dll")]
    private static partial IntPtr GetConsoleWindow();

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetCursorPos(ref Point lpPoint);

    [LibraryImport("gdi32.dll")]
    private static partial int GetDeviceCaps(IntPtr hdc, int nIndex);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial void mouse_event(uint flags, int x, int y, int data, int extraInfo);

    [LibraryImport("kernel32.dll")]
    private static partial EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

    #endregion

    private static void Main()
    {
        var moveTimer = new Timer(new Mover().Move, null, 3000, 10000);

        SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_DISPLAY_REQUIRED); // prevent Idle-to-Sleep

        Console.WriteLine("Place mouse over the target within 3 seconds.");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();

        moveTimer.Dispose();
    }

    private class Mover()
    {
        #region Objects and variables

        private readonly Random r = new(Environment.TickCount);
        private Point s = new();
        private int sW, sH;

        #endregion

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Needed to function as a TimerCallback")]
        internal void Move(object? state)
        {
            if (s.X == 0 && s.Y == 0) // first trigger
            {
                using var g = Graphics.FromHwnd(IntPtr.Zero);
                var desktop = g.GetHdc();
                sW = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPHORZRES);
                sH = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
                GetCursorPos(ref s); // store current position around which random moves will be performed
                ShowWindow(GetConsoleWindow(), 6); // minimize console window
            }
            mouse_event((uint)(MouseFlags.Move | MouseFlags.Absolute), (int)(65535.0 * (s.X + ((r.Next(0, 2) * 2 - 1) * r.Next(1, 51)) + 1) / sW), (int)(65535.0 * (s.Y + ((r.Next(0, 2) * 2 - 1) * r.Next(1, 51)) + 1) / sH), 0, 0);
        }
    }
}
