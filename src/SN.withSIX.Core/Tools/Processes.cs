// <copyright company="SIX Networks GmbH" file="Processes.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;

namespace SN.withSIX.Core
{
    public static partial class Tools
    {
        public static ProcessesTools Processes = new ProcessesTools();

        #region Nested type: Processes

        public class ProcessesTools : IEnableLogging
        {
            public UacHelper Uac = new UacHelper();

            public virtual Process[] FindProcess(string name, string path = null) {
                Contract.Requires<ArgumentNullException>(name != null);
                return Process.GetProcessesByName(name.Replace(".exe", String.Empty));
            }

            public async Task WaitForExitALittleMore(string procName, int timeoutSeconds = int.MaxValue) {
                await Task.Run(() => WaitForExit(procName, timeoutSeconds)).ConfigureAwait(false);
                await Task.Delay(2*1000).ConfigureAwait(false);
                // to fix file in use.... grr, can we monitor locks of the file instead??
            }

            public void WaitForExit(string procName, int timeoutSeconds = int.MaxValue) {
                if (timeoutSeconds != int.MaxValue)
                    timeoutSeconds *= 1000;
                var cProcId = Process.GetCurrentProcess().Id;
                var proc = FindProcess(procName).FirstOrDefault(iproc => iproc.Id != cProcId);
                if (proc == null)
                    return;
                if (!proc.WaitForExit(timeoutSeconds)) {
                    throw new TimeoutException(
                        String.Format("Process '{0}' did not exit within the specified timeout of {1} seconds.",
                            procName, timeoutSeconds));
                }
            }

            public virtual void Kill(int pid, bool gracefully = false) {
                using (var proc = Process.GetProcessById(pid))
                    KillProcess(proc, gracefully);
            }

            public void SetAffinity(Process process, IEnumerable<int> usedProcessors) {
                var newAffinity = usedProcessors.Aggregate(0, (current, item) => current | (1 << item - 1));
                process.ProcessorAffinity = (IntPtr) newAffinity;
            }

            public virtual void KillProcess(Process p, bool gracefully = false) {
                Contract.Requires<ArgumentNullException>(p != null);
                if (gracefully) {
                    p.CloseMainWindow();
                    var i = 0;
                    while (!p.SafeHasExited()) {
                        i++;
                        if (i > 4)
                            break;
                        Thread.Sleep(1000);
                    }
                }

                if (!p.SafeHasExited())
                    p.Kill();
            }

            public virtual bool KillByName(string name, string path = null, bool gracefully = false) {
                Contract.Requires<ArgumentNullException>(name != null);
                Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(name));
                var processes = FindProcess(name, path);
                foreach (var p in processes) {
                    using (p) {
                        try {
                            KillProcess(p, gracefully);
                        } catch (Exception e) {
                            this.Logger().FormattedErrorException(e);
                        }
                    }
                }

                return processes.Any();
            }

            public virtual void KillProcessInclChildren(int pid, bool gracefully = false) {
                // http://msdn.microsoft.com/en-us/library/aa394372(v=vs.85).aspx
                try {
                    Kill(pid, gracefully);
                } finally {
                    KillProcessChildren(pid, gracefully);
                }
            }

            public virtual void KillProcessChildren(int pid, bool gracefully = false) {
                var processes = GetWmiProcessesByParentId(pid);

                using (processes) {
                    foreach (var p in processes) {
                        using (p) {
                            try {
                                KillProcessInclChildren((int) (UInt32) p["ProcessId"]);
                            } catch (Exception e) {
                                this.Logger().FormattedErrorException(e);
                            }
                        }
                    }
                }
            }

            public virtual void KillNamedProcessChildren(string name, int pid, bool gracefully = false) {
                var processes = GetNamedWmiProcessesByParentId(name, pid);

                using (processes) {
                    foreach (var p in processes) {
                        using (p) {
                            try {
                                KillProcessInclChildren((int) (UInt32) p["ProcessId"]);
                            } catch (Exception e) {
                                this.Logger().FormattedErrorException(e);
                            }
                        }
                    }
                }
            }

            public virtual ManagementObjectCollection GetWmiProcessesByParentId(int pid) {
                return
                    new ManagementObjectSearcher(String.Format("Select * From Win32_Process Where ParentProcessID={0}",
                        pid))
                        .Get();
            }

