// <copyright company="SIX Networks GmbH" file="BindableStaticResource.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace SN.withSIX.Core.Presentation.Wpf.Extensions
{
    public class BindableStaticResource : StaticResourceExtension
    {
        static readonly DependencyProperty DummyProperty;

        static BindableStaticResource() {
            DummyProperty = DependencyProperty.RegisterAttached("Dummy",
                typeof (Object),
                typeof (DependencyObject),
                new UIPropertyMetadata(null));
        }

        public BindableStaticResource() {}

        public BindableStaticResource(Binding binding) {
            Binding = binding;
        }

        public Binding Binding { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            var target = (IProvideValueTarget) serviceProvider.GetService(typeof (IProvideValueTarget));
            var targetObject = (FrameworkElement) target.TargetObject;

            Binding.Source = targetObject.DataContext;
            var DummyDO = new DependencyObject();
            BindingOperations.SetBinding(DummyDO, DummyProperty, Binding);

            ResourceKey = DummyDO.GetValue(DummyProperty);
            if (string.IsNullOrWhiteSpace(ResourceKey as string))
                return null;

            return base.ProvideValue(serviceProvider);
        }
    }
}