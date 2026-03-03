using System.Windows;

namespace LogAnalyzer.Views
{
    public partial class AgreementWindow : Window
    {
        public AgreementWindow()
        {
            InitializeComponent();
        }

        private void ChkAgree_CheckedChanged(object sender, RoutedEventArgs e)
        {
            BtnAccept.IsEnabled = ChkAgree.IsChecked == true;
        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
