// <copyright company="SIX Networks GmbH" file="ImportCollectionCommand.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using ShortBus;

namespace SN.withSIX.Play.Applications.UseCases.Games
{
    public class ImportCollectionCommand : IAsyncRequest<UnitType>
    {
        public ImportCollectionCommand(Guid collectionId) {
            CollectionId = collectionId;
        }

        public Guid CollectionId { get; private set; }
    }
}