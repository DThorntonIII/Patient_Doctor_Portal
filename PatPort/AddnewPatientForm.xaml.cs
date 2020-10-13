using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {

        DataClasses1DataContext dataClasses;
        string[] appointments = new string[]
        {
            "9:00 AM",
            "9:30 AM",
            "10:00 AM",
            "10:30 AM",
            "11:00 AM",
            "11:30 AM",
            "12:00 PM",
            "2:00 PM",
            "2:30 PM",
            "3:00 PM",
            "3:30 PM",
            "4:00 PM",
            "4:30 PM"
        };

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

        List<TextBox> patientTextBoxes = new List<TextBox>();
        bool isPatientEdit = false;

        int editPatientID;
        MainWindow mainWindow;



        public Window1(MainWindow mainWindowPass)
        {
            InitializeComponent();

            string connectionstring = ConfigurationManager.ConnectionStrings["PatPort.Properties.Settings.PatientPortalConnectionString"].ConnectionString;
            dataClasses = new DataClasses1DataContext(connectionstring);
            mainWindow = mainWindowPass;

            InitTextboxes(patientTextBoxes);
        }

        public Window1(Patient pat, Doctor doc, Appointment app, MainWindow mainWindowPass)
        {
            InitializeComponent();
            string connectionstring = ConfigurationManager.ConnectionStrings["PatPort.Properties.Settings.PatientPortalConnectionString"].ConnectionString;
            dataClasses = new DataClasses1DataContext(connectionstring);

            IEnumerable<Doctor> doctors = from doctor in dataClasses.Doctors select doctor;
            isPatientEdit = true;
            mainWindow = mainWindowPass;
            addPatientButton.Content = "Update Patient";

            InitTextboxes(patientTextBoxes);

            foreach (TextBox box in patientTextBoxes)
            {
                box.DataContext = pat;
            }

            //set the state combo box to match patient's state
            for (int i = 0; i < states.Count; i++)
            {
                if (pat.State == states[i])
                {
                    stateComboBox.SelectedIndex = i;
                    break;
                }
            }
            
            //set doctor combo box to match the patients primary doctor
            int count = 0;
            foreach (Doctor doct in doctors)
            {
                if (doct.Display_Name == doc.Display_Name)
                {
                    primaryDoctorComboBox.SelectedIndex = count;
                    break;
                }
                count++;
            }

            //set appointment time combo to match patients next appointment time
            nextAppointmentTextBox.DataContext = app;
            for (int j = 0; j < appointments.Length; j++)
            {
                if (app.Time == appointments[j])
                {
                    appointmentTimeComboBox.SelectedIndex = j;
                    break;
                }
            }
            editPatientID = pat.Id;
        }



        //creates and adds a new patient with input from textboxes in window1 to the patient table. Also calls the AddAppointments method to submit the new appointment 
        private void addPatientButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPatientEdit == false) 
            {
                if (areBoxesFilled(patientTextBoxes) == true)
                {
                    Doctor doctor = dataClasses.Doctors.First(doc => doc.Id.Equals(primaryDoctorComboBox.SelectedValue));
                    Patient patient = new Patient
                    {
                        First_Name = firstNameTextBox.Text,
                        Last_Name = lastNameTextBox.Text,
                        Display_Name = $"{firstNameTextBox.Text} {lastNameTextBox.Text}",
                        Primary_Doctor = doctor.Id,
                        Age = Convert.ToDateTime(DOBTextBox.Text),
                        Sex = sexTextBox.Text,
                        Address = addressTextBox.Text,
                        City = cityTextBox.Text,
                        State = stateComboBox.Text,
                        Zipcode = Convert.ToInt32(zipcodeTextBox.Text),
                        SSN = SSNTextBox.Text,
                        Insurance = insuranceTextBox.Text,
                        Cell_Number = cellNumberTextBox.Text,
                        Home_Number = HomeNumberTextBox.Text,
                        Email = emailTextBox.Text
                    };
                    dataClasses.Patients.InsertOnSubmit(patient);

                    try
                    {
                        dataClasses.SubmitChanges(); // adds patient to database
                        AddNote(patient, doctor); // adds note to db but requires patient be submitted first
                        AddAppointment(patient, doctor);
                        dataClasses.SubmitChanges();
                        MessageBox.Show($"{patient.Display_Name} has been added to database.\nA new appointment has been scheduled.\n{nextAppointmentTextBox.Text} at {appointmentTimeComboBox.Text} with {doctor.Display_Name}");
                        mainWindow.PopulateFields(patient.Id);
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
            else // patientEdit is true
            {
                if (areBoxesFilled(patientTextBoxes) == true)
                {
                    Doctor doctor = dataClasses.Doctors.First(doc => doc.Id.Equals(primaryDoctorComboBox.SelectedValue));
                    Patient patient = dataClasses.Patients.First(pat => pat.Id.Equals(editPatientID));
                    patient.First_Name = firstNameTextBox.Text;
                    patient.Last_Name = lastNameTextBox.Text;
                    patient.Display_Name = ($"{firstNameTextBox.Text} {lastNameTextBox.Text}");
                    patient.Primary_Doctor = doctor.Id;
                    patient.Age = Convert.ToDateTime(DOBTextBox.Text);
                    patient.Address = addressTextBox.Text;
                    patient.City = cityTextBox.Text;
                    patient.State = stateComboBox.Text;
                    patient.Zipcode = Convert.ToInt32(zipcodeTextBox.Text);
                    patient.Insurance = insuranceTextBox.Text;
                    patient.Email = emailTextBox.Text;
                    patient.Cell_Number = cellNumberTextBox.Text;
                    patient.Home_Number = HomeNumberTextBox.Text;
                    AddNote(patient, doctor);
                    AddAppointment(patient, doctor);
                    try
                    {
                        dataClasses.SubmitChanges();
                        MessageBox.Show("Changes submitted");
                        mainWindow.PopulateFields(patient.Id);
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
            }
        }


        //sets the textbox to reflect what the calandar selection is
        private void calandar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
           DateTime appointment = (DateTime)calandar.SelectedDate;
           nextAppointmentTextBox.Text = appointment.ToShortDateString();
        }


        //creates and adds a new appointment to the appointment table
        private void AddAppointment(Patient patient, Doctor doctor)
        {
            if (isPatientEdit == false)
            {
                Appointment appoint = new Appointment
                {
                    Date = calandar.SelectedDate,
                    Time = appointmentTimeComboBox.Text,
                    Patient_Id = patient.Id,
                    Doctor_Id = doctor.Id
                };
                dataClasses.Appointments.InsertOnSubmit(appoint);
            }
            else
            {
                var deleteapp = from appointment in dataClasses.Appointments where appointment.Patient_Id == patient.Id select appointment;
                if (deleteapp != null)
                {
                    foreach (var app in deleteapp)
                    {
                        dataClasses.Appointments.DeleteOnSubmit(app);
                    }
                    try
                    {
                        dataClasses.SubmitChanges();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                    Appointment appoint = new Appointment
                    {
                        Date = calandar.SelectedDate,
                        Time = appointmentTimeComboBox.Text,
                        Patient_Id = patient.Id,
                        Doctor_Id = doctor.Id
                    };
                    dataClasses.Appointments.InsertOnSubmit(appoint);
                }
                else
                {
                    Appointment appoint = new Appointment
                    {
                        Date = calandar.SelectedDate,
                        Time = appointmentTimeComboBox.Text,
                        Patient_Id = patient.Id,
                        Doctor_Id = doctor.Id
                    };
                    dataClasses.Appointments.InsertOnSubmit(appoint);
                }
            }
        }


        // determines if an appointment is already set for the doctor at the selected time and date
        private void ShowAppointments(string[] appointments)
        {
            try
            {
                int numOfAppointments = 0;
                Doctor doctor = dataClasses.Doctors.First(doc => doc.Display_Name.Equals(primaryDoctorComboBox.Text));
                IEnumerable<Appointment> apps = from appoints in dataClasses.Appointments where (appoints.Doctor_Id == doctor.Id && appoints.Date == Convert.ToDateTime(nextAppointmentTextBox.Text))select appoints;

                string[] placeholder = new string[]
                {
                    "9:00 AM",
                    "9:30 AM",
                    "10:00 AM",
                    "10:30 AM",
                    "11:00 AM",
                    "11:30 AM",
                    "12:00 PM",
                    "2:00 PM",
                    "2:30 PM",
                    "3:00 PM",
                    "3:30 PM",
                    "4:00 PM",
                    "4:30PM"
                };
                //loops through appointments to check if the appointment for the slected doctor is booked
                foreach (Appointment appointment in apps)
                {
                    numOfAppointments += 1;
                    for (int i = 0; i < appointments.Length; i++)
                    {
                        if (appointment.Time == appointments[i])
                        {
                            appointments[i] = "**BOOKED**";
                        }
                        else if (appointment.Date != Convert.ToDateTime(nextAppointmentTextBox.Text))
                        {
                            appointments[i] = placeholder[i];
                        }
                    }
                }
                if (numOfAppointments == 0)
                {
                    appointments = placeholder;
                }
                appointmentTimeComboBox.ItemsSource = appointments;
            }
            catch (Exception)
            { }
        }


        //update appointments
        private void nextAppointmentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ShowAppointments(appointments);
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddNote (Patient patient, Doctor doctor)
        {
            if (noteTextBox.Text != "")
            {
                Note note = new Note
                {
                    Content = noteTextBox.Text,
                    Patient_Id = patient.Id,
                    Doctor_Id = doctor.Id
                };
                dataClasses.Notes.InsertOnSubmit(note);
            }

        }

        

        private bool areBoxesFilled(List<TextBox> textBoxes)
        {
            bool allFilled = true;
            foreach (TextBox box in textBoxes)
            {
                if (box.Text == "")
                {
                    box.Background = Brushes.Red;
                    allFilled = false;
                }
                else
                {
                    box.Background = Brushes.White;
                }
            }
            if (nextAppointmentTextBox.Text == "")
            {
                nextAppointmentTextBox.Background = Brushes.Red;
                allFilled = false;
            }


            if (stateComboBox.SelectedIndex < 0)
            {
                allFilled = false;
            }
            if (primaryDoctorComboBox.SelectedIndex < 0)
            {
                allFilled = false;
            }
            if(appointmentTimeComboBox.SelectedIndex < 0)
            {
                allFilled = false;            }


            if (allFilled == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private void InitTextboxes(List<TextBox> textBoxes)
        {
            textBoxes.Add(firstNameTextBox);
            textBoxes.Add(lastNameTextBox);
            textBoxes.Add(sexTextBox);
            textBoxes.Add(SSNTextBox);
            textBoxes.Add(DOBTextBox);
            textBoxes.Add(insuranceTextBox);
            textBoxes.Add(addressTextBox);
            textBoxes.Add(cityTextBox);
            textBoxes.Add(zipcodeTextBox);
            textBoxes.Add(cellNumberTextBox);
            textBoxes.Add(HomeNumberTextBox);
            textBoxes.Add(emailTextBox);

            primaryDoctorComboBox.ItemsSource = dataClasses.Doctors;
            primaryDoctorComboBox.DisplayMemberPath = "Display_Name";
            primaryDoctorComboBox.SelectedValuePath = "Id";

            appointmentTimeComboBox.ItemsSource = appointments;
            stateComboBox.ItemsSource = states;
        }


        #region Lost Focus Methods
        private void nextAppointmentTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            nextAppointmentTextBox.Text = FormatNumbers(nextAppointmentTextBox.Text);
        }
        private void cellNumberTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            cellNumberTextBox.Text = FormatNumbers(cellNumberTextBox.Text);   
        }

        private void HomeNumberTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            HomeNumberTextBox.Text = FormatNumbers(HomeNumberTextBox.Text);
        }

        private void SSNTextBox_LostFocus(object sender, RoutedEventArgs e)
        { 
            SSNTextBox.Text = FormatNumbers(SSNTextBox.Text);
        }

        private void DOBTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
             DOBTextBox.Text = FormatNumbers(DOBTextBox.Text);
        }

        private void zipcodeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            zipcodeTextBox.Text = FormatNumbers(zipcodeTextBox.Text);
        }

        private string FormatNumbers(string number)
        {
            if (number != "")
            {
                if (number == cellNumberTextBox.Text || number == HomeNumberTextBox.Text)
                {
                    string value = Regex.Replace(number, "[A-Za-z-().,/ ]", "");
                    if (value.Length != 10)
                    {
                        MessageBox.Show($"{number} is not a valid phone number. Please try again");
                        value = "";
                        return value;

                    }
                    else
                    {
                        long phoneNum = Convert.ToInt64(value);
                        value = string.Format("{0:(###) ###-####}", phoneNum);
                        return value;
                    }
                }
                else if (number == SSNTextBox.Text)
                {
                    string value = Regex.Replace(number, "[A-Za-z-().,/]", "");
                    if (value.Length != 9)
                    {
                        MessageBox.Show($"{number} is not a valid social security number. Please try again");
                        value = "";
                        return value;
                    }
                    else
                    {
                        long phoneNum = Convert.ToInt64(value);
                        value = string.Format("{0:###-##-####}", phoneNum);
                        return value;
                    }
                }
                else if (number == zipcodeTextBox.Text)
                {
                    if (number.Length != 5)
                    {
                        MessageBox.Show($"{number} is not a valid social security number. Please try again");
                        number = "";
                        return number;
                    }
                    return number;
                }
                else if (number == DOBTextBox.Text)
                {
                    string value = Regex.Replace(number, "[A-Za-z-().,/]", "");
                    if (value.Length != 8)
                    {
                        MessageBox.Show($"{number} is not a valid birthdate. Please try again");
                        value = "";
                        return value;
                    }
                    else
                    {
                        long phoneNum = Convert.ToInt64(value);
                        value = string.Format("{0:##/##/####}", phoneNum);
                        return value;
                    }
                }
                else if (number == nextAppointmentTextBox.Text)
                {
                    string value = Regex.Replace(number, "[A-Za-z-().,/]", "");
                    if (value.Length != 8)
                    {
                        MessageBox.Show($"{number} is not a valid date. Please try again");
                        value = "";
                        return value;
                    }
                    else
                    {
                        long phoneNum = Convert.ToInt64(value);
                        value = string.Format("{0:##/##/####}", phoneNum);
                        calandar.SelectedDate = Convert.ToDateTime(nextAppointmentTextBox.Text);
                        return value;
                    }
                }
                else
                {
                    return number;
                }
            }
            else
            {
                return number;
            }
        }

        #endregion



    }
}
