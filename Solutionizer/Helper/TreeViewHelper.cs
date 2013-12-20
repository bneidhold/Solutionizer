using System.Windows.Controls;
using System.Windows.Media;
using Solutionizer.Extensions;

namespace Solutionizer.Helper {
    public static class TreeViewHelper {
        public static TreeViewItem GetTreeViewItem(ItemsControl container, object item) {
            if (container != null) {
                if (container.DataContext == item) {
                    return container as TreeViewItem;
                }

                // Expand the current container
                //if (container is TreeViewItem && !((TreeViewItem) container).IsExpanded) {
                //    container.SetValue(TreeViewItem.IsExpandedProperty, true);
                //}

                // Try to generate the ItemsPresenter and the ItemsPanel.
                // by calling ApplyTemplate.  Note that in the 
                // virtualizing case even if the item is marked 
                // expanded we still need to do this step in order to 
                // regenerate the visuals because they may have been virtualized away.

                container.ApplyTemplate();
                var itemsPresenter =
                    (ItemsPresenter) container.Template.FindName("ItemsHost", container);
                if (itemsPresenter != null) {
                    itemsPresenter.ApplyTemplate();
                } else {
                    // The Tree template has not named the ItemsPresenter, 
                    // so walk the descendents and find the child.
                    itemsPresenter = container.FindVisualChild<ItemsPresenter>();
                    if (itemsPresenter == null) {
                        container.UpdateLayout();

                        itemsPresenter = container.FindVisualChild<ItemsPresenter>();
                    }
                }

                var itemsHostPanel = (Panel) VisualTreeHelper.GetChild(itemsPresenter, 0);

                // Ensure that the generator for this panel has been created.
#pragma warning disable 168
                var children = itemsHostPanel.Children;
#pragma warning restore 168

                for (int i = 0, count = container.Items.Count; i < count; i++) {
                    var subContainer = (TreeViewItem) container.ItemContainerGenerator.
                        ContainerFromIndex(i);
                    if (subContainer == null) {
                        continue;
                    }

                    subContainer.BringIntoView();

                    // Search the next level for the object.
                    var resultContainer = GetTreeViewItem(subContainer, item);
                    if (resultContainer != null) {
                        return resultContainer;
                    } else {
                        // The object is not under this TreeViewItem
                        // so collapse it.
                        //subContainer.IsExpanded = false;
                    }
                }
            }

            return null;
        }
    }
}