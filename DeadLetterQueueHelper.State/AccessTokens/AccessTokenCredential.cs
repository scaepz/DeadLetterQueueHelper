using Azure.Core;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace DeadLetterQueueHelper.State.AccessTokens
{
    public class AccessTokenCredential : TokenCredential
    {
        private readonly IAccessTokenProvider _accessTokenProvider;

        public AccessTokenCredential(IAccessTokenProvider accessTokenProvider)
        {
            _accessTokenProvider = accessTokenProvider;
        }

        public override Azure.Core.AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return GetTokenAsync(requestContext, cancellationToken).GetAwaiter().GetResult();
        }

        public override async ValueTask<Azure.Core.AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            var result = await _accessTokenProvider.RequestAccessToken();
            if (result.TryGetToken(out var token))
            {
                return new Azure.Core.AccessToken(token.Value, token.Expires);
            }
            else
            {
                throw new Exception("Could not get an access token");
            }
        }
    }
}
