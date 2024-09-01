using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Timer = System.Threading.Timer;

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

    [LibraryImport("user32.dll")]
    private static partial int GetWindowDC(IntPtr hwnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool InvalidateRect(IntPtr hwnd, IntPtr lpRect, [MarshalAs(UnmanagedType.Bool)] bool bErase);

    [LibraryImport("user32.dll", SetLastError = true)]
    private static partial void mouse_event(uint flags, int x, int y, int data, int extraInfo);

    [LibraryImport("kernel32.dll")]
    private static partial EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private static NotifyIcon? tray;

    #endregion

    private static void Main()
    {
        var mover = new Mover(GetConsoleWindow());
        var moveTimer = new Timer(mover.Move, null, 3000, 5000);

        SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_DISPLAY_REQUIRED); // prevent Idle-to-Sleep
        tray = new NotifyIcon
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
            Text = Console.Title,
            BalloonTipTitle = Console.Title,
            BalloonTipText = Console.Title,
            Visible = true,
        };
        tray.DoubleClick += (s, e) => {
            mover.ToggleWindow(true);
            Console.ReadKey();
            moveTimer.Dispose();
            tray.Dispose();
            Environment.ExitCode = 0;
            Application.Exit();
        };
        mover.ToggleWindow(true);
        Console.WriteLine("Place mouse over the target within 3 seconds.");
        Console.WriteLine("Press any key to exit...");
        Application.Run();
    }

    private class Mover( nint windowHandle)
    {
        #region Objects and variables

        private readonly Random r = new(Environment.TickCount);
        private Point l = new(); // last mouse position
        private Point s = new(); // first mouse position
        private int sW, sH;

        private const int SW_SHOW = 5;
        private const int SW_HIDE = 0;

        #endregion

        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Needed to function as a TimerCallback")]
        internal void Move(object? state)
        {
            var m = new Point(); // current mouse position

            if (s.X == 0 && s.Y == 0) // first trigger
            {
                using var g = Graphics.FromHwnd(IntPtr.Zero);
                var desktop = g.GetHdc();
                
                sW = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPHORZRES); // store screen resolution
                sH = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
                GetCursorPos(ref s); // store current position around which random moves will be performed
                l = s; // last position = start position
                ToggleWindow(false); // hide
            }
            GetCursorPos(ref m); // store current position to check if user was active
            if (m == l) // user inactive
            {
                // move mouse randomly (between -50 and +50 in both x and y direction)
                mouse_event((uint)(MouseFlags.Move | MouseFlags.Absolute), (int)(65535.0 * (s.X + ((r.Next(0, 2) * 2 - 1) * r.Next(1, 51)) + 1) / sW), (int)(65535.0 * (s.Y + ((r.Next(0, 2) * 2 - 1) * r.Next(1, 51)) + 1) / sH), 0, 0);
                InvalidateRect(new IntPtr(GetWindowDC(IntPtr.Zero)), 0, true); // prevent multiple cursor images
            }
            else // user active
            {
                l = m; // update last mouse position to current
            }
        }

        internal void ToggleWindow(bool show)
        {
            ShowWindow(windowHandle, show ? SW_SHOW : SW_HIDE);
        }
    }
}
