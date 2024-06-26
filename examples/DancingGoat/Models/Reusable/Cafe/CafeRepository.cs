﻿using CMS.ContentEngine;
using CMS.Helpers;
using CMS.Websites;
using CMS.Websites.Routing;

namespace DancingGoat.Models
{
    /// <summary>
    /// Represents a collection of cafes.
    /// </summary>
    public partial class CafeRepository : ContentRepositoryBase
    {
        private readonly ILinkedItemsDependencyAsyncRetriever linkedItemsDependencyRetriever;


        public CafeRepository(
            IWebsiteChannelContext websiteChannelContext,
            IContentQueryExecutor executor,
            IWebPageQueryResultMapper mapper,
            IProgressiveCache cache,
            ILinkedItemsDependencyAsyncRetriever linkedItemsDependencyRetriever)
            : base(websiteChannelContext, executor, mapper, cache)
        {
            this.linkedItemsDependencyRetriever = linkedItemsDependencyRetriever;
        }

        /// <summary>
        /// Returns an enumerable collection of company cafes ordered by a position in the content tree.
        /// </summary>
        /// <param name="count">The number of cafes to return. Use 0 as value to return all records.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task<IEnumerable<Cafe>> GetCompanyCafes(int count = 0, CancellationToken cancellationToken = default)
        {
            var queryBuilder = GetQueryBuilder(count);

            var cacheSettings = new CacheSettings(5, WebsiteChannelContext.WebsiteChannelName, nameof(CafeRepository), nameof(GetCompanyCafes), count);

            return await GetCachedQueryResult<Cafe>(queryBuilder, null, cacheSettings, GetDependencyCacheKeys, cancellationToken);
        }


        private static ContentItemQueryBuilder GetQueryBuilder(int count)
        {
            return new ContentItemQueryBuilder()
                    .ForContentType(Cafe.CONTENT_TYPE_NAME,
                    config => config
                        .WithLinkedItems(1)
                        .TopN(count)
                        .Where(where => where.WhereTrue(nameof(Cafe.CafeIsCompanyCafe))));
        }


        private async Task<ISet<string>> GetDependencyCacheKeys(IEnumerable<Cafe> cafes, CancellationToken cancellationToken)
        {
            var cafeIds = cafes.Select(cafe => cafe.SystemFields.ContentItemID);
            var dependencyCacheKeys = (await linkedItemsDependencyRetriever.Get(cafeIds, 1, cancellationToken))
                .ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            dependencyCacheKeys.Add(CacheHelper.BuildCacheItemName(new[] { "contentitem", "bycontenttype", Cafe.CONTENT_TYPE_NAME }, false));

            return dependencyCacheKeys;
        }
    }
}