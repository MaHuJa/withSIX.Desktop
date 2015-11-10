// <copyright company="SIX Networks GmbH" file="IObjectCacheManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive;

namespace SN.withSIX.Core.Applications.Infrastructure
{
    public interface IObjectCacheManager
    {
        IObservable<T> GetObject<T>(string key);
        IObservable<Unit> SetObject<T>(string key, T value);
        IObservable<Unit> SetObject<T>(string key, T value, DateTimeOffset? absoluteExpiration);
        IObservable<T> GetOrCreateObject<T>(string key, Func<T> createFunc);
        IObservable<T> GetOrCreateObject<T>(string key, Func<T> createFunc, DateTimeOffset? absoluteExpiration);
    }
}