using UnityEngine;
using UnityEngine.UI;

namespace AlphabetDetection
{
    public class Drawer : MonoBehaviour
    {
        public Texture2D DrawingTexture { get; private set; }
        public Texture2D BrushTexture { get; private set; }

        [SerializeField] private RawImage _drawingSurface;

        private Camera _mainCamera;

        private readonly Color _brushTextureColor = Color.white;
        private readonly Color _drawingTextureColor = Color.black;

        private const int BrushTextureSize = 32;
        private const int DrawingTextureSize = 512;

        private void Start()
        {
            _mainCamera = Camera.main;

            BrushTexture = InitializeTexture(_brushTextureColor, BrushTextureSize);
            DrawingTexture = InitializeTexture(_drawingTextureColor, DrawingTextureSize);

            _drawingSurface.texture = DrawingTexture;
        }

        private void OnDestroy()
        {
            Destroy(DrawingTexture);
            DrawingTexture = null;
            
            Destroy(BrushTexture);
            BrushTexture = null;
        }

        private void Update()
        {
            if (!Input.GetMouseButton(0) || !IsMouseOver())
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_drawingSurface.rectTransform,
                    Input.mousePosition, _mainCamera, out var texCoord))
            {
                return;
            }

            var normalizedPoint = Rect.PointToNormalized(_drawingSurface.rectTransform.rect, texCoord);

            DrawBrush(new Vector2(normalizedPoint.x * DrawingTexture.width,
                normalizedPoint.y * DrawingTexture.height));
        }

        private Texture2D InitializeTexture(Color color, int size)
        {
            var texture = new Texture2D(size, size);
            var fillColor = color;
            var fillPixels = new Color[size * size];

            for (var i = 0; i < fillPixels.Length; i++)
            {
                fillPixels[i] = fillColor;
            }

            texture.SetPixels(fillPixels);
            texture.Apply();
            return texture;
        }

        public void ResetTexture()
        {
            var fillColor = _drawingTextureColor;
            var fillPixels = new Color[DrawingTextureSize * DrawingTextureSize];

            for (var i = 0; i < fillPixels.Length; i++)
            {
                fillPixels[i] = fillColor;
            }

            DrawingTexture.SetPixels(fillPixels);
            DrawingTexture.Apply();
        }

        private bool IsMouseOver()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _drawingSurface.rectTransform,
                Input.mousePosition,
                _mainCamera,
                out var localMousePosition);

            return _drawingSurface.rectTransform.rect.Contains(localMousePosition);
        }

        private void DrawBrush(Vector2 coord)
        {
            var x = Mathf.Clamp((int)coord.x - BrushTexture.width / 2, 0, DrawingTexture.width - BrushTexture.width);
            var y = Mathf.Clamp((int)coord.y - BrushTexture.height / 2, 0, DrawingTexture.height - BrushTexture.height);

            var brushPixels = BrushTexture.GetPixels();
            DrawingTexture.SetPixels(x, y, BrushTexture.width, BrushTexture.height, brushPixels);
            DrawingTexture.Apply();
        }
    }
}