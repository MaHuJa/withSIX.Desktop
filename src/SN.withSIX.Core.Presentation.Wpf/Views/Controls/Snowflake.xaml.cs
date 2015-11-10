// <copyright company="SIX Networks GmbH" file="Snowflake.xaml.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Windows;
using System.Windows.Controls;

namespace SN.withSIX.Core.Presentation.Wpf.Views.Controls
{
    public partial class Snowflake
    {
        public Snowflake() {
            InitializeComponent();
        }

        public void UpdatePosition(Vector currentTransform) {
            var top = Canvas.GetTop(this);
            var left = Canvas.GetLeft(this);

            Canvas.SetTop(this, top + 5.0d + (currentTransform.Y*0.1d));
            Canvas.SetLeft(this, left + (currentTransform.X*0.1d));
        }

        public bool IsOutOfBounds(double width, double height) {
            var left = Canvas.GetLeft(this);
            var top = Canvas.GetTop(this);

            if (left < -ActualWidth)
                return true;

            if (left > width + ActualWidth)
                return true;

            if (top > height - ActualHeight)
                return true;

            return false;
        }
    }
}