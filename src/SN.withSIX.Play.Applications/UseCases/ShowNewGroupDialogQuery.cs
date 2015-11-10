// <copyright company="SIX Networks GmbH" file="ShowNewGroupDialogQuery.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using ShortBus;
using SN.withSIX.Play.Applications.ViewModels.Connect.Dialogs;

namespace SN.withSIX.Play.Applications.UseCases
{
    public class ShowNewGroupDialogQuery : IRequest<NewGroupViewModel> {}

    public class ShowNewGroupDialogQueryHandler : IRequestHandler<ShowNewGroupDialogQuery, NewGroupViewModel>
    {
        readonly Func<NewGroupViewModel> _factory;

        public ShowNewGroupDialogQueryHandler(Func<NewGroupViewModel> factory) {
            _factory = factory;
        }

        public NewGroupViewModel Handle(ShowNewGroupDialogQuery request) {
            return _factory();
        }
    }
}