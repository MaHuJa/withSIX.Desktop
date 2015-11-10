// <copyright company="SIX Networks GmbH" file="CustomVirtualizingWrapPanel.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Telerik.Windows.Controls;

namespace SN.withSIX.Play.Presentation.Wpf.Views.Controls
{
    /// <summary>
    ///     Workaround for Teleriks VirtualizingWrapPanel issues.
    ///     http://feedback.telerik.com/Project/143/Feedback/Details/147626-argumentoutofrangeexception-in-virtualizingwrappanel-onitemschanged
    /// </summary>
    public class CustomVirtualizingWrapPanel : VirtualizingWrapPanel
    {
        static readonly FieldInfo previousItemCountField;

        static CustomVirtualizingWrapPanel() {
            previousItemCountField = typeof (CustomVirtualizingWrapPanel).BaseType
                .GetField("previousItemCount", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        int PreviousItemCount
        {
            get { return (int) previousItemCountField.GetValue(this); }
            set { previousItemCountField.SetValue(this, value); }
        }

        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args) {
            ItemsControl itemsOwner;
            switch (args.Action) {
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
                // THE HACK
                RemoveChildRange(args.Position, args.ItemCount, args.ItemUICount);
                return;

            case NotifyCollectionChangedAction.Reset:
                itemsOwner = ItemsControl.GetItemsOwner(this);
                if (itemsOwner == null)
                    return;
                if (PreviousItemCount != itemsOwner.Items.Count) {
                    if (Orientation != Orientation.Horizontal) {
                        SetHorizontalOffset(0.0);
                        break;
                    }
                    SetVerticalOffset(0.0);
                }
                break;

            default:
                return;
            }
            var itemCount = itemsOwner.Items.Count;
            PreviousItemCount = itemCount;
        }

        // THE HACK implementation. A copy from Microsofts VirtualizingStackPanel
        void RemoveChildRange(GeneratorPosition position, int itemCount, int itemUICount) {
            if (IsItemsHost) {
                UIElementCollection children = InternalChildren;
                int pos = position.Index;
                if (position.Offset > 0) {
                    // An item is being removed after the one at the index 
                    pos++;
                }

                if (pos < children.Count) {
                    int uiCount = itemUICount;
                    Debug.Assert((itemCount == itemUICount) || (itemUICount == 0),
                        "Both ItemUICount and ItemCount should be equal or ItemUICount should be 0.");
                    if (uiCount > 0) {
                        RemoveInternalChildRange(pos, uiCount);
                        //VirtualizingPanel.RemoveInternalChildRange(children, pos, uiCount);

                        //if (IsVirtualizing && InRecyclingMode) {
                        //_realizedChildren.RemoveRange(pos, uiCount);
                        //}
                    }
                }
            }
        }
    }
}