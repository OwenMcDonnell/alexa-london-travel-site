// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Services
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MartinCostello.LondonTravel.Site.Identity;
    using MartinCostello.LondonTravel.Site.Services.Data;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;

    public class AccountService : IAccountService
    {
        /// <summary>
        /// The <see cref="IDocumentClient"/> to use. This field is read-only.
        /// </summary>
        private readonly IDocumentClient _client;

        /// <summary>
        /// The <see cref="IMemoryCache"/> to use. This field is read-only.
        /// </summary>
        private readonly IMemoryCache _cache;

        /// <summary>
        /// The <see cref="ILogger"/> to use. This field is read-only.
        /// </summary>
        private readonly ILogger<AccountService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountService"/> class.
        /// </summary>
        /// <param name="client">The <see cref="IDocumentClient"/> to use.</param>
        /// <param name="cache">The <see cref="IMemoryCache"/> to use.</param>
        /// <param name="logger">The <see cref="ILogger"/> to use.</param>
        public AccountService(IDocumentClient client, IMemoryCache cache, ILogger<AccountService> logger)
        {
            _client = client;
            _cache = cache;
            _logger = logger;
        }

        public async Task<LondonTravelUser> GetUserByAccessTokenAsync(string accessToken, CancellationToken cancellationToken)
        {
            LondonTravelUser user = null;

            if (!string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    user = (await _client.GetAsync<LondonTravelUser>((p) => p.AlexaToken == accessToken, cancellationToken)).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    _logger.LogError(default, ex, "Failed to find user by access token.");
                    throw;
                }
            }

            return user;
        }

        public Task<long> GetUserCountAsync(bool useCache)
        {
            return useCache ? GetUserCountFromCacheAsync() : GetUserCountFromDocumentStoreAsync();
        }

        private async Task<long> GetUserCountFromCacheAsync()
        {
            const string CacheKey = "DocumentStore.Count";

            if (!_cache.TryGetValue(CacheKey, out long count))
            {
                count = await GetUserCountFromDocumentStoreAsync();

                _cache.Set(CacheKey, count, TimeSpan.FromHours(12));
            }

            return count;
        }

        private Task<long> GetUserCountFromDocumentStoreAsync()
        {
            return _client.GetDocumentCountAsync();
        }
    }
}
