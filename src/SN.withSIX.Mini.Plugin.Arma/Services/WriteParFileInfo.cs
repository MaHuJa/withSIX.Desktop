// <copyright company="SIX Networks GmbH" file="WriteParFileInfo.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;

namespace SN.withSIX.Mini.Plugin.Arma.Services
{
    public class WriteParFileInfo
    {
        public WriteParFileInfo(Guid gameId, string content) {
            Contract.Requires<ArgumentNullException>(gameId != Guid.Empty);
            Contract.Requires<ArgumentNullException>(content != null);
            GameId = gameId;
            Content = content;
        }

        public WriteParFileInfo(Guid gameId, string content, string additionalIdentifier)
            : this(gameId, content) {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(additionalIdentifier));
            AdditionalIdentifier = additionalIdentifier;
        }

        public string Content { get; private set; }
        public string AdditionalIdentifier { get; private set; }
        public Guid GameId { get; private set; }
    }
}