using RestSharp;
using RestSharp.Authenticators;

namespace Finbot.Core.IEX
{
    public class TokenAuthenticator : IAuthenticator
    {
        private readonly string token;

        public TokenAuthenticator(string token)
        {
            this.token = token;
        }

        public void Authenticate(IRestClient client, IRestRequest request)
        {
            request.AddQueryParameter("token", token);
        }
    }
}
