// <copyright company="SIX Networks GmbH" file="SingleInstance.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Core.Presentation.Wpf
{
    [DoNotObfuscateType]
    enum WM
    {
        NULL = 0x0000,
        CREATE = 0x0001,
        DESTROY = 0x0002,
        MOVE = 0x0003,
        SIZE = 0x0005,
        ACTIVATE = 0x0006,
        SETFOCUS = 0x0007,
        KILLFOCUS = 0x0008,
        ENABLE = 0x000A,
        SETREDRAW = 0x000B,
        SETTEXT = 0x000C,
        GETTEXT = 0x000D,
        GETTEXTLENGTH = 0x000E,
        PAINT = 0x000F,
        CLOSE = 0x0010,
        QUERYENDSESSION = 0x0011,
        QUIT = 0x0012,
        QUERYOPEN = 0x0013,
        ERASEBKGND = 0x0014,
        SYSCOLORCHANGE = 0x0015,
        SHOWWINDOW = 0x0018,
        ACTIVATEAPP = 0x001C,
        SETCURSOR = 0x0020,
        MOUSEACTIVATE = 0x0021,
        CHILDACTIVATE = 0x0022,
        QUEUESYNC = 0x0023,
        GETMINMAXINFO = 0x0024,

        WINDOWPOSCHANGING = 0x0046,
        WINDOWPOSCHANGED = 0x0047,

        CONTEXTMENU = 0x007B,
        STYLECHANGING = 0x007C,
        STYLECHANGED = 0x007D,
        DISPLAYCHANGE = 0x007E,
        GETICON = 0x007F,
        SETICON = 0x0080,
        NCCREATE = 0x0081,
        NCDESTROY = 0x0082,
        NCCALCSIZE = 0x0083,
        NCHITTEST = 0x0084,
        NCPAINT = 0x0085,
        NCACTIVATE = 0x0086,
        GETDLGCODE = 0x0087,
        SYNCPAINT = 0x0088,
        NCMOUSEMOVE = 0x00A0,
        NCLBUTTONDOWN = 0x00A1,
        NCLBUTTONUP = 0x00A2,
        NCLBUTTONDBLCLK = 0x00A3,
        NCRBUTTONDOWN = 0x00A4,
        NCRBUTTONUP = 0x00A5,
        NCRBUTTONDBLCLK = 0x00A6,
        NCMBUTTONDOWN = 0x00A7,
        NCMBUTTONUP = 0x00A8,
        NCMBUTTONDBLCLK = 0x00A9,

        SYSKEYDOWN = 0x0104,
        SYSKEYUP = 0x0105,
        SYSCHAR = 0x0106,
        SYSDEADCHAR = 0x0107,
        COMMAND = 0x0111,
        SYSCOMMAND = 0x0112,

        MOUSEMOVE = 0x0200,
        LBUTTONDOWN = 0x0201,
        LBUTTONUP = 0x0202,
        LBUTTONDBLCLK = 0x0203,
        RBUTTONDOWN = 0x0204,
        RBUTTONUP = 0x0205,
        RBUTTONDBLCLK = 0x0206,
        MBUTTONDOWN = 0x0207,
        MBUTTONUP = 0x0208,
        MBUTTONDBLCLK = 0x0209,
        MOUSEWHEEL = 0x020A,
        XBUTTONDOWN = 0x020B,
        XBUTTONUP = 0x020C,
        XBUTTONDBLCLK = 0x020D,
        MOUSEHWHEEL = 0x020E,


        CAPTURECHANGED = 0x0215,

        ENTERSIZEMOVE = 0x0231,
        EXITSIZEMOVE = 0x0232,

        IME_SETCONTEXT = 0x0281,
        IME_NOTIFY = 0x0282,
        IME_CONTROL = 0x0283,
        IME_COMPOSITIONFULL = 0x0284,
        IME_SELECT = 0x0285,
        IME_CHAR = 0x0286,
        IME_REQUEST = 0x0288,
        IME_KEYDOWN = 0x0290,
        IME_KEYUP = 0x0291,

        NCMOUSELEAVE = 0x02A2,

        DWMCOMPOSITIONCHANGED = 0x031E,
        DWMNCRENDERINGCHANGED = 0x031F,
        DWMCOLORIZATIONCOLORCHANGED = 0x0320,
        DWMWINDOWMAXIMIZEDCHANGE = 0x0321,

        #region Windows 7

        DWMSENDICONICTHUMBNAIL = 0x0323,
        DWMSENDICONICLIVEPREVIEWBITMAP = 0x0326,

        #endregion

        USER = 0x0400,

        // This is the hard-coded message value used by WinForms for Shell_NotifyIcon.
        // It's relatively safe to reuse.
        TRAYMOUSEMESSAGE = 0x800, //WM_USER + 1024
        APP = 0x8000
    }

    [SuppressUnmanagedCodeSecurity]
    [DoNotObfuscateType]
    static class NativeMethods
    {
        #region Delegates

        public delegate IntPtr MessageHandler(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled);

        #endregion

        [DllImport("shell32.dll", EntryPoint = "CommandLineToArgvW", CharSet = CharSet.Unicode)]
        static extern IntPtr _CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string cmdLine,
            out int numArgs);

        [DllImport("kernel32.dll", EntryPoint = "LocalFree", SetLastError = true)]
        static extern IntPtr _LocalFree(IntPtr hMem);

        public static string[] CommandLineToArgvW(string cmdLine) {
            var argv = IntPtr.Zero;
            try {
                var numArgs = 0;

                argv = _CommandLineToArgvW(cmdLine, out numArgs);
                if (argv == IntPtr.Zero)
                    throw new Win32Exception();
                var result = new string[numArgs];

                for (var i = 0; i < numArgs; i++) {
                    var currArg = Marshal.ReadIntPtr(argv, i*Marshal.SizeOf(typeof (IntPtr)));
                    result[i] = Marshal.PtrToStringUni(currArg);
                }

                return result;
            } finally {
                var p = _LocalFree(argv);
                // Otherwise LocalFree failed.
                // Assert.AreEqual(IntPtr.Zero, p);
            }
        }
    }

    [DoNotObfuscateType]
    public static class SingleInstance<TApplication>
        where TApplication : Application, ISingleInstanceApp

    {
        #region Public Properties

        public static IList<string> CommandLineArgs { get; set; }

        #endregion

        #region Private Classes

        class IPCRemoteService : MarshalByRefObject
        {
            public void InvokeFirstInstance(IList<string> args) {
                if (Application.Current != null) {
                    // Do an asynchronous call to ActivateFirstInstance function
                    Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Normal, new DispatcherOperationCallback(ActivateFirstInstanceCallback), args);
                }
            }

            public override object InitializeLifetimeService() {
                return null;
            }
        }

        #endregion

        #region Private Fields

        static readonly string Delimiter = ":";
        static readonly string ChannelNameSuffix = "SingeInstanceIPCChannel";
        static readonly string RemoteServiceName = "SingleInstanceApplicationService";
        static readonly string IpcProtocol = "ipc://";
        static Mutex singleInstanceMutex;
        static IpcServerChannel channel;

        #endregion

        #region Public Methods

        public static bool TryInitializeAsFirstInstance<T>(string instanceName, string[] additionalArgs = null,
            params string[] executables)
            where T : Application, ISingleInstanceApp {
            try {
                return SingleInstance<T>.InitializeAsFirstInstance(instanceName, additionalArgs ?? new string[0]);
            } catch (RemotingException e) {
                if (MessageBox.Show(
                    "Another version of the application already appears to be running, would you like to close it?\n\n" +
                    e.Message,
                    "An older version is probably still running", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    throw;

                return KillOtherInstancesAndRetry<T>(instanceName, executables, additionalArgs);
            }
        }

        static bool KillOtherInstancesAndRetry<T>(string instanceName, IEnumerable<string> executables,
            params string[] additionalArgs)
            where T : Application, ISingleInstanceApp {
            foreach (var p in executables.SelectMany(Process.GetProcessesByName))
                KillGracefully(p);
            Thread.Sleep(1000);
            SingleInstance<T>.Cleanup();
            return SingleInstance<T>.InitializeAsFirstInstance(instanceName, additionalArgs);
        }

        static void KillGracefully(Process p) {
            p.CloseMainWindow();
            var i = 0;
            while (!p.SafeHasExited()) {
                i++;
                if (i > 4)
                    break;
                Thread.Sleep(1000);
            }
            if (!p.SafeHasExited())
                p.Kill();
        }

        static bool InitializeAsFirstInstance(string uniqueName, params string[] additionalArgs) {
            CommandLineArgs = GetCommandLineArgs(uniqueName).Concat(additionalArgs).ToArray();

            // Build unique application Id and the IPC channel name.
            var applicationIdentifier = uniqueName + Environment.UserName;

            var channelName = String.Concat(applicationIdentifier, Delimiter, ChannelNameSuffix);

            // Create mutex based on unique application Id to check if this is the first instance of the application. 
            bool firstInstance;
            singleInstanceMutex = new Mutex(true, applicationIdentifier, out firstInstance);
            if (firstInstance)
                CreateRemoteService(channelName);
            else
                SignalFirstInstance(channelName, CommandLineArgs);

            return firstInstance;
        }

        public static void Cleanup() {
            if (singleInstanceMutex != null) {
                singleInstanceMutex.Close();
                singleInstanceMutex = null;
            }

            if (channel != null) {
                ChannelServices.UnregisterChannel(channel);
                channel = null;
            }

            if (Common.OnExit != null)
                Common.OnExit();
        }

        #endregion

        #region Private Methods

        static IList<string> GetCommandLineArgs(string uniqueApplicationName) {
            string[] args = null;
            if (AppDomain.CurrentDomain.ActivationContext == null) {
                // The application was not clickonce deployed, get args from standard API's
                args = Environment.GetCommandLineArgs();
            } else {
                // The application was clickonce deployed
                // Clickonce deployed apps cannot recieve traditional commandline arguments
                // As a workaround commandline arguments can be written to a shared location before 
                // the app is launched and the app can obtain its commandline arguments from the 
                // shared location               
                var appFolderPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), uniqueApplicationName);

                var cmdLinePath = Path.Combine(appFolderPath, "cmdline.txt");
                if (File.Exists(cmdLinePath)) {
                    try {
                        using (TextReader reader = new StreamReader(cmdLinePath, Encoding.Unicode))
                            args = NativeMethods.CommandLineToArgvW(reader.ReadToEnd());
                        File.Delete(cmdLinePath);
                    } catch (IOException) {}
                }
            }

            if (args == null)
                args = new string[0];

            return new List<string>(args);
        }

        static void CreateRemoteService(string channelName) {
            var serverProvider = new BinaryServerFormatterSinkProvider {TypeFilterLevel = TypeFilterLevel.Full};
            IDictionary props = new Dictionary<string, string>();

            props["name"] = channelName;
            props["portName"] = channelName;
            props["exclusiveAddressUse"] = "false";

            // Create the IPC Server channel with the channel properties
            channel = new IpcServerChannel(props, serverProvider);

            // Register the channel with the channel services
            ChannelServices.RegisterChannel(channel, true);

            // Expose the remote service with the REMOTE_SERVICE_NAME
            var remoteService = new IPCRemoteService();
            RemotingServices.Marshal(remoteService, RemoteServiceName);
        }

        static void SignalFirstInstance(string channelName, IList<string> args) {
            var secondInstanceChannel = new IpcClientChannel();
            ChannelServices.RegisterChannel(secondInstanceChannel, true);

            var remotingServiceUrl = IpcProtocol + channelName + "/" + RemoteServiceName;

            // Obtain a reference to the remoting service exposed by the server i.e the first instance of the application
            var firstInstanceRemoteServiceReference =
                (IPCRemoteService) RemotingServices.Connect(typeof (IPCRemoteService), remotingServiceUrl);

            // Check that the remote service exists, in some cases the first instance may not yet have created one, in which case
            // the second instance should just exit
            if (firstInstanceRemoteServiceReference != null) {
                // Invoke a method of the remote service exposed by the first instance passing on the command line
                // arguments and causing the first instance to activate itself
                firstInstanceRemoteServiceReference.InvokeFirstInstance(args);
            }
        }

        static object ActivateFirstInstanceCallback(object arg) {
            // Get command line args to be passed to first instance
            var args = arg as IList<string>;
            ActivateFirstInstance(args);
            return null;
        }

        static void ActivateFirstInstance(IList<string> args) {
            // Set main window state and process command line args
            if (Application.Current == null)
                return;

            ((TApplication) Application.Current).SignalExternalCommandLineArgs(args);
        }

        #endregion
    }

    public abstract class SingleInstanceApp : Application, ISingleInstanceApp
    {
        protected SingleInstanceApp() {
            AppEvent += (sender, args) => LastAppEvent = args;
        }

        public IList<string> LastAppEvent { get; set; }
        public event EventHandler<IList<string>> AppEvent;

        #region ISingleInstanceApp Members

        public bool SignalExternalCommandLineArgs(IList<string> args) {
            OnAppEvent(args);
            return true;
        }

        #endregion

        protected virtual void OnAppEvent(IList<string> e) {
            if (AppEvent != null)
                AppEvent(this, e);
        }
    }
}