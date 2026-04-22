using System.Security.Cryptography;
using System.Text;

namespace FlagForge.Data.Services;

public static class EnvironmentApiKeyHasher
{
    public static string Hash(string apiKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(bytes);
    }
}
