using System.Threading.Tasks;

namespace CognitoAuthorizationExample
{
    public class AuthHelper
    {
        public static async Task<string> GetTokenForUser(string login, string pass)
        {
            var provider = new CognitoAuthorization();
            var response = await provider.GetCredsAsync(login, pass);
            var token = response.AuthenticationResult.IdToken;
            return "Bearer " + token;
        }
    }
}