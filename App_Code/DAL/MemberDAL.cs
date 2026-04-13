using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using serena.Entities;

namespace serena.DAL
{
    public class MemberDAL
    {
        public Member GetByUsername(string username)
        {
            string sql = "SELECT * FROM members WHERE username = @u";
            DataTable dt = Db.Query(sql, Db.P("u", username));
            if (dt.Rows.Count > 0) return MapRow(dt.Rows[0]);
            return null;
        }

        public Member GetById(int id)
        {
            string sql = "SELECT * FROM members WHERE id = @id";
            DataTable dt = Db.Query(sql, Db.P("id", id));
            if (dt.Rows.Count > 0) return MapRow(dt.Rows[0]);
            return null;
        }

        public int Insert(Member m)
        {
            string sql = @"
                INSERT INTO members (full_name, username, email, password, phone)
                VALUES (@fn, @u, @e, @p, @ph);
                SELECT SCOPE_IDENTITY();";

            return Convert.ToInt32(Db.Scalar<object>(sql, 
                Db.P("fn", m.FullName),
                Db.P("u", m.Username),
                Db.P("e", m.Email),
                Db.P("p", m.Password),
                Db.P("ph", m.Phone)
            ));
        }

        private Member MapRow(DataRow row)
        {
            return new Member
            {
                Id = Convert.ToInt32(row["id"]),
                FullName = Convert.ToString(row["full_name"]),
                Username = Convert.ToString(row["username"]),
                Email = Convert.ToString(row["email"]),
                Password = Convert.ToString(row["password"]),
                Phone = Convert.ToString(row["phone"]),
                CreatedAt = Convert.ToDateTime(row["created_at"]),
                UpdatedAt = Convert.ToDateTime(row["updated_at"])
            };
        }
    }
}
