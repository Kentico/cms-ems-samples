using System;

namespace DancingGoat.Models.News
{
    public class NewsViewModel : IRepetitivePageViewModel
    {
        public Guid PageGuid { get; set; }

        public string Title { get; set; }

        public string NewsText { get; set; }

        public DateTime PublicationDate { get; set; }

        public string UrlSlug { get; set; }
    }
}