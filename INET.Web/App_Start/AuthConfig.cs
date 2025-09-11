using WebMatrix.WebData;

namespace INET.Web
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            WebSecurity.InitializeDatabaseConnection("MembershipConnection", "UserProfile", "UserId", "UserName", autoCreateTables: true);
        }
    }
}
