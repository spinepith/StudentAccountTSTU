using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace StudentAccountTSTU.Views.Mobile;

public partial class MainViewMobile : UserControl
{
    public MainViewMobile()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e) {
        base.OnAttachedToVisualTree(e);
        App.TopLevel = Avalonia.Controls.TopLevel.GetTopLevel(this);
    }
}