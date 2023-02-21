using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SPRYPayServer.Plugins.Test.Data
{
    public class TestPluginData
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public DateTimeOffset Timestamp { get; set; }


    }
}
