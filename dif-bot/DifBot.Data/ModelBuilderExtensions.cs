using System;
using System.Linq;
using DifBot.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace DifBot.Data;

internal static class ModelBuilderExtensions
{
    internal static void ConfigureEnumLookupTable<TEnum>(this ModelBuilder modelBuilder, string? tableName = null, string? schema = null)
        where TEnum : Enum
    {
        var enumLookup = typeof(EnumLookup<>).MakeGenericType(typeof(TEnum));

        var data = Enum.GetValues(typeof(TEnum)).Cast<object>()
            .Select(x => Activator.CreateInstance(enumLookup, new object[] { x }))
            .ToArray();

        var mb = modelBuilder.Entity(enumLookup);

        mb.Property(nameof(EnumLookup<TEnum>.Id))
            .ValueGeneratedNever();

        if (schema != null && tableName == null)
        {
            throw new InvalidOperationException("If schema is specified, tableName must also be specified");
        }

        if (tableName != null)
        {
            if (schema != null)
            {
                mb.ToTable(tableName, schema);
            }
            else
            {
                mb.ToTable(tableName);
            }
        }

        mb.HasData(data!);
    }
}
