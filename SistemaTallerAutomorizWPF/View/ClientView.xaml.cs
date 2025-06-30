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
using SistemaTallerAutomorizWPF.ViewModels;
using SistemaTallerAutomorizWPF.Models;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using ClosedXML.Excel;
using Microsoft.Win32;

namespace SistemaTallerAutomorizWPF.View
{
    /// <summary>
    /// Lógica de interacción para ClientView.xaml
    /// </summary>
    public partial class ClientView : UserControl
    {
        public ClientView()
        {
            InitializeComponent();
                ClientsList = new ObservableCollection<Client>();
                ClientDataGrid.ItemsSource = ClientsList;
                LoadClientsFromDB();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void BuscarTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void BuscarTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (BuscarTextBox.Text == "Buscar...")
            {
                BuscarTextBox.Text = "";
                BuscarTextBox.Foreground = Brushes.Black;
            }
        }

        private void BuscarTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BuscarTextBox.Text))
            {
                BuscarTextBox.Text = "Buscar...";
            }
        }

        private void BuscarTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BuscarTextBox.Text = "Buscar...";
                BuscarTextBox.Foreground = Brushes.Gray;
                Keyboard.ClearFocus();
                BuscarTextBox_GotFocus(sender, e);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        public ObservableCollection<Client> ClientsList { get; set; }

        private void LoadClientsFromDB()
        {
            using (SqlConnection connection = Connections.GetConnection())
            {
                string query = "SELECT NameClient, Email, Vehicle, Orders, Debts FROM Clientes";
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        ClientsList.Add(new Client
                        {
                            NameClient = reader["NameClient"].ToString(),
                            Email = reader["Email"].ToString(),
                            Vehicle = reader["Vehicle"].ToString(),
                            Orders = Convert.ToInt32(reader["Orders"]),
                            Debts = Convert.ToDecimal(reader["Debts"])
                        });
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading data: " + ex.Message);
                }
            }
        }

        private void Button_Click_1()
        {

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                Title = "Exported_Clients.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Clients");

                        //Encabezados
                        worksheet.Cell(1, 1).Value = "Nombre del cliente";
                        worksheet.Cell(1, 2).Value = "Email";
                        worksheet.Cell(1, 3).Value = "Vehículo";
                        worksheet.Cell(1, 4).Value = "Órdenes";
                        worksheet.Cell(1, 5).Value = "Dedudas";

                        //Datos
                        for (int i = 0; i < ClientsList.Count; i++)
                        {
                            var Client = ClientsList[i];
                            worksheet.Cell(i + 2, 1).Value = Client.NameClient;
                            worksheet.Cell(i + 2, 2).Value = Client.Email;
                            worksheet.Cell(i + 2, 3).Value = Client.Vehicle;
                            worksheet.Cell(i + 2, 4).Value = Client.Orders;
                            worksheet.Cell(i + 2, 5).Value = Client.Debts;
                        }

                        workbook.SaveAs(saveFileDialog.FileName);
                        MessageBox.Show("Exportación completada correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al exportar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
