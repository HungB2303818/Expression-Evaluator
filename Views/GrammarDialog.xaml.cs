using System.Windows;
using ExpressionEvaluator.ViewModels;

namespace ExpressionEvaluator.Views;

public partial class GrammarDialog : Window
{
    public GrammarDialog()
    {
        InitializeComponent();
        DataContext = new GrammarViewModel();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
