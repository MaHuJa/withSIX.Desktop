// <copyright company="SIX Networks GmbH" file="LocalContainerBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Runtime.Serialization;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Games.Entities;
using SN.withSIX.Play.Core.Games.Legacy.Mods;

namespace SN.withSIX.Play.Core.Games.Legacy
{
    [DataContract]
    public abstract class LocalContainerBase<T> : SelectionList<T> where T : class
    {
        [DataMember] Guid _gameUuid = Collection.DefaultGameUuid;
        protected LocalContainerBase() {}

        protected LocalContainerBase(string name, string path, Game game) {
            Name = name;
            Path = path;
            _gameUuid = game.Id;
            Game = game;
        }

        public Game Game { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Path { get; set; }
        public Guid GameId
        {
            get { return _gameUuid; }
        }

        public bool GameMatch(Game game) {
            return game != null && game.Id == _gameUuid;
        }

        protected abstract void RefreshLocalLibrary();
        public abstract void FillLocalLibrary(string path);
        public abstract void FillLocalLibrary();
    }
}