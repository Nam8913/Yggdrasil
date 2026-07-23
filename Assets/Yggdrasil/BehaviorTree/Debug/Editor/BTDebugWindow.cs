#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviorTree.Debug.Editor
{
    public class BTDebugWindow : EditorWindow
    {
        private Vector2 _treeScrollPos;
        private Vector2 _blackboardScrollPos;
        private Vector2 _logScrollPos;
        private BehaviorTreeRunner _selectedRunner;
        private int _selectedTab;

        [MenuItem("Window/Behavior Tree Debugger")]
        public static void ShowWindow()
        {
            GetWindow<BTDebugWindow>("BT Debugger");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh", GUILayout.Width(80)))
            {
                Repaint();
            }

            if (Application.isPlaying)
            {
                BehaviorTreeRunner[] runners = FindObjectsByType<BehaviorTreeRunner>(FindObjectsInactive.Exclude);
                if (runners.Length > 0 && _selectedRunner == null)
                {
                    _selectedRunner = runners[0];
                }
            }

            _selectedRunner = (BehaviorTreeRunner)EditorGUILayout.ObjectField(
                "NPC", _selectedRunner, typeof(BehaviorTreeRunner), true
            );
            EditorGUILayout.EndHorizontal();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to debug behavior trees.", MessageType.Info);
                return;
            }

            if (_selectedRunner == null)
            {
                EditorGUILayout.HelpBox("Select a BehaviorTreeRunner to debug.", MessageType.Warning);
                return;
            }

            _selectedTab = GUILayout.Toolbar(_selectedTab, new[] { "Tree", "Blackboard", "Log", "Stats" });

            switch (_selectedTab)
            {
                case 0: DrawTreeView(); break;
                case 1: DrawBlackboardView(); break;
                case 2: DrawLogView(); break;
                case 3: DrawStatsView(); break;
            }
        }

        private void DrawTreeView()
        {
            EditorGUILayout.LabelField("Behavior Tree Structure", EditorStyles.boldLabel);

            if (_selectedRunner.Root == null)
            {
                EditorGUILayout.LabelField("No root node.");
                return;
            }

            _treeScrollPos = EditorGUILayout.BeginScrollView(_treeScrollPos);
            DrawNode(_selectedRunner.Root, 0);
            EditorGUILayout.EndScrollView();
        }

        private void DrawNode(NodeBT node, int depth)
        {
            if (node == null) return;

            string indent = new string(' ', depth * 2);
            string stateStr = node.CurrentState.ToString();
            bool isActive = node.IsRunning;
            bool isSuccess = node.CurrentState == BHState.Success;
            bool isFailure = node.CurrentState == BHState.Failure;
            GUIStyle style = new GUIStyle(EditorStyles.label);
            if (isActive)
            {
                style.normal.textColor = Color.yellow;
                style.fontStyle = FontStyle.Bold;
            }

            if (isSuccess)
            {
                style.normal.textColor = Color.green;
            }

            if (isFailure)
            {
                style.normal.textColor = Color.red;
            }

            string tickInfo = node.TotalTicks > 0 ? $" [{node.LastTickDurationMs:F2}ms]" : "";
            EditorGUILayout.LabelField($"{indent}{node.GetType().Name}: {stateStr}{tickInfo}", style);

            if (node is CompositeNode composite)
            {
                foreach (var child in composite.GetChildren())
                {
                    DrawNode(child, depth + 1);
                }
            }
            else if (node is DecoratorNode decorator)
            {
                DrawNode(decorator.Child, depth + 1);
            }
        }

        private void DrawBlackboardView()
        {
            EditorGUILayout.LabelField("Blackboard Values", EditorStyles.boldLabel);

            var bb = _selectedRunner.Blackboard;
            if (bb == null)
            {
                EditorGUILayout.LabelField("No blackboard.");
                return;
            }

            _blackboardScrollPos = EditorGUILayout.BeginScrollView(_blackboardScrollPos);

            List<string> keys = new List<string>(bb.GetAllKeys());
            keys.Sort();
            foreach (var key in keys)
            {
                DrawBlackboardKey(bb, key, new BBKey<object>(key));
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawBlackboardKey<T>(Blackboard bb, string label, BBKey<T> key)
        {
            if (bb.TryGet(key, out T value))
            {
                EditorGUILayout.LabelField($"{label}: {value}");
            }
            else
            {
                EditorGUILayout.LabelField($"{label}: (not set)", EditorStyles.miniLabel);
            }
        }

        private void DrawLogView()
        {
            EditorGUILayout.LabelField("Execution Log", EditorStyles.boldLabel);

            if (BTLogger.Instance == null)
            {
                EditorGUILayout.HelpBox("BTLogger not found. Add BTLogger to scene.", MessageType.Warning);
                return;
            }

            if (GUILayout.Button("Clear Log"))
            {
                BTLogger.Instance.Clear();
            }

            _logScrollPos = EditorGUILayout.BeginScrollView(_logScrollPos);

            var entries = BTLogger.Instance.GetEntries();
            int start = Mathf.Max(0, entries.Count - 100);

            for (int i = start; i < entries.Count; i++)
            {
                var entry = entries[i];
                EditorGUILayout.LabelField(entry.ToString(), EditorStyles.miniLabel);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawStatsView()
        {
            EditorGUILayout.LabelField("Performance Statistics", EditorStyles.boldLabel);

            if (BTStats.Instance == null)
            {
                EditorGUILayout.HelpBox("BTStats not found. Add BTStats to scene.", MessageType.Warning);
                return;
            }

            var stats = BTStats.Instance.GetAllStats();
            foreach (var kvp in stats)
            {
                EditorGUILayout.LabelField($"{kvp.Key}:", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"Total Ticks: {kvp.Value.TotalTicks}");
                EditorGUILayout.LabelField($"Avg Tick: {kvp.Value.AverageTickMs:F3}ms");
                EditorGUILayout.LabelField($"Max Tick: {kvp.Value.MaxTickMs:F3}ms");
                EditorGUILayout.LabelField($"Interrupts: {kvp.Value.InterruptCount}");
                EditorGUI.indentLevel--;
            }
        }

        private void OnInspectorUpdate()
        {
            if (Application.isPlaying)
            {
                Repaint();
            }
        }
    }
}
#endif
