using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using Solutionizer.Extensions;

namespace Solutionizer.Behaviors {
    public class BindableSelectedItemBehavior : Behavior<TreeView> {
        public object SelectedItem {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof (object), typeof (BindableSelectedItemBehavior),
                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                                                      OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            var tvi = e.NewValue as TreeViewItem;
            if (tvi == null) {
                var tree = ((BindableSelectedItemBehavior) sender).AssociatedObject;
                tvi = tree.GetContainer<TreeViewItem>(e.NewValue);
            }
            if (tvi != null) {
                tvi.IsSelected = true;
                tvi.Focus();
            }
        }

        protected override void OnAttached() {
            base.OnAttached();

            AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching() {
            base.OnDetaching();

            if (AssociatedObject != null) {
                AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
            }
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            SelectedItem = e.NewValue;
        }
    }
}