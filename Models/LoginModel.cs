namespace TMS_Project.Models
{
    public class LoginModel
    {


        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }=false;
        public bool isInvalid { get; set; }=false;
        public string InvalidMessage { get; set; }="";

    }
}
