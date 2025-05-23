namespace Finbot.Core.IEX;
using RestSharp;
using RestSharp.Authenticators;
using System.Threading.Tasks;

public class TokenAuthenticator : IAuthenticator
{
    private readonly string token;

    public TokenAuthenticator(string token) => this.token = token;

    public ValueTask Authenticate(IRestClient client, RestRequest request)
    {
        request.AddQueryParameter("token", this.token);
        return ValueTask.CompletedTask;
    }
}
