using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace SPRYPayServer
{
    public static class EnumExtensions
    {
        public static string DisplayName(this Type enumType, string input) =>
            enumType.GetMember(input).First().GetCustomAttribute<DisplayAttribute>()?.Name ?? input;
    }
}
