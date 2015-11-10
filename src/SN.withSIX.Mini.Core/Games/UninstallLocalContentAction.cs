// <copyright company="SIX Networks GmbH" file="UninstallLocalContentAction.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Threading;

namespace SN.withSIX.Mini.Core.Games
{
    public class UninstallLocalContentAction : UninstallContentAction<LocalContent>
    {
        public UninstallLocalContentAction(CancellationToken cancelToken = default(CancellationToken),
            params IContentSpec<LocalContent>[] content)
            : this(content, cancelToken) {}

        public UninstallLocalContentAction(IReadOnlyCollection<IContentSpec<LocalContent>> content,
            CancellationToken cancelToken = default(CancellationToken))
            : base(content, cancelToken) {}
    }

    public interface IUninstallContentAction<out T> : IContentAction<T> where T : IContent {}

    public abstract class UninstallContentAction<T> : ContentAction<T>, IUninstallContentAction<T> where T : IContent
    {
        protected UninstallContentAction(IReadOnlyCollection<IContentSpec<T>> content,
            CancellationToken cancelToken = new CancellationToken()) : base(content, cancelToken) {}
    }
}