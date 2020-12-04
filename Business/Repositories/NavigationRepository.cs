﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using CMS.DocumentEngine;
using Kentico.Content.Web.Mvc;

using XperienceAdapter.Models;
using XperienceAdapter.Repositories;
using Business.Models;
using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.DocumentEngine.Routing;

namespace Business.Repositories
{
    public class NavigationRepository : INavigationRepository
    {
        private const string RootPath = "/";

        private static readonly string[] NodeOrdering = new string[] { "NodeLevel", "NodeOrder" };

        private readonly IPageUrlRetriever _pageUrlRetriever;

        private readonly IPageRepository<BasePage, TreeNode> _basePageRepository;

        private readonly IPageRepository<BasicPageWithUrlSlug, TreeNode> _urlSlugPageRepository;

        private readonly ISiteCultureRepository _cultureRepository;

        private readonly SiteInfoIdentifier _siteInfoIdentifier = new SiteInfoIdentifier(SiteContext.CurrentSiteID);

        private readonly string[] _slugEnabledPageTypes = new string[]
        {
            CMS.DocumentEngine.Types.MedioClinic.HomePage.CLASS_NAME,
            CMS.DocumentEngine.Types.MedioClinic.BasicPageWithUrlSlug.CLASS_NAME,
            CMS.DocumentEngine.Types.MedioClinic.SiteSection.CLASS_NAME,
            CMS.DocumentEngine.Types.MedioClinic.NamePerexText.CLASS_NAME,
            CMS.DocumentEngine.Types.MedioClinic.User.CLASS_NAME,
            CMS.DocumentEngine.Types.MedioClinic.Doctor.CLASS_NAME
        };

        private PageRoutingModeEnum RoutingMode => PageRoutingHelper.GetRoutingMode(_siteInfoIdentifier);

        // TODO: Call the -byculture sibling method instead.
        private NavigationItem RootDto => _basePageRepository.GetPagesInCurrentCulture(query =>
            query
                .Path(RootPath, PathTypeEnum.Single)
                .CombineWithDefaultCulture()
                .TopN(1),
            buildCacheAction: cache => cache
                .Key($"{nameof(NavigationRepository)}|{nameof(RootDto)}"))
                .Select(basePageDto => new NavigationItem
                {
                    NodeId = basePageDto.NodeId,
                    Name = basePageDto.Name
                })
                .FirstOrDefault();

        public NavigationRepository(
            IPageUrlRetriever pageUrlRetriever,
            IPageRepository<BasePage, TreeNode> basePageRepository,
            IPageRepository<BasicPageWithUrlSlug, TreeNode> urlSlugPageRepository,
            ISiteCultureRepository siteCultureRepository)
        {
            _pageUrlRetriever = pageUrlRetriever ?? throw new ArgumentNullException(nameof(pageUrlRetriever));
            _basePageRepository = basePageRepository ?? throw new ArgumentNullException(nameof(basePageRepository));
            _urlSlugPageRepository = urlSlugPageRepository ?? throw new ArgumentNullException(nameof(urlSlugPageRepository));
            _cultureRepository = siteCultureRepository ?? throw new ArgumentNullException(nameof(siteCultureRepository));
        }

        //TODO: Why the dictionary with cultureCode key?
        public Dictionary<SiteCulture, NavigationItem> GetNavigation() =>
            RoutingMode == PageRoutingModeEnum.BasedOnContentTree
                ? GetContentTreeNavigation()
                : GetConventionalRoutingNavigation();

