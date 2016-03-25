using System;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace ConvenientHanYoung
{
    class Program : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int KEYEVENTF_EXTENDEDKEY = 0x1;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private const string APP_NAME = "ConvenientHanYoung";
        private const string START_WITH_REGISTRY_KEY = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private RegistryUtil registryUtil;

        [STAThread]
        static void Main(string[] args)
        {
            _hookID = SetHook(_proc);
            Application.Run(new Program());
            UnhookWindowsHookEx(_hookID);
        }

        public Program()
        {
            registryUtil = new RegistryUtil(Application.ExecutablePath.ToString());

            MenuItem startWithMenu = new MenuItem();
            startWithMenu.Text = "윈도 시작 시 같이 실행";
            startWithMenu.Checked = registryUtil.isRegisteredForStartProgram;
            startWithMenu.Click += OnToggleStartWith;
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add(startWithMenu);
            trayMenu.MenuItems.Add("종료", OnExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = "편리한 한영";
            trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.

            base.OnLoad(e);
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OnToggleStartWith(object sender, EventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            registryUtil.registerForStartProgram(!item.Checked);
            item.Checked = !item.Checked;
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            {
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
                }
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static bool isLShiftKeyDown = false;
        private static bool isLCtrlKeyDown = false;

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;
                if (wParam == (IntPtr)WM_KEYDOWN)
                {
                    if (Keys.LControlKey.Equals(key))
                    {
                        isLCtrlKeyDown = true;
                    }
                    else if (Keys.LShiftKey.Equals(key))
                    {
                        isLShiftKeyDown = true;
                    }
                    else if ((isLCtrlKeyDown || isLShiftKeyDown) && Keys.Space.Equals(key))
                    {
                        // raise keyboard event
                        // 0x15 is hangul mode key https://msdn.microsoft.com/ko-kr/library/windows/desktop/dd375731%28v=vs.85%29.aspx
                        keybd_event(0x15, 0, KEYEVENTF_EXTENDEDKEY, 0);
                        return (IntPtr)1; // disable default action
                    }
                }
                else if (wParam == (IntPtr)WM_KEYUP)
                {
                    if (Keys.LControlKey.Equals(key))
                    {
                        isLCtrlKeyDown = false;
                    }
                    else if (Keys.LShiftKey.Equals(key))
                    {
                        isLShiftKeyDown = false;
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    }
}
