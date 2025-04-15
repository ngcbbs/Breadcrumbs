#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Breadcrumbs.one_page_dungeon.Scripts.Editor {
    public static class AIAnimatorGenerator {
        private const string kAnimationsBase = "Assets/Breadcrumbs/one-page-dungeon/Animations";

        [MenuItem("Breadcrumbs/Tools/Generate Enemy Animators")]
        public static void GenerateEnemyAnimators() {
            var tablesPath = $"{kAnimationsBase}/AnimationTables";
            if (!Directory.Exists(tablesPath)) {
                Debug.LogWarning($"테이블 경로를 찾을 수 없습니다.\n경로: {tablesPath}");
                return;
            }
            
            var rootPath = Application.dataPath.Replace("Assets", "");

            var assetFiles = Directory.GetFiles($"{rootPath}{tablesPath}", "*.asset", SearchOption.TopDirectoryOnly);
            if (assetFiles.Length == 0) {
                Debug.LogWarning("테이블 파일을 찾을 수 없습니다.");
                return;
            }

            var test = AssetDatabase.LoadAssetAtPath<AnimationTable>(
                "Assets/Breadcrumbs/one-page-dungeon/Animations/AnimationTables/Enemy.asset");
            if (test == null) {
                Debug.Log("im failed.");
            }

            var tables = new List<AnimationTable>();
            foreach (var path in assetFiles) {
                var filename = Path.GetFileName(path);
                var assetPath = $"{tablesPath}/{filename}";
                if (AssetDatabase.GetMainAssetTypeAtPath(assetPath) != typeof(AnimationTable))
                    continue;
                var item = AssetDatabase.LoadAssetAtPath<AnimationTable>(assetPath);
                if (item == null)
                    continue;
                tables.Add(item);
            }
            Debug.Log($"에셋 경로: {tablesPath} / 에셋 확인: {tables.Count}개");

            foreach (var it in tables) {
                var table = it as AnimationTable;
                if (table == null)
                    continue;
                Debug.Log($"애니메이션 테이블 작업중: {table.name}");
                
                string basePath = $"{kAnimationsBase}/Enemy/";
                if (!Directory.Exists(basePath)) 
                    Directory.CreateDirectory(basePath);

                // Animator Controller 생성
                string controllerPath = basePath + $"{table.enemyType}_Controller.controller";
                var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

                // << 파라메터 설정 >>
                controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
                controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);
                controller.AddParameter("MoveSpeed", AnimatorControllerParameterType.Float);
                controller.AddParameter("isAttacking", AnimatorControllerParameterType.Bool);
                controller.AddParameter("isDead", AnimatorControllerParameterType.Bool);

                var sm = controller.layers[0].stateMachine;

                // Idle 상태
                var idleState = sm.AddState("Idle");
                idleState.motion = table.idleClip;

                // Move 상태 (Blend Tree)
                var moveState = sm.AddState("Move");
                var moveBt = new BlendTree();
                AssetDatabase.AddObjectToAsset(moveBt, controller);
                moveState.motion = moveBt;
                moveBt.name = "MoveBlendTree";
                moveBt.blendParameter = "MoveSpeed";
                moveBt.blendType = BlendTreeType.Simple1D;
                moveBt.AddChild(table.walkClip, 0f);
                moveBt.AddChild(table.runClip, 1f);

                // Attack 상태
                var attackState = sm.AddState("Attack");
                attackState.motion = table.attackClip;
                // exitTime 설정 필요 할지도?
                attackState.AddTransition(idleState, true); // attack end -> idle

                // Death 상태
                var deathState = sm.AddState("Death");
                deathState.motion = table.deathClip;

                // 트랜지션 설정
                sm.AddAnyStateTransition(attackState).AddCondition(AnimatorConditionMode.If, 0, "Attack");
                sm.AddAnyStateTransition(deathState).AddCondition(AnimatorConditionMode.If, 0, "Death");

                var idleToMove = idleState.AddTransition(moveState);
                idleToMove.AddCondition(AnimatorConditionMode.Greater, 0.1f, "MoveSpeed");
                idleToMove.hasExitTime = false;

                var moveToIdle = moveState.AddTransition(idleState);
                moveToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "MoveSpeed");
                moveToIdle.hasExitTime = false;
                
                // AnimatorOverrideController 생성
                string overridePath = basePath + $"{table.enemyType}_Override.overrideController";
                var overrideController = new AnimatorOverrideController(controller);
                AssetDatabase.CreateAsset(overrideController, overridePath);

                Debug.Log($"{table.enemyType} Animator 생성 완료");
            }

            AssetDatabase.SaveAssets();
            Debug.Log("모든 Animator 컨트롤러 및 오버라이드 생성 완료");
        }
    }
}
#endif
