using System;
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
                new PropertyMetadata(0.5, OnMergingChanged));

        public static readonly DependencyProperty DpiProperty =
            DependencyProperty.RegisterAttached(
                "Dpi",
                typeof(int),
                typeof(BlurredBackground),
                new PropertyMetadata(96, OnDpiChanged)); // Default to 96 DPI

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
            contentGrid.Children.Add(borderContent);

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
            int dpi = GetDpi(border); // Use the DPI property

            //If no cached data or size changed
            if (cachedBitmap == null || (int)border.ActualWidth != cachedBitmap.PixelWidth || (int)border.ActualHeight != cachedBitmap.PixelHeight)
            {
                cachedBitmap = new RenderTargetBitmap( (int)border.ActualWidth, (int)border.ActualHeight, 96d, 96d, PixelFormats.Pbgra32);
            }


            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                var window = border.FindParent<Window>();

                if (window != null)
                {
                    var borderPosition = border.TranslatePoint(new Point(0, 0), window);
                    var visualBrush = GetParentVisualFromWindow(window, border);

                    visualBrush.Stretch = Stretch.None;
                    visualBrush.Viewbox = new Rect(borderPosition.X, borderPosition.Y, border.ActualWidth, border.ActualHeight);
                    visualBrush.ViewboxUnits = BrushMappingMode.Absolute;

                    context.DrawRectangle(visualBrush, null, new Rect(0, 0, border.ActualWidth, border.ActualHeight));
                }
            }

            var renderTargetBitmap = cachedBitmap;
            renderTargetBitmap.Render(visual);

            var imageBrush = new ImageBrush(renderTargetBitmap)
            {
                Stretch = Stretch.None,
                Opacity = GetMerging(border),
            };

            if (!IsBrushEqual(backgroundRectangle.Fill, imageBrush))
            {
                backgroundRectangle.Fill = imageBrush;
            }
        }

        private static bool IsBrushEqual(Brush brush1, Brush brush2)
        {
            if (brush1 is ImageBrush imageBrush1 && brush2 is ImageBrush imageBrush2)
            {
                return imageBrush1.ImageSource == imageBrush2.ImageSource &&
                       imageBrush1.Opacity == imageBrush2.Opacity;
            }

            return false;
        }



        // This method creates a VisualBrush that represents the visual hierarchy of a given UIElement within a window.
        // It captures all the parent visuals except the specified target element itself.
        private static VisualBrush GetParentVisualFromWindow(Window window, UIElement uIElement)
        {
            DrawingVisual drawingVisual = new DrawingVisual();

            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                // Call a recursive method to draw the visual hierarchy, starting from the window (root element).
                DrawVisualHierarchy(window, uIElement, window, drawingContext);
            }

            return new VisualBrush(drawingVisual);
        }

        // This method recursively traverses and draws the visual hierarchy, skipping the target element.
        private static void DrawVisualHierarchy(Visual parent, UIElement targetElement, Window window, DrawingContext drawingContext)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                if (VisualTreeHelper.GetChild(parent, i) is UIElement child)
                {
                    // Ensure that the current child is not the target element that we want to exclude.
                    if (child != targetElement)
                    {
                        var bounds = VisualTreeHelper.GetContentBounds(child);

                        // Check if the bounds are valid (i.e., non-empty and positive dimensions).
                        if (!bounds.IsEmpty && bounds.Width > 0 && bounds.Height > 0)
                        {
                            // Create a brush from the visual.
                            var brush = MakeBrushFromVisual(child, bounds, GetDpi(targetElement));

                            Point childPosition = child.TranslatePoint(new Point(0, 0), window);

                            // Draw the child visual onto the DrawingContext with the calculated position and size.
                            drawingContext.DrawRectangle(brush, null, new Rect(childPosition, new Size(child.RenderSize.Width, child.RenderSize.Height)));
                        }

                        // Recursively call this method to process all the child elements of the current child.
                        DrawVisualHierarchy(child, targetElement, window, drawingContext);
                    }
                }
            }
        }

        private static Brush MakeBrushFromVisual(Visual visual, Rect bounds, int dpi)
        {
            Drawing drawing = VisualTreeHelper.GetDrawing(visual);
            if (drawing == null) 
                return Brushes.Transparent;


            // Create a matrix to translate the visual by the negative bounds (to handle offsets).

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawDrawing(drawing);
            }

            // Render the visual into a bitmap with the specified DPI.
            RenderTargetBitmap renderTargetBitmap = RenderVisual(drawingVisual, ((UIElement)visual).RenderSize, dpi);
            renderTargetBitmap.Freeze();

            // Create an ImageBrush using the rendered bitmap.
            ImageBrush imageBrush = new ImageBrush(renderTargetBitmap);
            imageBrush.Freeze();

            return imageBrush;
        }

        private static RenderTargetBitmap RenderVisual(Visual visual, Size bounds, int dpi)
        {
            const double BaseDpi = 96.0;

            // Calculate the scaling factors for X and Y based on the target DPI.
            double scaleX = dpi / BaseDpi;
            double scaleY = dpi / BaseDpi;

            // Compute the pixel dimensions of the bitmap, ensuring the size is rounded up to avoid clipping.
            int pixelWidth = (int)Math.Ceiling(scaleX * bounds.Width);
            int pixelHeight = (int)Math.Ceiling(scaleY * bounds.Height);

            var renderTargetBitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, dpi, dpi, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(visual);
            renderTargetBitmap.Freeze();

            return renderTargetBitmap;
        }
    }
}
