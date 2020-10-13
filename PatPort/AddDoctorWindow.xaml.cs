using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
    /// Interaction logic for AddDoctorWindow.xaml
    /// </summary>
    public partial class AddDoctorWindow : Window
    {
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
        DataClasses1DataContext dataClasses;
        public List<TextBox> textBoxes = new List<TextBox>();
        private bool isEdit = false;
        Doctor doc;

        public AddDoctorWindow()
        {
            InitializeComponent();
            string connectionstring = ConfigurationManager.ConnectionStrings["PatPort.Properties.Settings.PatientPortalConnectionString"].ConnectionString;
            dataClasses = new DataClasses1DataContext(connectionstring);

            textBoxes.Add(firstNameTextBox);
            textBoxes.Add(lastNameTextBox);
            textBoxes.Add(DOBTextBox);
            textBoxes.Add(sexTextBox);
            textBoxes.Add(specialtyTextBox);
            textBoxes.Add(addressTextBox);
            textBoxes.Add(cityTextBox);
            textBoxes.Add(zipcodeTextBox);
            textBoxes.Add(emailTextBox);
            textBoxes.Add(cellPhoneTextBox);
            textBoxes.Add(homePhoneTextBox);

            IEnumerable<Practice> practices = from practice in dataClasses.Practices select practice;
            practiceComboBox.ItemsSource = practices;
            practiceComboBox.DisplayMemberPath = "Name";
            practiceComboBox.SelectedValuePath = "Id";

            stateComboBox.ItemsSource = states;
        }

        public AddDoctorWindow(Doctor doctor)
        {
            InitializeComponent();
            string connectionstring = ConfigurationManager.ConnectionStrings["PatPort.Properties.Settings.PatientPortalConnectionString"].ConnectionString;
            dataClasses = new DataClasses1DataContext(connectionstring);

            addDoctorButton.Content = "Edit Doctor";
            isEdit = true;

            doc = doctor;

            textBoxes.Add(firstNameTextBox);
            textBoxes.Add(lastNameTextBox);
            textBoxes.Add(DOBTextBox);
            textBoxes.Add(sexTextBox);
            textBoxes.Add(specialtyTextBox);
            textBoxes.Add(addressTextBox);
            textBoxes.Add(cityTextBox);
            textBoxes.Add(zipcodeTextBox);
            textBoxes.Add(emailTextBox);
            textBoxes.Add(cellPhoneTextBox);
            textBoxes.Add(homePhoneTextBox);

            IEnumerable<Practice> practices = from practice in dataClasses.Practices select practice;
            practiceComboBox.ItemsSource = practices;
            practiceComboBox.DisplayMemberPath = "Name";
            practiceComboBox.SelectedValuePath = "Id";

            stateComboBox.ItemsSource = states;
            foreach (TextBox box in textBoxes)
            {
                box.DataContext = doctor;
            }

            for (int i = 0; i < states.Count; i++)
            {
                if (states[i] == doctor.State)
                {
                    stateComboBox.SelectedIndex = i;
                }    
            }
            int count = 0;
            foreach (Practice prac in practices)
            {
                if (prac.Id == doctor.Practice)
                {
                    practiceComboBox.SelectedIndex = count;
                }
                count++;
            }
        }

        private void AddDoctorButton_Click(object sender, RoutedEventArgs e)
        {
            if (CheckIfFilled(textBoxes) == false)
            {
                MessageBox.Show("Please filled the required information");
            }
            else
            {
                if (isEdit == false)
                {
                    Doctor doctor = new Doctor
                    {
                        First_Name = firstNameTextBox.Text,
                        Last_Name = lastNameTextBox.Text,
                        Display_Name = ($"{firstNameTextBox.Text} {lastNameTextBox.Text}"),
                        Practice = (int)practiceComboBox.SelectedValue,
                        DOB = Convert.ToDateTime(DOBTextBox.Text),
                        Sex = sexTextBox.Text,
                        Address = addressTextBox.Text,
                        City = cityTextBox.Text,
                        State = stateComboBox.Text,
                        Zipcode = Convert.ToInt32(zipcodeTextBox.Text),
                        Cell_Number = cellPhoneTextBox.Text,
                        Phone_Number = homePhoneTextBox.Text,
                        Specialty = specialtyTextBox.Text,
                        Email = emailTextBox.Text
                    };

                    dataClasses.Doctors.InsertOnSubmit(doctor);
                    
                }
                else
                {
                    doc.First_Name = firstNameTextBox.Text;
                    doc.Last_Name = lastNameTextBox.Text;
                    doc.Display_Name = ($"{firstNameTextBox.Text} {lastNameTextBox.Text}");
                    doc.Practice = (int)practiceComboBox.SelectedValue;
                    doc.DOB = Convert.ToDateTime(DOBTextBox.Text);
                    doc.Sex = sexTextBox.Text;
                    doc.Address = addressTextBox.Text;
                    doc.City = cityTextBox.Text;
                    doc.State = stateComboBox.Text;
                    doc.Zipcode = Convert.ToInt32(zipcodeTextBox.Text);
                    doc.Cell_Number = cellPhoneTextBox.Text;
                    doc.Phone_Number = homePhoneTextBox.Text;
                    doc.Specialty = specialtyTextBox.Text;
                    doc.Email = emailTextBox.Text;
                }
                try
                {
                    dataClasses.SubmitChanges();
                    if (isEdit == false)
                    {
                        MessageBox.Show("Doctor has been added");
                    }
                    else MessageBox.Show("Changes have been submitted");
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool CheckIfFilled(List<TextBox> textBoxes)
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
            if (allFilled == true)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private string FormatNumbers(string number)
        {
            if (number != "")
            {
                if (number == cellPhoneTextBox.Text || number == homePhoneTextBox.Text)
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
                else if (number == zipcodeTextBox.Text)
                {
                    if (number.Length != 5)
                    {
                        MessageBox.Show($"{number} is not a valid zipcode. Please try again");
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
                else
                {
                    return number;
                }
            }
            return number;
        }

        private void DOBTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
           DOBTextBox.Text = FormatNumbers(DOBTextBox.Text);
        }

        private void cellPhoneTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            cellPhoneTextBox.Text = FormatNumbers(cellPhoneTextBox.Text);
        }

        private void homePhoneTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            homePhoneTextBox.Text = FormatNumbers(homePhoneTextBox.Text);
        }
    }
}
