// <copyright company="SIX Networks GmbH" file="IMetroDialog.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Applications.Services
{
    public interface IDialog {}

    public interface IIsOpen
    {
        bool IsOpen { get; set; }
        bool StaysOpen { get; set; }
    }

    public interface IMetroDialog : IDialog, IRxClose
    {
        string DisplayName { get; set; }
    }
}