using System;

namespace DifBot.Common.Models;

public class EnumLookup<T>
        where T : Enum
{
    public EnumLookup(T value)
    {
        Id = Convert.ToInt32(value);
        Name = value.ToString();
    }

    private EnumLookup()
    {
    }

    public int Id { get; set; }

    public string Name { get; set; } = default!;
}
