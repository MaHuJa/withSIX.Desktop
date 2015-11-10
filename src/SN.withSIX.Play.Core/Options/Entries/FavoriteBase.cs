// <copyright company="SIX Networks GmbH" file="FavoriteBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;

namespace SN.withSIX.Play.Core.Options.Entries
{
    [DataContract]
    public abstract class FavoriteBase<T> : ObjectSaveBase<T> where T : class, IFavorite, IObjectTag
    {
        protected FavoriteBase(T obj) : base(obj) {}
    }
}