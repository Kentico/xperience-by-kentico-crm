﻿using CMS.Websites;

using DancingGoat;
using DancingGoat.Controllers;
using DancingGoat.Models;

using Kentico.Content.Web.Mvc;
using Kentico.Content.Web.Mvc.Routing;
using Kentico.PageBuilder.Web.Mvc.PageTemplates;

using Microsoft.AspNetCore.Mvc;

[assembly: RegisterWebPageRoute(ArticlesSection.CONTENT_TYPE_NAME, typeof(DancingGoatArticleController), WebsiteChannelNames = new[] { DancingGoatConstants.WEBSITE_CHANNEL_NAME })]
[assembly: RegisterWebPageRoute(ArticlePage.CONTENT_TYPE_NAME, typeof(DancingGoatArticleController), WebsiteChannelNames = new[] { DancingGoatConstants.WEBSITE_CHANNEL_NAME }, ActionName = "Article")]

namespace DancingGoat.Controllers
{
    public class DancingGoatArticleController : Controller
    {
        private readonly ArticlePageRepository articlePageRepository;
        private readonly ArticlesSectionRepository articlesSectionRepository;
        private readonly IWebPageUrlRetriever urlRetriever;
        private readonly IWebPageDataContextRetriever webPageDataContextRetriever;
        private readonly IPreferredLanguageRetriever currentLanguageRetriever;


        public DancingGoatArticleController(
            ArticlePageRepository articlePageRepository,
            ArticlesSectionRepository articlesSectionRepository,
            IWebPageUrlRetriever urlRetriever,
            IWebPageDataContextRetriever webPageDataContextRetriever,
            IPreferredLanguageRetriever currentLanguageRetriever)
        {
            this.articlePageRepository = articlePageRepository;
            this.articlesSectionRepository = articlesSectionRepository;
            this.urlRetriever = urlRetriever;
            this.webPageDataContextRetriever = webPageDataContextRetriever;
            this.currentLanguageRetriever = currentLanguageRetriever;
        }


        public async Task<IActionResult> Index()
        {
            var languageName = currentLanguageRetriever.Get();

            var webPage = webPageDataContextRetriever.Retrieve().WebPage;

            var articlesSection = await articlesSectionRepository.GetArticlesSection(webPage.WebPageItemID, languageName, HttpContext.RequestAborted);

            var articles = await articlePageRepository.GetArticles(articlesSection.SystemFields.WebPageItemTreePath, languageName, true, cancellationToken: HttpContext.RequestAborted);

            var models = new List<ArticleViewModel>();
            foreach (var article in articles)
            {
                var model = await ArticleViewModel.GetViewModel(article, urlRetriever, languageName);
                models.Add(model);
            }

            return View(models);
        }


        public async Task<IActionResult> Article()
        {
            var languageName = currentLanguageRetriever.Get();
            var webPageItemId = webPageDataContextRetriever.Retrieve().WebPage.WebPageItemID;

            var article = await articlePageRepository.GetArticle(webPageItemId, languageName, HttpContext.RequestAborted);

            if (article is null)
            {
                return NotFound();
            }

            var model = await ArticleDetailViewModel.GetViewModel(article, languageName, articlePageRepository, urlRetriever);

            return new TemplateResult(model);
        }
    }
}