        private Dictionary<SiteCulture, NavigationItem> GetContentTreeNavigation()
        {
            var cacheKeySuffix = $"{nameof(GetContentTreeNavigation)}";
            GetInputData(out IEnumerable<SiteCulture> cultures, out Dictionary<SiteCulture, NavigationItem> cultureSpecificNavigations);

            if (cultures != null && cultures.Any())
            {
                foreach (var culture in cultures)
                {
                    var allItems = _basePageRepository.GetPagesByTypeAndCulture(
                        _slugEnabledPageTypes,
                        culture,
                        $"{nameof(NavigationRepository)}|{nameof(GetContentTreeNavigation)}|{culture.IsoCode}",
                        filter => GetDefaultQuery(filter)
                            .FilterDuplicates()
                            .OrderByAscending(NodeOrdering)
                            .MenuItems())
                            .Select(basePage => MapBaseToNavigationDto(basePage));

                    var decorated = DecorateItems(RootDto, allItems, GetContentTreeBasedUrl);

                    cultureSpecificNavigations.Add(culture, decorated);
                }
            }

            return cultureSpecificNavigations;
        }

        private Dictionary<SiteCulture, NavigationItem> GetConventionalRoutingNavigation()
        {
            throw new NotImplementedException();
            //string cacheKeySuffix = $"{nameof(GetConventionalRoutingNavigation)}";
            //GetInputData(out IEnumerable<SiteCulture> cultures, out Dictionary<SiteCulture, NavigationItem> cultureSpecificNavigations);

            //if (cultures.Any())
            //{
            //    foreach (var culture in cultures)
            //    {
            //        var allItems = _urlSlugPageRepository.GetPagesByTypeAndCulture(
            //            _slugEnabledPageTypes,
            //            culture: culture,
            //            filter: query => query
            //                .FilterDuplicates()
            //                .OrderByAscending(NodeOrdering),
            //            buildCacheAction: cache => GetCacheBuilder(cache, $"{cacheKeySuffix}", RootPath, PathTypeEnum.Section, culture))
            //                .Select(dto => new NavigationItem
            //                {
            //                    NodeId = dto.NodeId,
            //                    Guid = dto.Guid,
            //                    ParentId = dto.ParentId,
            //                    Name = dto.Name,
            //                    NodeAliasPath = dto.NodeAliasPath,
            //                    Culture = dto.Culture,
            //                    UrlSlug = dto.UrlSlug
            //                });

            //        var decorated = DecorateItems(RootDto, allItems, GetConventionalRoutingUrl);

            //        cultureSpecificNavigations.Add(culture, decorated);
            //    }
            //}

            //return cultureSpecificNavigations;
        }

        public string? GetUrlByNodeId(int nodeId, SiteCulture pageCulture)
        {
            var navigation = RoutingMode == PageRoutingModeEnum.BasedOnContentTree
                ? GetContentTreeNavigation()[pageCulture]
                : GetConventionalRoutingNavigation()[pageCulture];

            return GetNavigationItemByNodeId(nodeId, navigation)?.RelativeUrl;
        }

        public NavigationItem? GetNavigationItemByNodeId(int nodeId, NavigationItem startPointItem)
        {
            if (startPointItem != null)
            {
                if (startPointItem.NodeId == nodeId)
                {
                    return startPointItem;
                }
                else
                {
                    var matches = new List<NavigationItem>();

                    foreach (var child in startPointItem.ChildItems)
                    {
                        var childMatch = GetNavigationItemByNodeId(nodeId, child);
                        matches.Add(childMatch!);
                    }

                    return matches.FirstOrDefault(match => match != null);
                }
            }

            return null;
        }

        /// <summary>
        /// Prepares cultures and a new dictionary for navigation sets.
        /// </summary>
        /// <param name="cultures">All site cultures.</param>
        /// <param name="cultureSpecificNavigations">Empty dictionary with navigation sets for each culture.</param>
        private void GetInputData(out IEnumerable<SiteCulture> cultures, out Dictionary<SiteCulture, NavigationItem> cultureSpecificNavigations)
        {
            cultures = _cultureRepository.GetAll();
            cultureSpecificNavigations = new Dictionary<SiteCulture, NavigationItem>();
        }

        /// <summary>
        /// Maps the <see cref="BasePage"/> onto a new <see cref="NavigationItem"/>.
        /// </summary>
        /// <param name="dto">The input DTO.</param>
        /// <returns>The <see cref="NavigationItem"/>.</returns>
        private static NavigationItem MapBaseToNavigationDto(BasePage dto) => new NavigationItem
        {
            NodeId = dto.NodeId,
            Guid = dto.Guid,
            Name = dto.Name,
            NodeAliasPath = dto.NodeAliasPath,
            ParentId = dto.ParentId,
            Culture = dto.Culture
        };

