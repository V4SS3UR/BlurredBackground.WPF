using System.Windows;
using System.Windows.Media;

namespace BlurredBackground.WPF
{
    internal static class DependencyObjectExtensions
    {
        public static T FindParent<T>(this DependencyObject obj) where T : DependencyObject
        {
            if (obj == null)
                return null;

            DependencyObject current = obj;
            while (current != null)
            {
                current = VisualTreeHelper.GetParent(current);

                // If the current parent is of type T, return it.
                if (current is T parent)
                    return parent;
            }

            return null;
        }
    }
}
