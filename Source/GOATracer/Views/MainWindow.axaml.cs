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
    // Constructor initializes the MainWindow and sets its DataContext to MainWindowViewModel, start of the program
    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = new MainWindowViewModel();
    }
    // Event handler for the Exit menu option, closes the application when clicked
    private void ExitOptionClicked(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }
    private void FileChooser_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }
}
