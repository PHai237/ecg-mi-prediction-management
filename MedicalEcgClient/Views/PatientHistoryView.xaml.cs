using MedicalEcgClient.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace MedicalEcgClient.Views
{
    public partial class PatientHistoryView : UserControl
    {
        public PatientHistoryView()
        {
            InitializeComponent();
        }

        private void OnHistoryItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is PatientHistoryViewModel vm)
            {
                if (vm.ViewCaseDetailCommand.CanExecute(null))
                {
                    vm.ViewCaseDetailCommand.Execute(null);
                }
            }
        }
    }
}