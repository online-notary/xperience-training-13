﻿#define no_suffix

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;

using XperienceAdapter.Extensions;
using XperienceAdapter.Models;
using XperienceAdapter.Repositories;
using Business.Models;
using Business.Repositories;

namespace MedioClinic.ViewComponents
{
    public class CultureSwitch : ViewComponent
    {
        private readonly INavigationRepository _navigationRepository;

        private readonly ISiteCultureRepository _siteCultureRepository;

        public CultureSwitch(ISiteCultureRepository siteCultureRepository, INavigationRepository navigationRepository)
        {
            _siteCultureRepository = siteCultureRepository ?? throw new ArgumentNullException(nameof(siteCultureRepository));
            _navigationRepository = navigationRepository ?? throw new ArgumentNullException(nameof(navigationRepository));
        }

        public IViewComponentResult Invoke(string cultureSwitchId)
        {
            var variants = GetUrlCultureVariants();
            var model = (cultureSwitchId, variants.ToDictionary(kvp1 => kvp1.Key, kvp2 => kvp2.Value));

            return View(model);
        }

        private IEnumerable<KeyValuePair<SiteCulture, string>>? GetUrlCultureVariants()
        {
            var defaultCulture = _siteCultureRepository.DefaultSiteCulture;
            var searchPath = Request.Path.Equals("/") && defaultCulture != null ? $"/{defaultCulture.IsoCode?.ToLowerInvariant()}/home/" : Request.Path.Value;
            var currentCulture = Thread.CurrentThread.CurrentUICulture.ToSiteCulture();

            if (currentCulture != null)
            {
                return GetDatabaseUrlVariants(searchPath, currentCulture) ?? GetNonDatabaseUrlVariants(searchPath);
            }

            return null;
        }

        private IEnumerable<KeyValuePair<SiteCulture, string>>? GetDatabaseUrlVariants(string searchPath, SiteCulture currentCulture)
        {
            var navigation = _navigationRepository.GetNavigation();
            var currentPageNavigationItem = GetNavigationItemByRelativeUrl(searchPath, navigation[currentCulture]);

            if (currentPageNavigationItem != null)
            {
                var databaseVariants = new List<KeyValuePair<SiteCulture, NavigationItem>>();
                databaseVariants.Add(new KeyValuePair<SiteCulture, NavigationItem>(currentCulture, currentPageNavigationItem));

                foreach (var cultureVariant in navigation.Where(cultureVariant => !cultureVariant.Key.Equals(currentCulture)))
                {
                    var otherCultureNavigationItem = GetNavigationItemByNodeId(currentPageNavigationItem.NodeId, cultureVariant.Value);

                    if (otherCultureNavigationItem != null)
                    {
                        databaseVariants.Add(new KeyValuePair<SiteCulture, NavigationItem>(cultureVariant.Key, otherCultureNavigationItem));
                    }
                }

                return databaseVariants.Select(variant => new KeyValuePair<SiteCulture, string>(variant.Key, variant.Value.RelativeUrl!));
            }

            return null;
        }

        private NavigationItem? GetNavigationItemByRelativeUrl(string searchPath, NavigationItem startPointItem)
        {
            if (startPointItem != null)
            {
                var parsed = Url.Content(startPointItem.RelativeUrl);

                if (parsed?.Equals(searchPath, StringComparison.OrdinalIgnoreCase) == true)
                {
                    return startPointItem;
                }
                else if (startPointItem.ChildItems?.Any() == true)
                {
                    foreach (var child in startPointItem.ChildItems)
                    {
                        return GetNavigationItemByRelativeUrl(searchPath, child);
                    }
                }
            }

            return null;
        }

        private NavigationItem? GetNavigationItemByNodeId(int nodeId, NavigationItem startPointItem) =>
            startPointItem.NodeId == nodeId
                ? startPointItem
                : startPointItem.ChildItems.FirstOrDefault(child => GetNavigationItemByNodeId(nodeId, child) != null);

        private IEnumerable<KeyValuePair<SiteCulture, string>>? GetNonDatabaseUrlVariants(string searchPath)
        {
            var cultures = _siteCultureRepository.GetAll();
            var segments = searchPath.Split('/');

            if (cultures.Any(culture => culture.IsoCode?.Equals(segments?[1], StringComparison.InvariantCultureIgnoreCase) == true))
            {
                var trailingPath = string.Join('/', segments.Skip(2));

                return cultures.Select(culture => new KeyValuePair<SiteCulture, string>(culture, $"/{culture.IsoCode?.ToLower()}/{trailingPath}"));
            }

            return null;
        }
    }
}