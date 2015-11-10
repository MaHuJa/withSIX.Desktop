// <copyright company="SIX Networks GmbH" file="ProcessLoginCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using ShortBus;
using SN.withSIX.Core.Applications.Infrastructure;

namespace SN.withSIX.Play.Applications.UseCases
{
    public class ProcessLoginCommand : IRequest<IAuthorizeResponse>
    {
        public ProcessLoginCommand(Uri uri, Uri callbackUri) {
            Uri = uri;
            CallbackUri = callbackUri;
        }

        public Uri Uri { get; }
        public Uri CallbackUri { get; }
    }


    public class ProcessLoginCommandHandler : IRequestHandler<ProcessLoginCommand, IAuthorizeResponse>
    {
        readonly IOauthConnect _connect;

        public ProcessLoginCommandHandler(IOauthConnect connect) {
            _connect = connect;
        }

        public IAuthorizeResponse Handle(ProcessLoginCommand request) {
            return _connect.GetResponse(request.CallbackUri, request.Uri);
        }
    }
}