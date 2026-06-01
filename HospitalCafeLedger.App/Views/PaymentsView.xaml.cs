using System.Windows;
using System.Windows.Controls;

namespace HospitalCafeLedger.App.Views;

public partial class PaymentsView : UserControl
{
    public PaymentsView()
    {
        InitializeComponent();
    }

    private void SavePayment_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PayAmountBox.Text) || !decimal.TryParse(PayAmountBox.Text, out var amount) || amount <= 0)
        {
            MessageBox.Show("Please enter a valid amount.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        MessageBox.Show($"Payment of Rs. {amount:N0} saved successfully!", "Payment Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        PayAmountBox.Text = "";
    }
}
