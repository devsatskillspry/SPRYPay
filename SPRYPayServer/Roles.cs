using System.Collections.Generic;
using System;
using System.Linq;

namespace SPRYPayServer
{
    public class Roles
    {
        public const string ServerAdmin = "ServerAdmin";
        public static bool HasServerAdmin(IList<string> roles)
        {
            return roles.Contains(Roles.ServerAdmin, StringComparer.Ordinal);
        }
    }
}
