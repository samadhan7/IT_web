namespace GTL.Models
{
    public class LoginResponse
    {
        public int ResponseCode { get; set; }
        public string? ResponseMessage { get; set; }
        public string? email { get; set; }
        public string? Role { get; set; }
        public int? UserId { get; set; }  
    }

}
