using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace Claudio
{
    /// <summary>
    /// Логика взаимодействия для Login.xaml
    /// </summary>

    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }
        RegistrationForm registration = new RegistrationForm();
        Welcome welcome = new Welcome();

        const string DB_SERVER = "DESKTOP-KB8RPIF";
        const string DB_NAME = "Claudio";


        private void closeLoginWindow(object sender, RoutedEventArgs e)
        {
            Close();

        }


        private void borderClick(object sender, MouseButtonEventArgs e)
        {

            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();

            }
        }

        private void button1_Click(object sender, RoutedEventArgs e)
            {


            if (textBoxEmail.Text.Length == 0)
            {
                errormessage.Text = "Enter an email.";
                textBoxEmail.Focus();
            }
            else if (!Regex.IsMatch(textBoxEmail.Text, @"^[a-zA-Z][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"))
            {
                errormessage.Text = "Enter a valid email.";
                textBoxEmail.Select(0, textBoxEmail.Text.Length);
                textBoxEmail.Focus();
            }
            else
            {
                string email = textBoxEmail.Text;
                string password = passwordBox1.Password;
                try
                {
                    SqlConnection con = new SqlConnection("Server=" + DB_SERVER + ";Database=" + DB_NAME + ";Integrated Security=SSPI");
                    con.Open();
                    SqlCommand cmd = new SqlCommand("Select * from Registration where email ='" + email + "'  and password ='" + password + "'", con);
                    cmd.CommandType = CommandType.Text;
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = cmd;
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet);

                    if (dataSet.Tables[0].Rows.Count > 0)
                    {
                        string login = dataSet.Tables[0].Rows[0]["login"].ToString();
                        welcome.TextBlockName.Text = login;//Sending value from one form to another form.
                        welcome.Show();
                        Close();
                    }
                    else
                    {
                        errormessage.Text = "Sorry! Please enter existing emailid/password.";
                    }
                    con.Close();

                } catch (SqlException sex) { Console.WriteLine(sex); }
                {
                    return;
                }
            }

        }

            private void buttonRegister_Click(object sender, RoutedEventArgs e)
            {
                registration.Show();
                Close();
            }

        }
    }

