using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace serena
{
    public static class ProductImageResolver
    {
        private static readonly object Sync = new object();
        private static string[] _productFiles;

        public static string Resolve(string dbValue, string productName = null)
        {
            string direct = ResolveStoredImage(dbValue);
            if (!string.IsNullOrWhiteSpace(direct))
                return direct;

            return ResolveLocalFallback(productName);
        }

        public static string ResolveLocalFallback(string productName = null)
        {
            var files = GetProductFiles();
            if (files.Length == 0)
                return VirtualPathUtility.ToAbsolute("~/Assets/images/products/");

            string normalized = Normalize(productName);
            string matched = FindBestMatch(files, normalized);
            if (string.IsNullOrWhiteSpace(matched))
            {
                int index = StableHash(normalized) % files.Length;
                matched = files[index];
            }

            return VirtualPathUtility.ToAbsolute("~/Assets/images/products/" + matched);
        }

        private static string ResolveStoredImage(string dbValue)
        {
            if (string.IsNullOrWhiteSpace(dbValue))
                return null;

            string value = dbValue.Trim();

            if (Uri.IsWellFormedUriString(value, UriKind.Absolute))
                return value;

            string relative = value.TrimStart('~', '/').Replace('\\', '/');
            string physical = HostingEnvironment.MapPath("~/" + relative);
            if (!string.IsNullOrWhiteSpace(physical) && File.Exists(physical))
                return VirtualPathUtility.ToAbsolute("~/" + relative);

            return null;
        }

        private static string[] GetProductFiles()
        {
            if (_productFiles != null)
                return _productFiles;

            lock (Sync)
            {
                if (_productFiles != null)
                    return _productFiles;

                string dir = HostingEnvironment.MapPath("~/Assets/images/products");
                if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                {
                    _productFiles = new string[0];
                }
                else
                {
                    _productFiles = Directory.GetFiles(dir)
                        .Select(Path.GetFileName)
                        .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                        .ToArray();
                }
            }

            return _productFiles;
        }

        private static string FindBestMatch(string[] files, string normalizedName)
        {
            if (files == null || files.Length == 0 || string.IsNullOrWhiteSpace(normalizedName))
                return null;

            foreach (var file in files)
            {
                string fileName = Normalize(Path.GetFileNameWithoutExtension(file));
                if (string.IsNullOrWhiteSpace(fileName))
                    continue;

                if (fileName == normalizedName || fileName.Contains(normalizedName) || normalizedName.Contains(fileName))
                    return file;
            }

            return null;
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var sb = new StringBuilder(value.Length);
            foreach (char c in value.ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 23;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    for (int i = 0; i < value.Length; i++)
                        hash = (hash * 31) + value[i];
                }
                return Math.Abs(hash == int.MinValue ? 0 : hash);
            }
        }
    }
}
