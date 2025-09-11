using System;

namespace INET.Data
{
    public partial class UserProfile
    {
        public string FullName 
        {
            get 
            {
                return (this != null) ? String.Format("{0} {1}", Name, LastName) : null;
            }
        }

        public string EnabledToString
        {
            get 
            {
                return (this.IsEnabled) ? "Sí" : "No";
            }
        }

    }
}
