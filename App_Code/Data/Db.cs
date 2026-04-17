using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

public static class Db
{
    // Hard-coded LocalDB connection to your MDF
    // Note the @ verbatim string so the backslashes don't need escaping.
    private static string _cs = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

    public static SqlConnection Open()
    {
        var con = new SqlConnection(_cs);
        con.Open();
        return con;
    }

    public static async Task<SqlConnection> OpenAsync()
    {
        var con = new SqlConnection(_cs);
        await con.OpenAsync();
        return con;
    }

    public static int Execute(string sql, params SqlParameter[] parameters)
    {
        using (var con = Open())
        using (var cmd = new SqlCommand(sql, con))
        {
            if (parameters != null && parameters.Length > 0) cmd.Parameters.AddRange(CloneParameters(parameters));
            return cmd.ExecuteNonQuery();
        }
    }

    public static async Task<int> ExecuteAsync(string sql, params SqlParameter[] parameters)
    {
        using (var con = await OpenAsync())
        using (var cmd = new SqlCommand(sql, con))
        {
            if (parameters != null && parameters.Length > 0) cmd.Parameters.AddRange(CloneParameters(parameters));
            return await cmd.ExecuteNonQueryAsync();
        }
    }

    public static T Scalar<T>(string sql, params SqlParameter[] parameters)
    {
        using (var con = Open())
        using (var cmd = new SqlCommand(sql, con))
        {
            if (parameters != null && parameters.Length > 0) cmd.Parameters.AddRange(CloneParameters(parameters));
            object o = cmd.ExecuteScalar();
            if (o == null || o == DBNull.Value) return default(T);
            return (T)Convert.ChangeType(o, typeof(T));
        }
    }

    public static async Task<T> ScalarAsync<T>(string sql, params SqlParameter[] parameters)
    {
        using (var con = await OpenAsync())
        using (var cmd = new SqlCommand(sql, con))
        {
            if (parameters != null && parameters.Length > 0) cmd.Parameters.AddRange(CloneParameters(parameters));
            object o = await cmd.ExecuteScalarAsync();
            if (o == null || o == DBNull.Value) return default(T);
            return (T)Convert.ChangeType(o, typeof(T));
        }
    }

    public static DataTable Query(string sql, params SqlParameter[] parameters)
    {
        using (var con = Open())
        using (var cmd = new SqlCommand(sql, con))
        {
            if (parameters != null && parameters.Length > 0) cmd.Parameters.AddRange(CloneParameters(parameters));
            using (var da = new SqlDataAdapter(cmd))
            {
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }

    public static async Task<DataTable> QueryAsync(string sql, params SqlParameter[] parameters)
    {
        using (var con = await OpenAsync())
        using (var cmd = new SqlCommand(sql, con))
        {
            if (parameters != null && parameters.Length > 0) cmd.Parameters.AddRange(CloneParameters(parameters));
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                var dt = new DataTable();
                dt.Load(reader);
                return dt;
            }
        }
    }

    public static SqlParameter P(string name, object value, SqlDbType? type = null)
    {
        var p = new SqlParameter();
        p.ParameterName = name != null && name.StartsWith("@") ? name : "@" + name;
        p.Value = value ?? DBNull.Value;
        if (type.HasValue) p.SqlDbType = type.Value;
        return p;
    }

    private static SqlParameter[] CloneParameters(SqlParameter[] parameters)
    {
        var clones = new SqlParameter[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            var c = new SqlParameter();
            c.ParameterName = p.ParameterName;
            c.Value = p.Value;
            c.SqlDbType = p.SqlDbType;
            c.Direction = p.Direction;
            c.Size = p.Size;
            c.Precision = p.Precision;
            c.Scale = p.Scale;
            clones[i] = c;
        }
        return clones;
    }
}
