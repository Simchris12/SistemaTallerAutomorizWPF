using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SistemaTallerAutomorizWPF.Models;
using SistemaTallerAutomorizWPF.ViewModel;

namespace SistemaTallerAutomorizWPF.ViewModels
{
    public class ServicesViewModel : ViewModelBase
    {
        public ObservableCollection<ResumenServicio> ServiciosList { get; set; } = new();
        private ResumenServicio _clienteSeleccionado;
        public ResumenServicio ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set
            {
                _clienteSeleccionado = value;
                OnPropertyChanged(nameof(ClienteSeleccionado));
            }
        }

        public void CargarDatos()
        {
            ServiciosList.Clear();

            using var connection = Connections.GetConnection();
            string query = @"
                    SELECT 
                        C.Id AS IdCliente,
                        C.NameClient,
                        C.Email,
                        C.Vehicle,
                        C.Debts,
                        V.Placa,
                        V.FechaRegistro,
                    COUNT(O.IdOrden) AS OrdenesTotales,
                    (
                    SELECT TOP 1 Estado
                    FROM Ordenes O2
                    WHERE O2.IdCliente = C.Id
                    ORDER BY O2.Fecha DESC
                    ) AS UltimoEstado

                    FROM Clientes C
                    LEFT JOIN Vehiculos V ON C.Id = V.ClienteId
                    LEFT JOIN Ordenes O ON C.Id = O.IdCliente

                    GROUP BY 
                        C.Id, C.NameClient, C.Email, C.Vehicle, C.Debts,
                        V.Placa, V.FechaRegistro";

            var command = new SqlCommand(query, connection);

            try
            {
                connection.Open();
                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var servicio = new ResumenServicio
                    {
                        IdCliente = Convert.ToInt32(reader["IdCliente"]),
                        NombreCliente = reader["NameClient"].ToString(),
                        Email = reader["Email"].ToString(),
                        Vehiculo = reader["Vehicle"].ToString(),
                        Deuda = Convert.ToDecimal(reader["Debts"]),
                        Placa = reader["Placa"]?.ToString(),
                        FechaRegistro = reader["FechaRegistro"] as DateTime?,
                        OrdenesTotales = Convert.ToInt32(reader["OrdenesTotales"]),
                        EstadoUltimaOrden = reader["UltimoEstado"]?.ToString() ?? "Sin órdenes"
                    };

                    ServiciosList.Add(servicio);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar datos de servicios: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Agregar orden nueva (simulada por ahora)
        public void CrearOrdenParaClienteSeleccionado()
        {
            if (ClienteSeleccionado == null) return;

            using var connection = Connections.GetConnection();
            var command = new SqlCommand(@"
            INSERT INTO Ordenes (IdCliente, IdVehiculo, Total, Estado, Fecha)
            VALUES (@IdCliente, 
                   (SELECT Id FROM Vehiculos WHERE ClienteId = @IdCliente), 
                   0.00, 'Nueva', GETDATE());
        ", connection);

            command.Parameters.AddWithValue("@IdCliente", ClienteSeleccionado.IdCliente);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
                MessageBox.Show("Orden creada correctamente.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al crear orden: " + ex.Message);
            }
        }
    }
}
