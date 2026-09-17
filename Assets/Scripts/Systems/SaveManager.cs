using System.IO;
using StealAMillion.Core;
using UnityEngine;

namespace StealAMillion
{
    public sealed class SaveManager
    {
        private readonly SaveStore store;
        public SaveManager()
        {
            string directory = Application.persistentDataPath;
#if UNITY_EDITOR
            string testDirectory = System.Environment.GetEnvironmentVariable("SAM_TEST_SAVE_DIRECTORY");
            if (!string.IsNullOrEmpty(testDirectory)) directory = testDirectory;
#endif
            store = new SaveStore(Path.Combine(directory, "progress.sav"),
                data => JsonUtility.ToJson(data), json => JsonUtility.FromJson<SaveData>(json));
        }
        public SaveData Load() { return store.Load(); }
        public bool Save(SaveData data) { return store.Write(data); }
        public bool Reset() { return store.Reset(); }
    }
}
