using NuGet.Protocol;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace Aptosnaut.Models
{
    public class AptosActions
    {

        public GoogleAuthTokenSchema? GetValidToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtDecoded = handler.ReadJwtToken(token);
            string decoded = jwtDecoded.ToJson();
            var googleAuthObject = JsonSerializer.Deserialize<GoogleAuthTokenSchema>(decoded);
            return googleAuthObject;
        }

    }
}
