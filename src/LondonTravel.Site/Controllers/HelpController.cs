﻿// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Controllers
{
    using System.Threading.Tasks;
    using Identity;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Models;

    /// <summary>
    /// A class representing the controller for the <c>/help/</c> resource.
    /// </summary>
    public class HelpController : Controller
    {
        /// <summary>
        /// The <see cref="UserManager{TUser}"/> to use. This field is read-only.
        /// </summary>
        private readonly UserManager<LondonTravelUser> _userManager;

        /// <summary>
        /// The <see cref="ILogger"/> to use. This field is read-only.
        /// </summary>
        private readonly ILogger<HelpController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HelpController"/> class.
        /// </summary>
        /// <param name="userManager">The <see cref="UserManager{TUser}"/> to use.</param>
        /// <param name="logger">The <see cref="ILogger"/> to use.</param>
        public HelpController(UserManager<LondonTravelUser> userManager, ILogger<HelpController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Gets the result for the <c>/help/</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/help/</c> action.
        /// </returns>
        [HttpGet]
        [Route("help", Name = SiteRoutes.Help)]
        public async Task<IActionResult> Index()
        {
            var model = new HelpViewModel()
            {
                IsSignedIn = User.Identity.IsAuthenticated,
            };

            if (model.IsSignedIn)
            {
                var user = await _userManager.GetUserAsync(User);

                model.HasFavorites = user?.FavoriteLines?.Count > 0;
                model.IsLinkedToAlexa = !string.IsNullOrEmpty(user?.AlexaToken);
            }

            return View(model);
        }

        /// <summary>
        /// Gets the result for the <c>/support/</c> action.
        /// </summary>
        /// <returns>
        /// The result for the <c>/support/</c> action.
        /// </returns>
        [HttpGet]
        [Route("support")]
        public IActionResult Support() => RedirectToRoute(SiteRoutes.Help);
    }
}
