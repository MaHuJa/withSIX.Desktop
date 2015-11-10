// <copyright company="SIX Networks GmbH" file="AddRepositoryViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Applications.Errors;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Play.Core.Games.Legacy;
using SN.withSIX.Play.Core.Games.Legacy.Mods;
using SN.withSIX.Play.Core.Games.Legacy.Repo;
using SN.withSIX.Sync.Core.Transfer;

namespace SN.withSIX.Play.Applications.ViewModels.Games.Popups
{
    public interface IAddRepositoryViewModel
    {
        ReactiveCommand<Unit> AddRepoCommand { get; }
    }

    [DoNotObfuscate]
    public class AddRepositoryViewModel : PopupBase, IAddRepositoryViewModel
    {
        readonly IContentManager _contentManager;
        readonly IDialogManager _dialogManager;
        Uri _repoURL;

        public AddRepositoryViewModel(IContentManager contentManager, IDialogManager dialogManager) {
            _contentManager = contentManager;
            _dialogManager = dialogManager;

            AddRepoCommand = ReactiveCommand.CreateAsyncTask(this.WhenAnyValue(x => x.RepoURL)
                .Select(x => x != null && x.IsRepoUrl()), x => AddRepo());
            AddRepoCommand.DefaultSetup("Add Repository");
            AddRepoCommand
                .Subscribe(x => TryClose(true));

            AddRepoCommand.IsExecuting.BindTo(this, x => x.StaysOpen);

            // TODO: Should we log here, or log in the UI?
            // http://reactiveui.readthedocs.org/en/stable/basics/errors/#less-obvious-uses-of-the-handler-chain
            AddRepoCommand.ThrownExceptions
                .Select(HandleException)
                .SelectMany(UserError.Throw)
                // This makes it delayed and therefore the command is executed properly with IsExecuting back to true etc.
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => {
                    if (x == RecoveryOptionResult.RetryOperation) {
                        // TODO: Prefer InvokeCommand, however on cancellation / failure, we still want to close the popup, and subscribing multiple times causes multiple times processing
                        AddRepoCommand.Execute(null);
                        // Put on top of the window again as the messagebox dialog seems to put it behind the window :S
                        // TODO: Does move the popup if it's not attached to an element :S
                        IsOpen = false;
                        IsOpen = true;
                    } else {
                        StaysOpen = false;
                        TryClose(false);
                    }
                });
            /*
 // BAD: Makes the errors thrown twice!!
            ex.Where(x => x == RecoveryOptionResult.RetryOperation)
                .Subscribe(x => AddRepoCommand.Execute(null));
            ex.Where(x => x != RecoveryOptionResult.RetryOperation)
                .Subscribe(x => TryClose(false));
*/
        }

        public Uri RepoURL
        {
            get { return _repoURL; }
            set { SetProperty(ref _repoURL, value); }
        }
        [Browsable(false)]
        public ReactiveCommand<Unit> AddRepoCommand { get; }

        public static UserError HandleException(Exception ex) {
            if (ex is DownloadException)
                return new RepositoryDownloadUserError(null, ex);
            return UiTaskHandler.HandleException(ex);
        }

        async Task AddRepo() {
            if (RepoURL == null) {
                await _dialogManager.MessageBoxAsync(new MessageBoxDialogParams("Repository URI must not be blank.",
                    "Repository URI Error"));
                return;
            }
            if (!SixRepo.URLSchemes.Contains(RepoURL.Scheme)) {
                await _dialogManager.MessageBoxAsync(
                    new MessageBoxDialogParams(
                        "Repository URI must start with 'pws://' (or one of the other supported protocols)",
                        "Repository URI Error"));
                return;
            }
            await _contentManager.HandlePwsUrl(RepoURL.ToString()).ConfigureAwait(false);
        }
    }

    public class RepositoryDownloadUserError : UserErrorBase
    {
        public RepositoryDownloadUserError(Dictionary<string, object> contextInfo = null,
            Exception innerException = null)
            : base("Error while trying to access the custom repo. Retry?",
                "This is usually caused by the custom repository being down, internet connection issues, or faulty address.",
                RecoveryCommands.RetryCommands, contextInfo, innerException) {}
    }
}