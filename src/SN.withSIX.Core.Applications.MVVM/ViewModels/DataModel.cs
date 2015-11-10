// <copyright company="SIX Networks GmbH" file="DataModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

namespace SN.withSIX.Core.Applications.MVVM.ViewModels
{
    public abstract class DataModel : ReactiveValidatableObject {}

    public abstract class DataModel<TId> : DataModel, IHaveId<TId>
    {
        public TId Id { get; set; }
    }
}