using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Stl.Fusion;

namespace DeadLetterQueueHelper.State
{
    public class AccessTokenService : IComputeService
    {
        private readonly IAccessTokenProvider _accessTokenProvider;

        public AccessTokenService(IAccessTokenProvider accessTokenProvider)
        {
            _accessTokenProvider = accessTokenProvider;
        }

        [ComputeMethod]
        public async Task<Azure.Core.AccessToken?> GetAccessToken()
        {
            var result = await _accessTokenProvider.RequestAccessToken();
            if (result.TryGetToken(out var token))
            {
                return new Azure.Core.AccessToken(token.Value, token.Expires);
            }

            // TODO something about redirecting..? maybe fix some day
            return null;
        }
}
}
