using HairSalonIS.Data;
using Npgsql;
using System;

namespace HairSalonIS.Services
{
    public static class LogService
    {
        public static void AddLog(string actionType, string actionDescription)
        {
            try
            {
                Database.ExecuteNonQuery(
                    @"insert into user_action_log
                      (company_number_admin, admin_full_name, role_admin, action_type, action_description)
                      values (@adminNumber, @adminName, @role, @actionType, @description)",
                    new NpgsqlParameter("@adminNumber", string.IsNullOrWhiteSpace(AuthService.CurrentAdminNumber)
                        ? DBNull.Value
                        : AuthService.CurrentAdminNumber),
                    new NpgsqlParameter("@adminName", string.IsNullOrWhiteSpace(AuthService.CurrentAdminName)
                        ? DBNull.Value
                        : AuthService.CurrentAdminName),
                    new NpgsqlParameter("@role", string.IsNullOrWhiteSpace(AuthService.CurrentAdminRole)
                        ? DBNull.Value
                        : AuthService.CurrentAdminRole),
                    new NpgsqlParameter("@actionType", actionType),
                    new NpgsqlParameter("@description", actionDescription)
                );
            }
            catch
            {

            }
        }
    }
}