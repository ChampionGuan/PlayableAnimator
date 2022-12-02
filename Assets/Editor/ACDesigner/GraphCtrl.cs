using UnityEditor;
using UnityEngine;

namespace ACDesigner
{
    public class GraphCtrl : Singleton<GraphCtrl>
    {
        private Rect m_graphScrollRect;
        private Material m_gridMaterial;
        private float m_graphScrollZoom = 1f;
        private Rect m_graphScrollAreaRect;
        private Vector2 m_graphScrollPosition;
        private Vector2 m_graphScrollOffset;
        private Vector2 m_graphScrollSize = new Vector2(20000f, 20000f);

        public float GraphScrollZoom
        {
            get => m_graphScrollZoom;
            private set
            {
                m_graphScrollZoom = value;
                StoragePrefs.SetPref(PrefsType.CtrlScrollZoom, value);
            }
        }

        public Vector2 GraphScrollPosition
        {
            get => m_graphScrollPosition;
            private set
            {
                m_graphScrollPosition = value;
                StoragePrefs.SetPref(PrefsType.CtrlScrollPos, value);
            }
        }

        public Vector2 GraphScrollOffset
        {
            get => m_graphScrollOffset;
            private set
            {
                m_graphScrollOffset = value;
                StoragePrefs.SetPref(PrefsType.CtrlScrollOffset, value);
            }
        }

        protected override void OnInstance()
        {
            m_graphScrollZoom = (float) StoragePrefs.GetPref(PrefsType.CtrlScrollZoom);
            m_graphScrollPosition = (Vector2) StoragePrefs.GetPref(PrefsType.CtrlScrollPos);
            m_graphScrollOffset = (Vector2) StoragePrefs.GetPref(PrefsType.CtrlScrollOffset);
            m_gridMaterial = new Material(Shader.Find("Hidden/Designer/Grid"));
            m_gridMaterial.hideFlags = HideFlags.HideAndDontSave;
            m_gridMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        }

        public void OnGUI()
        {
            if (ACDesignerWindow.Instance.ScreenSizeChange)
            {
                m_graphScrollRect = new Rect(300f, 18f, ACDesignerWindow.Instance.ScreenSizeWidth - 300f - 15f, ACDesignerWindow.Instance.ScreenSizeHeight - 36f - 21f - 15f);
                m_graphScrollAreaRect = new Rect(m_graphScrollRect.xMin + 15f, m_graphScrollRect.yMin + 15f, m_graphScrollRect.width - 30f, m_graphScrollRect.height - 30f);

                if (GraphScrollPosition == Vector2.zero)
                {
                    LocateToCtrl();
                }
            }

            if (Event.current.type != EventType.ScrollWheel)
            {
                var pos = GUI.BeginScrollView(new Rect(m_graphScrollRect.x, m_graphScrollRect.y, m_graphScrollRect.width + 15f, m_graphScrollRect.height + 15f), GraphScrollPosition, new Rect(0f, 0f, m_graphScrollSize.x, m_graphScrollSize.y), true, true);
                if (pos != GraphScrollPosition && Event.current.type != EventType.DragUpdated && Event.current.type != EventType.Ignore)
                {
                    GraphScrollOffset -= (pos - GraphScrollPosition) / GraphScrollZoom;
                    GraphScrollPosition = pos;
                }

                GUI.EndScrollView();
            }

            GUI.Box(m_graphScrollRect, string.Empty, ACDesignerUIUtility.GraphBackgroundGUIStyle);
            DrawGridBG();

            EditorZoomArea.Begin(m_graphScrollRect, GraphScrollZoom);
            DrawChart(IsThePointInGraph(out Vector2 mousePos), mousePos, GraphScrollPosition);
            EditorZoomArea.End();
        }

        public void OnEvent()
        {
            var currentEvent = Event.current;
            if (currentEvent.type == EventType.Repaint || currentEvent.type == EventType.Layout)
            {
                return;
            }

            var mousePos = Vector2.zero;
            switch (currentEvent.type)
            {
                case EventType.MouseDrag:
                    if (IsThePointInGraph(out mousePos))
                    {
                        if (currentEvent.button == 2 && ScrollGraph(currentEvent.delta))
                        {
                            currentEvent.Use();
                        }
                    }

                    break;
                case EventType.ScrollWheel:
                    if (IsThePointInGraph(out mousePos) && ZoomGraph(mousePos))
                    {
                        currentEvent.Use();
                    }

                    break;
            }
        }

