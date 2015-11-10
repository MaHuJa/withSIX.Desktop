// <copyright company="SIX Networks GmbH" file="InputBindingTrigger.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using SmartAssembly.Attributes;

namespace SN.withSIX.Core.Presentation.Wpf.Extensions
{
    [DoNotObfuscateType]
    public class InputBindingTrigger : TriggerBase<FrameworkElement>, ICommand
    {
        public static readonly DependencyProperty InputBindingProperty = DependencyProperty.Register("InputBinding",
            typeof (InputBinding), typeof (InputBindingTrigger), new UIPropertyMetadata(null));
        public InputBinding InputBinding
        {
            get { return (InputBinding) GetValue(InputBindingProperty); }
            set { SetValue(InputBindingProperty, value); }
        }
        public event EventHandler CanExecuteChanged = delegate { };

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            InvokeActions(parameter);
        }

        protected override void OnAttached() {
            if (InputBinding != null) {
                InputBinding.Command = this;
                AssociatedObject.InputBindings.Add(InputBinding);
            }
            base.OnAttached();
        }
    }
}