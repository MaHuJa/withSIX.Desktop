// <copyright company="SIX Networks GmbH" file="ValidatorBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using FluentValidation;
using FluentValidation.Results;

namespace SN.withSIX.Core.Applications.MVVM.ViewModels
{
    public class ValidatorBase<T> : AbstractValidator<T>
    {
        protected const string ValidPathMessage = "Please specify a valid path";

        protected static bool BeValidPath(string path) {
            return Tools.FileUtil.IsValidRootedPath(path);
        }
    }


    public abstract class ChainedValidator<T> : ValidatorBase<T>
    {
        readonly IValidator _otherValidator;

        protected ChainedValidator(IValidator otherValidator) {
            _otherValidator = otherValidator;
        }

        public override ValidationResult Validate(T instance) {
            var otherResult = _otherValidator.Validate(instance);

            var myResult = base.Validate(instance);
            foreach (var v in myResult.Errors)
                otherResult.Errors.Add(v);

            return otherResult;
        }
    }
}