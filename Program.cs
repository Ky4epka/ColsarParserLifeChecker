using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColsarParser;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ColsarParserLifeChecker
{
    class Program
    {
        [Flags]
        public enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0x0,
            SMTO_BLOCK = 0x1,
            SMTO_ABORTIFHUNG = 0x2,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x8,
            SMTO_ERRORONEXIT = 0x0020
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(
            IntPtr windowHandle,
            uint Msg,
            IntPtr wParam,
            IntPtr lParam,
            SendMessageTimeoutFlags flags,
            uint timeout,
            out IntPtr result);

        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();

        [DllImport("User32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        static void StartParser()
        {
            Process.Start(ProgramSettings.ExecutePath(ProgramSettings.PARSER_EXECUTABLE_FILE), ProgramSettings.PARAMSTR_MONITOR_KEY + " " + ProgramSettings.PARAMSTR_SILENT_KEY);
        }

        static void Main(string[] args)
        {
            ProgramSettings.RegisterMonitorMessage();
            Process[] procs = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(ProgramSettings.PARSER_EXECUTABLE_FILE));
            
            if (procs.Length <= 0)
            {
                StartParser();
            }
            else
            {
                if (procs.Length > 1)
                {
                    for (int i=procs.Length - 1; i >= 1; i--)
                    {
                        procs[i].Kill();
                    }
                }
                IntPtr hwnd = FindWindow(null, ProgramSettings.MAIN_FORM_CAPTION);

                if (hwnd != IntPtr.Zero)
                {
                    IntPtr result;
                    if (SendMessageTimeout(
                            hwnd,
                            0,
                            IntPtr.Zero,
                            IntPtr.Zero,
                            SendMessageTimeoutFlags.SMTO_ABORTIFHUNG,
                            ProgramSettings.LIFECHECKER_PROCESS_NOT_ASK_TIMEOUT,
                            out result) == IntPtr.Zero)
                    {
                        Console.WriteLine("Приложение зависло: " + GetLastError());
                        procs[0].Kill();
                        StartParser();
                    }
                    else
                    {
                        PostMessage(hwnd, ProgramSettings.WM_MONITOR_STATUS, IntPtr.Zero, IntPtr.Zero);
                    }
                }
            }

        }
    }
}
