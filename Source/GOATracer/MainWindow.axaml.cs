using System.IO;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GOATracer.Importer.Obj;

public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();
    }




    private void ExitOptionClicked(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }



    private void RenderClick(object? sender, RoutedEventArgs e)
    {
        var renderWindow = new RenderWindow();
        renderWindow.Show();
    }

    private void FileChooser_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }
}
