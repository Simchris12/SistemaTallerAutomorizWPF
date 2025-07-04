﻿using System;
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
    public class VehicleViewModel : ViewModelBase
    {
        public ObservableCollection<Vehiculo> VehiculosList { get; set; } = new ObservableCollection<Vehiculo>();

        internal void CargarDatos()
        {
            LoadVehiculosFromDB();
        }

        private List<Vehiculo> VehiculosBackup = new List<Vehiculo>();

        public void FiltrarVehiculos(string filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                VehiculosList.Clear();
                foreach (var v in VehiculosBackup)
                    VehiculosList.Add(v);
            }
            else
            {
                var filtroLower = filtro.ToLower();

                var filtrados = VehiculosBackup.Where(v =>
                    (v.NombreCliente != null && v.NombreCliente.ToLower().Contains(filtroLower)) ||
                    (v.Placa != null && v.Placa.ToLower().Contains(filtroLower))
                ).ToList();

                VehiculosList.Clear();
                foreach (var v in filtrados)
                    VehiculosList.Add(v);
            }
        }

        private void LoadVehiculosFromDB()
        {
            VehiculosList.Clear();
            VehiculosBackup.Clear();

            using (SqlConnection connection = Connections.GetConnection())
            {
                string query = @"
            SELECT 
                C.Id AS ClienteId,
                C.NameClient AS NombreCliente,
                C.Vehicle AS MarcaVehiculo,
                V.Anio,
                V.Placa,
                V.Color,
                V.FechaRegistro
            FROM Clientes C
            LEFT JOIN Vehiculos V ON V.ClienteId = C.Id";

                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var vehiculo = new Vehiculo
                        {
                            ClienteId = Convert.ToInt32(reader["ClienteId"]),
                            NombreCliente = reader["NombreCliente"].ToString(),
                            MarcaVehiculo = reader["MarcaVehiculo"].ToString(),
                            Anio = reader["Anio"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["Anio"]),
                            Placa = reader["Placa"] == DBNull.Value ? null : reader["Placa"].ToString(),
                            Color = reader["Color"] == DBNull.Value ? null : reader["Color"].ToString(),
                            FechaRegistro = reader["FechaRegistro"] == DBNull.Value ? null : (DateTime?)Convert.ToDateTime(reader["FechaRegistro"])
                        };
                        VehiculosList.Add(vehiculo);
                        VehiculosBackup.Add(vehiculo);
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar los vehículos: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}
