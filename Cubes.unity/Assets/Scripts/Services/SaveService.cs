using System.IO;
using UnityEngine;

namespace CubeGame
{
    public class SaveService : ISaveService
    {
        private static readonly string SavePath =
            Path.Combine(Application.persistentDataPath, "tower_save.json");

        private readonly TowerModel _model;

        public bool HasSave => File.Exists(SavePath);

        public SaveService(TowerModel model)
        {
            _model = model;
        }

        public void Save()
        {
            var state = _model.ToState();
            string json = JsonUtility.ToJson(state, true);
            File.WriteAllText(SavePath, json);
        }

        public void Load()
        {
            if (!HasSave) return;

            string json = File.ReadAllText(SavePath);
            var state = JsonUtility.FromJson<TowerState>(json);
            if (state != null)
                _model.LoadState(state);
        }

        public void ClearSave()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }
    }
}
