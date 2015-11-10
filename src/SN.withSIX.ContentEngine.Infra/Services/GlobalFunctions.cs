// <copyright company="SIX Networks GmbH" file="GlobalFunctions.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using SmartAssembly.Attributes;
using SN.withSIX.ContentEngine.Core;

namespace SN.withSIX.ContentEngine.Infra.Services
{
    [DoNotObfuscateType]
    public interface IGlobalFunctions_TS
    {
        [ActualType(null, typeof (IModS))]
        void registerMod(string guid, dynamic mod);

        //[ActualType(typeof (object))]
        //IModS GetMod(dynamic mod);

        //[ActualType(typeof (object))]
        //void IncludeScript(string script);

        [ActualType(typeof (object), typeof (object))]
        object getService(string name, string token);
    }

    [DoNotObfuscateType, DoNotSealType]
    class GlobalFunctions
    {
        readonly ContentEngine _engine;

        internal GlobalFunctions(ContentEngine engine) {
            _engine = engine;
        }

        public void registerMod(string guid, IModS mod) {
            Console.WriteLine("Registering Mod");
            _engine.RegisterMod(Guid.Parse(guid), mod);
            Console.WriteLine("Registered Mod");
        }

        //public IModS GetMod(string mod) {
        //    throw new NotImplementedException();
        //    //Rather than getting the IModS that has all the calling functionality that the internal object is supposed to be able to access
        //    //We could instead have a registration of mod information DTOs that are there so that mods can request information about other mods.
        //}

        //public void IncludeScript(string script) {
        //    throw new NotImplementedException();
        //    //This would allow a mod to include other scripts that are adjacant to it.
        //    //The intention being that A mod folder might have multiple scripts in it. One main one for the registration of the mod
        //    //and others for other objects relating to that mod.
        //}

        public object getService(string name, string token) {
            return _engine.GetService(name, token);
        }
    }
}