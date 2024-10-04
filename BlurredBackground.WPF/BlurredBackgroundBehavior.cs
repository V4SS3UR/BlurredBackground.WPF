using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BlurredBackground.WPF
{
    public class BlurredBackground
    {
        public static readonly DependencyProperty EnableBlurProperty =
            DependencyProperty.RegisterAttached(
                "EnableBlur",
                typeof(bool),
                typeof(BlurredBackground),
                new PropertyMetadata(false, OnEnableBlurChanged));

        public static readonly DependencyProperty BlurRadiusProperty =
            DependencyProperty.RegisterAttached(
                "BlurRadius",
                typeof(double),
                typeof(BlurredBackground),
                new PropertyMetadata(20.0, OnBlurRadiusChanged));        

        public static readonly DependencyProperty MergingProperty =
            DependencyProperty.RegisterAttached(
                "Merging",
                typeof(double),
                typeof(BlurredBackground),
                new PropertyMetadata(1.0, OnMergingChanged));

        public static readonly DependencyProperty DpiProperty =
            DependencyProperty.RegisterAttached(
                "Dpi",
                typeof(int),
                typeof(BlurredBackground),
                new PropertyMetadata(96, OnDpiChanged));

        // Getters and Setters
        public static bool GetEnableBlur(DependencyObject obj) => (bool)obj.GetValue(EnableBlurProperty);
        public static void SetEnableBlur(DependencyObject obj, bool value) => obj.SetValue(EnableBlurProperty, value);

        public static double GetBlurRadius(DependencyObject obj) => (double)obj.GetValue(BlurRadiusProperty);
        public static void SetBlurRadius(DependencyObject obj, double value) => obj.SetValue(BlurRadiusProperty, value);

        public static double GetMerging(DependencyObject obj) => (double)obj.GetValue(MergingProperty);
        public static void SetMerging(DependencyObject obj, double value) => obj.SetValue(MergingProperty, value);

        public static int GetDpi(DependencyObject obj) => (int)obj.GetValue(DpiProperty);
        public static void SetDpi(DependencyObject obj, int value) => obj.SetValue(DpiProperty, value);

        private static RenderTargetBitmap cachedBitmap;

        private static void OnEnableBlurChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Border border && e.NewValue is bool isEnabled)
            {
                if (isEnabled)
                {
                    border.Loaded += (sender, args) => ApplyBlurEffect(border);
                    border.SizeChanged += (sender, args) => UpdateBlurEffect(border);

                    if (!(border.Child is Grid grid && grid.Children[0] is Rectangle rect) && border.Child != null)
                    {
                        ApplyBlurEffect(border);
                    }
                }
                else
                {
                    border.Loaded -= (sender, args) => ApplyBlurEffect(border);
                    border.SizeChanged -= (sender, args) => UpdateBlurEffect(border);
                    UnapplyBlurEffect(border);
                }
            }
        }
        private static void OnBlurRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Border border)
            {
                UpdateBlurEffect(border);
            }
        }
        private static void OnMergingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Border border)
            {
                UpdateBlurEffect(border);
            }
        }
        private static void OnDpiChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Border border)
            {
                UpdateBlurEffect(border);
            }
        }

        private static void ApplyBlurEffect(Border border)
        {
            // Create a Rectangle to serve as the blurred background
            var backgroundRectangle = new Rectangle
            {
                RadiusX = border.CornerRadius.TopLeft,
                RadiusY = border.CornerRadius.TopRight,
            };

            //The new content must be a grid with the blurry rectangle under the current content
            var borderContent = border.Child;

            //Overwrite the border content
            Grid contentGrid = new Grid();
            border.Child = contentGrid;

            //Insert blurry rectangle then content 
            contentGrid.Children.Insert(0, backgroundRectangle);

            if(borderContent!=null)
            {
                contentGrid.Children.Add(borderContent);
            }

            UpdateBlurEffect(border);
        }
        private static void UnapplyBlurEffect(Border border)
        {
            if (border.Child is Grid grid && grid.Children[0] is Rectangle rect)
            {
                rect.Fill = Brushes.Transparent;
                rect.Effect = null;

                //reaply the original content
                var gridContent = grid.Children[1];
                grid.Children.Clear();
                border.Child = gridContent;
            }
        }

        private static void UpdateBlurEffect(Border border)
        {
            if (border.Child is Grid grid && grid.Children[0] is Rectangle backgroundRectangle)
            {
                //Check if blur radius need update
                var currentBlurEffect = backgroundRectangle.Effect as BlurEffect;
                if (currentBlurEffect?.Radius != GetBlurRadius(border))
                {
                    // Update the blur radius
                    var blurEffect = new BlurEffect { Radius = GetBlurRadius(border) };
                    backgroundRectangle.Effect = blurEffect;
                }

                // Create a geometry for clipping
                var clipGeometry = new RectangleGeometry
                {
                    // Adjust the rectangle size to match the border dimensions plus negative margins
                    Rect = new Rect(0, 0, border.ActualWidth, border.ActualHeight),
                    RadiusX = border.CornerRadius.TopLeft,
                    RadiusY = border.CornerRadius.TopRight
                };
                if(backgroundRectangle.Clip == null)
                {
                    backgroundRectangle.Clip = clipGeometry;
                }
                else
                {
                    //Update the clip if existing and different
                    if (!backgroundRectangle.Clip.Bounds.Equals(clipGeometry.Bounds))
                    {
                        backgroundRectangle.Clip = clipGeometry;
                    }
                }     

                // Update the visual brush
                UpdateVisualBrush(border, backgroundRectangle);
            }
        }

        private static void UpdateVisualBrush(Border border, Rectangle backgroundRectangle)
        {
            if (border.Visibility != Visibility.Visible)
                return;

            // Find the parent window of the border
            var window = border.FindParent<Window>();
            var dpi = GetDpi(border);
            var borderSize = new Size(border.ActualWidth, border.ActualHeight);
            var baseOpacity = border.Opacity;

            // Get the root content of the window (this is typically the main content container)
            var rootVisual = window.Content as Visual;

            // Hide the child component temporarily
            if (border is UIElement uiElementToExclude)
            {
                uiElementToExclude.Opacity = 0;
            }

            // Get the dimensions of the content area
            var transform = border.TransformToVisual(window); // Transform from border to window coordinates
            var borderPosition = transform.Transform(new Point(0, 0)); // Get the top-left position of the border relative to the window

            // Calculate the size of the RenderTargetBitmap based on the border's size
            int width = (int)(window.ActualWidth * dpi / 96);
            int height = (int)(window.ActualHeight * dpi / 96);            

            // Render only the root content of the window (exclude window chrome)
            RenderTargetBitmap renderTarget = new RenderTargetBitmap(width, height, dpi, dpi, PixelFormats.Pbgra32);
            renderTarget.Render(rootVisual);

            // Create an ImageBrush from the RenderTargetBitmap
            var imageBrush = new ImageBrush(renderTarget)
            {
                Stretch = Stretch.None,
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(borderPosition.X, borderPosition.Y, borderSize.Width, borderSize.Height),
                Opacity = GetMerging(border)
            };

            // Apply the VisualBrush to the background rectangle
            backgroundRectangle.Fill = imageBrush;

            // Restore the visibility of the excluded child component
            if (border is UIElement uiElementToRestore)
            {
                uiElementToRestore.Opacity = baseOpacity;
            }
        }
    }
}
