// <copyright company="SIX Networks GmbH" file="WinRTNotificationProvider.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Core.Presentation.WinRT
{
    public class WinRTNotificationProvider : INotificationProvider
    {
        public Task<bool?> Notify(string subject, string text, string icon = null, TimeSpan? expirationTime = null) {
            const ToastTemplateType toastTemplate = ToastTemplateType.ToastImageAndText02;
            var toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

            var textElements = toastXml.GetElementsByTagName("text");
            var titleEl = textElements[0];
            var textEl = textElements[1];
            foreach (var c in titleEl.ChildNodes.ToArray())
                titleEl.RemoveChild(c);
            foreach (var c in textEl.ChildNodes.ToArray())
                titleEl.RemoveChild(c);
            titleEl.AppendChild(toastXml.CreateTextNode(subject));
            textEl.AppendChild(toastXml.CreateTextNode(text));

            // TODO: Convert ?
            if (icon != null) {
                var imgEl = ((XmlElement) toastXml.GetElementsByTagName("image")[0]);
                imgEl.SetAttribute("src", icon);
            }

            var notification = new ToastNotification(toastXml) {
                ExpirationTime = expirationTime == null ? null : (DateTime?) DateTime.UtcNow.Add(expirationTime.Value)
            };
            var tcs = GenerateTcs(notification);
            var notifier = ToastNotificationManager.CreateToastNotifier("withSIX"); // TODO: Configure per app?
            notifier.Show(notification);
            return tcs.Task;
        }

        static TaskCompletionSource<bool?> GenerateTcs(ToastNotification notification) {
            var tcs = new TaskCompletionSource<bool?>();
            notification.Dismissed += (sender, args) => tcs.SetResult(false);
            notification.Activated += (sender, args) => tcs.SetResult(true);
            notification.Failed += (sender, args) => tcs.SetException(args.ErrorCode);
            return tcs;
        }
    }
}