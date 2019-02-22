using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MonteCristo.Application.Models.Constants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Enterprise = "Enterprise";

        // public const string Worker = "Worker"; default

        public static List<string> Values
        {
            get
            {
                return typeof(Roles).GetAllPublicConstantValues<string>();
            }
        }
    }
}
