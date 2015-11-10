// <copyright company="SIX Networks GmbH" file="UnsubscribeFromCollectionCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using ShortBus;

namespace SN.withSIX.Play.Applications.UseCases.Games
{
    public class UnsubscribeFromCollectionCommand : IAsyncRequest<UnitType>
    {
        public UnsubscribeFromCollectionCommand(Guid id) {
            Id = id;
        }

        public Guid Id { get; private set; }
    }
}