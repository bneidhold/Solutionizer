using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using Solutionizer.Helper;

namespace Solutionizer.Infrastructure {
    public class MultipleItemSelectionAttachedBehavior : Behavior<TreeView> {
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.RegisterAttached(
            "SelectedItems", typeof(IList), typeof(MultipleItemSelectionAttachedBehavior), new PropertyMetadata(default(IList)));

        public static void SetSelectedItems(DependencyObject element, IList value) {
            element.SetValue(SelectedItemsProperty, value);
        }

        public static IList GetSelectedItems(DependencyObject element) {
            return (IList) element.GetValue(SelectedItemsProperty);
        }

        private static readonly PropertyInfo IsSelectionChangeActiveProperty = typeof (TreeView).GetProperty("IsSelectionChangeActive",
            BindingFlags.NonPublic | BindingFlags.Instance);

        public IList SelectedItems {
            get { return (IList) GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.SelectedItemChanged += OnSelectedItemChanged;
        }

        protected override void OnDetaching() {
            base.OnDetaching();
            AssociatedObject.SelectedItemChanged -= OnSelectedItemChanged;
        }

        private readonly List<TreeViewItem> _selectedItems = new List<TreeViewItem>();

        private void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            if (IsSelectionChangeActiveProperty == null) {
                return;
            }

            var treeViewItem = TreeViewHelper.GetTreeViewItem(AssociatedObject, e.NewValue);
            if (treeViewItem == null) return;

            // allow multiple selection
            // when control key is pressed
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
                var isSelectionChangeActive = IsSelectionChangeActiveProperty.GetValue(AssociatedObject, null);

                IsSelectionChangeActiveProperty.SetValue(AssociatedObject, true, null);
                _selectedItems.ForEach(item => item.IsSelected = true);

                IsSelectionChangeActiveProperty.SetValue(AssociatedObject, isSelectionChangeActive, null);
            } else {
                // deselect all selected items except the current one
                _selectedItems.ForEach(item => item.IsSelected = (Equals(item, treeViewItem)));
                _selectedItems.Clear();
            }

            if (!_selectedItems.Contains(treeViewItem)) {
                _selectedItems.Add(treeViewItem);
            } else {
                // deselect if already selected
                treeViewItem.IsSelected = false;
                _selectedItems.Remove(treeViewItem);
            }

            var items = _selectedItems.Select(tvi => tvi.DataContext).ToList();
            for (var i = SelectedItems.Count - 1; i >= 0; i--) {
                if (!items.Contains(SelectedItems[i])) {
                    SelectedItems.RemoveAt(i);
                }
            }
            foreach (var item in items.Where(item => !SelectedItems.Contains(item))) {
                SelectedItems.Add(item);
            }
        }
    }
}