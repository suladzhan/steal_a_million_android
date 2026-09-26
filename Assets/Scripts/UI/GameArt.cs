using UnityEngine;
using UnityEngine.UI;

namespace StealAMillion
{
    public enum ArtKind { Vault, Coin, Pause, Back, Settings, Shield, Bolt, Check, Cross, Star }

    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class GameArt : MaskableGraphic
    {
        public ArtKind kind;
        private static readonly Color Metal = new Color32(66, 75, 77, 255);
        private static readonly Color Shadow = new Color32(24, 31, 33, 255);

        public GameArt() { useLegacyMeshGeneration = false; }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect r = GetPixelAdjustedRect();
            Vector2 origin = r.center;
            float size = Mathf.Min(r.width, r.height);
            if (kind == ArtKind.Vault)
            {
                Quad(vh, origin, size * .90f, size * .76f, Shadow);
                Quad(vh, origin + new Vector2(0, size * .025f), size * .82f, size * .69f, Metal);
                Quad(vh, origin + new Vector2(0, size * .025f), size * .74f, size * .61f, Shadow);
                Quad(vh, origin + new Vector2(0, size * .025f), size * .70f, size * .57f, new Color32(43, 52, 54, 255));
                Circle(vh, origin + new Vector2(0, size * .025f), size * .205f, Metal);
                Circle(vh, origin + new Vector2(0, size * .025f), size * .166f, Shadow);
                for (int i = 0; i < 3; i++)
                {
                    float angle = i * Mathf.PI * 2 / 3 + .3f;
                    Vector2 center = origin + new Vector2(0, size * .025f);
                    Line(vh, center, center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * size * .132f, size * .036f, color);
                }
                Circle(vh, origin + new Vector2(0, size * .025f), size * .05f, color);
                for (int i = -1; i <= 1; i += 2)
                {
                    Quad(vh, origin + new Vector2(-size * .35f, i * size * .19f), size * .065f, size * .1f, color);
                    Quad(vh, origin + new Vector2(i * size * .3f, -size * .41f), size * .13f, size * .06f, Metal);
                }
                for (int i = 0; i < 3; i++)
                {
                    Vector2 stack = origin + new Vector2(size * .29f, -size * .32f + i * size * .045f);
                    Quad(vh, stack, size * .28f, size * .032f, color);
                    Quad(vh, stack, size * .05f, size * .036f, new Color32(248, 236, 187, 255));
                }
                return;
            }
            if(kind==ArtKind.Star){for(int i=0;i<10;i++){float a=Mathf.PI*.5f+i*Mathf.PI/5,b=a+Mathf.PI/5;float first=i%2==0?.46f:.21f,second=i%2==0?.21f:.46f;Triangle(vh,origin,origin+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*size*first,origin+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*size*second,color);}}
            else if (kind == ArtKind.Coin)
            {
                Circle(vh, origin, size * .45f, color);
                Circle(vh, origin, size * .32f, Shadow);
                Quad(vh, origin, size * .11f, size * .4f, color);
            }
            else if (kind == ArtKind.Pause)
            {
                Quad(vh, origin + Vector2.left * size * .14f, size * .13f, size * .5f, color);
                Quad(vh, origin + Vector2.right * size * .14f, size * .13f, size * .5f, color);
            }
            else if (kind == ArtKind.Back)
            {
                Line(vh, origin + new Vector2(size * .15f, size * .26f), origin + Vector2.left * size * .12f, size * .09f, color);
                Line(vh, origin + Vector2.left * size * .12f, origin + new Vector2(size * .15f, -size * .26f), size * .09f, color);
            }
            else if (kind == ArtKind.Settings)
            {
                for (int i = 0; i < 8; i++)
                {
                    float a = i * Mathf.PI / 4;
                    Vector2 direction = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    Line(vh, origin + direction * size * .2f, origin + direction * size * .38f, size * .12f, color);
                }
                Circle(vh, origin, size * .28f, color);
                Circle(vh, origin, size * .12f, Shadow);
            }
            else if (kind == ArtKind.Check)
            {
                Line(vh, origin + new Vector2(-size * .28f, 0), origin + new Vector2(-size * .07f, -size * .2f), size * .1f, color);
                Line(vh, origin + new Vector2(-size * .07f, -size * .2f), origin + new Vector2(size * .3f, size * .24f), size * .1f, color);
            }
            else if (kind == ArtKind.Cross)
            {
                Line(vh, origin - Vector2.one * size * .25f, origin + Vector2.one * size * .25f, size * .1f, color);
                Line(vh, origin + new Vector2(-size * .25f, size * .25f), origin + new Vector2(size * .25f, -size * .25f), size * .1f, color);
            }
            else if (kind == ArtKind.Shield)
            {
                Triangle(vh, origin + new Vector2(-size * .32f, size * .3f), origin + new Vector2(size * .32f, size * .3f), origin + new Vector2(0, -size * .38f), color);
                Quad(vh, origin + Vector2.up * size * .17f, size * .64f, size * .26f, color);
            }
            else
            {
                Triangle(vh, origin + new Vector2(size * .1f, size * .4f), origin + new Vector2(-size * .3f, -size * .04f), origin + new Vector2(size * .15f, -size * .04f), color);
                Triangle(vh, origin + new Vector2(-size * .1f, -size * .4f), origin + new Vector2(size * .3f, size * .04f), origin + new Vector2(-size * .15f, size * .04f), color);
            }
        }

        private static void Quad(VertexHelper vh, Vector2 c, float width, float height, Color tint)
        {
            Vector2 a = c - new Vector2(width, height) / 2;
            Vector2 b = c + new Vector2(width, height) / 2;
            int start = vh.currentVertCount;
            vh.AddVert(new Vector3(a.x, a.y), tint, Vector2.zero);
            vh.AddVert(new Vector3(a.x, b.y), tint, Vector2.zero);
            vh.AddVert(new Vector3(b.x, b.y), tint, Vector2.zero);
            vh.AddVert(new Vector3(b.x, a.y), tint, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start, start + 2, start + 3);
        }

        private static void Triangle(VertexHelper vh, Vector2 a, Vector2 b, Vector2 c, Color tint)
        {
            int start = vh.currentVertCount;
            vh.AddVert(a, tint, Vector2.zero); vh.AddVert(b, tint, Vector2.zero); vh.AddVert(c, tint, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2);
        }

        private static void Circle(VertexHelper vh, Vector2 c, float radius, Color tint)
        {
            for (int i = 0; i < 40; i++)
            {
                float a = i * Mathf.PI * 2 / 40;
                float b = (i + 1) * Mathf.PI * 2 / 40;
                Triangle(vh, c, c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius,
                    c + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius, tint);
            }
        }

        private static void Line(VertexHelper vh, Vector2 a, Vector2 b, float width, Color tint)
        {
            Vector2 direction = (b - a).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x) * width / 2;
            Triangle(vh, a - normal, a + normal, b + normal, tint);
            Triangle(vh, a - normal, b + normal, b - normal, tint);
        }
    }
}
