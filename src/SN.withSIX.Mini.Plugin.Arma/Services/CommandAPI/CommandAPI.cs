// <copyright company="SIX Networks GmbH" file="CommandAPI.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Logging;
using SN.withSIX.Mini.Plugin.Arma.Services.CommandAPI.Commands;

namespace SN.withSIX.Mini.Plugin.Arma.Services.CommandAPI
{
    public class CommandAPI : IDisposable, IEnableLogging
    {
        const int DefaultBufferSize = 1*FileSizeUnits.KB;
        const string DefaultUniquePipeId = "PlaywithSIX";
        static readonly Dictionary<string, Type> mapping;
        static readonly string defaultPipeGuid = Tools.HashEncryption.MD5Hash("Play withSIX");
        public static readonly string DefaultPipeTag = defaultPipeGuid + DefaultUniquePipeId;
        readonly ISubject<IReceiveOnlyMessage, IReceiveOnlyMessage> _messageReceived;
        readonly NamedPipeClientStream _pipeClient;
        readonly List<SentCommand> _tasks = new List<SentCommand>();
        readonly CancellationToken _token;
        public readonly IObservable<IReceiveOnlyMessage> MessageReceived;
        volatile bool _isConnected;

        static CommandAPI() {
            mapping = new Dictionary<string, Type> {
                {ReplyCommand.Command, typeof (ReplyCommand)},
                {ConnectCommand.Command, typeof (ConnectCommand)},
                {SessionCommand.Command, typeof (SessionCommand)},
                {MessageCommand.Command, typeof (MessageCommand)},
                {ShutdownCommand.Command, typeof (ShutdownCommand)},
                {MissingAddonsMessage.Command, typeof (MissingAddonsMessage)}
            };
        }

        public CommandAPI(CancellationToken token, string uniquePipeId = null) {
            _token = token;
            if (uniquePipeId == null)
                uniquePipeId = DefaultUniquePipeId;

            _messageReceived = Subject.Synchronize(new Subject<IReceiveOnlyMessage>());
            MessageReceived = _messageReceived.AsObservable();
            PipeTag = defaultPipeGuid + uniquePipeId;
            _pipeClient = new NamedPipeClientStream(".", PipeTag,
                PipeDirection.InOut, PipeOptions.Asynchronous);
        }

        public string PipeTag { get; }
        public bool IsConnected
        {
            get { return _isConnected; }
            set { _isConnected = value; }
        }
        public bool IsReady { get; set; }

        #region IDisposable Members

        void IDisposable.Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public async Task<IMessage> QueueSend(ISendMessage command) {
            var tcs = new TaskCompletionSource<IMessage>();

            var done = await TryQueueSend(command, tcs).ConfigureAwait(false);

            if (done) {
                lock (_tasks) {
                    _tasks.Add(new SentCommand(tcs, command));
                }
            }

            return await tcs.Task.ConfigureAwait(false);
        }

        async Task<bool> TryQueueSend(ISendMessage command, TaskCompletionSource<IMessage> tcs) {
            var done = false;
            try {
                await Write(command.ToGameCommand()).ConfigureAwait(false);
                if (!(command is IReceiveMessage))
                    tcs.SetResult(command);
                else
                    done = true;
            } catch (Exception e) {
                tcs.SetException(e);
            }
            return done;
        }

        public async Task<T> QueueSend<T>(ISendMessage command) {
            return (T) await QueueSend(command);
        }

        public Task ReadLoop() {
            return
                Task.Factory.StartNew(() => ReadLoopInternal(), _token, TaskCreationOptions.LongRunning,
                    TaskScheduler.Default).Unwrap();
        }

        async Task ReadLoopInternal() {
            while (_isConnected) {
                _token.ThrowIfCancellationRequested();
                await TryWaitForMessage().ConfigureAwait(false);
            }
        }

        async Task TryWaitForMessage() {
            try {
                var result = await ReadPipe().ConfigureAwait(false);
                var message = ParseResult(result);
                if (message != null)
                    HandleMessage(message);
            } catch (InvalidCommandNoSpaceException e) {
                this.Logger().FormattedWarnException(e);
                Close();
            } catch (Exception e) {
                this.Logger().FormattedWarnException(e);
            }
        }

