using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.LogicalTree;

namespace GOATracer;

public class LightControl
{
    /// <summary>
    /// ID of this light control
    /// </summary>
    public int Id { get; private set; }
    /// <summary>
    /// The UI control element itself
    /// </summary>
    public Grid Control { get; }
    /// <summary>
    /// Callback for a method to call when the Control gets deleted
    /// </summary>
    private Action<int> _deleteCallback;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="lightId">ID of the light control</param>
    /// <param name="deleteCallback">Callback for deletion</param>
    public LightControl(int lightId, Action<int> deleteCallback)
    {
        Id = lightId;
        Control = GenerateLightControls(lightId);
        _deleteCallback = deleteCallback;
    }

    /// <summary>
    /// Method for generating the layout of the light control
    /// </summary>
    /// <param name="lightId">ID number of the light to use for the names</param>
    /// <returns>A Grid control which contains all the element of the Light control UI element</returns>
    private Grid GenerateLightControls(int lightId)
    {
        // <Grid Margin="5" RowDefinitions="*,*" ColumnDefinitions="3*,3*,3*,*">
        var grid = new Grid
        {
            Name = "LightControl" + lightId,
            Margin = new Thickness(5),
            RowDefinitions = RowDefinitions.Parse("*,*"),
            ColumnDefinitions = ColumnDefinitions.Parse("3*,3*,3*,*")
        };
        
        // <Label Content="Light 1 X" VerticalAlignment="Center" />
        var labelX = new Label
        {
            Content = "Light " + lightId + " X",
            VerticalAlignment = VerticalAlignment.Center
        };
        
        // Grid.SetRow(labelX, 0);
        // Grid.SetColumn(labelX, 0);
        grid.Children.Add(labelX);

        // <TextBox Grid.Row="1" Grid.Column="0" x:Name="LightX1" ... />
        var lightX = new TextBox
        {
            Name = "LightX" + lightId,
            VerticalContentAlignment = VerticalAlignment.Center,
            Text = "0"
        };
        Grid.SetRow(lightX, 1);
        Grid.SetColumn(lightX, 0);
        grid.Children.Add(lightX);

        // --- Light Y ---
        // <Label Grid.Row="0" Grid.Column="1" Content="Light 1 Y" ... />
        var labelY = new Label
        {
            Content = "Light " + lightId + " Y",
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(labelY, 0);
        Grid.SetColumn(labelY, 1);
        grid.Children.Add(labelY);

        // <TextBox Grid.Row="1" Grid.Column="1" x:Name="LightY1" ... />
        var lightY = new TextBox
        {
            Name = "LightY" + lightId,
            VerticalContentAlignment = VerticalAlignment.Center,
            Text = "0"
        };
        Grid.SetRow(lightY, 1);
        Grid.SetColumn(lightY, 1);
        grid.Children.Add(lightY);
        
        // <Label Grid.Row="0" Grid.Column="2" Content="Light 1 Z" ... />
        var labelZ = new Label
        {
            Content = "Light " + lightId + " Z",
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(labelZ, 0);
        Grid.SetColumn(labelZ, 2);
        grid.Children.Add(labelZ);

        // <TextBox Grid.Row="1" Grid.Column="2" x:Name="LightZ1" ... />
        var lightZ = new TextBox
        {
            Name = "LightZ" +  lightId,
            VerticalContentAlignment = VerticalAlignment.Center,
            Text = "0"
        };
        Grid.SetRow(lightZ, 1);
        Grid.SetColumn(lightZ, 2);
        grid.Children.Add(lightZ);
        
        // <Button Grid.Row="1" Grid.Column="3" Content="X"/>
        var deleteButton = new Button
        {
            Content = "X",
        };

        // Add an event handler for the deletion of the control
        deleteButton.Click += (_, __) =>
        {
            // Tell the parent StackPanel of this control to remove this control from itself
            var parent = this.Control.FindLogicalAncestorOfType<StackPanel>();
            parent?.Children.Remove(this.Control);
            // Call the callback in the main window to clean up the list
            _deleteCallback(lightId);
        };
        Grid.SetRow(deleteButton, 1);
        Grid.SetColumn(deleteButton, 3);
        grid.Children.Add(deleteButton);

        return grid;
    }
}