using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Verndale.RedirectManager.Util
{
    public static class SqlUtil
    {
        private static Func<SqlConnection> sqlConnectionFunc;

        public static void Initialize(Func<SqlConnection> func)
        {
            sqlConnectionFunc = func ?? throw new ArgumentNullException("func");
        }

        public static T ExecuteScalar<T>(string query)
        {
            return Execute<T>(c => c.ExecuteScalar<T>(query));
        }

        public static int ExecuteNonQuery(string query)
        {
            return Execute<int>(c => c.ExecuteNonQuery(query));
        }

        private static T Execute<T>(Func<SqlConnection, T> func)
        {
            if (sqlConnectionFunc == null)
                throw new NullReferenceException("SqlUtil must be initialized first.");

            using (SqlConnection conn = sqlConnectionFunc())
            {
                conn.Open();

                return func(conn);
            }
        }

        public static IEnumerable<T> ExecuteQueryReader<T>(string query, Func<SqlDataReader, T> recordReader)
        {
            if (sqlConnectionFunc == null)
                throw new NullReferenceException("SqlUtil must be initialized first.");

            using (SqlConnection conn = sqlConnectionFunc())
            {
                conn.Open();

                using (var command = new SqlCommand(query, conn))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return recordReader(reader);
                        }
                    }
                }
            }
        }

        #region SqlConnection Extensions

        public static T ExecuteScalar<T>(this SqlConnection conn, string query)
        {
            using (var command = new SqlCommand(query, conn))
            {
                return (T)command.ExecuteScalar();
            }
        }

        public static int ExecuteNonQuery(this SqlConnection conn, string query)
        {
            using (var command = new SqlCommand(query, conn))
            {
                return command.ExecuteNonQuery();
            }
        }

        #endregion

    }
}