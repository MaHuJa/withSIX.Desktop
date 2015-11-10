// <copyright company="SIX Networks GmbH" file="MenuItem.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using SmartAssembly.Attributes;
using SN.withSIX.Core.Applications.Extensions;
using SN.withSIX.Core.Applications.Services;

namespace SN.withSIX.Core.Applications.MVVM.ViewModels
{
    public interface IMenuItemBase : ISelectable
    {
        IReactiveCommand Command { get; }
        string Name { get; set; }
        bool IsEnabled { get; set; }
        bool IsVisible { get; set; }
        string Icon { get; set; }
        bool IsSeparator { get; set; }
    }

    public interface IMenuItem : IMenuItemBase
    {
        Action Action { get; set; }
        Func<Task> AsyncAction { get; set; }
    }

    public interface IMenuItem<T> : IMenuItemBase
    {
        Action<T> Action { get; set; }
        Func<T, Task> AsyncAction { get; set; }
        void SetNextItem(T item);
        void SetCurrentItem();
    }

    public class MenuItem : MenuBase, IMenuItem
    {
        [DoNotObfuscate] readonly ObservableAsPropertyHelper<IReactiveCommand> _command;
        Action _action;
        Func<Task> _asyncAction;
        string _icon;
        bool _isCheckable;
        bool _isChecked;
        bool _isEnabled = true;
        bool _isSelected;
        bool _isSeparator;
        bool _isVisible = true;
        string _name;

        protected MenuItem(string name, Action action, Func<Task> task, string icon = null) {
            Name = name;
            Icon = icon;
            Action = action;
            AsyncAction = task;
            var ena = this.WhenAnyValue(x => x.IsEnabled).ObserveOn(RxApp.MainThreadScheduler);
            _command =
                this.WhenAnyValue(x => x.Action, v => v.AsyncAction,
                    (act, asyncAct) => CreateCommand(act, asyncAct, ena))
                    .ToProperty(this, v => v.Command, CreateCommand(action, task, ena), Scheduler.Immediate);

            this.WhenAnyValue(x => x.IsSeparator)
                .Where(x => x)
                .Subscribe(x => IsEnabled = false);
        }

        public MenuItem(string name, Action action, string icon = null) : this(name, action, null, icon) {}
        public MenuItem(string name, Func<Task> action, string icon = null) : this(name, null, action, icon) {}
        public MenuItem(string name, string icon = null) : this(name, null, null, icon) {}
        public MenuItem() : this(null, null, null, null) {}
        public bool IsCheckable
        {
            get { return _isCheckable; }
            set { this.RaiseAndSetIfChanged(ref _isCheckable, value); }
        }
        public bool IsChecked
        {
            get { return _isChecked; }
            set { this.RaiseAndSetIfChanged(ref _isChecked, value); }
        }
        public bool IsSubMenu { get; set; }
        public bool IsSeparator
        {
            get { return _isSeparator; }
            set { this.RaiseAndSetIfChanged(ref _isSeparator, value); }
        }
        public Action Action
        {
            get { return _action; }
            set { this.RaiseAndSetIfChanged(ref _action, value); }
        }
        public Func<Task> AsyncAction
        {
            get { return _asyncAction; }
            set { this.RaiseAndSetIfChanged(ref _asyncAction, value); }
        }
        public IReactiveCommand Command
        {
            get { return _command.Value; }
        }
        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { this.RaiseAndSetIfChanged(ref _isEnabled, value); }
        }
        public bool IsVisible
        {
            get { return _isVisible; }
            set { this.RaiseAndSetIfChanged(ref _isVisible, value); }
        }
        public string Icon
        {
            get { return _icon; }
            set { this.RaiseAndSetIfChanged(ref _icon, value); }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set { this.RaiseAndSetIfChanged(ref _isSelected, value); }
        }

        IReactiveCommand CreateCommand(Action act, Func<Task> asyncAct, IObservable<bool> ena) {
            if (act == null && asyncAct == null)
                return null;

            IReactiveCommand c;
            if (asyncAct != null) {
                var cmd = ReactiveCommand.CreateAsyncTask(ena, async x => await asyncAct());
                if (act != null)
                    cmd.Subscribe(act);
                c = cmd;
            } else {
                // using async still because we don't get exceptions thrown on subscribe...
                var cmd = ReactiveCommand.CreateAsyncTask(ena, async x => act());
                c = cmd;
            }
            return c.DefaultSetup(GetName(act, asyncAct));
        }

