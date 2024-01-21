using System;
using System.Collections.Generic;
using System.Linq;
using ZooIds;

namespace DifBot.Helpers;

public static class PseudonymGenerator
{
    public static readonly GeneratorConfig ZooIdsConfig = new(1, " ", CaseStyle.TitleCase);

    public static string GetRandomPseudonym()
    {
        var zoo = new ZooIdGenerator(ZooIdsConfig);

        return zoo.GenerateId();
    }

    public static string GetRandomPseudonym(int seed)
    {
        var zoo = new ZooIdGenerator(ZooIdsConfig, seed);

        return zoo.GenerateId();
    }

    public static (string Role, string RoleColor) GetRandomRole(int seed)
    {
        var fakeRoles = new Dictionary<string, string>()
        {
            { "Beginner", "#8be671" },
            { "Intermediate", "#38a263" },
            { "Advanced", "#3ba29e" },
            { "Teacher", "#2c91b4" },
        };

        var randomGenerator = new Random(seed);

        var index = randomGenerator.Next(0, fakeRoles.Count);
        var fakeRole = fakeRoles.ElementAt(index);

        return (fakeRole.Key, fakeRole.Value);
    }
}
