using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SN.withSIX.Mini.Presentation.Shell
{
    public class Helper
    {
        public static bool IsNotKnownToSync(string x) => !IsKnownToSync(x);
        // TODO: Probably should have a global registry per user account somewhere e.g in a json file instead?
        public static bool IsKnownToSync(string x) => File.Exists(Path.Combine(x, ".sync.txt"));

        public static async Task<List<FolderInfo>> TryGetInfo(List<string> folderPaths) {
            try {
                return await GetInfo(folderPaths).ConfigureAwait(false);
            } catch {
                return new List<FolderInfo>();
            }
        }

        public static async Task<List<FolderInfo>> GetInfo(List<string> folderPaths) {
            using (var client = new HttpClient()) {
                var uri = new Uri("https://127.0.0.66:9666/api/get-upload-folders");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                using (var content = new StringContent(JsonConvert.SerializeObject(folderPaths), Encoding.UTF8,
                    "application/json")) {
                    var r = await client.PostAsync(uri, content).ConfigureAwait(false);
                    r.EnsureSuccessStatusCode();
                    var c = await r.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return JsonConvert.DeserializeObject<List<FolderInfo>>(c);
                }
            }
        }
    }

    public class ContentInfo
    {
        public ContentInfo(Guid userId, Guid gameId, Guid contentId)
        {
            UserId = userId;
            GameId = gameId;
            ContentId = contentId;
        }

        public Guid UserId { get; }
        public Guid GameId { get; }
        public Guid ContentId { get; }
    }

    public class FolderInfo
    {
        public FolderInfo(string path, ContentInfo contentInfo)
        {
            Path = path;
            ContentInfo = contentInfo;
        }

        public string Path { get; }
        public ContentInfo ContentInfo { get; set; }
    }

    public class ShortGuid
    {
        static readonly ShortGuid Default = new ShortGuid(Guid.Empty);
        readonly Guid _guid;
        readonly string _value;

        /// <summary>Create a 22-character case-sensitive short GUID.</summary>
        public ShortGuid(Guid guid)
        {
            if (guid == null)
                throw new ArgumentNullException("guid");

            _guid = guid;
            _value = Convert.ToBase64String(guid.ToByteArray())
                .Substring(0, 22)
                .Replace("/", "_")
                .Replace("+", "-");
        }

        /// <summary>Get the short GUID as a string.</summary>
        public override string ToString()
        {
            return _value;
        }

        /// <summary>Get the Guid object from which the short GUID was created.</summary>
        public Guid ToGuid()
        {
            return _guid;
        }

        /// <summary>Get a short GUID as a Guid object.</summary>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.FormatException"></exception>
        public static ShortGuid Parse(string shortGuid)
        {
            if (shortGuid == null)
                throw new ArgumentNullException("shortGuid");
            if (shortGuid.Length != 22)
                throw new FormatException("Input string was not in a correct format.");

            return new ShortGuid(new Guid(Convert.FromBase64String
                (shortGuid.Replace("_", "/").Replace("-", "+") + "==")));
        }

        public static bool TryParse(string shortGuid, out ShortGuid id)
        {
            try
            {
                id = Parse(shortGuid);
                return true;
            }
            catch
            {
                id = null;
                return false;
            }
        }

        public static ShortGuid ParseWithFallback(string shortGuid)
        {
            try
            {
                return Parse(shortGuid);
            }
            catch (ArgumentException)
            {
                return Default;
            }
            catch (FormatException)
            {
                return Default;
            }
        }

        public static implicit operator String(ShortGuid guid)
        {
            return guid.ToString();
        }

        public static implicit operator Guid(ShortGuid shortGuid)
        {
            return shortGuid._guid;
        }
    }
}