// <copyright company="SIX Networks GmbH" file="DialogMessageCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Threading.Tasks;
using ShortBus;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Play.Applications.UseCases
{
    public class DialogMessageCommand : IAsyncRequest<UnitType>
    {
        public string Message { get; set; }
        public string Title { get; set; }
    }

    public class DialogMessageCommandHandler : IAsyncRequestHandler<DialogMessageCommand, UnitType>
    {
        readonly IDialogManager _dialogManager;

        public DialogMessageCommandHandler(IDialogManager dialogManager) {
            _dialogManager = dialogManager;
        }

        public async Task<UnitType> HandleAsync(DialogMessageCommand request) {
            await _dialogManager.MessageBoxAsync(new MessageBoxDialogParams(request.Message, request.Title));
            return UnitType.Default;
        }
    }
}