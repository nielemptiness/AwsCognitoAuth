using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;

namespace CognitoAuthorizationExample
{
    public class CognitoAuthorization
    {
        public enum Token
        {
            Id,
            Access
        }
        private static AmazonCognitoIdentityProviderClient _cognitoClient;
        
        //Here you should assign your AWS credentials. I recommend to store them at least at appsettings.json
        //But it is better to do so by docker-compose .env files or via env. variables at your build system
        private static string AWS_KEY_ID;
        private static string AWS_SECRET_KEY;
        private static string COGNITO_USER_POOL_ID;
        private static string COGNITO_APP_CLIENT_ID;
        private static string COGNITO_APP_CLIENT_SECRET;
        private static RegionEndpoint _endpoint;
        private const string UserName = "admin";
        private const string Password = "root";
        
        /// <summary>
        /// Create static client using your credentials
        /// Be sure to set them before calling constructor
        /// </summary>
        public CognitoAuthorization()
        {
            _cognitoClient =
                    new AmazonCognitoIdentityProviderClient(AWS_KEY_ID, AWS_SECRET_KEY, _endpoint);
        }
        
        public static string GetAuthToken(Token token = Token.Id)
        {
            var provider = new CognitoAuthorization();
            
            return token switch
            {
                //OpenId OAuth 2.0 JWT
                //Use for scope 
                Token.Id => "Bearer " + provider.GetIdToken().Result,
                //Access without further identification
                Token.Access => "Bearer " + provider.GetAccessToken().Result,
                _ => throw new ArgumentException("No such token!", nameof(token))
            };
        }
        
        private async Task<string> GetIdToken()
        {
            return (await GetCredsAsync()).AuthenticationResult.IdToken;
        }

        private async Task<string> GetAccessToken()
        {
            return (await GetCredsAsync()).AuthenticationResult.AccessToken;
        }

        public async Task<AuthFlowResponse> GetCredsAsync(string userName = UserName, string password = Password)
        {
            CognitoUserPool userPool = new CognitoUserPool(COGNITO_USER_POOL_ID, COGNITO_APP_CLIENT_ID, _cognitoClient);
            CognitoUser user = new CognitoUser(userName, COGNITO_APP_CLIENT_ID, userPool, _cognitoClient, COGNITO_APP_CLIENT_SECRET);
            InitiateSrpAuthRequest authRequest = new InitiateSrpAuthRequest()
            {
                Password = password
            };

            return await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);
        }

        
        public static async Task SetPermanentPasswordAsync(string userName, string password)
        {
            await _cognitoClient
                .AdminSetUserPasswordAsync(new AdminSetUserPasswordRequest()
                {
                    Username = userName,
                    UserPoolId = COGNITO_USER_POOL_ID,
                    Password = password,
                    Permanent = true
                });
        }
    }
}