using System.Collections.Generic;
using UnityEngine;

namespace MapEditor
{
    public class MapEditorGUI : MonoBehaviour
    {
        private MapEditor editor;
        private Vector2 scrollPos;

        public void Awake()
        {
            this.editor = this.gameObject.GetComponent<MapEditor>();
            this.scrollPos = Vector2.zero;
        }

        public void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 200, 400));
            GUILayout.BeginVertical();

            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos, GUILayout.Width(200), GUILayout.Height(200));
            GUILayout.BeginVertical();

            foreach (var entry in MapEditorMod.instance.mapObjects)
            {
                if (GUILayout.Button(entry.Key))
                {
                    this.editor.SpawnObject(entry.Value);
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
            GUILayout.EndArea();

            var selectionStyle = new GUIStyle(GUI.skin.box);
            selectionStyle.normal.background = GUIUtils.GetTexture(2, 2, new Color32(255, 255, 255, 20));

            var selectionRect = this.editor.GetSelection();

            if (selectionRect.width > 11 && selectionRect.height > 11)
            {
                GUI.Box(selectionRect, GUIContent.none, selectionStyle);
            }

            foreach (var obj in this.editor.selectedMapObjects)
            {
                var bounds = this.GetMapObjectBounds(obj);
                float padding = 0.5f;
                bounds.x -= padding;
                bounds.y -= padding;
                bounds.width += 2 * padding;
                bounds.height += 2 * padding;
                GUI.Box(GUIUtils.WorldToGUIRect(bounds), GUIContent.none, selectionStyle);
            }
        }

        private Rect GetMapObjectBounds(GameObject go)
        {
            var colliders = new List<Collider2D>();

            if (go.GetComponent<Collider2D>())
            {
                colliders.Add(go.GetComponent<Collider2D>());
            }

            colliders.AddRange(go.GetComponentsInChildren<Collider2D>());

            float minX = 9999f;
            float maxX = -9999f;
            float minY = 9999f;
            float maxY = -9999f;

            foreach (var collider in colliders)
            {
                minX = Mathf.Min(minX, collider.bounds.min.x);
                maxX = Mathf.Max(maxX, collider.bounds.max.x);
                minY = Mathf.Min(minY, collider.bounds.min.y);
                maxY = Mathf.Max(maxY, collider.bounds.max.y);
            }

            return new Rect(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
