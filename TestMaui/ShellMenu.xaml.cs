using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace TestMaui;

public partial class ShellMenu : Shell
{
    public ShellMenu()
    {
        InitializeComponent();
        BindingContext = this;
        NavigateTo = new Command(() =>
        {
            //CurrentItem = (ShellSection);
        });
    }

    public ICommand NavigateTo { get; }
}
