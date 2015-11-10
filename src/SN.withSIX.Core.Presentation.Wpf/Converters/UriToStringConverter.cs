// <copyright company="SIX Networks GmbH" file="UriToStringConverter.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace SN.withSIX.Core.Presentation.Wpf.Converters
{
    public class UriToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var input = value as Uri;
            return input == null ? String.Empty : input.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var input = value as string;
            return String.IsNullOrEmpty(input) ? null : new Uri(input, UriKind.Absolute);
        }
    }

    public class UriValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            var input = value as string;
            if (String.IsNullOrEmpty(input)) // Valid input, converts to null.
                return new ValidationResult(true, null);
            Uri outUri;
            return Uri.TryCreate(input, UriKind.Absolute, out outUri)
                ? new ValidationResult(true, null)
                : new ValidationResult(false, "String is not a valid URI");
        }
    }
}