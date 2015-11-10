// <copyright company="SIX Networks GmbH" file="ModScriptRegistry.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using SN.withSIX.ContentEngine.Core;
using SN.withSIX.Core.Infra.Services;
using SN.withSIX.Core.Logging;

namespace SN.withSIX.ContentEngine.Infra.UseCases
{
    public interface IModScriptRegistry : IInfrastructureService
    {
        bool TryGetMod(IContentEngineContent mod, out RegisteredMod registeredMod);
        bool TryGetMod(Guid modGuid, out RegisteredMod registeredMod);
        bool TryGetMod(string modToken, out RegisteredMod registeredMod);
        RegisteredMod GetMod(string modToken);
        RegisteredMod GetMod(IContentEngineContent mod);
        RegisteredMod GetMod(Guid modGuid);
        bool IsModLoaded(Guid modGuid);
        void RegisterMod(RegisteredMod mod);
        void UnregisterMod(string modToken);
        void UnregisterMod(Guid modGuid);
        void UnregisterMod(IContentEngineContent mod);
        void UnregisterMod(RegisteredMod mod);
    }

    public class ModScriptRegistry : IModScriptRegistry
    {
        readonly List<string> _invalidatedTokens = new List<string>();
        readonly ILogger _logger;
        readonly Dictionary<Guid, RegisteredMod> _mods = new Dictionary<Guid, RegisteredMod>();
        readonly Dictionary<string, Guid> _tokenMap = new Dictionary<string, Guid>();

        public ModScriptRegistry() {
            _logger = MainLog.Logger;
        }

        public bool TryGetMod(IContentEngineContent mod, out RegisteredMod registeredMod) {
            try {
                registeredMod = GetMod(mod);
                return true;
            } catch (Exception e) {
                _logger.FormattedWarnException(e, "Failure getting mod");
            }
            registeredMod = null;
            return false;
        }

        public bool TryGetMod(Guid modGuid, out RegisteredMod registeredMod) {
            try {
                registeredMod = GetMod(modGuid);
                return true;
            } catch (Exception e) {
                _logger.FormattedWarnException(e, "Failure getting mod");
            }
            registeredMod = null;
            return false;
        }

        public bool TryGetMod(string modToken, out RegisteredMod registeredMod) {
            try {
                registeredMod = GetMod(modToken);
                return true;
            } catch (Exception e) {
                _logger.FormattedWarnException(e, "Failure getting mod");
            }
            registeredMod = null;
            return false;
        }

        public RegisteredMod GetMod(string modToken) {
            ModTokenTests(modToken);
            return GetMod(_tokenMap[modToken]);
        }

        public RegisteredMod GetMod(IContentEngineContent mod) {
            return GetMod(mod.Id);
        }

        public RegisteredMod GetMod(Guid modGuid) {
            if (!IsModLoaded(modGuid))
                throw new KeyNotFoundException("CRITICAL: The Mod Token Existed but was not attached to a mod");
            return _mods[modGuid];
        }

        public bool IsModLoaded(Guid modGuid) {
            return _mods.ContainsKey(modGuid);
        }

        public void RegisterMod(RegisteredMod mod) {
            if (_mods.ContainsKey(mod.Guid))
                throw new Exception("This mod guid was already registered!");
            mod.OnModUnloaded += OnModUnloaded;
            _mods.Add(mod.Guid, mod);
            _tokenMap.Add(mod.GetAccessToken(), mod.Guid);
        }

        public void UnregisterMod(string modToken) {
            UnregisterMod(GetMod(modToken));
        }

        public void UnregisterMod(Guid modGuid) {
            UnregisterMod(GetMod(modGuid));
        }

        public void UnregisterMod(IContentEngineContent mod) {
            UnregisterMod(GetMod(mod));
        }

        public void UnregisterMod(RegisteredMod mod) {
            mod.UnloadScript();
        }

        void OnModUnloaded(RegisteredMod registeredMod) {
            _invalidatedTokens.Add(registeredMod.GetAccessToken(true));
        }

        void ModTokenTests(string modToken) {
            if (_invalidatedTokens.Contains(modToken))
                throw new ModTokenInvalidatedException(modToken);
            if (!_tokenMap.ContainsKey(modToken))
                throw new ModTokenNotFoundException(modToken);
        }
    }
}