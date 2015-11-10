// <copyright company="SIX Networks GmbH" file="MenuBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ReactiveUI;
using SN.withSIX.Core.Applications.MVVM.Attributes;
using SN.withSIX.Core.Extensions;

namespace SN.withSIX.Core.Applications.MVVM.ViewModels
{
    public interface IMenuBase
    {
        bool IsOpen { get; set; }
    }

    public abstract class MenuBase : SelectionCollectionHelper<IMenuItem>, IMenuBase
    {
        readonly ConcurrentDictionary<Func<Task>, IMenuItem> _asyncItemCache =
            new ConcurrentDictionary<Func<Task>, IMenuItem>();
        readonly ConcurrentDictionary<Action, IMenuItem> _itemCache = new ConcurrentDictionary<Action, IMenuItem>();
        bool _isOpen;

        protected MenuBase() {
            foreach (var methodAttrInfo in GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => Attribute.IsDefined(x, typeof (MenuItemAttribute)))
                .Select(
                    x =>
                        Tuple.Create(x,
                            Attribute.GetCustomAttribute(x,
                                typeof (MenuItemAttribute)) as MenuItemAttribute)))
                CreateItem(methodAttrInfo);

            /*BindingOperations.EnableCollectionSynchronization(Items, _itemsLock); // TODO: UI Framework agnostic*/
        }

        protected virtual bool UppercaseFirst
        {
            get { return true; }
        }
        public bool IsOpen
        {
            get { return _isOpen; }
            set { this.RaiseAndSetIfChanged(ref _isOpen, value); }
        }

        public static string GetMenuNameFromMethodName(string methodName, bool upperCaseFirst = true) {
            var str = methodName.SplitCamelCase().ToLower();
            return upperCaseFirst ? str.UppercaseFirst() : str;
        }

        void CreateItem(Tuple<MethodInfo, MenuItemAttribute> methodAttrInfo) {
            var name = methodAttrInfo.Item2.DisplayName ??
                       GetMenuNameFromMethodName(methodAttrInfo.Item1.Name, UppercaseFirst);
            var rp = methodAttrInfo.Item1.ReturnParameter;
            if (rp != null && rp.ParameterType == typeof (Task))
                AddTask(methodAttrInfo, name);
            else
                AddAction(methodAttrInfo, name);

            if (methodAttrInfo.Item2.IsSeparator)
                Items.Last().IsSeparator = true;
        }

        void AddTask(Tuple<MethodInfo, MenuItemAttribute> methodAttrInfo, string name) {
            var action = (Func<Task>) Delegate.CreateDelegate(typeof (Func<Task>), this, methodAttrInfo.Item1);
            var childType = methodAttrInfo.Item2.Type;
            if (childType != null)
                AddTask(name, action, childType, methodAttrInfo.Item2.Icon);
            else
                AddTask(name, action, methodAttrInfo.Item2.Icon);
        }

        void AddAction(Tuple<MethodInfo, MenuItemAttribute> methodAttrInfo, string name) {
            var action = (Action) Delegate.CreateDelegate(typeof (Action), this, methodAttrInfo.Item1);
            var childType = methodAttrInfo.Item2.Type;
            if (childType != null)
                AddAction(name, action, childType, methodAttrInfo.Item2.Icon);
            else
                AddAction(name, action, methodAttrInfo.Item2.Icon);
        }

        void AddAction(string name, Action action, string icon = null) {
            var item = new MenuItem(name, action, icon);
            Items.Add(item);
        }

        void AddAction(string name, Action action, Type t, string icon = null) {
            var item = (MenuItem) Activator.CreateInstance(t, this);
            item.Name = name;
            item.Action = action;
            item.Icon = icon;
            item.IsSubMenu = true;
            Items.Add(item);
        }

        void AddTask(string name, Func<Task> action, string icon = null) {
            var item = new MenuItem(name, action, icon);
            Items.Add(item);
        }

        void AddTask(string name, Func<Task> action, Type t, string icon = null) {
            var item = (MenuItem) Activator.CreateInstance(t, this);
            item.Name = name;
            item.AsyncAction = action;
            item.Icon = icon;
            Items.Add(item);
        }

        protected IMenuItem GetItem(Action action) {
            return _itemCache.GetOrAdd(action, Items.First(x => x.Action == action));
        }

