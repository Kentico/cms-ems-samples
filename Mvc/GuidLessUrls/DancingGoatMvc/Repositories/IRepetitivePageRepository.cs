using CMS.DocumentEngine;

namespace DancingGoat.Repositories
{
    // TODO: Document
    public interface IRepetitivePageRepository : IRepository
    {
        DocumentQuery<TPage> GetPages<TPage>() where TPage : TreeNode, new();

        DocumentQuery<TPage> GetPage<TPage>(string urlSlug) where TPage : TreeNode, new();
    }
}