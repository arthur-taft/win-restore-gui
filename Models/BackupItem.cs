namespace win_restore.Models
{
    public class BackupItem
    {
        public string Name { get; set; }
        public string Src { get; set; }
        public string Dest { get; set; }
        public bool Enabled { get; set; }
    }
}