            public virtual ManagementObjectCollection GetNamedWmiProcessesByParentId(string name, int pid) {
                return
                    new ManagementObjectSearcher(
                        String.Format("Select * From Win32_Process Where ParentProcessID={0} And Name=\"{1}\"",
                            pid, name))
                        .Get();
            }

            public virtual ManagementObjectCollection GetWmiProcessesById(int pid) {
                return new ManagementObjectSearcher("Select * From Win32_Process Where ProcessID=#{id}").Get();
            }

            public virtual string GetCommandlineArgs(Process process) {
                Contract.Requires<ArgumentNullException>(process != null);
                return GetCommandlineArgs(process.Id);
            }

            public virtual string GetCommandlineArgs(int id) {
                var wmiQuery = String.Format("select CommandLine from Win32_Process where ProcessId='{0}'", id);
                using (var searcher = new ManagementObjectSearcher(wmiQuery))
                using (var retObjectCollection = searcher.Get()) {
                    if (retObjectCollection.Count == 0)
                        return null;

                    foreach (ManagementObject retObject in retObjectCollection) {
                        using (retObject)
                            return (string) retObject["CommandLine"];
                    }
                }
                return null;
            }

            public virtual Dictionary<Process, string> GetCommandlineArgs(string name) {
                Contract.Requires<ArgumentNullException>(name != null);
                Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(name));

                var parsedName = name.EndsWith(".exe") ? Path.GetFileNameWithoutExtension(name) : name;
                var procs = FindProcess(parsedName);
                if (procs == null || !procs.Any())
                    return new Dictionary<Process, string>();

                return procs
                    .ToDictionary(proc => proc, GetCommandlineArgs);
            }

            public virtual bool Running(string exe) {
                Contract.Requires<ArgumentNullException>(exe != null);
                Contract.Requires<ArgumentException>(!String.IsNullOrWhiteSpace(exe));

                return Process.GetProcessesByName(exe.Replace(".exe", String.Empty)).Any();
            }

            #region Nested type: NativeMethods

            public class NativeMethods
            {
                public const int SW_SHOWNORMAL = 1;
                public const int SW_SHOWMINIMIZED = 2;
                public const int SW_SHOWMAXIMIZED = 3;
                public const int SW_RESTORE = 9;

                [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
                static extern int EnumWindows(Action<int, int> x, int y);

                [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
                static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

                [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
                static extern int GetWindowTextLength(IntPtr hWnd);

                [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
                public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

                [DllImport("user32.dll", SetLastError = true)]
                static extern bool SetForegroundWindow(IntPtr hwnd);

                [DllImport("user32.dll")]
                static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

                static void SetForeground(string process) {
                    Contract.Requires<ArgumentNullException>(process != null);
                    Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(process));

                    SetForeground(Processes.FindProcess(process));
                }

                public static void SetForeground(Process process, int cmdShow = SW_SHOWNORMAL) {
                    Contract.Requires<ArgumentNullException>(process != null);

                    ShowWindow(process.MainWindowHandle, cmdShow);
                    SetForegroundWindow(process.MainWindowHandle);
                }

                public static void MinimizeWindow(Process process) {
                    Contract.Requires<ArgumentNullException>(process != null);

                    ShowWindow(process.MainWindowHandle, SW_SHOWMINIMIZED);
                }

                static void MaximizeWindow(Process process) {
                    Contract.Requires<ArgumentNullException>(process != null);

                    ShowWindow(process.MainWindowHandle, SW_SHOWMAXIMIZED);
                }

                static void ShowWindow(Process process) {
                    Contract.Requires<ArgumentNullException>(process != null);
                    ShowWindow(process.MainWindowHandle, SW_SHOWNORMAL);
                }

                static void SetForeground(Process[] processes) {
                    Contract.Requires<ArgumentNullException>(processes != null);

                    foreach (var p in processes)
                        SetForeground(p);
                }
            }

            #endregion

            sealed class Win32
            {
                [DllImport("user32.dll")]
                static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

                public static void HideConsoleWindow() {
                    var hWnd = Process.GetCurrentProcess().MainWindowHandle;
                    if (hWnd != IntPtr.Zero)
                        ShowWindow(hWnd, 0); // 0 = SW_HIDE
                }
            }
        }

        #endregion
    }
}