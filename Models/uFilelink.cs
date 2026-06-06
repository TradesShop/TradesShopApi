namespace TradePlatform.Api.Models
{
    public class uFilelink
    {
        public Guid id { get; set; }
        public Guid file_id { get; set; }
        public int entity_type { get; set; }
        public Guid entity_id { get; set; }
        public string upload_type { get; set; }
        public string work_stage { get; set; }
        public bool is_primary { get; set; }
        public bool is_verified { get; set; }
        public DateTime created_at { get; set; }
    }
}
