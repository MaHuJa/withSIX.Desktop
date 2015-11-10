// <copyright company="SIX Networks GmbH" file="PlayContentAction.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Generic;
using System.Threading;

namespace SN.withSIX.Mini.Core.Games
{
    public abstract class PlayContentAction<T> : LaunchContentAction<T>, IPlayContentAction<T> where T : IContent
    {
        protected PlayContentAction(IReadOnlyCollection<IContentSpec<T>> content,
            LaunchType launchType = LaunchType.Default,
            CancellationToken cancelToken = new CancellationToken()) : base(content, launchType, cancelToken) {}
    }

    public class PlayContentAction : PlayContentAction<Content>
    {
        public PlayContentAction(LaunchType launchType = LaunchType.Default,
            CancellationToken cancelToken = default(CancellationToken), params IContentSpec<Content>[] content)
            : this(content, launchType, cancelToken) {}

        public PlayContentAction(IReadOnlyCollection<IContentSpec<Content>> content,
            LaunchType launchType = LaunchType.Default,
            CancellationToken cancelToken = default(CancellationToken))
            : base(content, launchType, cancelToken) {}
    }

    public class PlayLocalContentAction : PlayContentAction<LocalContent>
    {
        public PlayLocalContentAction(LaunchType launchType = LaunchType.Default,
            CancellationToken cancelToken = default(CancellationToken),
            params IContentSpec<LocalContent>[] content)
            : this(content, launchType, cancelToken) {}

        public PlayLocalContentAction(IReadOnlyCollection<IContentSpec<LocalContent>> content,
            LaunchType launchType = LaunchType.Default,
            CancellationToken cancelToken = default(CancellationToken))
            : base(content, launchType, cancelToken) {}
    }

    public interface IPlayContentAction<out T> : ILaunchContentAction<T> where T : IContent {}
}