namespace win_restore.Models
{
    public class CopyProgress
    {
        public string ItemName { get; set; }
        public string CurrentFile { get; set; }
        public string Status { get; set; }
        public int ItemIndex { get; set; }
        public int TotalItems { get; set; }
    }
}
