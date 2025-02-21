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
        public static bool HigherAccess { get; set; }
        public static bool IsLoggedIn { get; set; } = false;

    }
}
