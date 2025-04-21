using System.Collections.Generic;
using UnityEngine;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 난이도 조절 UI 예시
    /// </summary>
    public class DifficultyUI : MonoBehaviour {
        [SerializeField]
        private List<DifficultySettings> availableDifficulties;

        public void SetDifficulty(int difficultyIndex) {
            if (difficultyIndex >= 0 && difficultyIndex < availableDifficulties.Count) {
                SpawnManager.Instance.ChangeDifficulty(availableDifficulties[difficultyIndex]);
            }
        }
    }
}