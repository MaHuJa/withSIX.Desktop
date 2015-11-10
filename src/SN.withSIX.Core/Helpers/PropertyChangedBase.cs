// <copyright company="SIX Networks GmbH" file="PropertyChangedBase.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using ReactiveUI;
using SN.withSIX.Core.Logging;
using SN.withSIX.Core.Properties;
using INotifyPropertyChanging = ReactiveUI.INotifyPropertyChanging;
using PropertyChangingEventArgs = ReactiveUI.PropertyChangingEventArgs;
using PropertyChangingEventHandler = ReactiveUI.PropertyChangingEventHandler;

namespace SN.withSIX.Core.Helpers
{
    [DataContract]
    public abstract class PropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            var handler = PropertyChanged;
            if (handler != null) {
                var propertyChangedEventArgs = new PropertyChangedEventArgs(propertyName);
                handler(this, propertyChangedEventArgs);
            }
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // Workaround: https://social.msdn.microsoft.com/Forums/vstudio/en-US/c69b3837-a296-4232-9916-89ed7be3fb59/nullreferenceexception-at-msinternaldataclrbindingworkeronsourcepropertychangedobject-o-string?forum=csharpgeneral
        protected bool SetPropertySafe<T>(ref T storage, T value, [CallerMemberName] String propertyName = null) {
            try {
                return SetProperty(ref storage, value, propertyName);
            } catch (NullReferenceException ex) {
                if (!ex.StackTrace.Contains(
                    "MS.Internal.Data.ClrBindingWorker.OnSourcePropertyChanged(Object o, String propName)"))
                    throw;
                MainLog.Logger.Warn("NullReferenceException trying to SetProperty, probably Framework error!");
            }
            return true;
        }

        public void Refresh() {
            OnPropertyChanged("");
        }
    }

    [DataContract]
    public abstract class ObservablePropertyChangedBase : PropertyChangedBase, INotifyPropertyChanging
    {
        IObservable<PropertyChangedInfo> _changed;
        ISubject<PropertyChangedInfo, PropertyChangedInfo> _changedSubject;
        IObservable<PropertyChangingInfo> _changing;
        ISubject<PropertyChangingInfo, PropertyChangingInfo> _changingSubject;
        public IObservable<PropertyChangedInfo> Changed
        {
            get { return GetChangedObservable(); }
        }
        public IObservable<PropertyChangingInfo> Changing
        {
            get { return GetChangingObservable(); }
        }
        public event PropertyChangingEventHandler PropertyChanging;

        [NotifyPropertyChangedInvocator]
        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            base.OnPropertyChanged(propertyName);
            GetChangedSubject().OnNext(new PropertyChangedInfo(this, propertyName));
        }

        protected virtual void OnPropertyChanging([CallerMemberName] string propertyName = null) {
            var handler = PropertyChanging;
            if (handler != null)
                handler(this, new PropertyChangingEventArgs(propertyName));
            GetChangingSubject().OnNext(new PropertyChangingInfo(this, propertyName));
        }

        protected override bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;
            OnPropertyChanging(propertyName);
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // Workaround: https://social.msdn.microsoft.com/Forums/vstudio/en-US/c69b3837-a296-4232-9916-89ed7be3fb59/nullreferenceexception-at-msinternaldataclrbindingworkeronsourcepropertychangedobject-o-string?forum=csharpgeneral
        protected bool SetPropertySafe<T>(ref T storage, T value, [CallerMemberName] String propertyName = null) {
            try {
                return SetProperty(ref storage, value, propertyName);
            } catch (NullReferenceException ex) {
                if (!ex.StackTrace.Contains(
                    "MS.Internal.Data.ClrBindingWorker.OnSourcePropertyChanged(Object o, String propName)"))
                    throw;
                MainLog.Logger.Warn("NullReferenceException trying to SetProperty, probably Framework error!");
            }
            return true;
        }

        IObservable<PropertyChangedInfo> GetChangedObservable() {
            return (_changed ?? (_changed = GetChangedSubject().AsObservable()));
        }

        ISubject<PropertyChangedInfo, PropertyChangedInfo> GetChangedSubject() {
            // TODO: Lock?            
            return (_changedSubject ?? (_changedSubject = Subject.Synchronize(new Subject<PropertyChangedInfo>())));
        }

        IObservable<PropertyChangingInfo> GetChangingObservable() {
            return (_changing ?? (_changing = GetChangingSubject().AsObservable()));
        }

        ISubject<PropertyChangingInfo, PropertyChangingInfo> GetChangingSubject() {
            // TODO: Lock?
            return (_changingSubject ?? (_changingSubject = Subject.Synchronize(new Subject<PropertyChangingInfo>())));
        }

        protected ObservableAsPropertyHelper<TRet> observableToProperty<TObj, TRet>(TObj obj,
            IObservable<TRet> observable,
            Expression<Func<TObj, TRet>> property,
            TRet initialValue = default(TRet),
            IScheduler scheduler = null) {
            Contract.Requires(observable != null);
            Contract.Requires(property != null);

            var expression = Reflection.Rewrite(property.Body);

            if (expression.GetParent().NodeType != ExpressionType.Parameter)
                throw new ArgumentException("Property expression must be of the form 'x => x.SomeProperty'");

            var ret = new ObservableAsPropertyHelper<TRet>(observable,
                _ => OnPropertyChanged(expression.GetMemberInfo().Name),
                initialValue, scheduler);

            return ret;
        }
    }

    public abstract class PropertyInfoBase
    {
        protected PropertyInfoBase(PropertyChangedBase propertyChangedBase, string propertyName) {
            Sender = propertyChangedBase;
            PropertyName = propertyName;
        }

        public PropertyChangedBase Sender { get; private set; }
        public string PropertyName { get; private set; }
    }

    public class PropertyChangedInfo : PropertyInfoBase
    {
        public PropertyChangedInfo(PropertyChangedBase propertyChangedBase, string propertyName)
            : base(propertyChangedBase, propertyName) {}
    }

    public class PropertyChangingInfo : PropertyInfoBase
    {
        public PropertyChangingInfo(PropertyChangedBase propertyChangedBase, string propertyName)
            : base(propertyChangedBase, propertyName) {}
    }

    [DataContract]
    public abstract class ReactivePropertyChanged : ReactiveObject
    {
        // Hopefully no longer required..
        /*
        protected ReactivePropertyChanged() {
            var changed = Changed; // Trigger lazy init to workaround problem again
        }*/

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;
            this.RaisePropertyChanging(propertyName);
            storage = value;
            this.RaisePropertyChanged(propertyName);
            return true;
        }

        // Workaround: https://social.msdn.microsoft.com/Forums/vstudio/en-US/c69b3837-a296-4232-9916-89ed7be3fb59/nullreferenceexception-at-msinternaldataclrbindingworkeronsourcepropertychangedobject-o-string?forum=csharpgeneral
        protected bool SetPropertySafe<T>(ref T storage, T value, [CallerMemberName] String propertyName = null) {
            try {
                return SetProperty(ref storage, value, propertyName);
            } catch (NullReferenceException ex) {
                if (!ex.StackTrace.Contains(
                    "MS.Internal.Data.ClrBindingWorker.OnSourcePropertyChanged(Object o, String propName)"))
                    throw;
                MainLog.Logger.Warn("NullReferenceException trying to SetProperty, probably Framework error!");
            }
            return true;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            this.RaisePropertyChanged(propertyName);
        }

        protected virtual void OnPropertyChanging([CallerMemberName] string propertyName = null) {
            this.RaisePropertyChanging(propertyName);
        }
    }
}