// <copyright company="SIX Networks GmbH" file="ReactiveValidatableObject.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using FluentValidation;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Core.Applications.MVVM.ViewModels
{
    [DataContract]
    public abstract class ReactiveValidatableObject : ReactivePropertyChanged, IDataErrorInfo
    {
        string _error;
        Dictionary<string, string> _errors = new Dictionary<string, string>();
        bool _isValid;
        [IgnoreDataMember]
        protected IValidator Validator { get; set; }
        [IgnoreDataMember, Browsable(false)]
        public bool IsValid
        {
            get { return _isValid; }
            protected set { SetProperty(ref _isValid, value); }
        }
        [IgnoreDataMember]
        string IDataErrorInfo.Error
        {
            get { return _error; }
        }
        [IgnoreDataMember]
        public string this[string columnName]
        {
            get
            {
                string value;
                _errors.TryGetValue(columnName, out value);
                return value;
            }
        }

        protected override bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;
            OnPropertyChanging(propertyName);
            storage = value;
            if (Validator != null)
                UpdateValidation();
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void UpdateValidation() {
            var result = Validator.Validate(this);
            _errors = result.Errors.ToDictionary(x => x.PropertyName, x => x.ErrorMessage);
            _error = string.Join(Environment.NewLine, result.Errors.Select(x => x.ErrorMessage));
            // we could decide not to use this at all, thus always leave the OK/Save button enabled?
            IsValid = !_errors.Any();
            OnPropertyChanged("Error"); // Needed / used?
        }

        protected void ClearErrors() {
            _error = null;
            IsValid = true;
            _errors.Clear();
        }
    }
}