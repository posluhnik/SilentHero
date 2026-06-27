using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

class GenerateEverything<T>
{
    public static Dictionary<string, T> Generate(string fileName)
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

        string jsonObjects = File.ReadAllText(filePath);
        Dictionary<string, T> objectsDict = JsonSerializer.Deserialize<Dictionary<string, T>>(jsonObjects);

        return objectsDict;
    }
}