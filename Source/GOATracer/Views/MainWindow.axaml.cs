namespace GOATracer.Views;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using GOATracer.ViewModels;
using GOATracer.Descriptions;
using GOATracer.Importer.Obj;
using System;
using System.IO;
using System.Text;

public partial class MainWindow : Window
{


    public MainWindow()
    {
        InitializeComponent();

        this.DataContext = new MainWindowViewModel();
    }




    private void ExitOptionClicked(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void LightBoxChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is CheckBox cb && cb.DataContext is Light light && this.DataContext is MainWindowViewModel vm)
        {
            vm.SetLightEnabled(light, cb.IsChecked ?? false);

        }
    }




    private void FileChooser_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }
}
