// <copyright company="SIX Networks GmbH" file="CollectionMod.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Play.Tests.Core.Unit.Playground.Design.Domain.Content
{
    public class CollectionMod<TGameData, TMod> : PropertyChangedBase where TGameData : IModdingGameData
        where TMod : Mod, IProcessableContent<TGameData>
    {
        readonly Collection<TGameData, TMod> _collection;
        readonly bool _isRequired;
        readonly TMod _mod;
        bool _isEnabled;

        public CollectionMod(Collection<TGameData, TMod> collection, TMod mod, bool isEnabled, bool isRequired,
            Dependency desiredVersion) {
            _collection = collection;
            _mod = mod;
            _isEnabled = isEnabled;
            _isRequired = isRequired;
            DesiredVersion = desiredVersion;
        }

        public Dependency DesiredVersion { get; set; }

        public TMod Mod {
            get { return _mod; }
        }
        public Collection<TGameData, TMod> Collection {
            get { return _collection; }
        }

        public bool IsRequired {
            get { return _isRequired; }
        }

        // TODO: This is actually a Presentation property? Collection is the aggregate root, so we would enable/disable there, based on a ViewModel's change request?
        public bool IsEnabled {
            get { return _isEnabled; }
            set {
                if (!SetProperty(ref _isEnabled, value))
                    return;
                if (value)
                    _collection.EnableMod(_mod);
                else
                    _collection.DisableMod(_mod);
            }
        }
    }
}