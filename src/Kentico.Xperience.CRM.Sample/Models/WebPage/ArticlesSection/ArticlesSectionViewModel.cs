namespace DancingGoat.Models
{
    public record ArticlesSectionViewModel(IEnumerable<ArticleViewModel> Articles, string ArticlesPath)
    {
        /// <summary>
        /// Maps <see cref=Cafe"/> to a <see cref="CafeViewModel"/>.
        /// </summary>
        public static ArticlesSectionViewModel GetViewModel(IEnumerable<ArticleViewModel> Articles, string ArticlesPath) => new ArticlesSectionViewModel(Articles, ArticlesPath);
    }
}
