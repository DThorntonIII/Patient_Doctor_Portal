using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Data.Linq.SqlClient;

namespace PatPort
{
    /// <summary>
    /// Interaction logic for MultiPatientPopup.xaml
    /// </summary>
    public partial class MultiPatientPopup : Window
    {
        MainWindow mainWindow1;

        DataClasses1DataContext dataClasses;

        public MultiPatientPopup()
        {
            InitializeComponent();
            string connectionstring = ConfigurationManager.ConnectionStrings["PatPort.Properties.Settings.PatientPortalConnectionString"].ConnectionString;
            dataClasses = new DataClasses1DataContext(connectionstring);

        }

        public MultiPatientPopup(IEnumerable<Patient> patients, MainWindow mainWindow)
        {
            InitializeComponent();
            List<TextBlock> patientTexts = new List<TextBlock>();
            int count = 0;
            foreach (Patient patient in patients)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = ($"{patient.Display_Name} - {patient.SSN} - {patient.Address}, {patient.City}, {patient.State}{patient.Zipcode}");
                textBlock.DataContext = patient.Id;
                count++;
                textBlock.Width = PatientBox.Width;
                if (count % 2 == 0)
                {
                    textBlock.Background = Brushes.White;
                }
                else
                {
                    textBlock.Background = Brushes.PapayaWhip;
                }
                patientTexts.Add(textBlock);

            }
            PatientBox.ItemsSource = patientTexts;
            PatientBox.SelectedValuePath = "Id";
            mainWindow1 = mainWindow;
        }
        public MultiPatientPopup(IEnumerable<Doctor> doctors, MainWindow mainWindow)
        {
            InitializeComponent();
            List<TextBlock> doctextblocks = new List<TextBlock>();
            int count = 0;
            foreach (Doctor doctor in doctors)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = ($"{doctor.Display_Name} - {doctor.Address}, {doctor.City}, {doctor.State} {doctor.Zipcode}");
                textBlock.DataContext = doctor.Id;
                count++;
                textBlock.Width = PatientBox.Width;
                if (count % 2 == 0)
                {
                    textBlock.Background = Brushes.White;
                }
                else
                {
                    textBlock.Background = Brushes.PapayaWhip;
                }
                doctextblocks.Add(textBlock);

            }
            PatientBox.ItemsSource = doctextblocks;
            PatientBox.SelectedValuePath = "Id";
            mainWindow1 = mainWindow;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TextBlock textBlock = new TextBlock();
            textBlock = (TextBlock)PatientBox.SelectedItem;
            mainWindow1.PopulateFields((int)textBlock.DataContext);
            this.Close();
        }



        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
