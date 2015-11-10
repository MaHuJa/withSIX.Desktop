// <copyright company="SIX Networks GmbH" file="SocketExtensions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SN.withSIX.Core.Extensions
{
    public static class SocketExtensions
    {
        public static async Task<int> SendWithTimeoutAfter(this UdpClient client, Byte[] list, int delay) {
            using (var token = new CancellationTokenSource())
                return await SendWithTimeoutAfter(client, list, delay, token).ConfigureAwait(false);
        }

        public static async Task<int> SendWithTimeoutAfter(this UdpClient client, Byte[] list, int delay,
            CancellationTokenSource cancel) {
            if (client == null)
                throw new NullReferenceException();

            if (list == null)
                throw new ArgumentNullException("list");

            var task = client.SendAsync(list, list.Length);

            if (task.IsCompleted || (delay == Timeout.Infinite))
                return await task;

            await Task.WhenAny(task, Task.Delay(delay, cancel.Token)).ConfigureAwait(false);

            if (!task.IsCompleted) {
                client.Close();
                try {
                    return await task.ConfigureAwait(false);
                } catch (ObjectDisposedException) {
                    throw new TimeoutException("Send timed out.");
                }
            }

            cancel.Cancel(); // Cancel the timer! 
            return await task.ConfigureAwait(false);
        }

        public static async Task<UdpReceiveResult> ReceiveWithTimeoutAfter(this UdpClient client, int milliSecondsDelay) {
            using (var token = new CancellationTokenSource())
                return await ReceiveWithTimeoutAfter(client, milliSecondsDelay, token).ConfigureAwait(false);
        }

        public static async Task<UdpReceiveResult> ReceiveWithTimeoutAfter(this UdpClient client, int milliSecondsDelay,
            CancellationTokenSource cancel) {
            if (client == null)
                throw new NullReferenceException();

            var task = client.ReceiveAsync();

            if (task.IsCompleted || (milliSecondsDelay == Timeout.Infinite))
                return (await task.ConfigureAwait(false));

            await Task.WhenAny(task, Task.Delay(milliSecondsDelay, cancel.Token)).ConfigureAwait(false);

            if (!task.IsCompleted) {
                client.Close();
                try {
                    return await task.ConfigureAwait(false);
                } catch (ObjectDisposedException) {
                    throw new TimeoutException("Receive timed out.");
                }
            }

            cancel.Cancel(); // Cancel the timer! 
            return await task.ConfigureAwait(false);
        }
    }
}