// <copyright company="SIX Networks GmbH" file="IBaseYaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using NDepend.Path;
using YamlDotNet.RepresentationModel;

namespace SN.withSIX.Sync.Core.Legacy
{
    public interface IBaseYaml
    {
        void FromYaml(YamlMappingNode mapping);
        string ToYaml();
    }
}