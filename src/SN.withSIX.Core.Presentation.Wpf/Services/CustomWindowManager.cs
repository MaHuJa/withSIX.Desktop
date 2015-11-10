// <copyright company="SIX Networks GmbH" file="CustomWindowManager.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Caliburn.Micro;
using MahApps.Metro.Controls;
using ReactiveUI;
using SN.withSIX.Core.Applications.Services;
using SN.withSIX.Core.Presentation.Wpf.Behaviors;
using SN.withSIX.Core.Presentation.Wpf.Views.Controls;
using Action = Caliburn.Micro.Action;
using ViewLocator = Caliburn.Micro.ViewLocator;

namespace SN.withSIX.Core.Presentation.Wpf.Services
{
    public class CustomWindowManager : WindowManager
    {
        protected override Window EnsureWindow(object model, object view, bool isDialog) {
            var window = view as Window;

            if (window == null) {
                var metroWindow = new MetroWindow {
                    Content = view,
                    SizeToContent = SizeToContent.WidthAndHeight,
                    TitlebarHeight = 0
                };
                window = metroWindow;
                //Interaction.GetBehaviors(metroWindow).Add(new GlowWindowBehavior());
                metroWindow.SetValue(MoveableWindowBehavior.IsEnabledProperty, true);
                metroWindow.SetValue(View.IsGeneratedProperty, true);

                var owner = InferOwnerOf(window);
                if (owner != null) {
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    window.Owner = owner;
                } else
                    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            } else {
                var owner = InferOwnerOf(window);
                if (owner != null && isDialog)
                    window.Owner = owner;
            }

            SetupRxWindow(model, view, window);

            return window;
        }

        static void SetupRxWindow(object model, object view, Window window) {
            var vFor = view as IViewFor;
            if (vFor == null)
                return;
            var rxClose = model as IRxClose;
            if (rxClose != null) {
                // Is this how we want to do it?
                // Doesnt this memory leak??
                vFor.WhenActivated(d => {
                    if (rxClose.Close != null) {
                        d(rxClose.Close
                            .Subscribe(x => {
                                window.DialogResult = x;
                                window.Close();
                            }));
                    }
                });
            }
        }

        public override void ShowPopup(object rootModel, object context = null,
            IDictionary<string, object> settings = null) {
            var popup = CreatePopup(rootModel, settings);
            var view = ViewLocator.LocateForModel(rootModel, popup, context);

            SetupRxPopup(rootModel, view, popup);

            popup.Child = view;
            popup.SetValue(View.IsGeneratedProperty, true);

            SetupCaliburn(rootModel, popup, view);

            popup.IsOpen = true;
            popup.CaptureMouse();
        }

        static void SetupRxPopup(object rootModel, UIElement view, Popup popup) {
            var isOpen = rootModel as IIsOpen;
            if (isOpen != null) {
                isOpen.IsOpen = true;
                popup.SetBinding(Popup.IsOpenProperty, new Binding("IsOpen") {Mode = BindingMode.TwoWay});
                popup.SetBinding(Popup.StaysOpenProperty, new Binding("StaysOpen") {Mode = BindingMode.TwoWay});
            }

            var vFor = view as IViewFor;
            if (vFor == null)
                return;
            var rxClose = rootModel as IRxClose;
            if (rxClose != null) {
                // Is this how we want to do it?
                vFor.WhenActivated(
                    d => {
                        d(
                            rxClose.Close.ObserveOn(RxApp.MainThreadScheduler)
                                .Subscribe(x => { popup.IsOpen = !popup.IsOpen; }));
                    });
            }
        }

        static void SetupCaliburn(object rootModel, Popup popup, UIElement view) {
            ViewModelBinder.Bind(rootModel, popup, null);
            Action.SetTargetWithoutContext(view, rootModel);

            var activatable = rootModel as IActivate;
            if (activatable != null)
                activatable.Activate();

            var deactivator = rootModel as IDeactivate;
            if (deactivator != null)
                popup.Closed += delegate { deactivator.Deactivate(true); };
        }

        protected override Popup CreatePopup(object rootModel, IDictionary<string, object> settings) {
            var popup = new NonTopmostPopup();
            // This makes double takes happen (TM)
            //popup.SetValue(PopupMenuCloseBehavior.IsEnabledProperty, true);

            if (ApplySettings(popup, settings)) {
                if (!settings.ContainsKey("PlacementTarget") && !settings.ContainsKey("Placement"))
                    popup.Placement = PlacementMode.MousePoint;
                if (!settings.ContainsKey("AllowsTransparency"))
                    popup.AllowsTransparency = true;
            } else {
                popup.AllowsTransparency = true;
                popup.Placement = PlacementMode.MousePoint;
            }

            return popup;
        }

        bool ApplySettings(object target, IEnumerable<KeyValuePair<string, object>> settings) {
            if (settings == null)
                return false;
            var type = target.GetType();

            foreach (var pair in settings) {
                var propertyInfo = type.GetProperty(pair.Key);

                if (propertyInfo != null)
                    propertyInfo.SetValue(target, pair.Value, null);
            }

            return true;
        }
    }
}