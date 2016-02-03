using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FF_TouchlessControllerViewer.cs
{
    /// <summary>
    /// Auxiliary methods class that are unrelated to the sample directly
    /// </summary>
    public static class HelperMethods
    {
        public static DependencyObject GetScrollViewer(DependencyObject o)
        {
            // Return the DependencyObject if it is a ScrollViewer
            if (o is ScrollViewer)
            { return o; }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                return result;
            }
            return null;
        }

        public static bool PointOnButton(Point point, Button button, Grid containinGrid)
        {
            var p = button.TranslatePoint(new Point(), containinGrid);

            return (p.X <= point.X * containinGrid.ActualWidth) &&
                   ((p.X + button.ActualWidth) >= point.X * containinGrid.ActualWidth) &&
                   ((p.Y + button.ActualHeight) >= point.Y * containinGrid.ActualHeight) &&
                   (p.Y <= point.Y * containinGrid.ActualHeight);
        }
    }

    public static class ExtensionMethods
    {
        public static string ToFormattedString(this PXCMPoint3DF32 pos)
        {
            return String.Format("{0:###0.000}, {1:###0.000}, {2:###0.000}", pos.x, pos.y, pos.z);
        }
    }
}
