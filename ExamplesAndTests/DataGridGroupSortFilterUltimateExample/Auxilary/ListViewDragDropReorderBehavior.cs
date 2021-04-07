using Microsoft.Xaml.Behaviors;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace DataGridGroupSortFilterUltimateExample.Auxilary
{
    /// <summary>
    /// Manages the dragging and dropping of ListViewItems in a ListView.
    /// </summary>
    /// <remarks>
    /// Based on code by Josh Smith, Copyright (C) Josh Smith - January 2007
    /// https://www.codeproject.com/Articles/17266/Drag-and-Drop-Items-in-a-WPF-ListView
    /// </remarks>

    public class ListViewDragDropReorderBehavior : Behavior<ListView>
    {
        #region Private Fields

        private bool _canInitiateDrag;
        private DragAdorner _dragAdorner;
        private double _dragAdornerOpacity;
        private int _indexToSelect;
        private object _itemUnderDragCursor;
        private ListView _listView;
        private Point _mouseDownPos;
        private bool _showDragAdorner;

        #endregion Private Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of ListViewDragManager.
        /// </summary>
        public ListViewDragDropReorderBehavior()
        {
            _canInitiateDrag = false;
            _dragAdornerOpacity = 0.8;
            _indexToSelect = -1;
            _showDragAdorner = true;
        }


        #endregion // Constructors

        #region Protected Overrides

        /// <summary>
        /// Attaches to an ItemsControl
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject is ListView listView)
            {
                ListView = listView;
            }
        }

        protected override void OnDetaching()
        {
            ListView = null;

            base.OnDetaching();
        }

        #endregion Protected Overrides

        #region Properties

        /// <summary>
        /// Gets/sets the opacity of the drag adorner.  This property has no
        /// effect if ShowDragAdorner is false. The default value is 0.7
        /// </summary>
        public double DragAdornerOpacity
        {
            get => _dragAdornerOpacity;
            set => _dragAdornerOpacity = Math.Max(0, Math.Min(1.0, value));
        }

        /// <summary>
        /// Gets/sets the ListView whose dragging is managed.  This property
        /// can be set to null, to prevent drag management from occuring.  If
        /// the ListView's AllowDrop property is false, it will be set to true.
        /// </summary>
        public ListView ListView
        {
            get => _listView;
            set
            {
                if (_listView != null)
                {
                    // Unhook Events

                    _listView.PreviewMouseLeftButtonDown -= listView_PreviewMouseLeftButtonDown;
                    _listView.PreviewMouseMove -= listView_PreviewMouseMove;
                    _listView.DragOver -= listView_DragOver;
                    _listView.DragLeave -= listView_DragLeave;
                    _listView.DragEnter -= listView_DragEnter;
                    _listView.Drop -= listView_Drop;
                }

                _listView = value;

                if (_listView != null)
                {
                    _listView.AllowDrop = true;

                    // Hook Events

                    _listView.PreviewMouseLeftButtonDown += listView_PreviewMouseLeftButtonDown;
                    _listView.PreviewMouseMove += listView_PreviewMouseMove;
                    _listView.DragOver += listView_DragOver;
                    _listView.DragLeave += listView_DragLeave;
                    _listView.DragEnter += listView_DragEnter;
                    _listView.Drop += listView_Drop;
                }
            }
        }

        /// <summary>
        /// Gets/sets whether a visual representation of the ListViewItem being dragged
        /// follows the mouse cursor during a drag operation.  The default value is true.
        /// </summary>
        public bool ShowDragAdorner
        {
            get { return _showDragAdorner; }
            set => _showDragAdorner = value;
        }

        #endregion Properties

        #region Event Handling Methods

        void listView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseOverScrollbar)
            {
                // 4/13/2007 - Set the flag to false when cursor is over scrollbar.
                _canInitiateDrag = false;
                return;
            }

            int index = IndexUnderDragCursor;
            _canInitiateDrag = index > -1;

            if (_canInitiateDrag)
            {
                // Remember the location and index of the ListViewItem the user clicked on for later.
                _mouseDownPos = GetMousePosition(_listView);
                _indexToSelect = index;
            }
            else
            {
                _mouseDownPos = new Point(-10000, -10000);
                _indexToSelect = -1;
            }
        }

        void listView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!CanStartDragOperation)
            {
                return;
            }

            // Select the item the user clicked on.
            if (_listView.SelectedIndex != _indexToSelect)
            {
                _listView.SelectedIndex = _indexToSelect;
            }

            // If the item at the selected index is null, there's nothing
            // we can do, so just return;
            if (_listView.SelectedItem == null)
            {
                return;
            }

            ListViewItem itemToDrag = GetListViewItem(_listView.SelectedIndex);
            if (itemToDrag == null)
            {
                return;
            }

            AdornerLayer adornerLayer = ShowDragAdornerResolved ? InitializeAdornerLayer(itemToDrag) : null;

            InitializeDragOperation(itemToDrag);
            PerformDragOperation();
            FinishDragOperation(itemToDrag, adornerLayer);
        }


        void listView_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;

            if (ShowDragAdornerResolved)
            {
                UpdateDragAdornerLocation();
            }

            // Update the item which is known to be currently under the drag cursor.
            int index = IndexUnderDragCursor;
            ItemUnderDragCursor = index < 0 ? null : ListView.Items[index];
        }


        void listView_DragLeave(object sender, DragEventArgs e)
        {
            if (!IsMouseOver(_listView))
            {
                if (ItemUnderDragCursor != null)
                {
                    ItemUnderDragCursor = null;
                }

                if (_dragAdorner != null)
                {
                    _dragAdorner.Visibility = Visibility.Collapsed;
                }
            }
        }

        void listView_DragEnter(object sender, DragEventArgs e)
        {
            if (_dragAdorner != null && _dragAdorner.Visibility != Visibility.Visible)
            {
                // Update the location of the adorner and then show it.				
                UpdateDragAdornerLocation();
                _dragAdorner.Visibility = Visibility.Visible;
            }
        }

        void listView_Drop(object sender, DragEventArgs e)
        {
            if (ItemUnderDragCursor != null)
            {
                ItemUnderDragCursor = null;
            }

            e.Effects = DragDropEffects.None;

            // Get the ObservableCollection<ItemType> which contains the dropped data object.
            IList itemsSource = _listView.ItemsSource as IList;
            if (itemsSource == null)
                throw new Exception(
                  "A ListView managed by ListViewDragDropReorderBehavior must have its ItemsSource set to an IList.");

            int oldIndex = _indexToSelect;
            int newIndex = IndexUnderDragCursor;

            // Dragging and dropping between lists not implemented, could be added
            // Dropping an item back onto itself is not considered an actual 'drop'
            if (newIndex < 0 || oldIndex < 0 || oldIndex == newIndex)
            {
                return;
            }
            // Move the dragged data object from it's original index to the
            // new index (according to where the mouse cursor is).
            var oldData = itemsSource[oldIndex];
            itemsSource.RemoveAt(oldIndex);
            itemsSource.Insert(newIndex, oldData);

            // Set the Effects property so that the call to DoDragDrop will return 'Move'.
            e.Effects = DragDropEffects.Move;
        }

        #endregion // Event Handling Methods

        #region Private Helpers

        // POINT

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref POINT pt);

        /// <summary>
        /// Returns the mouse cursor location.  This method is necessary during 
        /// a drag-drop operation because the WPF mechanisms for retrieving the
        /// cursor coordinates are unreliable.
        /// </summary>
        /// <param name="relativeTo">The Visual to which the mouse coordinates will be relative.</param>
        /// <remarks>
        /// Original version written by Dan Crevier (Microsoft).  
        /// http://blogs.msdn.com/llobo/archive/2006/09/06/Scrolling-Scrollviewer-on-Mouse-Drag-at-the-boundaries.aspx
        /// </remarks>
        public static Point GetMousePosition(Visual relativeTo)
        {
            POINT mouse = new POINT();
            GetCursorPos(ref mouse);

            // Using PointFromScreen instead of Dan Crevier's code (commented out below)
            // is a bug fix created by William J. Roberts.  Read his comments about the fix:

            // After picking my brain for a long while, I think I have come up with a solution.
            // I believe the problem resides in Dan's GetMousePosition(...) static method.
            // First he grabs the mouses position and converts the value to be relative to the hWnd.
            // This point is represented by pixels.
            // Next, Dan gets the hWnd's transform. This is where I believe he is incorrect.
            // The problem is, WPF's internal representation of coordinates is not pixel values.
            // Instead, a virtual coordinate system is used under the hood.
            // He then takes this virtual coordinate offset and subtracts it from actual
            // screen pixel coordinates. Although the coordiantes seemed to luckily work out
            // in standard resolutions, the virtual coordinates are scaled for wide screen
            // resolution. Hence, the issue I was seeing would occur.
            // I noticed that the values where being scaled as I moved the mouse in the X direction.
            // I believe his post was made prior to the PointFromScreen and ScreenToPoint methods
            // being added. Using the PointFromScreen method, I believe I have created a much more
            // elegant solution. I have tested the below fix on both a wide screen monitor and a
            // standard monitor. Please let me know if my assumptions are incorrect. I am by no
            // means a WPF expert yet.

            return relativeTo.PointFromScreen(new Point((double)mouse.x, (double)mouse.y));

            //System.Windows.Interop.HwndSource presentationSource = (System.Windows.Interop.HwndSource)PresentationSource.FromVisual( relativeTo );
            //ScreenToClient( presentationSource.Handle, ref mouse );
            //GeneralTransform transform = relativeTo.TransformToAncestor( presentationSource.RootVisual );
            //Point offset = transform.Transform( new Point( 0, 0 ) );
            //return new Point( mouse.X - offset.X, mouse.Y - offset.Y );
        }

        private bool CanStartDragOperation
        {
            get
            {
                if (Mouse.LeftButton != MouseButtonState.Pressed)
                    return false;

                if (!_canInitiateDrag)
                    return false;

                if (_indexToSelect == -1)
                    return false;

                if (!HasCursorLeftDragThreshold)
                    return false;

                return true;
            }
        }

        private void FinishDragOperation(ListViewItem draggedItem, AdornerLayer adornerLayer)
        {
            // Let the ListViewItem know that it is not being dragged anymore.
            ListViewItemDragState.SetIsBeingDragged(draggedItem, false);

            ItemUnderDragCursor = null;

            // Remove the drag adorner from the adorner layer.
            if (adornerLayer != null)
            {
                adornerLayer.Remove(_dragAdorner);
                _dragAdorner = null;
            }
        }

        private ListViewItem GetListViewItem(int index)
        {
            if (_listView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;

            return _listView.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
        }

        private ListViewItem GetListViewItem(object dataItem)
        {
            if (_listView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;

            return _listView.ItemContainerGenerator.ContainerFromItem(dataItem) as ListViewItem;
        }


        private bool HasCursorLeftDragThreshold
        {
            get
            {
                if (_indexToSelect < 0)
                    return false;

                ListViewItem item = GetListViewItem(_indexToSelect);
                Rect bounds = VisualTreeHelper.GetDescendantBounds(item);
                Point ptInItem = _listView.TranslatePoint(_mouseDownPos, item);

                // In case the cursor is at the very top or bottom of the ListViewItem
                // we want to make the vertical threshold very small so that dragging
                // over an adjacent item does not select it.
                double topOffset = Math.Abs(ptInItem.Y);
                double btmOffset = Math.Abs(bounds.Height - ptInItem.Y);
                double vertOffset = Math.Min(topOffset, btmOffset);

                double width = SystemParameters.MinimumHorizontalDragDistance * 2;
                double height = Math.Min(SystemParameters.MinimumVerticalDragDistance, vertOffset) * 2;
                Size szThreshold = new Size(width, height);

                Rect rect = new Rect(_mouseDownPos, szThreshold);
                rect.Offset(szThreshold.Width / -2, szThreshold.Height / -2);
                Point ptInListView = GetMousePosition(_listView);
                return !rect.Contains(ptInListView);
            }
        }

        /// <summary>
        /// Returns the index of the ListViewItem underneath the
        /// drag cursor, or -1 if the cursor is not over an item.
        /// </summary>
        private int IndexUnderDragCursor
        {
            get
            {
                int index = -1;
                for (int i = 0; i < _listView.Items.Count; ++i)
                {
                    ListViewItem item = GetListViewItem(i);
                    if (IsMouseOver(item))
                    {
                        index = i;
                        break;
                    }
                }
                return index;
            }
        }

        private AdornerLayer InitializeAdornerLayer(ListViewItem itemToDrag)
        {
            // Create a brush which will paint the ListViewItem onto
            // a visual in the adorner layer.
            VisualBrush brush = new VisualBrush(itemToDrag);

            // Create an element which displays the source item while it is dragged.
            _dragAdorner = new DragAdorner(_listView, itemToDrag.RenderSize, brush);

            // Set the drag adorner's opacity.		
            _dragAdorner.Opacity = DragAdornerOpacity;

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(_listView);
            layer.Add(_dragAdorner);

            // Save the location of the cursor when the left mouse button was pressed.
            _mouseDownPos = GetMousePosition(_listView);

            return layer;
        }

        private void InitializeDragOperation(ListViewItem itemToDrag)
        {
            // Set some flags used during the drag operation.
            _canInitiateDrag = false;

            // Let the ListViewItem know that it is being dragged.
            ListViewItemDragState.SetIsBeingDragged(itemToDrag, true);
        }


        private bool IsMouseOver(Visual target)
        {
            // We need to use MouseUtilities to figure out the cursor
            // coordinates because, during a drag-drop operation, the WPF
            // mechanisms for getting the coordinates behave strangely.

            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            Point mousePos = GetMousePosition(target);
            return bounds.Contains(mousePos);
        }


        /// <summary>
        /// Returns true if the mouse cursor is over a scrollbar in the ListView.
        /// </summary>
        private bool IsMouseOverScrollbar
        {
            get
            {
                Point ptMouse = GetMousePosition(_listView);
                HitTestResult res = VisualTreeHelper.HitTest(_listView, ptMouse);
                if (res == null)
                    return false;

                DependencyObject depObj = res.VisualHit;
                while (depObj != null)
                {
                    if (depObj is ScrollBar)
                        return true;

                    // VisualTreeHelper works with objects of type Visual or Visual3D.
                    // If the current object is not derived from Visual or Visual3D,
                    // then use the LogicalTreeHelper to find the parent element.
                    if (depObj is Visual || depObj is System.Windows.Media.Media3D.Visual3D)
                        depObj = VisualTreeHelper.GetParent(depObj);
                    else
                        depObj = LogicalTreeHelper.GetParent(depObj);
                }

                return false;
            }
        }

        private object ItemUnderDragCursor
        {
            get { return _itemUnderDragCursor; }
            set
            {
                if (_itemUnderDragCursor == value)
                    return;

                // The first pass handles the previous item under the cursor.
                // The second pass handles the new one.
                for (int i = 0; i < 2; ++i)
                {
                    if (i == 1)
                        _itemUnderDragCursor = value;

                    if (_itemUnderDragCursor != null)
                    {
                        ListViewItem listViewItem = GetListViewItem(_itemUnderDragCursor);
                        if (listViewItem != null)
                            ListViewItemDragState.SetIsUnderDragCursor(listViewItem, i == 1);
                    }
                }
            }
        }


        private void PerformDragOperation()
        {
            object selectedItem = _listView.SelectedItem;
            DragDropEffects allowedEffects = DragDropEffects.Move | DragDropEffects.Move | DragDropEffects.Link;
            if (DragDrop.DoDragDrop(_listView, selectedItem, allowedEffects) != DragDropEffects.None)
            {
                // The item was dropped into a new location,
                // so make it the new selected item.
                _listView.SelectedItem = selectedItem;
            }
        }


        private bool ShowDragAdornerResolved => ShowDragAdorner && DragAdornerOpacity > 0.0;


        private void UpdateDragAdornerLocation()
        {
            if (_dragAdorner != null)
            {
                Point ptCursor = GetMousePosition(ListView);

                double left = ptCursor.X - _mouseDownPos.X;

                // 4/13/2007 - Made the top offset relative to the item being dragged.
                ListViewItem itemBeingDragged = GetListViewItem(_indexToSelect);
                Point itemLoc = itemBeingDragged.TranslatePoint(new Point(0, 0), ListView);
                double top = itemLoc.Y + ptCursor.Y - _mouseDownPos.Y;

                _dragAdorner.SetOffsets(left, top);
            }
        }

        #endregion // Private Helpers
    }


    #region ListViewItemDragState

    /// <summary>
    /// Exposes attached properties used in conjunction with the ListViewDragDropManager class.
    /// Those properties can be used to allow triggers to modify the appearance of ListViewItems
    /// in a ListView during a drag-drop operation.
    /// </summary>
    public static class ListViewItemDragState
    {
        /// <summary>
        /// Identifies the ListViewItemDragState's IsBeingDragged attached property.  
        /// This field is read-only.
        /// </summary>
        public static readonly DependencyProperty IsBeingDraggedProperty =
          DependencyProperty.RegisterAttached(
            "IsBeingDragged",
            typeof(bool),
            typeof(ListViewItemDragState),
            new UIPropertyMetadata(false));

        /// <summary>
        /// Returns true if the specified ListViewItem is being dragged, else false.
        /// </summary>
        /// <param name="item">The ListViewItem to check.</param>
        public static bool GetIsBeingDragged(ListViewItem item)
        {
            return (bool)item.GetValue(IsBeingDraggedProperty);
        }

        /// <summary>
        /// Sets the IsBeingDragged attached property for the specified ListViewItem.
        /// </summary>
        /// <param name="item">The ListViewItem to set the property on.</param>
        /// <param name="value">Pass true if the element is being dragged, else false.</param>
        internal static void SetIsBeingDragged(ListViewItem item, bool value)
        {
            item.SetValue(IsBeingDraggedProperty, value);
        }

        /// <summary>
        /// Identifies the ListViewItemDragState's IsUnderDragCursor attached property.  
        /// This field is read-only.
        /// </summary>
        public static readonly DependencyProperty IsUnderDragCursorProperty =
          DependencyProperty.RegisterAttached(
            "IsUnderDragCursor",
            typeof(bool),
            typeof(ListViewItemDragState),
            new UIPropertyMetadata(false));

        /// <summary>
        /// Returns true if the specified ListViewItem is currently underneath the cursor 
        /// during a drag-drop operation, else false.
        /// </summary>
        /// <param name="item">The ListViewItem to check.</param>
        public static bool GetIsUnderDragCursor(ListViewItem item)
        {
            return (bool)item.GetValue(IsUnderDragCursorProperty);
        }

        /// <summary>
        /// Sets the IsUnderDragCursor attached property for the specified ListViewItem.
        /// </summary>
        /// <param name="item">The ListViewItem to set the property on.</param>
        /// <param name="value">Pass true if the element is underneath the drag cursor, else false.</param>
        internal static void SetIsUnderDragCursor(ListViewItem item, bool value)
        {
            item.SetValue(IsUnderDragCursorProperty, value);
        }

    }

    #endregion // ListViewItemDragState
}
