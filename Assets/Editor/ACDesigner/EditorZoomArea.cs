using UnityEngine;

namespace ACDesigner
{
    public static class RectExtensions
    {
        public static Vector2 TopLeft(this Rect rect)
        {
            return new Vector2(rect.xMin, rect.yMin);
        }

        public static Rect ScaleSizeBy(this Rect rect, float scale)
        {
            return rect.ScaleSizeBy(scale, rect.center);
        }

        public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
        {
            var result = rect;
            result.x = result.x - pivotPoint.x;
            result.y = result.y - pivotPoint.y;
            result.xMin = result.xMin * scale;
            result.xMax = result.xMax * scale;
            result.yMin = result.yMin * scale;
            result.yMax = result.yMax * scale;
            result.x = result.x + pivotPoint.x;
            result.y = result.y + pivotPoint.y;
            return result;
        }
    }

    public static class EditorZoomArea
    {
        private static Matrix4x4 _prevGuiMatrix;

        private static Rect groupRect = default(Rect);

        public static Rect Begin(Rect screenCoordsArea, float zoomScale)
        {
            GUI.EndGroup();
            Rect val = screenCoordsArea.ScaleSizeBy(1f / zoomScale, screenCoordsArea.TopLeft());
            val.y = val.y + 21f;
            GUI.BeginGroup(val);
            _prevGuiMatrix = GUI.matrix;
            Matrix4x4 val2 = Matrix4x4.TRS((Vector2) (val.TopLeft()), Quaternion.identity, Vector3.one);
            Vector3 one = Vector3.one;
            one.x = one.y = zoomScale;
            Matrix4x4 val3 = Matrix4x4.Scale(one);
            GUI.matrix = val2 * val3 * val2.inverse * GUI.matrix;
            return val;
        }

        public static void End()
        {
            GUI.matrix = _prevGuiMatrix;
            GUI.EndGroup();
            groupRect.y = 21f;
            groupRect.width = (float) Screen.width;
            groupRect.height = (float) Screen.height;
            GUI.BeginGroup(groupRect);
        }
    }
}