        public void LocateToCtrl()
        {
            GraphScrollZoom = 1;
            GraphScrollOffset = Vector2.zero;
            GraphScrollPosition = (m_graphScrollSize - (m_graphScrollRect.size - Vector2.one * Define.TaskWidth * 0.5f)) * 0.5f;
        }

        private bool ZoomGraph(Vector2 zoomPos)
        {
            GraphScrollZoom += -Event.current.delta.y / 150f;
            GraphScrollZoom = Mathf.Clamp(GraphScrollZoom, 0.4f, 1.4f);

            IsThePointInGraph(out var point);
            GraphScrollOffset += point - zoomPos;
            GraphScrollPosition += point - zoomPos;
            return true;
        }

        private bool ScrollGraph()
        {
            var mousePos = ACDesignerWindow.Instance.CurrMousePos;
            if (m_graphScrollAreaRect.Contains(mousePos))
            {
                return false;
            }

            var result = false;
            var offset = Vector2.zero;
            if (mousePos.y < m_graphScrollAreaRect.yMin + 15f)
            {
                offset.y = 5f;
                result = true;
            }
            else if (mousePos.y > m_graphScrollAreaRect.yMax - 15f)
            {
                offset.y = -5f;
                result = true;
            }

            if (mousePos.x < m_graphScrollAreaRect.xMin + 15f)
            {
                offset.x = 5f;
                result = true;
            }
            else if (mousePos.x > m_graphScrollAreaRect.xMax - 15f)
            {
                offset.x = -5f;
                result = true;
            }

            if (result) ScrollGraph(offset);
            return result;
        }

        private bool ScrollGraph(Vector2 delta)
        {
            GraphScrollOffset += delta / GraphScrollZoom;
            GraphScrollPosition -= delta;
            return true;
        }

        private bool IsThePointInGraph(out Vector2 point)
        {
            point = ACDesignerWindow.Instance.CurrMousePos;
            if (!m_graphScrollRect.Contains(point))
            {
                return false;
            }

            point -= new Vector2(m_graphScrollRect.xMin, m_graphScrollRect.yMin);
            point /= GraphScrollZoom;
            return true;
        }

        private void DrawChart(bool inArea, Vector2 mousePos, Vector2 scrollPos)
        {
        }

        private void DrawGridBG()
        {
            if (Event.current.type == EventType.Repaint)
            {
                m_gridMaterial.SetPass(!EditorGUIUtility.isProSkin ? 1 : 0);
                GL.PushMatrix();
                GL.Begin(1);
                DrawGridLines(Define.MeshSize * GraphScrollZoom, new Vector2(GraphScrollOffset.x % Define.MeshSize * GraphScrollZoom, GraphScrollOffset.y % Define.MeshSize * GraphScrollZoom));
                GL.End();
                GL.PopMatrix();

                m_gridMaterial.SetPass(!EditorGUIUtility.isProSkin ? 3 : 2);
                GL.PushMatrix();
                GL.Begin(1);
                DrawGridLines(Define.MeshSize * 5 * GraphScrollZoom, new Vector2(GraphScrollOffset.x % (Define.MeshSize * 5) * GraphScrollZoom, GraphScrollOffset.y % (Define.MeshSize * 5) * GraphScrollZoom));
                GL.End();
                GL.PopMatrix();
            }
        }

        private void DrawGridLines(float gridSize, Vector2 offset)
        {
            var num = m_graphScrollRect.x + offset.x;
            if (offset.x < 0f)
            {
                num += gridSize;
            }

            for (var num2 = num; num2 < m_graphScrollRect.x + m_graphScrollRect.width; num2 += gridSize)
            {
                DrawGridLine(new Vector2(num2, m_graphScrollRect.y), new Vector2(num2, m_graphScrollRect.y + m_graphScrollRect.height));
            }

            var num3 = m_graphScrollRect.y + offset.y;
            if (offset.y < 0f)
            {
                num3 += gridSize;
            }

            for (var num4 = num3; num4 < m_graphScrollRect.y + m_graphScrollRect.height; num4 += gridSize)
            {
                DrawGridLine(new Vector2(m_graphScrollRect.x, num4), new Vector2(m_graphScrollRect.x + m_graphScrollRect.width, num4));
            }
        }

        private void DrawGridLine(Vector2 p1, Vector2 p2)
        {
            GL.Vertex(p1);
            GL.Vertex(p2);
        }
    }
}