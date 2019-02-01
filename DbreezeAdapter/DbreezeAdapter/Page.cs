namespace DbreezeAdapter
{
    public class Page
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int Skip
        {
            get
            {
                return PageNumber * PageSize;
            }
        }
    }
}
