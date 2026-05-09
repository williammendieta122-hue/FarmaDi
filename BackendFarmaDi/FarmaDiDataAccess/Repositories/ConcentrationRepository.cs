using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using FarmaDiCore.Common;
using FarmaDiCore.Entities;

namespace FarmaDiDataAccess.Interfaces
{
    public class ConcentrationRepository : IConcentrationsRepository
    {
        private readonly string _connectionString;

        public ConcentrationRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<IEnumerable<Concentrations>>> GetAllAsync()
        {
            var list = new List<Concentrations>();
            var response = new RepositoryResponse<IEnumerable<Concentrations>>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("USP_GetConcentrations", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                list.Add(new Concentrations
                                {
                                    ConcentrationId = (int)reader["ConcentrationId"],
                                    Volume = reader["Volume"]?.ToString() ?? string.Empty,
                                    Porcentage = reader["Porcentage"]?.ToString() ?? string.Empty,
                                    IsActive = reader["Isactive"] != DBNull.Value && (bool)reader["Isactive"],
                                    RegisteredDate = reader["RegisteredDate"] != DBNull.Value ? (DateTime)reader["RegisteredDate"] : default
                                });
                            }
                        }
                    }
                }

                response.Data = list;
                response.OperationStatusCode = list.Count > 0 ? 50008 : 50009;
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<Concentrations>> GetByIdAsync(int id)
        {
            var response = new RepositoryResponse<Concentrations>();
            Concentrations item = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("USP_GetConcentrationById", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ConcentrationId", id);
                        cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                item = new Concentrations
                                {
                                    ConcentrationId = (int)reader["ConcentrationId"],
                                    Volume = reader["Volume"]?.ToString() ?? string.Empty,
                                    Porcentage = reader["Porcentage"]?.ToString() ?? string.Empty,
                                    IsActive = reader["Isactive"] != DBNull.Value && (bool)reader["Isactive"],
                                    RegisteredDate = reader["RegisteredDate"] != DBNull.Value ? (DateTime)reader["RegisteredDate"] : default
                                };
                            }
                        }

                        var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                        response.Data = item;
                        response.OperationStatusCode = returnedValue;
                    }
                }
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<Concentrations>> AddAsync(Concentrations concentration)
        {
            var response = new RepositoryResponse<Concentrations>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("USP_AddConcentration", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Volume", concentration.Volume);
                        cmd.Parameters.AddWithValue("@Porcentage", concentration.Porcentage);

                        var messageParam = new SqlParameter("@Message", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output };
                        cmd.Parameters.Add(messageParam);
                        var resultParam = new SqlParameter("@Result", SqlDbType.Int) { Direction = ParameterDirection.Output };
                        cmd.Parameters.Add(resultParam);
                        cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                        Concentrations created = null;
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                created = new Concentrations
                                {
                                    ConcentrationId = (int)reader["ConcentrationId"],
                                    Volume = reader["Volume"]?.ToString() ?? string.Empty,
                                    Porcentage = reader["Porcentage"]?.ToString() ?? string.Empty,
                                    IsActive = reader["IsActive"] != DBNull.Value && (bool)reader["IsActive"],
                                    RegisteredDate = reader["RegisteredDate"] != DBNull.Value ? (DateTime)reader["RegisteredDate"] : default
                                };
                            }
                        }

                        var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                        response.OperationStatusCode = returnedValue;
                        response.Message = cmd.Parameters["@Message"].Value?.ToString();

                        if (created != null)
                        {
                            response.Data = created;
                            return response;
                        }

                        var resultId = cmd.Parameters["@Result"].Value != DBNull.Value ? Convert.ToInt32(cmd.Parameters["@Result"].Value) : 0;
                        if (resultId > 0)
                        {
                            var getResp = await GetByIdAsync(resultId);
                            response.Data = getResp.Data;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<Concentrations>> UpdateAsync(int id, Concentrations concentration)
        {
            var response = new RepositoryResponse<Concentrations>();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("USP_UpdateConcentration", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ConcentrationId", id);
                        cmd.Parameters.AddWithValue("@Volume", concentration.Volume);
                        cmd.Parameters.AddWithValue("@Porcentage", concentration.Porcentage);
                        cmd.Parameters.AddWithValue("@IsActive", concentration.IsActive);

                        var messageParam = new SqlParameter("@Message", SqlDbType.VarChar, 500) { Direction = ParameterDirection.Output };
                        cmd.Parameters.Add(messageParam);
                        var resultParam = new SqlParameter("@Result", SqlDbType.Int) { Direction = ParameterDirection.Output };
                        cmd.Parameters.Add(resultParam);
                        cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                        await cmd.ExecuteNonQueryAsync();
                        var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                        response.OperationStatusCode = returnedValue;
                        response.Message = cmd.Parameters["@Message"].Value?.ToString();

                        if (returnedValue == 50008)
                        {
                            var getResp = await GetByIdAsync(id);
                            response.Data = getResp.Data;
                        }
                        else
                        {
                            response.Data = concentration;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<Concentrations>> GetByCodeAsync(string code)
        {
            var response = new RepositoryResponse<Concentrations>();
            Concentrations item = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("USP_GetConcentrationByCode", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ConcentrationCode", code);
                        cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                item = new Concentrations
                                {
                                    ConcentrationId = (int)reader["ConcentrationId"],
                                    Volume = reader["Volume"]?.ToString() ?? string.Empty,
                                    Porcentage = reader["Porcentage"]?.ToString() ?? string.Empty,
                                    IsActive = reader["Isactive"] != DBNull.Value && (bool)reader["Isactive"],
                                    RegisteredDate = reader["RegisteredDate"] != DBNull.Value ? (DateTime)reader["RegisteredDate"] : default
                                };
                            }
                        }

                        var returnedValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                        response.Data = item;
                        response.OperationStatusCode = returnedValue;
                    }
                }
            }
            catch (SqlException ex)
            {
                response.Data = null;
                response.OperationStatusCode = ex.Number;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
