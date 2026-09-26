using System.IO;
using StealAMillion.Core;
using UnityEngine;

namespace StealAMillion
{
    public sealed class SaveManager
    {
        private readonly SaveStore store;
        private readonly string path;
        public SaveManager()
        {
            string directory = Application.persistentDataPath;
#if UNITY_EDITOR
            string testDirectory = System.Environment.GetEnvironmentVariable("SAM_TEST_SAVE_DIRECTORY");
            if (!string.IsNullOrEmpty(testDirectory)) directory = testDirectory;
#endif
            path = Path.Combine(directory, "progress.sav");
            store = new SaveStore(path,
                data => JsonUtility.ToJson(data), json => JsonUtility.FromJson<SaveData>(json));
        }
        public SaveData Load()
        {
            var data = store.Load();
            data.loadedFromDisk = store.LoadedValidSave;
            if (data.version == 1 && data.loadedFromDisk)
            {
                try
                {
                    if (File.Exists(path) && !File.Exists(path + ".v1")) File.Copy(path, path + ".v1");
                    if (File.Exists(path + ".bak") && !File.Exists(path + ".v1.bak")) File.Copy(path + ".bak", path + ".v1.bak");
                }
                catch (System.IO.IOException e) { Debug.LogWarning("V1 archive unavailable: " + e.Message); }
                catch (System.UnauthorizedAccessException e) { Debug.LogWarning("V1 archive unavailable: " + e.Message); }
            }
            return data;
        }
        public bool Save(SaveData data) { return store.Write(data); }
        public bool Reset() { return store.Reset(new SaveData{version=2,runner=new RunnerSave()}); }
    }
}
