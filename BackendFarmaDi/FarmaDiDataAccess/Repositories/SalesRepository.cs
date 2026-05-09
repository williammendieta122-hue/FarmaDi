using FarmaDiCore.Common;
using FarmaDiCore.Entities;
using FarmaDiDataAccess.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace FarmaDiDataAccess.Repositories
{
    public class SalesRepository : ISalesRepository
    {
        private readonly string _connectionString;

        public SalesRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Recibimos 'Invoice' como maestro, ya que trae los datos del cliente y descuento. 
        public async Task<RepositoryResponse<SaleTransaction>> InsertAsync(Invoice master, IEnumerable<SaleDetails> details ,int paymentMethodId)
        {
            var transaction = new SaleTransaction();

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand("USP_InsertSale", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Definimos los parametros basados en la entidad Invoice
                        cmd.Parameters.AddWithValue("@ClientName", master.ClientName);
                        cmd.Parameters.AddWithValue("@UserId", master.UserId);
                        cmd.Parameters.AddWithValue("@Discount", master.Discount);
                        cmd.Parameters.AddWithValue("@PaymentMethodId", paymentMethodId); 
                        var detailsTable = new DataTable();
                        detailsTable.Columns.Add("Quantity", typeof(int));    // <--- 1. Quantity
                        detailsTable.Columns.Add("ProductId", typeof(int));   // <--- 2. ProductId
                        // primero cantidad y despues producto.
                        // asi lo cree en la db

                        foreach (var item in details)
                        {
                            detailsTable.Rows.Add(item.Quantity, item.ProductId);
                        }

                        SqlParameter detailParm = cmd.Parameters.AddWithValue("@SalesDetails", detailsTable);
                        detailParm.SqlDbType = SqlDbType.Structured;
                        detailParm.TypeName = "SalesDetalilsType";

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            //result set 1: LA FACTURA (INVOICE) 
                            if (await reader.ReadAsync())
                            {
                                transaction.InvoiceMaster = new Invoice // Usamos la entidad de Factura
                                {
                        
                                    InvoiceId = (int)reader["InvoiceId"],
                                    UserId = (int)reader["UserId"],
                                    ClientName = reader["ClientName"].ToString(),
                                    RegisteredDate = (DateTime)reader["RegisteredDate"],
                                    Discount = (decimal)reader["Discount"],
                                    SubTotal = (decimal)reader["SubTotal"],
                                    Total = (decimal)reader["Total"],
                                    IsPrinted = (bool)reader["IsPrinted"] // este campo lo agrege recientemente y es para 
                                    // saber si la factura esta impresa o no, es para la simulacion de cola
                                };
                            }

                            //result set 2: EL DETALLE DE FACTURA (CON NOMBRES DE PRODUCTO)  :)
                            await reader.NextResultAsync();

                            var invoiceDetailsList = new List<InvoiceDetails>();
                            while (await reader.ReadAsync())
                            {
                                invoiceDetailsList.Add(new InvoiceDetails
                                {
                                    // Mapeamos a la entidad InvoiceDetail 
                                    InvoicesDetailId = (int)reader["InvoicesDetailId"], 
                                    InvoiceId = (int)reader["InvoiceId"],
                                    ProductId = (int)reader["ProductId"],

                                    // Propiedades extendidas (JOIN con Products)
                                    ProductTradeName = reader["ProductTradeName"].ToString(),
                                    ProductGenericName = reader["ProductGenericName"].ToString(),

                                    Quantity = (int)reader["Quantity"],
                                    UnitPrice = (decimal)reader["UnitPrice"], // Precio calculado en el SP
                                    TotalPrice = (decimal)reader["TotalPrice"],

                                    // RegisteredDate = (DateTime)reader["RegisteredDate"] 
                                    // supongo que no es necesario, anteriormente hice un join para que la impresion de la factura salga bonita
                                });
                            }

                            transaction.InvoiceDetails = invoiceDetailsList;
                        }
                    }

                    return new RepositoryResponse<SaleTransaction>
                    {
                        Data = transaction,
                        OperationStatusCode = 0,
                        Message = "Venta registrada exitosamente."
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<SaleTransaction>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<SaleTransaction>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = "Error inesperado: " + ex.Message
                };
            }
        }



        public async Task<RepositoryResponse<SaleTransaction>> GetInvoiceByIdAsync(int invoiceId)
        {
            var transaction = new SaleTransaction();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("USP_GetInvoiceById", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            // 1. Leer Cabecera
                            if (await reader.ReadAsync())
                            {
                                transaction.InvoiceMaster = new Invoice
                                {
                                    InvoiceId = (int)reader["InvoiceId"],
                                    ClientName = reader["ClientName"].ToString(),
                                    UserId = (int)reader["UserId"],
                                    RegisteredDate = (DateTime)reader["RegisteredDate"],
                                    Discount = (decimal)reader["Discount"], 
                                    SubTotal = (decimal)reader["SubTotal"],
                                    Total = (decimal)reader["Total"],
                                    IsPrinted = (bool)reader["IsPrinted"]
                                };
                            }
                            else
                            {
                                return new RepositoryResponse<SaleTransaction>
                                { /*
                                    IsSuccess = false,
                                    MessageCode = 404,
                                    Message = "Factura no encontrada"
                                    */
                                };
                            }

                            // 2. Leer Detalles
                            await reader.NextResultAsync();
                            var detailsList = new List<InvoiceDetails>();

                            while (await reader.ReadAsync())
                            {
                                detailsList.Add(new InvoiceDetails
                                {
                                    InvoicesDetailId = (int)reader["InvoicesDetailId"],
                                    InvoiceId = (int)reader["InvoiceId"],
                                    ProductId = (int)reader["ProductId"],
            
                                    ProductTradeName = reader["ProductTradeName"].ToString(),
                                    ProductGenericName = reader["ProductGenericName"].ToString(),
                                    Quantity = (int)reader["Quantity"],
                                    UnitPrice = (decimal)reader["UnitPrice"], 
                                    TotalPrice = (decimal)reader["TotalPrice"]
                                });
                            }
                            transaction.InvoiceDetails = detailsList;
                        }
                    }
                }

                return new RepositoryResponse<SaleTransaction>
                {
                    Data = transaction,
                    OperationStatusCode = 0,
                    Message = "Factura recuperada exitosamente"
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<SaleTransaction>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }



       

     
        public async Task<RepositoryResponse<IEnumerable<Invoice>>> GetPendingInvoicesAsync()
        {
            var list = new List<Invoice>(); // Lista de Entidades
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("USP_GetPendingInvoices", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                
                                list.Add(new Invoice
                                {
                                    InvoiceId = (int)reader["InvoiceId"],
                                    ClientName = reader["ClientName"].ToString(),
                                    RegisteredDate = (DateTime)reader["RegisteredDate"],
                                    Total = (decimal)reader["Total"],
                                    IsPrinted = false 
                                });
                            }
                        }
                    }
                }
                return new RepositoryResponse<IEnumerable<Invoice>>
                {
                    Data = list,
                    OperationStatusCode = 0,
                    Message = "Datos obtenidos"
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<IEnumerable<Invoice>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }


        // solo para una simulación de salida de facturas, no cumple con las buenas practicas es solo un pequeño ejemplo
        // funcional y práctico
        public async Task<bool> ConfirmPrintAsync(int invoiceId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("USP_ConfirmPrint", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@InvoiceId", invoiceId);

                      
                        var result = await cmd.ExecuteScalarAsync();

                       
                        int rowsAffected = (result != null) ? Convert.ToInt32(result) : 0;

             
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
     
                return false;
            }
        }
    }
}