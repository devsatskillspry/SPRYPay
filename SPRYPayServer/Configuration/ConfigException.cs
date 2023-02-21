using System;

namespace SPRYPayServer.Configuration
{
    public class ConfigException : Exception
    {
        public ConfigException(string message) : base(message)
        {

        }
    }
}
