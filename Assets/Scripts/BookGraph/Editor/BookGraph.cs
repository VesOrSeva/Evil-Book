using System;
using Unity.GraphToolkit.Editor;
using UnityEditor;

namespace BookGraph.Editor
{
    [Serializable]
    [Graph(AssetExtension)]
    public class BookGraph : Graph
    {
        public const string AssetExtension = "bookGraph";

        [MenuItem("Assets/Create/Book Graph/Book Graph", false)]
        private static void CreateAssetFile()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<BookGraph>();
        }
    }
}