        /// <summary>
        /// Gets default <see cref="DocumentQuery{TDocument}"/> configuration.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The modified query.</returns>
        private static MultiDocumentQuery GetDefaultQuery(MultiDocumentQuery query) =>
            query
                .FilterDuplicates()
                .OrderByAscending(NodeOrdering);

        /// <summary>
        /// Decorates items with references and URLs.
        /// </summary>
        /// <param name="root">Root navigation item.</param>
        /// <param name="defaultCultureItems">A flat sequence of all other items.</param>
        /// <returns></returns>
        public NavigationItem DecorateItems(NavigationItem root, IEnumerable<NavigationItem> navigationItems, Func<NavigationItem, string?> urlDecorator)
        {
            var connectableItems = GetConnectableItems(navigationItems.Concat(new[] { root })).ToList();

            return BuildHierarchyLevel(root, connectableItems, urlDecorator);
        }

        /// <summary>
        /// Filters out items with orphaned <see cref="NavigationItem.ParentId"/> values.
        /// </summary>
        /// <param name="navigationItems">Navigation items.</param>
        /// <returns></returns>
        public IEnumerable<NavigationItem> GetConnectableItems(IEnumerable<NavigationItem> navigationItems) =>
            from navigationItem in navigationItems
            where navigationItems.Select(item => item?.NodeId).Contains(navigationItem.ParentId.GetValueOrDefault())
            select navigationItem;

        /// <summary>
        /// Decorates items with references to parents and children.
        /// </summary>
        /// <param name="parent">Current parent item.</param>
        /// <param name="allItems">A flat sequence of all items.</param>
        /// <returns>Hierarchical navigation item.</returns>
        public NavigationItem BuildHierarchyLevel(NavigationItem parent, IEnumerable<NavigationItem> allItems, Func<NavigationItem, string?> urlDecorator)
        {
            var children = allItems
                .Where(item => item.ParentId.HasValue && item.ParentId == parent.NodeId);

            parent.ChildItems.AddRange(children);

            foreach (var item in children)
            {
                if (item != parent && !parent.AllParents.Contains(item))
                {
                    item.Parent = parent;
                    item.AllParents.AddRange(parent.AllParents);
                    item.AllParents.Add(parent);
                    item.RelativeUrl = urlDecorator(item);
                    BuildHierarchyLevel(item, allItems, urlDecorator);
                }
            }

            return parent;
        }

        /// <summary>
        /// Gets URL for a content tree-based navigation item.
        /// </summary>
        /// <param name="item">Item to get the URL for.</param>
        /// <returns>URL.</returns>
        private string? GetContentTreeBasedUrl(NavigationItem item) => GetPageUrl(item);

        /// <summary>
        /// Gets URL for conventional routing.
        /// </summary>
        /// <param name="item">Item to get the URL for.</param>
        /// <returns>URL.</returns>
        private string GetConventionalRoutingUrl(NavigationItem item)
        {
            var patternBasedUrl = GetPageUrl(item);

            if (string.IsNullOrEmpty(patternBasedUrl))
            {
                var trailingPath = string.Join('/', item.AllParents.Concat(new[] { item }).Select(item => item.UrlSlug));
                var culture = item.Culture ?? _cultureRepository.DefaultSiteCulture;

                return $"~/{culture?.IsoCode?.ToLowerInvariant()}{trailingPath}/";
            }

            return patternBasedUrl;
        }

        private string? GetPageUrl(NavigationItem item)
        {
            var culture = item?.Culture?.IsoCode;

            try
            {
                var url = _pageUrlRetriever.Retrieve(item?.NodeAliasPath, culture)?.RelativePath?.ToLowerInvariant()!;

                return url;
            }
            catch
            {
                return null;
            }

        }
    }
}
