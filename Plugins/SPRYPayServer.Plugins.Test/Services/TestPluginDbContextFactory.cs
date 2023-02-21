using SPRYPayServer.Abstractions.Contracts;
using SPRYPayServer.Abstractions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;

namespace SPRYPayServer.Plugins.Test
{

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TestPluginDbContext>
    {
        public TestPluginDbContext CreateDbContext(string[] args)
        {

            var builder = new DbContextOptionsBuilder<TestPluginDbContext>();

            builder.UseSqlite("Data Source=temp.db");

            return new TestPluginDbContext(builder.Options, true);
        }
    }

    public class TestPluginDbContextFactory : BaseDbContextFactory<TestPluginDbContext>
    {
        public TestPluginDbContextFactory(IOptions<DatabaseOptions> options) : base(options, "SPRYPayServer.Plugins.Test")
        {
        }

        public override TestPluginDbContext CreateContext()
        {
            var builder = new DbContextOptionsBuilder<TestPluginDbContext>();
            ConfigureBuilder(builder);
            return new TestPluginDbContext(builder.Options);

        }
    }
}
