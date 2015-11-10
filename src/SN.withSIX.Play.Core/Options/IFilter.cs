// <copyright company="SIX Networks GmbH" file="IFilter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;

namespace SN.withSIX.Play.Core.Options
{
    public interface IFilter
    {
        IObservable<int> FilterChanged { get; }
        bool Filtered { get; set; }
        void DefaultFilters();
        void PublishFilter();
        void PublishFilterInternal();
        bool AnyFilterEnabled();
        void ResetFilter();
        bool Handler(object o);
    }

    public interface IHaveModdingFilters
    {
        bool Modded { get; set; }
        bool IncompatibleServers { get; set; }
    }
}