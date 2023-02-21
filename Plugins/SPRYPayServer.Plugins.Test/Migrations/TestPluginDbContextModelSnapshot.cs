﻿// <auto-generated />
using System;
using SPRYPayServer.Plugins.Test;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SPRYPayServer.Plugins.Test.Migrations
{
    [DbContext(typeof(TestPluginDbContext))]
    partial class TestPluginDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("SPRYPayServer.Plugins.Test")
                .HasAnnotation("ProductVersion", "3.1.10");

            modelBuilder.Entity("SPRYPayServer.Plugins.Test.Data.TestPluginData", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("TestPluginRecords");
                });
#pragma warning restore 612, 618
        }
    }
}
