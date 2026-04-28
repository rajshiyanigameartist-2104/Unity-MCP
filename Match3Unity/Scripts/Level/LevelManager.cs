using System.Linq;
using Match3.Level;
using UnityEngine;

namespace Match3.Level
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private TextAsset levelJson;
        [SerializeField] private int currentLevelId = 1;

        private LevelCollection _levels;

        public LevelConfig ActiveLevel { get; private set; }

        private void Awake()
        {
            LoadLevels();
            SetLevel(currentLevelId);
        }

        public void SetLevel(int levelId)
        {
            ActiveLevel = _levels.levels.FirstOrDefault(x => x.levelId == levelId) ?? _levels.levels.First();
        }

        public void RestartLevel()
        {
            SetLevel(currentLevelId);
        }

        private void LoadLevels()
        {
            _levels = JsonUtility.FromJson<LevelCollection>(levelJson.text);
            if (_levels == null || _levels.levels.Count == 0)
            {
                Debug.LogError("No levels loaded from JSON.");
                _levels = new LevelCollection();
                _levels.levels.Add(new LevelConfig());
            }
        }
    }
}
