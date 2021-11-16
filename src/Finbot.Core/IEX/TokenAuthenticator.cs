namespace Finbot.Core.IEX;
using RestSharp;
using RestSharp.Authenticators;

public class TokenAuthenticator : IAuthenticator
{
    private readonly string token;

    public TokenAuthenticator(string token) => this.token = token;

    public void Authenticate(IRestClient client, IRestRequest request) => request.AddQueryParameter("token", this.token);
}
