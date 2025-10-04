using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ImageUtility;

public partial class RenamerView : UserControl
{
    public RenamerView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}