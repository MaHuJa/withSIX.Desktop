// <copyright company="SIX Networks GmbH" file="Snow.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using SN.withSIX.Core.Applications.MVVM;
using SN.withSIX.Core.Helpers;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    public class Snow : IDisposable
    {
        const int SnowFlakeAmount = 25;
        const int Interval = 50;
        readonly Canvas _canvas;
        readonly TimerWithElapsedCancellation _timer;
        readonly Random random = new Random();
        Vector _currentTransform = new Vector(0, 0);

        public Snow(Canvas canvas, IEventAggregator eventBus) {
            _timer = new TimerWithElapsedCancellation(TimeSpan.FromMilliseconds(Interval), OnTimerTicker, null, false);
            _canvas = canvas;
            eventBus.Subscribe(this);
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing) {
            if (disposing)
                _timer.Dispose();
        }

        public void Loaded() {
            _canvas.ManipulationDelta += OnManipulationDelta;
            CreateInitialSnowflakes();
            _timer.Start();
        }

        public void Unloaded() {
            _canvas.ManipulationDelta -= OnManipulationDelta;
            _timer.Stop();
            lock (_canvas.Children) {
                _canvas.Children.Clear();
            }
        }

        void CreateInitialSnowflakes() {
            for (var i = 0; i < SnowFlakeAmount; i++) {
                var left = random.NextDouble()*_canvas.ActualWidth;
                var top = random.NextDouble()*_canvas.ActualHeight;
                var size = random.Next(10, 50);

                CreateSnowflake(left, top, size);
            }
        }

        void CreateSnowflake(double left, double top, double size) {
            var snowflake = new Snowflake {
                Width = size,
                Height = size
            };

            Canvas.SetLeft(snowflake, left);
            Canvas.SetTop(snowflake, top);

            lock (_canvas.Children)
                _canvas.Children.Add(snowflake);
        }

        bool OnTimerTicker() {
            UiHelper.TryOnUiThread(PerformAction);

            return true;
        }

        void PerformAction() {
            var snowflakes = _canvas.Children.OfType<Snowflake>().ToList();
            foreach (var snowflake in snowflakes) {
                snowflake.UpdatePosition(_currentTransform);

                if (snowflake.IsOutOfBounds(((Grid) _canvas.Parent).ActualWidth,
                    ((Grid) _canvas.Parent).ActualHeight)) {
                    lock (_canvas.Children) {
                        _canvas.Children.Remove(snowflake);
                    }
                    AddNewSnowflake();
                }

                _currentTransform.X = _currentTransform.X*0.999d;
                _currentTransform.Y = _currentTransform.Y*0.999d;
            }
        }

        void AddNewSnowflake() {
            var left = random.NextDouble()*_canvas.ActualWidth;
            var size = random.Next(10, 50);

            CreateSnowflake(left, 0, size);
        }

        public void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e) {
            _currentTransform = e.CumulativeManipulation.Translation;
        }
    }
}