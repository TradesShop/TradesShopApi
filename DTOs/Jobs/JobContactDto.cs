namespace TradePlatform.Api.DTOs.Jobs
{


    public class JobContactBase {
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
    }

    public class JobContactDto: JobContactBase
    {
        public Guid contact_id { get; set; }
        public Guid job_id { get; set; }
        public Guid? user_id { get; set; }        
       
    }


}
