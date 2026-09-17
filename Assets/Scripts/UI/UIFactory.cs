using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StealAMillion
{
    public static class UIFactory
    {
        public static readonly Color Background = Hex("#111719");
        public static readonly Color Surface = Hex("#20292C");
        public static readonly Color White = Hex("#F2F4EF");
        public static readonly Color Muted = Hex("#A1B0AF");
        public static readonly Color Green = Hex("#70E0AF");
        public static readonly Color Red = Hex("#F18A92");
        public static readonly Color Gold = Hex("#F4CA68");
        private static Sprite rounded;

        public static Color Hex(string code) { Color value; return ColorUtility.TryParseHtmlString(code, out value) ? value : White; }

        public static RectTransform Rect(Transform parent, string name, float x, float y, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(x, y);
            rect.anchorMax = new Vector2(x + w, y + h);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        public static Image Panel(Transform parent, string name, Color color, float x, float y, float w, float h, bool round = false)
        {
            var image = Rect(parent, name, x, y, w, h).gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            if (round) { image.sprite = Rounded(); image.type = Image.Type.Sliced; }
            return image;
        }

        public static TextMeshProUGUI Label(Transform parent, string text, float size, Color color,
            float x, float y, float w, float h, TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            var label = Rect(parent, "Text", x, y, w, h).gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.fontSizeMax = size;
            label.fontSizeMin = Mathf.Min(size, Mathf.Max(14, size * .66f));
            label.enableAutoSizing = true;
            label.fontStyle = FontStyles.Bold;
            label.color = color;
            label.alignment = align;
            label.characterSpacing = 0;
            label.raycastTarget = false;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.margin = new Vector4(4, 2, 4, 2);
            if (TMP_Settings.defaultFontAsset != null) label.font = TMP_Settings.defaultFontAsset;
            return label;
        }

        public static Button Button(Transform parent, string text, Color background, Color foreground,
            float x, float y, float w, float h, Action action)
        {
            var panel = Panel(parent, text, background, x, y, w, h, true);
            panel.raycastTarget = true;
            var button = panel.gameObject.AddComponent<Button>();
            button.targetGraphic = panel;
            var colors = button.colors;
            colors.highlightedColor = new Color(.92f, .95f, .94f);
            colors.pressedColor = new Color(.8f, .84f, .83f);
            colors.disabledColor = new Color(.42f, .47f, .46f, .7f);
            button.colors = colors;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            panel.gameObject.AddComponent<PressFeedback>();
            if (!string.IsNullOrEmpty(text)) Label(panel.transform, text, 24, foreground, .04f, .05f, .92f, .9f);
            button.onClick.AddListener(() => action());
            return button;
        }

        public static GameArt Art(Transform parent, ArtKind kind, Color color, float x, float y, float w, float h)
        {
            var art = Rect(parent, kind.ToString(), x, y, w, h).gameObject.AddComponent<GameArt>();
            art.kind = kind;
            art.color = color;
            art.raycastTarget = false;
            return art;
        }

        private static Sprite Rounded()
        {
            if (rounded != null) return rounded;
            const int size = 32;
            const float radius = 7;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "Rounded 7px";
            texture.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Max(radius - x - .5f, x + .5f - (size - radius));
                    float dy = Mathf.Max(radius - y - .5f, y + .5f - (size - radius));
                    float distance = new Vector2(Mathf.Max(0, dx), Mathf.Max(0, dy)).magnitude;
                    texture.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(radius - distance + .5f)));
                }
            texture.Apply(false, true);
            rounded = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100, 0,
                SpriteMeshType.FullRect, new Vector4(8, 8, 8, 8));
            return rounded;
        }
    }
}
