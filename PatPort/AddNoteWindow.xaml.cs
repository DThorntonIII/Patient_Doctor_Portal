using System;
using System.Collections.Generic;
using System.Configuration;
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
using System.Windows.Shapes;

namespace PatPort
{
    /// <summary>
    /// Interaction logic for AddNoteWindow.xaml
    /// </summary>
    public partial class AddNoteWindow : Window
    {
        DataClasses1DataContext dataClasses;
        public Patient pat;
        public Doctor doc;
        public MainWindow mw;

        public AddNoteWindow(Patient patient, Doctor doctor, MainWindow mainWindow)
        {
            InitializeComponent();
            string connectionstring = ConfigurationManager.ConnectionStrings["PatPort.Properties.Settings.PatientPortalConnectionString"].ConnectionString;
            dataClasses = new DataClasses1DataContext(connectionstring);
            pat = patient;
            doc = doctor;
            mw = mainWindow;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (noteTextBox.Text != "")
            {
                Note note = new Note
                {
                    Content = noteTextBox.Text,
                    Patient_Id = pat.Id,
                    Doctor_Id = doc.Id
                };
                dataClasses.Notes.InsertOnSubmit(note);
                try
                {
                    dataClasses.SubmitChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    mw.ShowNotes(pat.Id);
                    this.Close();
                }
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