        void HandleMessage(IMessage message) {
            var ro = message as IReceiveOnlyMessage;
            if (ro != null) {
                _messageReceived.OnNext(ro);
                return;
            }

            var t = message.GetType();
            lock (_tasks) {
                var task = _tasks.FirstOrDefault(x => x.OriginalCommand.GetType() == t);
                if (task == null) {
                    throw new Exception(String.Format("Received message {0} for unknown command pair ({1})", t,
                        String.Join(", ", _tasks.Select(x => x.OriginalCommand))));
                }
                task.Tcs.SetResult(message);
                _tasks.Remove(task);
            }
        }

        static IMessage ParseResult(string result) {
            var index = result.IndexOf(' ');
            if (index <= -1) {
                throw new InvalidCommandNoSpaceException(String.Format("Invalid command received (no space): {0}",
                    result));
            }

            var command = result.Substring(0, index);
            if (!mapping.ContainsKey(command))
                throw new InvalidCommandException(String.Format("Invalid command received: {0}\n{1}", command, result));

            var t = mapping[command];
            var msg = (IMessage) Activator.CreateInstance(t);
            var r = msg as IReceiveMessage;
            if (r != null)
                r.ParseInput(result.Substring(index + 1));
            return msg;
        }

        void Connect(int timeout = 5000) {
            _pipeClient.Connect(timeout);
            _pipeClient.ReadMode = PipeTransmissionMode.Message;
            IsConnected = true;
        }

        public void Close() {
            ((IDisposable) this).Dispose();
        }

        public bool RetryConnect(int times = 0, int timeout = 3000) {
            var i = 0;
            var completed = false;
            while (times == 0 || i < times) {
                i++;
                if (TryConnect(timeout, ref completed))
                    break;
            }

            return completed;
        }

        bool TryConnect(int timeout, ref bool completed) {
            try {
                Connect(timeout);
                completed = true;
                return true;
            } catch (TimeoutException) {
                Thread.Sleep(3.Seconds());
            }
            return false;
        }

        public async Task<bool> WaitUntilReady() {
            var uniqueId = GetUniqueId();
            while (IsConnected) {
                if (await IsAlive(uniqueId).ConfigureAwait(false)) {
                    IsReady = true;
                    return true;
                }
                await Task.Delay(1500, _token).ConfigureAwait(false);
            }
            return false;
        }

        static string GetUniqueId() {
            return "Play withSIX" + new Random().Next(9999);
        }

        async Task<bool> IsAlive(string uniqueId) {
            var msg = String.Format("reply {0}", uniqueId);
            await Write(msg).ConfigureAwait(false);
            return await TryWaitForReply(msg).ConfigureAwait(false);
        }

        async Task<bool> TryWaitForReply(string msg) {
            try {
                var message = await ReadPipe(5000).ConfigureAwait(false);
                if (message == msg)
                    return true;
#if DEBUG
            } catch (TimeoutException e) {
                this.Logger().FormattedDebugException(e);
#else
            } catch (TimeoutException) {
#endif
            }
            return false;
        }

        async Task<string> ReadPipe(int timeout = 0, int size = DefaultBufferSize) {
            var buffer = new byte[size];
            var retSize = timeout > 0
                ? await
                    _pipeClient.ReadAsync(buffer, 0, size, _token)
                        .TimeoutAfter(timeout, _token)
                        .ConfigureAwait(false)
                : await _pipeClient.ReadAsync(buffer, 0, size, _token).ConfigureAwait(false);

            return Encoding.UTF8.GetString(buffer.Take(retSize).ToArray());
        }

        Task Write(string message, int timeout = 0) {
            Contract.Requires<ArgumentNullException>(message != null);
            return WriteMessageToPipe(Encoding.UTF8.GetBytes(message), timeout);
        }

        Task WriteMessageToPipe(byte[] msg, int timeout) {
            var task = _pipeClient.WriteAsync(msg, 0, msg.Length, _token);
            return timeout > 0
                ? task.TimeoutAfter(timeout, _token)
                : task;
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                // dispose managed resources
                IsConnected = false;
                _pipeClient.Close();
            }
            // free native resources
        }

        public async Task<IMessage> TryQueueSend(ISendMessage message) {
            try {
                return await QueueSend(message).ConfigureAwait(false);
            } catch (IOException e) {
                this.Logger().FormattedWarnException(e);
                return null;
            }
        }

        public class InvalidCommandException : Exception
        {
            public InvalidCommandException(string message) : base(message) {}
        }

        public class InvalidCommandNoSpaceException : InvalidCommandException
        {
            public InvalidCommandNoSpaceException(string message) : base(message) {}
        }
    }
}