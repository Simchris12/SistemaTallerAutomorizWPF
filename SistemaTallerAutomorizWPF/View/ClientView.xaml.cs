using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClosedXML.Excel;
using Microsoft.Win32;
using SistemaTallerAutomorizWPF.Models;
using SistemaTallerAutomorizWPF.ViewModels;
using System.IO;

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

        //validación del Email
        private bool EmailEsValido(string email)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        public ObservableCollection<Client> ClientsList { get; set; }

        private void LoadClientsFromDB()
        {
            using (SqlConnection connection = Connections.GetConnection())
            {
                string query = "SELECT Id, NameClient, Email, Vehicle, Orders, Debts FROM Clientes";
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        ClientsList.Add(new Client
                        {
                            Id = Convert.ToInt32(reader["Id"]),
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

                        //Autoajustar columnas
                        worksheet.Columns().AdjustToContents();

                        //Fondo de encabezados
                        var headerRange = worksheet.Range("A1:E1");
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;

                        //Estilo de encabezados
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        //Bordes
                        worksheet.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                        //formato de moneda para la columna de deudas
                        worksheet.Column(5).Style.NumberFormat.Format = "$#,##0.00";

                        //Filtros automaticos en los encabezados
                        worksheet.RangeUsed().SetAutoFilter();

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

        private void AgregarCliente_Click(object sender, RoutedEventArgs e)
        {
            Button boton = (Button)sender;
            boton.IsEnabled = false; // Desactiva el botón

            // Para no guardar los placeholders
            if (NombreTextBox.IsPlaceHolderVisible || EmailTextBox.IsPlaceHolderVisible || VehiculoTextBox.IsPlaceHolderVisible)
            {
                MessageBox.Show("Por favor, completa todos los campos obligatorios.");
                boton.IsEnabled = true;
                return;
            }

            // Validación básica
            if (string.IsNullOrWhiteSpace(NombreTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text) ||
                string.IsNullOrWhiteSpace(VehiculoTextBox.Text))
            {
                MessageBox.Show("Nombre, Email y Vehículo son campos obligatorios.");
                boton.IsEnabled = true;
                return;
            }

            // Alerta de validación del formato de Email
            if (!EmailEsValido(EmailTextBox.Text.Trim()))
            {
                MessageBox.Show("El correo electrónico no tiene un formato válido.");
                boton.IsEnabled = true;
                return;
            }

            // Validar Órdenes
            if (!int.TryParse(OrdenesTextBox.IsPlaceHolderVisible ? "0" : OrdenesTextBox.Text, out int orders) || orders < 0)
            {
                MessageBox.Show("El número de órdenes debe ser un número entero positivo.");
                boton.IsEnabled = true;
                return;
            }

            //Validar Deudas
            if (!decimal.TryParse(DeudasTextBox.IsPlaceHolderVisible ? "0" : DeudasTextBox.Text, out decimal debts) || debts < 0)
            {
                MessageBox.Show("El monto de deudas debe ser un número decimal positivo.");
                boton.IsEnabled = true;
                return;
            }

            using (SqlConnection connection = SistemaTallerAutomorizWPF.Models.Connections.GetConnection())
            {
                string insertQuery = "INSERT INTO Clientes (NameClient, Email, Vehicle, Orders, Debts) VALUES (@NameClient, @Email, @Vehicle, @Orders, @Debts)";
                SqlCommand command = new SqlCommand(insertQuery, connection);

                command.Parameters.AddWithValue("@NameClient", NombreTextBox.Text.Trim());
                command.Parameters.AddWithValue("@Email", EmailTextBox.Text.Trim());
                command.Parameters.AddWithValue("@Vehicle", VehiculoTextBox.Text.Trim());
                command.Parameters.AddWithValue("@Orders", orders);
                command.Parameters.AddWithValue("@Debts", debts);

                try
                {
                    connection.Open();
                    //Validar si el correo electrónico ya existe
                    string checkQuery = "SELECT COUNT(*) FROM Clientes WHERE Email = @Email";
                    SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@Email", EmailTextBox.Text.Trim());
                    int count = Convert.ToInt32(checkCommand.ExecuteScalar());

                    if (count > 0)
                    {
                        MessageBox.Show("Ya existe un cliente con ese correo.");
                        boton.IsEnabled = true;
                        return;
                    }

                    command.ExecuteNonQuery();
                    MessageBox.Show("Cliente agregado correctamente.");

                    // Crear un SolidColorBrush mutable (si el botón no lo tiene aún)
                    var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F0F0"));
                    AgregarClienteBtn.Background = brush;

                    // Animación suave al verde
                    var animationToGreen = new ColorAnimation
                    {
                        To = (Color)ColorConverter.ConvertFromString("#9FA324"), // Verde SITAUTO
                        Duration = TimeSpan.FromSeconds(0.5)
                    };
                    brush.BeginAnimation(SolidColorBrush.ColorProperty, animationToGreen);

                    // Desactivar el botón
                    AgregarClienteBtn.Foreground = Brushes.White;
                    AgregarClienteBtn.IsEnabled = false;

                    // Esperar y luego restaurar visualmente
                    Task.Delay(3000).ContinueWith(_ =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var animationToGray = new ColorAnimation
                            {
                                To = (Color)ColorConverter.ConvertFromString("#F0F0F0"), // Color original
                                Duration = TimeSpan.FromSeconds(1)
                            };
                            brush.BeginAnimation(SolidColorBrush.ColorProperty, animationToGray);

                            AgregarClienteBtn.Foreground = Brushes.Black;
                            AgregarClienteBtn.IsEnabled = true;
                        });
                    });


                    // Limpiar campos
                    NombreTextBox.Text = "";
                    EmailTextBox.Text = "";
                    VehiculoTextBox.Text = "";
                    OrdenesTextBox.Text = "";
                    DeudasTextBox.Text = "";

                    //Vuelve el foco al primer campo
                    NombreTextBox.Focus();


                    // Recargar datos
                    ClientsList.Clear();
                    LoadClientsFromDB();
                    NombreTextBox.Focus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al agregar el cliente: " + ex.Message);
                }
                finally
                {
                    boton.IsEnabled = true; // Reactiva el botón
                }
            }
        }

        private void ActivarModoEdicion_Click(object sender, RoutedEventArgs e)
        {
            ClientDataGrid.IsReadOnly = !ClientDataGrid.IsReadOnly;

            if (ClientDataGrid.IsReadOnly)
            {
                EditarClienteBtn.Content = "Modo Edición";
            }
            else
            {
                EditarClienteBtn.Content = "Bloquear Edición";
            }
        }

        private void EliminarCliente_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExportarLogDiario_Click(object sender, RoutedEventArgs e)
        {
            var hoy = DateTime.Today;
            var clientesDelDia = ClientsList.Where(c => c.FechaRegistro.Date == hoy).ToList();

            if (clientesDelDia.Count == 0)
            {
                MessageBox.Show("No hay clientes registrados hoy para exportar.");
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog
            {
                Filter = "Archivo de texto (*.txt)|*.txt",
                Title = "Exportar log diario",
                FileName = $"LogClientes_{DateTime.Now:yyyyMMdd}.txt"
            };

            if (saveDialog.ShowDialog() == true)
            {
                var lineas = new List<string>();

                // Encabezado bonito
                lineas.Add("===============================================");
                lineas.Add($"      LOG DE CLIENTES - {DateTime.Now:dd/MM/yyyy}");
                lineas.Add("===============================================");
                lineas.Add("");

                foreach (var c in clientesDelDia)
                {
                    lineas.Add($"[📅 {c.FechaRegistro:HH:mm:ss}] Cliente agregado:");
                    lineas.Add($"  🔹 Nombre:   {c.NameClient}");
                    lineas.Add($"  🔹 Email:    {c.Email}");
                    lineas.Add($"  🔹 Vehículo: {c.Vehicle}");
                    lineas.Add($"  🔹 Órdenes:  {c.Orders}");
                    lineas.Add($"  🔹 Deuda:    {c.Debts:C}");
                    lineas.Add("-----------------------------------------------");
                }

                File.WriteAllLines(saveDialog.FileName, lineas, Encoding.UTF8);
                MessageBox.Show("Log diario exportado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ClientDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClientDataGrid.SelectedItem is Client cliente)
            {
                NombreTextBox.Text = cliente.NameClient;
                EmailTextBox.Text = cliente.Email;
                VehiculoTextBox.Text = cliente.Vehicle;
                OrdenesTextBox.Text = cliente.Orders.ToString();
                DeudasTextBox.Text = cliente.Debts.ToString("0.00");
            }
        }

        private void GuardarCambios_Click(object sender, RoutedEventArgs e)
        {
            foreach (var cliente in ClientsList)
            {
                using (SqlConnection connection = Connections.GetConnection())
                {
                    string updateQuery = "UPDATE Clientes SET NameClient = @NameClient, Vehicle = @Vehicle, Orders = @Orders, Debts = @Debts WHERE Id = @Id";

                    SqlCommand command = new SqlCommand(updateQuery, connection);
                    command.Parameters.AddWithValue("@Id", cliente.Id);
                    command.Parameters.AddWithValue("@NameClient", cliente.NameClient);
                    command.Parameters.AddWithValue("@Vehicle", cliente.Vehicle);
                    command.Parameters.AddWithValue("@Orders", cliente.Orders);
                    command.Parameters.AddWithValue("@Debts", cliente.Debts);

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al actualizar el cliente {cliente.NameClient}: {ex.Message}");
                    }
                }
            }

            MessageBox.Show("Cambios guardados correctamente.");
        }

    }
}
