// <copyright company="SIX Networks GmbH" file="ModMenuBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics.Contracts;
using SN.withSIX.Core.Applications.MVVM.ViewModels;
using SN.withSIX.Play.Applications.ViewModels.Games.Library;
using SN.withSIX.Play.Core.Games.Legacy;

namespace SN.withSIX.Play.Applications.ViewModels.Games
{
    public abstract class ModMenuBase<TContent> : ContextMenuBase<TContent> where TContent : class, IContent
    {
        public readonly ModLibraryViewModel Library;

        protected ModMenuBase(ModLibraryViewModel library) {
            Contract.Requires<ArgumentNullException>(library != null);
            Library = library;
        }
    }
}