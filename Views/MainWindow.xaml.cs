using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ExpressionEvaluator.Models;
using ExpressionEvaluator.ViewModels;

namespace ExpressionEvaluator.Views;

public partial class MainWindow : Window
{
    private void ExpressionInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || DataContext is not MainViewModel viewModel) return;

        // Cùng command với nút Tính toán, không thực hiện logic phân tích trong view.
        if (viewModel.CalculateCommand.CanExecute(null))
            viewModel.CalculateCommand.Execute(null);
        e.Handled = true;
    }

    public MainWindow()
    {
        InitializeComponent();
        var viewModel = new MainViewModel();
        DataContext = viewModel;
        viewModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != nameof(MainViewModel.CurrentAnalysis)) return;

            // Binding/TextBox cập nhật xong trước khi chọn token lỗi; không chọn lại khi đang gõ.
            ExpressionAnalysisResult? analysis = viewModel.CurrentAnalysis;
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                if (!ReferenceEquals(viewModel.CurrentAnalysis, analysis)
                    || viewModel.IsResultOutdated || analysis?.Error?.Span is not SourceSpan span)
                    return;

                ExpressionInput.Focus();
                int start = Math.Min(span.StartIndex, ExpressionInput.Text.Length);
                if (span.Length == 0)
                    ExpressionInput.CaretIndex = ExpressionInput.Text.Length;
                else
                    ExpressionInput.Select(start, Math.Min(span.Length, ExpressionInput.Text.Length - start));
            }));
        };
    }
}
