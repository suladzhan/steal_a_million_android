using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace StealAMillion.Core
{
    public sealed class SaveStore
    {
        private readonly string path;
        private readonly Func<SaveData, string> encode;
        private readonly Func<string, SaveData> decode;
        public string LastError { get; private set; }

        public SaveStore(string file, Func<SaveData, string> serialize, Func<string, SaveData> deserialize)
        {
            path = file;
            encode = serialize;
            decode = deserialize;
        }

        public SaveData Load()
        {
            SaveData data;
            if (TryRead(path, out data) || TryRead(path + ".bak", out data)) return data;
            return new SaveData();
        }

        public bool Write(SaveData data)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                string body = encode(data);
                byte[] bytes = Encoding.UTF8.GetBytes("SAM1\n" + Digest(body) + "\n" + body);
                using (var stream = new FileStream(path + ".tmp", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }
                SaveData previous;
                // Keep a known-good backup, including when recovering from a corrupt main file.
                if (TryRead(path, out previous)) File.Copy(path, path + ".bak", true);
                if (File.Exists(path)) File.Delete(path);
                File.Move(path + ".tmp", path);
                LastError = null;
                return true;
            }
            catch (Exception e)
            {
                if (!(e is IOException) && !(e is UnauthorizedAccessException) && !(e is System.Security.SecurityException)) throw;
                LastError = "Progress could not be saved. Check device storage.";
                return false;
            }
        }

        public bool Reset()
        {
            // Write fresh progress to both generations so old progress cannot reappear on recovery.
            var fresh = new SaveData();
            return Write(fresh) && Write(fresh);
        }

        private bool TryRead(string file, out SaveData data)
        {
            data = null;
            try
            {
                if (!File.Exists(file) || new FileInfo(file).Length > 1024 * 1024) return false;
                string[] parts = File.ReadAllText(file).Split(new[] { '\n' }, 3);
                if (parts.Length != 3 || parts[0] != "SAM1" || parts[1] != Digest(parts[2])) return false;
                data = decode(parts[2]);
                return data != null && data.version == 1;
            }
            catch (Exception e)
            {
                if (e is OutOfMemoryException) throw;
                return false;
            }
        }

        private static string Digest(string value)
        {
            using (var sha = SHA256.Create())
                return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(value)));
        }
    }
}
