using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Subnautica_Echelon
{
    public class RecipePurger
    {
        private static List<string> PurgeContents { get; } = new List<string>
        {
            "{\r\n    \"Ingredients\": [\r\n        {\r\n            \"techType\": \"PowerCell\",\r\n            \"amount\": 1\r\n        },\r\n        {\r\n            \"techType\": \"Welder\",\r\n            \"amount\": 1\r\n        },\r\n        {\r\n            \"techType\": \"AdvancedWiringKit\",\r\n            \"amount\": 2\r\n        },\r\n        {\r\n            \"techType\": \"UraniniteCrystal\",\r\n            \"amount\": 5\r\n        },\r\n        {\r\n            \"techType\": \"Diamond\",\r\n            \"amount\": 2\r\n        },\r\n        {\r\n            \"techType\": \"Kyanite\",\r\n            \"amount\": 2\r\n        },\r\n        {\r\n            \"techType\": \"PlasteelIngot\",\r\n            \"amount\": 4\r\n        }\r\n    ],\r\n    \"LinkedItems\": [],\r\n    \"craftAmount\": 1\r\n}",
            "{\r\n    \"Ingredients\": [\r\n        {\r\n            \"techType\": \"PowerCell\",\r\n            \"amount\": 1\r\n        },\r\n        {\r\n            \"techType\": \"Welder\",\r\n            \"amount\": 1\r\n        },\r\n        {\r\n            \"techType\": \"AdvancedWiringKit\",\r\n            \"amount\": 2\r\n        },\r\n        {\r\n            \"techType\": \"UraniniteCrystal\",\r\n            \"amount\": 3\r\n        },\r\n        {\r\n            \"techType\": \"Lead\",\r\n            \"amount\": 3\r\n        },\r\n        {\r\n            \"techType\": \"Diamond\",\r\n            \"amount\": 2\r\n        },\r\n        {\r\n            \"techType\": \"PlasteelIngot\",\r\n            \"amount\": 4\r\n        }\r\n    ],\r\n    \"LinkedItems\": [],\r\n    \"craftAmount\": 1\r\n}",
        };

        private const string RecipePath = @"..\VehicleFramework\recipes\Echelon_recipe.json";

        public static void Purge()
        {
            var path = Path.Combine(MainPatcher.RootFolder, RecipePath);
            if (!File.Exists(path))
            {
                Debug.Log($"No existing recipe found at {path}");
                return;
            }
            var content = File.ReadAllText(path);
            if (PurgeContents.Contains(content))
            {
                Debug.Log($"Purging {path}");
                File.Delete(path);
            }
            else
                Debug.LogWarning($"Content mismatch. Not purging {path}");
        }


    }
}