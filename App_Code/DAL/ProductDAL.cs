using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using serena.Entities;

namespace serena.DAL
{
    public class ProductDAL
    {
        public List<Product> GetNewArrivals(int limit = 8)
        {
            string sql = string.Format(@"
                SELECT TOP {0} p.*, c.name AS CategoryName
                FROM products p
                LEFT JOIN categories c ON c.id = p.category_id
                WHERE p.is_show = 1
                ORDER BY p.created_at DESC, p.id DESC", limit);

            DataTable dt = Db.Query(sql);
            return MapList(dt);
        }

        public List<Product> GetTopPicks(int limit = 8)
        {
            string sql = string.Format(@"
                SELECT TOP {0} p.*, c.name AS CategoryName
                FROM products p
                LEFT JOIN categories c ON c.id = p.category_id
                WHERE p.is_show = 1
                ORDER BY NEWID()", limit);

            DataTable dt = Db.Query(sql);
            return MapList(dt);
        }

        public Product GetBySlug(string slug)
        {
            // Match either by exact name or a normalized slug-like comparison
            string sql = @"
                SELECT TOP 1 p.*, c.name AS CategoryName
                FROM products p
                LEFT JOIN categories c ON c.id = p.category_id
                WHERE p.is_show = 1 AND
                (
                    LOWER(p.name) = @s
                    OR REPLACE(REPLACE(REPLACE(LOWER(p.name), ' ', '-'), '&', 'and'), '''', '') = @s
                )";

            DataTable dt = Db.Query(sql, Db.P("s", slug.ToLowerInvariant()));
            if (dt.Rows.Count > 0) return MapRow(dt.Rows[0]);
            return null;
        }

        public Product GetById(int id)
        {
            string sql = "SELECT p.*, c.name AS CategoryName FROM products p LEFT JOIN categories c ON c.id = p.category_id WHERE p.id = @id";
            DataTable dt = Db.Query(sql, Db.P("id", id));
            if (dt.Rows.Count > 0) return MapRow(dt.Rows[0]);
            return null;
        }

        public List<Product> GetByIds(List<int> ids)
        {
            if (ids == null || ids.Count == 0) return new List<Product>();

            var prms = new List<SqlParameter>();
            var inParts = new List<string>();
            for (int i = 0; i < ids.Count; i++)
            {
                string p = "@id" + i;
                inParts.Add(p);
                prms.Add(new SqlParameter(p, ids[i]));
            }

            string sql = "SELECT p.*, c.name AS CategoryName FROM products p LEFT JOIN categories c ON c.id = p.category_id WHERE p.id IN (" + string.Join(",", inParts) + ")";
            DataTable dt = Db.Query(sql, prms.ToArray());
            return MapList(dt);
        }

        private List<Product> MapList(DataTable dt)
        {
            var list = new List<Product>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRow(row));
            }
            return list;
        }

        private Product MapRow(DataRow row)
        {
            return new Product
            {
                Id = Convert.ToInt32(row["id"]),
                CategoryId = Convert.ToInt32(row["category_id"]),
                Name = Convert.ToString(row["name"]),
                Description = Convert.ToString(row["description"]),
                Stock = Convert.ToInt32(row["stock"]),
                Price = Convert.ToDecimal(row["price"]),
                Image = Convert.ToString(row["image"]),
                IsShow = Convert.ToBoolean(row["is_show"]),
                CreatedAt = Convert.ToDateTime(row["created_at"]),
                UpdatedAt = Convert.ToDateTime(row["updated_at"]),
                CategoryName = row.Table.Columns.Contains("CategoryName") ? Convert.ToString(row["CategoryName"]) : null
            };
        }
    }
}
