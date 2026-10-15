using HairSalonIS.Data;
using Npgsql;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace HairSalonIS.Services
{
    public static class AuthService
    {
        public static string CurrentAdminName { get; private set; } = "";
        public static string CurrentAdminNumber { get; private set; } = "";
        public static string CurrentAdminRole { get; private set; } = "";

        public static bool Login(string login, string password)
        {
            string passwordHash = GetSha256Hash(password);

            DataTable table = Database.ExecuteQuery(
                @"select 
                    company_number_admin,
                    admin_full_name_admin,
                    role_admin
                  from Administrator
                  where login_admin = @login
                    and password_hash_admin = @passwordHash",
                new NpgsqlParameter("@login", login),
                new NpgsqlParameter("@passwordHash", passwordHash)
            );

            if (table.Rows.Count == 0)
            {
                return false;
            }

            DataRow row = table.Rows[0];

            CurrentAdminNumber = row["company_number_admin"].ToString() ?? "";
            CurrentAdminName = row["admin_full_name_admin"].ToString() ?? "";
            CurrentAdminRole = row["role_admin"].ToString() ?? "";

            return true;
        }

        public static void Logout()
        {
            CurrentAdminName = "";
            CurrentAdminNumber = "";
            CurrentAdminRole = "";
        }

        private static string GetSha256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(bytes);

                StringBuilder builder = new StringBuilder();

                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}