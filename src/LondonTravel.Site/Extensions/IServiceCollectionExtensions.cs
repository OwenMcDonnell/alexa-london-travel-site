// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Extensions
{
    using System.Linq;
    using Identity.Amazon;
    using MartinCostello.LondonTravel.Site.Identity;
    using Microsoft.AspNetCore.ApplicationInsights.HostingStartup;
    using Microsoft.AspNetCore.Authentication.OAuth;
    using Microsoft.AspNetCore.Authentication.Twitter;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Options;

    /// <summary>
    /// A class containing extension methods for the <see cref="IServiceCollection"/> interface. This class cannot be inherited.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Removes the registered Application Insights JavaScript tag helper.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
        /// <returns>
        /// The <see cref="IServiceCollection"/> specified by <paramref name="services"/>.
        /// </returns>
        public static IServiceCollection RemoveApplicationInsightsTagHelper(this IServiceCollection services)
        {
            // See https://github.com/aspnet/AzureIntegration/issues/88
            var copy = services
                .Where((p) => p.ImplementationType == typeof(JavaScriptSnippetTagHelperComponent))
                .ToArray();

            foreach (var item in copy)
            {
                services.Remove(item);
            }

            return services;
        }

        /// <summary>
        /// Configures identity services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
        /// <returns>
        /// The <see cref="IServiceCollection"/> specified by <paramref name="services"/>.
        /// </returns>
        public static IServiceCollection AddIdentity(this IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<SiteOptions>();

            if (options?.Authentication?.IsEnabled == true)
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var builder = services.AddAuthentication();

                if (TryGetProvider("Amazon", options, out ExternalSignInOptions amazonOptions))
                {
                    builder.AddAmazon((p) => SetupOAuth(p, amazonOptions, loggerFactory));
                }

                if (TryGetProvider("Facebook", options, out ExternalSignInOptions facebookOptions))
                {
                    builder.AddFacebook((p) => SetupOAuth(p, facebookOptions, loggerFactory));
                }

                if (TryGetProvider("Google", options, out ExternalSignInOptions googleOptions))
                {
                    builder.AddGoogle((p) => SetupOAuth(p, googleOptions, loggerFactory));
                }

                if (TryGetProvider("Microsoft", options, out ExternalSignInOptions microsoftOptions))
                {
                    builder.AddMicrosoftAccount((p) => SetupOAuth(p, microsoftOptions, loggerFactory));
                }

                if (TryGetProvider("Twitter", options, out ExternalSignInOptions twitterOptions))
                {
                    builder.AddTwitter(
                        (p) =>
                        {
                            p.ConsumerKey = twitterOptions.ClientId;
                            p.ConsumerSecret = twitterOptions.ClientSecret;
                            p.RetrieveUserDetails = true;

                            if (p.Events is TwitterEvents twitterEvents)
                            {
                                twitterEvents.OnRemoteFailure =
                                    (context) => OAuthEventsHandler.HandleRemoteFailure(
                                        context,
                                        p.SignInScheme,
                                        p.StateDataFormat,
                                        loggerFactory.CreateLogger("Twitter"),
                                        (token) => token?.Properties?.Items);
                            }
                        });
                }
            }

            return services;
        }

        /// <summary>
        /// Sets up an instance of <typeparamref name="T"/> for an OAuth provider.
        /// </summary>
        /// <typeparam name="T">The type of the OAuth options to set up.</typeparam>
        /// <param name="auth">The OAuth options to set up.</param>
        /// <param name="options">The <see cref="ExternalSignInOptions"/> to use to set up the instance.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use.</param>
        private static void SetupOAuth<T>(T auth, ExternalSignInOptions options, ILoggerFactory loggerFactory)
            where T : OAuthOptions
        {
            auth.ClientId = options.ClientId;
            auth.ClientSecret = options.ClientSecret;
            auth.Events = new OAuthEventsHandler(auth, loggerFactory);
        }

        /// <summary>
        /// Tries to get the external sign-in settings for the specified provider.
        /// </summary>
        /// <param name="name">The name of the provider to get the provider settings for.</param>
        /// <param name="options">The current site options.</param>
        /// <param name="provider">When the method returns, containsint the provider settings, if enabled.</param>
        /// <returns>
        /// <see langword="true"/> if the specified provider is enabled; otherwise <see langword="false"/>.
        /// </returns>
        private static bool TryGetProvider(string name, SiteOptions options, out ExternalSignInOptions provider)
        {
            provider = null;
            ExternalSignInOptions signInOptions = null;

            bool isEnabled =
                options?.Authentication?.ExternalProviders?.TryGetValue(name, out signInOptions) == true &&
                signInOptions?.IsEnabled == true &&
                !string.IsNullOrEmpty(signInOptions?.ClientId) &&
                !string.IsNullOrEmpty(signInOptions?.ClientSecret);

            if (isEnabled)
            {
                provider = signInOptions;
            }

            return isEnabled;
        }
    }
}
