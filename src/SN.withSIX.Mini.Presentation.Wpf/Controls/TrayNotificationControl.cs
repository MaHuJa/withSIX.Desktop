// <copyright company="SIX Networks GmbH" file="TrayNotificationControl.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;

namespace SN.withSIX.Mini.Presentation.Wpf.Controls
{
    //[TemplatePart(Name = PART_Menu, Type = typeof (MenuItem))]
    [TemplatePart(Name = PART_Header, Type = typeof (WindowHeader))]
    public class TrayNotificationControl : ContentControl
    {
        //const string PART_Menu = "PART_Menu";
        const string PART_Header = "PART_Header";
        public static readonly DependencyProperty MenuAreaProperty = DependencyProperty.Register("MenuArea",
            typeof (UIElement), typeof (TrayNotificationControl), new PropertyMetadata(default(UIElement)));
        public static readonly DependencyProperty FooterAreaProperty = DependencyProperty.Register("FooterArea",
            typeof (UIElement), typeof (TrayNotificationControl), new PropertyMetadata(default(UIElement)));

        static TrayNotificationControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (TrayNotificationControl),
                new FrameworkPropertyMetadata(typeof (TrayNotificationControl)));
        }

        //public MenuItem Menu { get; private set; }
        public WindowHeader Header { get; private set; }
        public UIElement MenuArea
        {
            get { return (UIElement) GetValue(MenuAreaProperty); }
            set { SetValue(MenuAreaProperty, value); }
        }
        public UIElement FooterArea
        {
            get { return (UIElement) GetValue(FooterAreaProperty); }
            set { SetValue(FooterAreaProperty, value); }
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            //Menu = GetTemplateChild(PART_Menu) as MenuItem;
            Header = GetTemplateChild(PART_Header) as WindowHeader;
        }
    }
}