        protected IMenuItem GetAsyncItem(Func<Task> action) {
            return _asyncItemCache.GetOrAdd(action, Items.First(x => x.AsyncAction == action));
        }
    }

    public abstract class MenuBase<T> : SelectionCollectionHelper<IMenuItem<T>>, IMenuBase where T : class
    {
        readonly ConcurrentDictionary<Func<T, Task>, IMenuItem<T>> _asyncItemCache =
            new ConcurrentDictionary<Func<T, Task>, IMenuItem<T>>();
        readonly ConcurrentDictionary<Action<T>, IMenuItem<T>> _itemCache =
            new ConcurrentDictionary<Action<T>, IMenuItem<T>>();
        T _currentItem;
        bool _isOpen;
        protected T _nextItem;

        protected MenuBase() {
            foreach (var methodAttrInfo in GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => Attribute.IsDefined(x, typeof (MenuItemAttribute)))
                .Select(
                    x =>
                        Tuple.Create(x,
                            Attribute.GetCustomAttribute(x,
                                typeof (MenuItemAttribute)) as MenuItemAttribute)))
                CreateItem(methodAttrInfo);

            // TODO: UI Agnostic
            /*BindingOperations.EnableCollectionSynchronization(Items, _itemsLock);*/

            this.WhenAnyValue(x => x.IsOpen)
                .Where(x => x)
                .Subscribe(x => Open());
        }

        public T CurrentItem
        {
            get { return _currentItem; }
            set { this.RaiseAndSetIfChanged(ref _currentItem, value); }
        }
        protected virtual bool UppercaseFirst
        {
            get { return true; }
        }
        public bool IsOpen
        {
            get { return _isOpen; }
            set { this.RaiseAndSetIfChanged(ref _isOpen, value); }
        }

        protected IMenuItem<T> GetItem(Action<T> action) {
            return _itemCache.GetOrAdd(action, Items.First(x => x.Action == action));
        }

        protected IMenuItem<T> GetAsyncItem(Func<T, Task> action) {
            return _asyncItemCache.GetOrAdd(action, Items.First(x => x.AsyncAction == action));
        }

        void CreateItem(Tuple<MethodInfo, MenuItemAttribute> methodAttrInfo) {
            var name = methodAttrInfo.Item2.DisplayName ??
                       MenuBase.GetMenuNameFromMethodName(methodAttrInfo.Item1.Name, UppercaseFirst);
            var rp = methodAttrInfo.Item1.ReturnParameter;
            if (rp != null && rp.ParameterType == typeof (Task))
                AddTask(methodAttrInfo, name);
            else
                AddAction(methodAttrInfo, name);

            if (methodAttrInfo.Item2.IsSeparator)
                Items.Last().IsSeparator = true;
        }

        void AddAction(Tuple<MethodInfo, MenuItemAttribute> methodAttrInfo, string name) {
            var action = (Action<T>) Delegate.CreateDelegate(typeof (Action<T>), this, methodAttrInfo.Item1);
            var childType = methodAttrInfo.Item2.Type;
            if (childType != null)
                AddAction(name, action, childType, methodAttrInfo.Item2.Icon);
            else
                AddAction(name, action, methodAttrInfo.Item2.Icon);
        }

        void AddTask(Tuple<MethodInfo, MenuItemAttribute> methodAttrInfo, string name) {
            var action = (Func<T, Task>) Delegate.CreateDelegate(typeof (Func<T, Task>), this, methodAttrInfo.Item1);
            var childType = methodAttrInfo.Item2.Type;
            if (childType != null)
                AddTask(name, action, childType, methodAttrInfo.Item2.Icon);
            else
                AddTask(name, action, methodAttrInfo.Item2.Icon);
        }

        void AddAction(string name, Action<T> action, string icon = null) {
            var item = new MenuItem<T>(name, action, icon);
            Items.Add(item);
        }

        void AddAction(string name, Action<T> action, Type t, string icon = null) {
            var item = (IMenuItem<T>) Activator.CreateInstance(t, this);
            item.Name = name;
            item.Action = action;
            item.Icon = icon;
            Items.Add(item);
        }

        void AddTask(string name, Func<T, Task> action, string icon = null) {
            var item = new MenuItem<T>(name, action, icon);
            Items.Add(item);
        }

        void AddTask(string name, Func<T, Task> action, Type t, string icon = null) {
            var item = (MenuItem<T>) Activator.CreateInstance(t, this);
            item.IsSubMenu = true;
            item.Name = name;
            item.AsyncAction = action;
            item.Icon = icon;
            Items.Add(item);
        }

        protected void Open() {
            SetCurrentItem();
            UpdateItemsFor(CurrentItem);
        }

        protected void SetCurrentItem() {
            SetCurrentItemInternal(_nextItem);
            foreach (var i in Items)
                i.SetCurrentItem();
        }

        void SetCurrentItemInternal(T item) {
            if (item == null)
                throw new ArgumentNullException("item");
            Contract.Requires<ArgumentNullException>(item != null);
            CurrentItem = item;
        }

        public void SetNextItem(T item) {
            if (item == null)
                throw new ArgumentNullException("item");
            Contract.Requires<ArgumentNullException>(item != null);
            _nextItem = item;

            // TODO: Should we consider just linking the parent menu's CurrentItem directly?
            foreach (var i in Items)
                i.SetNextItem(item);
        }

        protected virtual void UpdateItemsFor(T item) {}
    }

    public abstract class ContextMenuBase : MenuBase, IContextMenu {}

    public abstract class PopupMenuBase : ContextMenuBase
    {
        readonly List<IMenuBase> _subMenus = new List<IMenuBase>();
        string _displayName;

        protected PopupMenuBase() {
            this.WhenAnyValue(x => x.IsOpen)
                .Where(x => x)
                .Subscribe(x => _subMenus.ForEach(y => y.IsOpen = false));
        }

        public string DisplayName
        {
            get { return _displayName; }
            set { this.RaiseAndSetIfChanged(ref _displayName, value); }
        }

        protected void RegisterSubMenu(IMenuBase menu) {
            _subMenus.Add(menu);
        }
    }

    public abstract class PopupMenuBase<T> : ContextMenuBase<T> where T : class
    {
        readonly List<IMenuBase> _subMenus = new List<IMenuBase>();
        string _header;

        protected PopupMenuBase() {
            this.WhenAnyValue(x => x.IsOpen)
                .Where(x => x)
                .Subscribe(x => _subMenus.ForEach(y => y.IsOpen = false));
        }

        public string Header
        {
            get { return _header; }
            set { this.RaiseAndSetIfChanged(ref _header, value); }
        }

        protected void RegisterSubMenu(IMenuBase menu) {
            _subMenus.Add(menu);
        }
    }

    public abstract class ContextMenuBase<T> : MenuBase<T>, IContextMenu<T> where T : class
    {
        public void ShowForItem(T item) {
            SetNextItem(item);
            Open();
        }
    }

    public interface IContextMenu {}

    public interface IContextMenu<in T> : IContextMenu
    {
        void ShowForItem(T item);
    }
}