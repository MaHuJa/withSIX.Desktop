// <copyright company="SIX Networks GmbH" file="FilterBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Serialization;
using SmartAssembly.Attributes;
using SN.withSIX.Core;
using SN.withSIX.Core.Extensions;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Play.Core.Options.Filters
{
    [DataContract]
    public abstract class FilterBase<T> : PropertyChangedBase, IFilter where T : class
    {
        static readonly string[] _defaultSplit = {" && ", "&&"};
        ISubject<int, int> _FilterChanged;
        [DataMember] protected bool _Filtered;
        protected bool _save;
        protected bool _supressPublish;

        protected FilterBase() {
            SetupFilterChanged();
            DefaultFilters();
            Filtered = AnyFilterEnabled();
        }

        public bool Filtered
        {
            get { return _Filtered; }
            set { SetProperty(ref _Filtered, value); }
        }
        public IObservable<int> FilterChanged { get; private set; }

        public virtual void DefaultFilters() {
            Filtered = AnyFilterEnabled();
        }

        public virtual void PublishFilter() {
            if (_supressPublish)
                return;

            ExecutePublish();
        }

        public virtual void PublishFilterInternal() {
            //PublishFilter(false);
            _save = false;
            ExecutePublish();
        }

        public abstract bool AnyFilterEnabled();

        public bool Handler(object item) {
            return Handler((T) item);
        }

        [DoNotObfuscate]
        public void ResetFilter() {
            ClearFilters();
            PublishFilter();
        }

        public abstract bool Handler(T item);

        void SetupFilterChanged() {
            _FilterChanged = Subject.Synchronize(new Subject<int>());
            FilterChanged = _FilterChanged.AsObservable().Throttle(Common.AppCommon.DefaultFilterDelay);
        }

        protected virtual void ExecutePublish() {
            Filtered = AnyFilterEnabled();
            _FilterChanged.OnNext(0);
        }

        protected virtual void ClearFilters() {
            DefaultFilters();
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext sc) {
            SetupFilterChanged();
            Filtered = AnyFilterEnabled();
        }

        protected bool AdvancedStringSearch(string searchField, string queryField, string[] split = null,
            Func<string, bool> normalTestFunc = null, Func<string, bool> reverseTestFunc = null) {
            if (split == null)
                split = _defaultSplit;

            var searchStrings = searchField.Split(split, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in searchStrings) {
                var search = item.Trim();
                var reverse = search.Substring(0, 1) == "!";
                if (!reverse) {
                    if (normalTestFunc != null) {
                        if (!normalTestFunc(search))
                            return false;
                    } else if (!queryField.NullSafeContainsIgnoreCase(search))
                        return false;
                } else {
                    if (reverseTestFunc != null) {
                        if (reverseTestFunc(search.Substring(1)))
                            return false;
                    } else if (queryField.NullSafeContainsIgnoreCase(search.Substring(1)))
                        return false;
                }
            }
            return true;
        }
    }
}