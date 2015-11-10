// <copyright company="SIX Networks GmbH" file="GameSettingsProfile.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using ReactiveUI;
using SN.withSIX.Core.Helpers;
using SN.withSIX.Play.Core.Options.Filters;

namespace SN.withSIX.Play.Core.Options.Entries
{
    public interface IGetData
    {
        T GetData<T>(Guid gameId, string propertyName);
    }

    // Usage: A gameset should create it's desired GameSettings implementation, given a SettingsController. It should then Register() it on the Controller.
    // The instance can then be used in the UI and elsewhere to obtain settings.

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    [KnownType(typeof (ArmaServerFilter))]
    public abstract class GameSettingsProfileBase : PropertyChangedBase, IGetData
    {
        [DataMember] readonly Dictionary<Guid, ConcurrentDictionary<string, object>> _gameSettings =
            new Dictionary<Guid, ConcurrentDictionary<string, object>>();
        [DataMember] string _color;
        [DataMember] string _name;
        GameSettingsProfileBase _parent;

        protected GameSettingsProfileBase(Guid id) {
            Id = id;
        }

        protected GameSettingsProfileBase(Guid id, string name, string color) : this(id) {
            _name = name;
            _color = color;
        }

        [DataMember]
        public virtual Guid? ParentId { get; protected set; }
        [DataMember(Name = "Uuid")]
        public virtual Guid Id { get; private set; }
        public virtual bool CanDelete
        {
            get { return true; }
        }
        public virtual string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        public virtual string Color
        {
            get { return _color; }
            set { SetProperty(ref _color, value); }
        }
        public virtual GameSettingsProfileBase Parent
        {
            get { return _parent; }
            set
            {
                Contract.Requires<ArgumentNullException>(value != null);
                _parent = value;
            }
        }

        public T GetData<T>(Guid gameId, string propertyName) {
            var settings = _gameSettings[gameId];
            object propertyValue;
            settings.TryGetValue(propertyName, out propertyValue);
            return propertyValue == null ? default(T) : (T) propertyValue;
        }

        public bool SetData<T>(Guid gameId, string propertyName, T value) {
            var equalityComparer = EqualityComparer<T>.Default;
            if (equalityComparer.Equals(value, GetData<T>(gameId, propertyName)))
                return false;
            if (equalityComparer.Equals(value, default(T))) {
                object currentVal;
                _gameSettings[gameId].TryRemove(propertyName, out currentVal);
            } else
                _gameSettings[gameId][propertyName] = value;

            return true;
        }

        public void Setup(Guid gameId) {
            if (!_gameSettings.ContainsKey(gameId))
                _gameSettings.Add(gameId, new ConcurrentDictionary<string, object>());

            // TODO: We actually want to copy this from a Parent profile probably?
            var recent = GetData<RecentGameSettings>(gameId, "Recent");
            if (recent == null)
                SetData(gameId, "Recent", new RecentGameSettings());
            else
                FixRecent(recent);
        }

        [Obsolete("Workaround for serialization issues!")]
        static void FixRecent(RecentGameSettings recent) {
            if (recent.Collection != null && recent.Collection.Id == Guid.Empty)
                recent.Collection = null;
            if (recent.Mission != null && recent.Mission.Key == null)
                recent.Mission = null;
            if (recent.Server != null && recent.Server.Address == null)
                recent.Server = null;
        }
    }

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    public class GlobalGameSettingsProfile : GameSettingsProfileBase
    {
        public static readonly Guid GlobalId = new Guid("8b15f343-0ec6-4693-8b30-6508d6c44837");
        public GlobalGameSettingsProfile() : base(GlobalId) {}
        public override Guid Id
        {
            get { return GlobalId; }
        }
        public override string Name
        {
            get { return "Global"; }
            set { }
        }
        public override string Color
        {
            get { return "#146bff"; }
            set { }
        }
        public override GameSettingsProfileBase Parent
        {
            get { return null; }
            set { }
        }
        public override bool CanDelete
        {
            get { return false; }
        }
    }

    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/SN.withSIX.Play.Core")]
    public class GameSettingsProfile : GameSettingsProfileBase
    {
        GameSettingsProfileBase _parent;

        protected GameSettingsProfile(Guid id, string name, string color, GameSettingsProfileBase parent)
            : base(id, name, color) {
            Contract.Requires<ArgumentNullException>(parent != null);
            _parent = parent;

            SetupRefresh();
        }

        public GameSettingsProfile(string name, string color, GameSettingsProfileBase parent)
            : this(Guid.NewGuid(), name, color, parent) {}

        [DataMember(Name = "ParentUuid")]
        public override Guid? ParentId { get; protected set; }
        public override GameSettingsProfileBase Parent
        {
            get { return _parent; }
            set { SetProperty(ref _parent, value); }
        }

        void SetupRefresh() {
            this.WhenAnyValue(x => x.Parent)
                .Skip(1)
                .Subscribe(x => Refresh());
        }

        [OnSerializing]
        void OnSerializing(StreamingContext context) {
            ParentId = Parent == null ? (Guid?) null : Parent.Id;
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context) {
            SetupRefresh();
        }
    }
}