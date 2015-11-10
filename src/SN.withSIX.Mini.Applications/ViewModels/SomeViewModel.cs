// <copyright company="SIX Networks GmbH" file="SomeViewModel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using FluentValidation;
using ReactiveUI;

namespace SN.withSIX.Mini.Applications.ViewModels
{
    public interface ISomeViewModel
    {
        string DisplayName { get; }
    }

    public abstract class SomeViewModel : ViewModel
    {
        public abstract string DisplayName { get; }
    }

    public interface IValidatableViewModel : IDataErrorInfo
    {
        bool IsValid { get; }
    }

    public abstract class ValidatableViewModel : SomeViewModel, IValidatableViewModel
    {
        string _error;
        IDictionary<string, string> _errors = new Dictionary<string, string>();
        bool _isValid = true;
        [IgnoreDataMember]
        protected IValidator Validator { get; set; }
        public virtual bool IsValid
        {
            get { return _isValid; }
            protected set { this.RaiseAndSetIfChanged(ref _isValid, value); }
        }
        [IgnoreDataMember]
        string IDataErrorInfo.Error => _error;
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

        protected void UpdateValidation() {
            var result = Validator.Validate(this);
            _errors = result.Errors.ToDictionary(x => x.PropertyName, x => x.ErrorMessage);
            _error = string.Join(Environment.NewLine, result.Errors.Select(x => x.ErrorMessage));
            // we could decide not to use this at all, thus always leave the OK/Save button enabled?
            IsValid = !_errors.Any();
            this.RaisePropertyChanged("Error"); // Needed / used?
        }

        protected void ClearErrors() {
            _error = null;
            IsValid = true;
            _errors.Clear();
        }
    }
}