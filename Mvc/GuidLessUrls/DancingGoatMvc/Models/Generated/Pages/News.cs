using System;

namespace CMS.DocumentEngine.Types.DancingGoatMvc
{
    public partial class News
    {
        public DateTime PublicationDate
        {
            get
            {
                return GetDateTimeValue("DocumentPublishFrom", GetDateTimeValue("DocumentCreatedWhen", DateTime.MinValue));
            }
        }
    }
}