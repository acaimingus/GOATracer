using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace GOATracer.Raytracer;

public partial class RaytraceWindow : Window
{
    public RaytraceWindow()
    {
        Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
    }

    public RaytraceWindow(Bitmap image)
    {
        Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);

        var imageControl = this.FindControl<Image>("RaytraceImage");
        if (imageControl != null)
        {
            imageControl.Source = image;
        }
    }
}