using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movers_Scheduling_Program
{
    public static class SessionManager
    {
        public static string Username { get; set; }
        public static string FirstName { get; set; }
        public static string SecondName { get; set; }
        public static string Email { get; set; }
        public static string PhoneNo { get; set; }
        public static string Role { get; set; }

        public static void InitializeSession(string username, string firstName, string secondName, string email, string phoneNo, string role)
        {
            Username = username;
            FirstName = firstName;
            SecondName = secondName;
            Email = email;
            PhoneNo = phoneNo;
            Role = role;
        }

        public static void ClearSession()
        {
            Username = null;
            FirstName = null;
            SecondName = null;
            Email = null;
            PhoneNo = null;
            Role = null;
        }
    }
}
