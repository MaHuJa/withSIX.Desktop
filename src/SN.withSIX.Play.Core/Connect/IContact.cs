// <copyright company="SIX Networks GmbH" file="IContact.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.ComponentModel;
using SmartAssembly.Attributes;

namespace SN.withSIX.Play.Core.Connect
{
    // Workaround for the  Ambigious match exception
    [DoNotObfuscateType]
    public interface IContact : IHaveGuidId, IHaveDisplayName, INotifyPropertyChanged
    {
        Uri GetUri();
        Uri GetOnlineConversationUrl();
    }
}