        static string GetName(Action value, Func<Task> func) {
            if (value != null)
                return value.Target.GetType().Name + "." + value.Method.Name;
            if (func != null)
                return func.Target.GetType().Name + "." + func.Method.Name;
            throw new Exception("No name info found for menu action");
        }
    }

    public class MenuItem<T> : MenuBase<T>, IMenuItem<T> where T : class
    {
        [DoNotObfuscate] readonly ObservableAsPropertyHelper<IReactiveCommand> _command;
        Action<T> _action;
        Func<T, Task> _asyncAction;
        string _icon;
        bool _isCheckable;
        bool _isChecked;
        bool _isEnabled = true;
        bool _isSelected;
        bool _isSeparator;
        bool _isVisible = true;
        string _name;

        MenuItem(string name, Action<T> action, Func<T, Task> task, string icon = null) {
            Name = name;
            Icon = icon;
            Action = action;
            AsyncAction = task;
            var ena = this.WhenAnyValue(x => x.IsEnabled).ObserveOn(RxApp.MainThreadScheduler);
            _command = this.WhenAnyValue(x => x.Action, v => v.AsyncAction,
                (act, asyncAct) => CreateCommand(act, asyncAct, ena))
                .ToProperty(this, v => v.Command, null, Scheduler.Immediate);

            this.WhenAnyValue(x => x.IsSeparator)
                .Where(x => x)
                .Subscribe(x => IsEnabled = false);
        }

        public MenuItem(string name, Action<T> action, string icon = null) : this(name, action, null, icon) {}
        public MenuItem(string name, Func<T, Task> action, string icon = null) : this(name, null, action, icon) {}
        public MenuItem(string name, string icon = null) : this(name, null, null, icon) {}
        protected MenuItem() : this(null, null, null, null) {}
        public bool IsCheckable
        {
            get { return _isCheckable; }
            set { this.RaiseAndSetIfChanged(ref _isCheckable, value); }
        }
        public bool IsChecked
        {
            get { return _isChecked; }
            set { this.RaiseAndSetIfChanged(ref _isChecked, value); }
        }
        public bool IsSubMenu { get; set; }

        public new void SetCurrentItem() {
            base.SetCurrentItem();
        }

        public bool IsSeparator
        {
            get { return _isSeparator; }
            set { this.RaiseAndSetIfChanged(ref _isSeparator, value); }
        }
        public Action<T> Action
        {
            get { return _action; }
            set { this.RaiseAndSetIfChanged(ref _action, value); }
        }
        public Func<T, Task> AsyncAction
        {
            get { return _asyncAction; }
            set { this.RaiseAndSetIfChanged(ref _asyncAction, value); }
        }
        public IReactiveCommand Command
        {
            get { return _command.Value; }
        }
        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { this.RaiseAndSetIfChanged(ref _isEnabled, value); }
        }
        public bool IsVisible
        {
            get { return _isVisible; }
            set { this.RaiseAndSetIfChanged(ref _isVisible, value); }
        }
        public string Icon
        {
            get { return _icon; }
            set { this.RaiseAndSetIfChanged(ref _icon, value); }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set { this.RaiseAndSetIfChanged(ref _isSelected, value); }
        }

        IReactiveCommand CreateCommand(Action<T> act, Func<T, Task> asyncAct, IObservable<bool> ena) {
            IReactiveCommand c;
            if (asyncAct != null) {
                if (act != null)
                    throw new NotSupportedException("already an async action");

                var cmd = ReactiveCommand.CreateAsyncTask(ena, async x => {
                    var currentItem = CurrentItem;
                    var test = this;
                    ConfirmCurrentItem(currentItem);
                    await asyncAct(currentItem);
                });
                cmd.Subscribe();
                c = cmd;
            } else {
                var cmd = ReactiveCommand.CreateAsyncTask(ena, async x => {
                    var currentItem = CurrentItem;
                    ConfirmCurrentItem(currentItem);
                    act(currentItem);
                });
                cmd.Subscribe();
                c = cmd;
            }
            return c.DefaultSetup(GetName(act, asyncAct));
        }

        static void ConfirmCurrentItem(T currentItem) {
            if (currentItem == null)
                throw new ArgumentNullException("CurrentItem");
        }

        static string GetName(Action<T> value, Func<T, Task> func) {
            if (value != null)
                return value.Target.GetType().Name + "." + value.Method.Name;
            if (func != null)
                return func.Target.GetType().Name + "." + func.Method.Name;
            throw new Exception("No name info found for menu action");
        }
    }
}