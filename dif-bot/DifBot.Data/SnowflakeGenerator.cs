using IdGen;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace DifBot.Data;

public class SnowflakeGenerator : ValueGenerator<ulong>
{
    private static IdGenerator? _generator;

    public override bool GeneratesTemporaryValues => false;

    private static IdGenerator Generator => _generator ??= new IdGenerator(0);

    public override ulong Next(EntityEntry entry)
    {
        return (ulong)Generator.CreateId();
    }
}
