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
using System.Drawing;
using Brushes = System.Windows.Media.Brushes;

namespace PatPort
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DataClasses1DataContext dataClasses;
        public List<string> states = new List<string>
        {
            ("AL"),
            ("AK"),
            ("AZ"),
            ("AR"),
            ("CA"),
            ("CO"),
            ("CT"),
            ("DE"),
            ("DC"),
            ("FL"),
            ("GA"),
            ("HI"),
            ("ID"),
            ("IL"),
            ("IN"),
            ("IA"),
            ("KS"),
            ("KY"),
            ("LA"),
            ("ME"),
            ("MD"),
            ("MA"),
            ("MI"),
            ("MN"),
            ("MS"),
            ("MO"),
            ("MT"),
            ("NE"),
            ("NV"),
            ("NH"),
            ("NJ"),
            ("NM"),
            ("NY"),
            ("NC"),
            ("ND"),
            ("OH"),
            ("OK"),
            ("OR"),
            ("PA"),
            ("RI"),
            ("SC"),
            ("SD"),
            ("TN"),
            ("TX"),
            ("UT"),
            ("VT"),
            ("VA"),
            ("WV"),
            ("WI"),
            ("WY"),
        };

        // public database objects for reference from multiple methods. 
        public Patient Pat;
        public Doctor Doc;
        public Appointment App;


        public MainWindow()
        {
            InitializeComponent();

            string connectionstring = ConfigurationManager.ConnectionStrings["PatPort.Properties.Settings.PatientPortalConnectionString"].ConnectionString;
            dataClasses = new DataClasses1DataContext(connectionstring);
            populatePracticeComboBox();
        }




        public void PopulateFields(int Id)
        {
            List<TextBox> patBoxes = new List<TextBox>
            {
                firstNameTextBox,
                lastNameTextBox,
                dobTextBox,
                sexTextBox,
                addressTextBox,
                cityTextBox,
                zipcodeTextBox,
                ssnTextBox,
                insuranceTextBox,
                emailTextBox,
                cellphoneTextBox,
                homephoneTextBox,
                stateTextBox
            };
            List<TextBox> docBoxes = new List<TextBox>
            {
                docFirstNameTextBox,
                docLastNameTextBox,
                docDOBTextBox,
                docSexTextBox,
                docAddressTextBox,
                docCityTextBox,
                docStateTextbox,
                docZipCodeTextbox,
                docCellNumberTextBox,
                docHomeNumberTextBox,
                docSpecialtyTextBox,
                docEmailTextBox,
            };
            if (MainTab.SelectedIndex == 0)
            {
                editPatientButton.IsEnabled = true;

                Patient pat = dataClasses.Patients.First(pati => pati.Id.Equals(Id));
                Doctor doc = dataClasses.Doctors.First(doct => doct.Id.Equals(pat.Primary_Doctor));
                Practice prac = dataClasses.Practices.First(pra => pra.Id.Equals(doc.Practice));
                Appointment app = null;

                try
                {
                    app = dataClasses.Appointments.First(apt => apt.Patient_Id.Equals(pat.Id));
                }
                catch
                {
                    if (app == null)
                    {
                        Appointment appoint = new Appointment
                        {
                            Date = null,
                            Patient_Id = pat.Id,
                            Doctor_Id = doc.Id,
                            Time = null
                        };
                        app = appoint;
                    }
                }

                
                Pat = pat; Doc = doc; App = app;

                ShowNotes(Id);

                primarydoctorTextBox.DataContext = doc;
                foreach (TextBox textBox in patBoxes)
                {
                    textBox.DataContext = pat;
                }
                nextAppointmentDateTextBox.DataContext = app;
                nextAppointmentTimeTextBox.DataContext = app;
                practiceTextBox.DataContext = prac;
            }
            else if (MainTab.SelectedIndex == 1)
            {
                docAppointmentsButton.IsEnabled = true;
                docPatientsButton.IsEnabled = true;
                editDoctorButton.IsEnabled = true;

                Doctor doctor = dataClasses.Doctors.First(doc => doc.Id.Equals(Id));
                Practice practice = dataClasses.Practices.First(pract => pract.Id.Equals(doctor.Practice));
                Doc = doctor;

                foreach (TextBox box in docBoxes)
                {
                    box.DataContext = doctor;
                }
                docPracticeTextBox.DataContext = practice;
                docNoteListBox.ItemsSource = null;
                docListBox.ItemsSource = null;
            }
        }


        #region Patient tab methods

        public void ShowNotes(int patient_Id)
        {

            IEnumerable<Note> notes = from note in dataClasses.Notes where note.Patient_Id == patient_Id select note;
            List<TextBlock> textBlocks = new List<TextBlock>();
            int count = 0;
            foreach (Note note in notes)

            {
                TextBlock textblock = new TextBlock();
                textblock.Text = note.Content;
                textblock.Width = 745;
                textblock.TextWrapping = TextWrapping.WrapWithOverflow;
                if (count % 2 == 0)
                {
                    textblock.Background = Brushes.White;
                }
                else
                {
                    textblock.Background = Brushes.PapayaWhip;
                }
                textBlocks.Add(textblock);
                count++;
            }
            if (MainTab.SelectedIndex == 0)
            {
                NoteListBox.ItemsSource = textBlocks;
            }
            else
            {
                docNoteListBox.ItemsSource = textBlocks;
            }

        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (patientSearchTextBox.Text != "")
            {
                int patCount = 0;
                IEnumerable<Patient> patients;
                switch (searchbyComboBox.Text)
                {
                    case "Full Name":
                        patients = from patient in dataClasses.Patients
                                   where SqlMethods.Like(patient.Display_Name, $"%{patientSearchTextBox.Text}%")
                                   select patient;

                        break;
                    case "Last Name":
                        patients = from patient in dataClasses.Patients
                                   where SqlMethods.Like(patient.Last_Name, $"%{patientSearchTextBox.Text}%")
                                   select patient;
                        break;
                    case "SSN":
                        patients = from patient in dataClasses.Patients
                                   where patient.SSN == patientSearchTextBox.Text
                                   select patient;
                        break;
                    case "Address":
                        patients = from patient in dataClasses.Patients
                                   where SqlMethods.Like(patient.Address, $"%{patientSearchTextBox.Text}%")
                                   select patient;
                        break;
                    case "Cell Phone":
                        patients = from patient in dataClasses.Patients
                                   where patient.Cell_Number == (patientSearchTextBox.Text)
                                   select patient;
                        break;
                    case "Home Phone":
                        patients = from patient in dataClasses.Patients
                                   where patient.Home_Number == (patientSearchTextBox.Text)
                                   select patient;
                        break;
                    default:
                        patients = from patient in dataClasses.Patients select patient;
                        break;
                }

                foreach (Patient pati in patients)
                {
                    patCount++;
                }
                if (patCount == 0)
                {
                    MessageBox.Show("No patient found please refine search or add new patient");
                }
                else if (patCount == 1)
                {
                    Patient pati = patients.First();
                    PopulateFields(pati.Id);
                }
                else
                {
                    MultiPatientPopup multiPatientPopup = new MultiPatientPopup(patients, this);
                    multiPatientPopup.Show();
                }

            }

        }

        private void AddNewPatientButton_Click(object sender, RoutedEventArgs e)
        {
            Window1 window1 = new Window1(this);
            window1.Show();
        }

        private void EditPatientButton_Click(object sender, RoutedEventArgs e)
        {
            Window1 window = new Window1(Pat, Doc, App, this);
            window.Show();
        }

        private void addNoteButton_Click(object sender, RoutedEventArgs e)
        {
            AddNoteWindow addNoteWindow = new AddNoteWindow(Pat, Doc, this);
            addNoteWindow.Show();
        }

        #endregion

        #region Doctor tab methods

        private void docSearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (docSearchTextBox.Text != "")
            {
                int docCount = 0;
                IEnumerable<Doctor> doctors;
                switch (docSearchComboBox.Text)
                {
                    case "Full Name":
                        doctors = from doctor in dataClasses.Doctors
                                  where SqlMethods.Like(doctor.Display_Name, $"%{docSearchTextBox.Text}%")
                                  select doctor;

                        break;
                    case "Last Name":
                        doctors = from doctor in dataClasses.Doctors
                                  where SqlMethods.Like(doctor.Last_Name, $"%{docSearchTextBox.Text}%")
                                  select doctor;
                        break;

                    case "Address":
                        doctors = from doctor in dataClasses.Doctors
                                  where SqlMethods.Like(doctor.Address, $"%{docSearchTextBox.Text}%")
                                  select doctor;
                        break;
                    case "Cell Phone":
                        doctors = from doctor in dataClasses.Doctors
                                  where doctor.Cell_Number == (docSearchTextBox.Text)
                                  select doctor;
                        break;
                    case "Home Phone":
                        doctors = from doctor in dataClasses.Doctors
                                  where doctor.Phone_Number == (docSearchTextBox.Text)
                                  select doctor;
                        break;
                    default:
                        doctors = from doctor in dataClasses.Doctors select doctor;
                        break;
                }

                foreach (Doctor doct in doctors)
                {
                    docCount++;
                }
                if (docCount == 0)
                {
                    MessageBox.Show("No patient found please refine search or add new patient");
                }
                else if (docCount == 1)
                {
                    Doctor doc = doctors.First();
                    PopulateFields(doc.Id);

                }
                else
                {
                    MultiPatientPopup multiPatientPopup = new MultiPatientPopup(doctors, this);
                    multiPatientPopup.Show();

                }
            }
        }

        private void AppointmentsButton_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<Appointment> appointments = from appointment in dataClasses.Appointments where appointment.Doctor_Id == Doc.Id orderby appointment.Date, appointment.Time select appointment;
            List<TextBlock> textBlocks = new List<TextBlock>();

            int count = 0;
            foreach (Appointment app in appointments)
            {
                Patient patient = dataClasses.Patients.First(pati => pati.Id.Equals(app.Patient_Id));

                string input = string.Format("{0}\t{1:d}\t{2}", patient.Display_Name.PadRight(25, ' '), app.Date, app.Time);

                TextBlock block = new TextBlock();
                block.Text = input;
                block.Width = 325;
                block.DataContext = app.Patient_Id;
                if (count % 2 == 0)
                {
                    block.Background = Brushes.White;
                }
                else
                {
                    block.Background = Brushes.PapayaWhip;
                }

                textBlocks.Add(block);
                count++;

            }
            docListBox.ItemsSource = textBlocks;
            docListBox.SelectedValuePath = "Patient_Id";

        }

        private void docAddNoteButton_Click(object sender, RoutedEventArgs e)
        {
            TextBlock block = new TextBlock();
            block = (TextBlock)docListBox.SelectedItem;
            Patient patient = dataClasses.Patients.First(pat => pat.Id.Equals((int)block.DataContext));
            AddNoteWindow addNoteWindow = new AddNoteWindow(patient, Doc, this);
            addNoteWindow.Show();

        }

        private void docListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (docListBox.SelectedIndex >= 0)
            {
                docAddNoteButton.IsEnabled = true;
                TextBlock block = new TextBlock();
                block = (TextBlock)docListBox.SelectedItem;
                Note note = dataClasses.Notes.First(not => not.Patient_Id.Equals((int)block.DataContext));
                ShowNotes(note.Patient_Id);
            }
        }

        private void docPatientsButton_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<Patient> patients = from patient in dataClasses.Patients where patient.Primary_Doctor == Doc.Id select patient;
            List<TextBlock> textBlocks = new List<TextBlock>();
            int count = 0;
            foreach (Patient pat in patients)
            {
                TextBlock block = new TextBlock();
                block.Text = pat.Display_Name;
                block.Width = 325;
                block.DataContext = pat.Id;
                if (count % 2 == 0)
                {
                    block.Background = Brushes.White;
                }
                else
                {
                    block.Background = Brushes.PapayaWhip;
                }

                textBlocks.Add(block);
                count++;
            }
            docListBox.ItemsSource = textBlocks;
            docListBox.SelectedValuePath = "Id";
        }

        private void AddDoctorButton_Click(object sender, RoutedEventArgs e)
        {
            AddDoctorWindow addDoctorWindow = new AddDoctorWindow();
            addDoctorWindow.Show();
        }

        private void editDoctorButton_Click(object sender, RoutedEventArgs e)
        {
            AddDoctorWindow addDoctorWindow = new AddDoctorWindow(Doc);
            addDoctorWindow.Show();
        }
        #endregion

        #region Practice Tabe Methods

        private void populatePracticeComboBox()
        {
            IEnumerable<Practice> practices = from practice in dataClasses.Practices select practice;
            pracTabPracticeComboBox.ItemsSource = practices;
            pracTabPracticeComboBox.DisplayMemberPath = "Name";
            pracTabPracticeComboBox.SelectedValuePath = "Id";
        }

        private void pracTabPracticeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int ID = (int)pracTabPracticeComboBox.SelectedValue;

            IEnumerable<Doctor> doctors = from doctor in dataClasses.Doctors where doctor.Practice == ID select doctor;
            List<TextBlock> docBlocks = new List<TextBlock>();
            List<TextBlock> patBlocks = new List<TextBlock>();

            int count = 0; 
            int count1 = 0;
            foreach (Doctor doctor in doctors)
            {
                IEnumerable<Patient> patients = from patient in dataClasses.Patients where patient.Primary_Doctor == doctor.Id select patient;
                foreach (Patient patient in patients)
                {
                    TextBlock block1 = new TextBlock();
                    string input1 = string.Format("{0}\t{1:d}\t{2}", patient.Display_Name.PadRight(25, ' '), patient.Age, patient.Cell_Number);
                    block1.Text = input1;
                    block1.DataContext = patient.Id;
                    if (count % 2 == 0)
                    {
                        block1.Background = Brushes.White;
                    }
                    else
                    {
                        block1.Background = Brushes.PapayaWhip;
                    }
                    patBlocks.Add(block1);
                    count++;

                }
                TextBlock block = new TextBlock();
                string input = string.Format("{0}\t{1:d}\t{2}", doctor.Display_Name.PadRight(25, ' '), doctor.DOB, doctor.Cell_Number);
                block.Text = input;
                block.DataContext = doctor.Id;
                if (count1 % 2 == 0)
                {
                    block.Background = Brushes.White;
                }
                else
                {
                    block.Background = Brushes.PapayaWhip;
                }
                docBlocks.Add(block);
                count1++;
            }
            pracTabDocListBox.ItemsSource = docBlocks;
            pracTabPatientListBox.ItemsSource = patBlocks;

        }

        private void loadDoctorButton_Click(object sender, RoutedEventArgs e)
        {
            TextBlock block = new TextBlock();
            block = (TextBlock)pracTabDocListBox.SelectedItem;
            MainTab.SelectedIndex = 1;
            PopulateFields((int)block.DataContext);

        }

        private void LoadPatientButton_Click(object sender, RoutedEventArgs e)
        {
            TextBlock block = new TextBlock();
            block = (TextBlock)pracTabPatientListBox.SelectedItem;
            MainTab.SelectedIndex = 0;
            PopulateFields((int)block.DataContext);
        }

        private void addPracticeButton_Click(object sender, RoutedEventArgs e)
        {
            practiceNameEntryTextBox.Visibility = Visibility.Visible;
            savePracticeButton.Visibility = Visibility.Visible;
        }

        private void savePracticeButton_Click(object sender, RoutedEventArgs e)
        {
            IEnumerable<Practice> practices = from practice in dataClasses.Practices select practice;
            bool alreadyExists = false;
            if (practiceNameEntryTextBox.Text != "")
            {
                foreach (Practice prac in practices)
                {
                    if (practiceNameEntryTextBox.Text == prac.Name)
                    {
                        alreadyExists = true;
                    }
                }
                if (alreadyExists == false)
                {
                    Practice practice = new Practice
                    {
                        Name = practiceNameEntryTextBox.Text
                    };
                    dataClasses.Practices.InsertOnSubmit(practice);
                    try
                    {
                        dataClasses.SubmitChanges();
                        MessageBox.Show("Practice Added");
                        practiceNameEntryTextBox.Visibility = Visibility.Hidden;
                        savePracticeButton.Visibility = Visibility.Hidden;
                        populatePracticeComboBox();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
                else
                {
                    MessageBox.Show("Practice already exists");
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid practice name");
            }
        }

        private void pracTabDocListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            loadDoctorButton.IsEnabled = true;
        }

        private void pracTabPatientListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadPatientButton.IsEnabled = true;
        }

        #endregion
    }
}