using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.IO;

namespace Hadal.Tests
{
    public static class TestUtils
    {
        private const string PrefabFolderPath = "Prefabs/";
        private const string MaterialFolderPath = "Materials/";
        public static T GetPrefab<T>(string path) where T : Object
            => (T)Resources.Load(Path.Combine(PrefabFolderPath, path));
        public static T GetMaterial<T>(string path) where T : Material
            => (T)Resources.Load(Path.Combine(MaterialFolderPath, path));
    }
}