using System.Windows;
using ExpressionEvaluator.Views;

namespace ExpressionEvaluator.Services;

/// <summary>Phần phụ thuộc WPF nằm ở service, không nằm trong MainViewModel.</summary>
public sealed class GrammarDialogService : IGrammarDialogService
{
    public void ShowGrammar()
    {
        var dialog = new GrammarDialog();
        if (Application.Current?.MainWindow is Window owner && owner != dialog)
            dialog.Owner = owner;
        dialog.ShowDialog();
    }
}
