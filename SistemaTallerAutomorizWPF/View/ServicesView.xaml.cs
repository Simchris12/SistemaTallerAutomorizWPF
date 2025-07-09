﻿using System;
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
using System.Data.SqlClient;

namespace SistemaTallerAutomorizWPF.View
{
    /// <summary>
    /// Lógica de interacción para ServicesView.xaml
    /// </summary>
    public partial class ServicesView : UserControl
    {
        private ServicesViewModel viewModel;

        public ServicesView()
        {
            InitializeComponent();
            viewModel = new ServicesViewModel();
            DataContext = viewModel;
            viewModel.CargarDatos();
        }

        //agregar ordenes
        private void AgregarOrdenBtn_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.ClienteSeleccionado == null)
            {
                MessageBox.Show("Selecciona un cliente primero.");
                return;
            }

            int idCliente = viewModel.ClienteSeleccionado.IdCliente;

            // Buscar el ID del vehículo del cliente
            int idVehiculo = -1;
            using (var con = Connections.GetConnection())
            {
                var cmd = new SqlCommand("SELECT Id FROM Vehiculos WHERE ClienteId = @id", con);
                cmd.Parameters.AddWithValue("@id", idCliente);

                con.Open();
                var result = cmd.ExecuteScalar();
                if (result != null)
                    idVehiculo = Convert.ToInt32(result);
                else
                {
                    MessageBox.Show("Este cliente no tiene un vehículo asignado.");
                    return;
                }
            }

            // Insertar nueva orden
            using (var con = Connections.GetConnection())
            {
                var insert = new SqlCommand(@"
            INSERT INTO Ordenes (IdCliente, IdVehiculo, Total, Estado, Fecha)
            VALUES (@idCliente, @idVehiculo, @total, @estado, GETDATE())", con);

                insert.Parameters.AddWithValue("@idCliente", idCliente);
                insert.Parameters.AddWithValue("@idVehiculo", idVehiculo);
                insert.Parameters.AddWithValue("@total", 0.00m); // se puede personalizar luego
                insert.Parameters.AddWithValue("@estado", "Nueva");

                try
                {
                    con.Open();
                    insert.ExecuteNonQuery();
                    MessageBox.Show("Orden agregada correctamente.");
                    viewModel.CargarDatos(); // Actualiza contador y estado
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al agregar orden: " + ex.Message);
                }
            }
        }
    }
}
