// <copyright company="SIX Networks GmbH" file="RefreshCollectionCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using ShortBus;

namespace SN.withSIX.Play.Applications.UseCases.Games
{
    public class RefreshCollectionCommand : IAsyncRequest<UnitType>
    {
        public RefreshCollectionCommand(Guid collectionId) {
            CollectionId = collectionId;
        }

        public Guid CollectionId { get; set; }
    }
}