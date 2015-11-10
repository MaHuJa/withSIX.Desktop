// <copyright company="SIX Networks GmbH" file="HaveReactiveItems.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Runtime.Serialization;
using ReactiveUI;

namespace SN.withSIX.Core.Helpers
{
    [DataContract]
    public abstract class HaveReactiveItems<T> : PropertyChangedBase, IHaveReactiveItems<T> where T : class
    {
        protected HaveReactiveItems() {
            Items = new ReactiveList<T>();
        }

        public ReactiveList<T> Items { get; protected set; }

        [OnDeserialized]
        protected void OnDeserializedSl(StreamingContext context) {
            if (Items == null)
                Items = new ReactiveList<T>();
        }
    }
}