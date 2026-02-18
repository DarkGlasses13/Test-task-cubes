using UnityEngine;

namespace CubeGame
{
    public class SaveService : ISaveService
    {
        private const string SaveKey = "cube_tower_save";
        private readonly TowerModel _model;

        public bool HasSave => PlayerPrefs.HasKey(SaveKey);

        public SaveService(TowerModel model)
        {
            _model = model;
        }

        public void Save()
        {
            var state = _model.ToState();
            string json = JsonUtility.ToJson(state);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public void Load()
        {
            if (!HasSave) return;

            string json = PlayerPrefs.GetString(SaveKey);
            var state = JsonUtility.FromJson<TowerState>(json);
            if (state != null)
                _model.LoadState(state);
        }

        public void ClearSave()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
        }
    }
}
