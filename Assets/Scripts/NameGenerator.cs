using UnityEngine;
using System.Collections.Generic;

class NameGenerator
{
    // Lists of possible name parts
    private List<string> prefixes = new List<string> {
        "Al", "Bel", "Cor", "Dor", "El", "Fen", "Gal", "Hal", "Is", "Jar", "Kel", "Lor", "Mor", "Nor", "Or", "Pel", "Quin", "Ral", "Sel", "Tor", "Ul", "Val", "Wil", "Xan", "Yel", "Zor"
    };

    private List<string> suffixes = new List<string> {
        "a", "an", "ar", "as", "el", "en", "er", "ess", "ia", "iel", "in", "is", "ith", "or", "os", "us", "ys"
    };

    // Constructor
    public NameGenerator()
    {
        // Optionally, could initialize or load name parts from external sources here
    }

    // Public Methods
    public string GenerateName()
    {
        // Randomly pick a prefix and suffix and combine them
        string prefix = prefixes[Random.Range(0, prefixes.Count)];
        string suffix = suffixes[Random.Range(0, suffixes.Count)];

        // Combine and capitalize the first letter
        string name = prefix + suffix;
        return char.ToUpper(name[0]) + name.Substring(1);
    }

    // Private Variables
    // Private Methods
}
