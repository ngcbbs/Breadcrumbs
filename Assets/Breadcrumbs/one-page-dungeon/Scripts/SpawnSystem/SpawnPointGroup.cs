using System;
using System.Collections.Generic;

namespace Breadcrumbs.SpawnSystem {
    /// <summary>
    /// 스폰 포인트들을 그룹으로 관리하는 클래스
    /// </summary>
    [Serializable]
    public class SpawnPointGroup {
        public string GroupId;
        public string GroupName;
        public bool IsActive = true;
        public List<SpawnPoint> SpawnPoints = new List<SpawnPoint>();

        /// <summary>
        /// 그룹 초기화
        /// </summary>
        public void Initialize() {
            foreach (var spawnPoint in SpawnPoints) {
                spawnPoint.SetActive(IsActive);
            }
        }

        /// <summary>
        /// 그룹 활성화/비활성화
        /// </summary>
        public void SetActive(bool active) {
            IsActive = active;
            foreach (var spawnPoint in SpawnPoints) {
                spawnPoint.SetActive(active);
            }
        }
    }
}