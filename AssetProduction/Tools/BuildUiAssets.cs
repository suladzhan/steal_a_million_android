// Original vector artwork for STEAL A MILLION. Run with Build-UiAssets.ps1.
// SVG and PNG share the same geometry; no text is baked into game sprites.
using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Collections.Generic;

public sealed class ProductionUi : IDisposable
{
    const string Blue="#3E78FF", Dark="#16202A", Gold="#FFC928", Pale="#FFF5B1", Green="#35D06F", White="#FFFFFF", Red="#FF4D5A";
    Bitmap bitmap = new Bitmap(1024,1024,PixelFormat.Format32bppArgb);
    Graphics g;
    StringBuilder svg = new StringBuilder("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 100 100\">\n");
    public ProductionUi() { g=Graphics.FromImage(bitmap); g.Clear(Color.Transparent); g.SmoothingMode=SmoothingMode.AntiAlias; g.ScaleTransform(10.24f,10.24f); }
    static string F(float v) { return v.ToString("0.###",CultureInfo.InvariantCulture); }
    static Color C(string hex) { return ColorTranslator.FromHtml(hex); }
    void Rect(float x,float y,float w,float h,string color,float radius=0)
    {
        using(var p=new GraphicsPath()) {
            float d=Math.Min(radius*2,Math.Min(w,h));
            if(d==0) p.AddRectangle(new RectangleF(x,y,w,h));
            else { p.AddArc(x,y,d,d,180,90);p.AddArc(x+w-d,y,d,d,270,90);p.AddArc(x+w-d,y+h-d,d,d,0,90);p.AddArc(x,y+h-d,d,d,90,90);p.CloseFigure(); }
            using(var b=new SolidBrush(C(color))) g.FillPath(b,p);
        }
        svg.AppendFormat(CultureInfo.InvariantCulture,"<rect x=\"{0}\" y=\"{1}\" width=\"{2}\" height=\"{3}\" rx=\"{4}\" fill=\"{5}\"/>\n",x,y,w,h,radius,color);
    }
    void Ellipse(float x,float y,float w,float h,string color)
    {
        using(var b=new SolidBrush(C(color)))g.FillEllipse(b,x,y,w,h);
        svg.AppendFormat(CultureInfo.InvariantCulture,"<ellipse cx=\"{0}\" cy=\"{1}\" rx=\"{2}\" ry=\"{3}\" fill=\"{4}\"/>\n",x+w/2,y+h/2,w/2,h/2,color);
    }
    void Line(string color,float width,params float[] xy)
    {
        PointF[] points=Points(xy);
        using(var p=new Pen(C(color),width)) {p.StartCap=LineCap.Round;p.EndCap=LineCap.Round;p.LineJoin=LineJoin.Round;g.DrawLines(p,points);}
        svg.Append("<polyline fill=\"none\" stroke=\"").Append(color).Append("\" stroke-width=\"").Append(F(width)).Append("\" stroke-linecap=\"round\" stroke-linejoin=\"round\" points=\"");
        foreach(var p in points)svg.Append(F(p.X)).Append(",").Append(F(p.Y)).Append(" ");svg.Append("\"/>\n");
    }
    static PointF[] Points(float[] xy) {var a=new PointF[xy.Length/2];for(int i=0;i<a.Length;i++)a[i]=new PointF(xy[i*2],xy[i*2+1]);return a;}
    void Poly(string color,params float[] xy)
    {
        using(var b=new SolidBrush(C(color))) g.FillPolygon(b,Points(xy));
        svg.Append("<polygon fill=\"").Append(color).Append("\" points=\"");for(int i=0;i<xy.Length;i+=2)svg.Append(F(xy[i])).Append(",").Append(F(xy[i+1])).Append(" ");svg.Append("\"/>\n");
    }
    void Star(float x,float y,float r,string color,int points=4)
    {
        var xy=new float[points*4];for(int i=0;i<points*2;i++){double a=-Math.PI/2+i*Math.PI/points;float rr=i%2==0?r:r*.40f;xy[i*2]=x+(float)Math.Cos(a)*rr;xy[i*2+1]=y+(float)Math.Sin(a)*rr;}Poly(color,xy);
    }
    void Ring(float x,float y,float radius,string color,float width)
    {
        var xy=new float[130];for(int i=0;i<=64;i++){xy[i*2]=x+radius*(float)Math.Cos(i*Math.PI/32);xy[i*2+1]=y+radius*(float)Math.Sin(i*Math.PI/32);}Line(color,width,xy);
    }
    void Arrow(float x,float y,float dx,float dy,string color,float width=8)
    {
        Line(color,width,x,y,x+dx,y+dy);double a=Math.Atan2(dy,dx);float ax=(float)Math.Cos(a)*13,ay=(float)Math.Sin(a)*13;
        Line(color,width,x+dx-ax-ay*.7f,y+dy-ay+ax*.7f,x+dx,y+dy,x+dx-ax+ay*.7f,y+dy-ay-ax*.7f);
    }
    void Cash(float x=12,float y=28,float w=76,float h=44)
    {
        Rect(x,y+8,w,h,"#159447",4);Rect(x,y+4,w,h,Green,4);Rect(x,y,w,h,"#A9FFBE",4);
        Rect(x+5,y+5,w-10,h-10,Green,2);Rect(x+w*.40f,y,w*.20f,h+8,White,1);
    }
    void Crown(float y=28)
    {
        Poly(Gold,18,y,35,y+16,50,y-8,65,y+16,82,y,75,y+43,25,y+43);Rect(25,y+35,50,10,"#E5A521",3);
        Ellipse(44,y+17,12,12,Pale);
    }
    void Globe()
    {
        Ring(50,50,32,Blue,7);Line(Blue,5,19,50,81,50);var xy=new float[66];
        for(int i=0;i<=32;i++){double a=i*Math.PI/16;xy[i*2]=50+14*(float)Math.Cos(a);xy[i*2+1]=50+31*(float)Math.Sin(a);}Line(Blue,5,xy);
    }
    void Trophy()
    {
        Line("#E5A521",8,26,30,14,30,16,48,32,55);Line("#E5A521",8,74,30,86,30,84,48,68,55);
        Poly(Gold,27,20,73,20,70,48,62,60,50,65,38,60,30,48);Rect(45,61,10,16,Gold);Rect(31,77,38,8,"#E5A521",3);Star(50,39,12,Pale,5);
    }
    void Receipt()
    {
        Poly(Red,27,15,73,15,73,85,65,79,57,85,49,79,41,85,33,79,27,84);
        Line(White,5,37,33,63,33);Line(White,5,37,46,63,46);Line(White,5,37,59,53,59);
    }
    void Icon(string id)
    {
        switch(id) {
        case "cash":Cash();break;
        case "key":Ring(31,34,16,Gold,10);Line(Gold,12,43,47,76,80);Line(Gold,9,64,67,75,56);Line(Gold,9,74,77,85,66);break;
        case "xp":Star(50,50,39,Blue);Star(50,48,22,"#7DD8FF");break;
        case "trophy":case "achievement":Trophy();break;
        case "crown":Crown();break;
        case "level":Rect(15,64,22,19,Blue,3);Rect(39,44,22,39,Blue,3);Rect(63,23,22,60,Blue,3);break;
        case "distance":Line(Blue,7,25,80,25,65,55,65,55,48);Ellipse(17,76,16,16,Gold);Line(Dark,5,57,48,57,14);Poly(Red,60,15,87,15,77,25,87,35,60,35);break;
        case "play":case "resume":Poly(Blue,28,16,82,50,28,84);break;
        case "shop":Line(Dark,7,36,33,36,20,42,14,58,14,64,20,64,33);Poly(Blue,23,29,77,29,83,84,17,84);Star(50,56,14,White);break;
        case "worlds":case "language":Globe();break;
        case "missions":Rect(23,18,58,70,Blue,5);Rect(37,12,30,14,"#253A70",4);for(int i=0;i<3;i++){Line(White,4,32,37+i*18,36,41+i*18,43,32+i*18);Line(White,4,51,37+i*18,70,37+i*18);}break;
        case "profile":Ellipse(33,14,34,34,Blue);Ellipse(19,51,62,36,Blue);break;
        case "retry":{var p=new List<float>();for(int i=0;i<=36;i++){double a=(-35+i*8)*Math.PI/180;p.Add(50+29*(float)Math.Cos(a));p.Add(50+29*(float)Math.Sin(a));}Line(Blue,9,p.ToArray());Poly(Blue,64,14,87,22,64,37);break;}
        case "home":Poly(Blue,12,44,50,13,88,44,78,44,78,84,59,84,59,59,41,59,41,84,22,84,22,44);break;
        case "lock":Line("#82909D",10,31,45,31,27,39,17,61,17,69,27,69,45);Rect(21,40,58,46,Gold,6);Ellipse(44,53,12,12,Dark);Rect(47,61,6,13,Dark,2);break;
        case "info":Ring(50,50,33,Blue,7);Ellipse(45,28,10,10,Blue);Line(Blue,9,50,47,50,70);break;
        case "warning":Poly(Gold,50,12,92,84,8,84);Line(Dark,8,50,38,50,60);Ellipse(46,69,8,8,Dark);break;
        case "claim":Rect(20,40,60,44,Blue,3);Rect(15,32,70,17,"#7DD8FF",3);Rect(44,32,12,52,Gold);Line(Gold,6,49,31,27,27,24,17,34,12,49,31,66,12,77,17,73,27,49,31);break;
        case "sound":Poly(Blue,15,37,32,37,54,19,54,81,32,63,15,63);Line(Blue,6,67,35,73,42,73,58,67,65);Line(Blue,5,79,23,90,37,90,63,79,77);break;
        case "music":Line(Blue,8,40,69,40,25,77,17,77,61);Ellipse(17,61,26,19,Blue);Ellipse(54,54,26,19,Blue);Line(Blue,9,42,32,75,24);break;
        case "haptics":Rect(32,15,36,70,Blue,6);Rect(38,24,24,44,"#7DD8FF",2);Ellipse(46,74,8,5,White);Line(Blue,5,21,32,16,41,21,50,16,59,21,68);Line(Blue,5,79,32,84,41,79,50,84,59,79,68);break;
        case "endless":{var p=new List<float>();for(int i=0;i<=96;i++){double a=i*Math.PI/48;p.Add(50+35*(float)Math.Cos(a));p.Add(50+24*(float)(Math.Sin(a)*Math.Cos(a)));}Line(Blue,9,p.ToArray());break;}
        case "bonus":Star(50,50,41,Gold,5);Star(50,48,23,Pale,5);break;
        case "daily":Rect(16,25,68,59,Blue,5);Rect(16,25,68,17,"#253A70",4);Line(Blue,7,32,17,32,32);Line(Blue,7,68,17,68,32);for(int i=0;i<3;i++)for(int j=0;j<2;j++)Rect(28+i*17,51+j*17,9,9,White,2);break;
        case "collection":Rect(15,18,42,62,"#7DD8FF",4);Rect(29,25,42,62,Blue,4);Rect(43,32,42,62,"#253A70",4);Star(64,61,15,Gold);break;
        case "magnet":Line(Red,18,25,21,25,60,33,76,50,81,67,76,75,60,75,21);Rect(16,16,18,19,Blue,2);Rect(66,16,18,19,Blue,2);break;
        case "luck":Line("#159447",7,49,51,62,86);Ellipse(18,16,35,35,Green);Ellipse(49,16,35,35,Green);Ellipse(18,47,35,35,Green);Ellipse(49,47,35,35,Green);Ellipse(44,42,14,14,"#A9FFBE");break;
        case "double_cash":Cash(28,20,62,33);Cash(10,48,62,33);break;
        case "slow_motion":Rect(42,10,16,9,Gold,3);Line(Blue,7,50,18,50,26);Ellipse(20,24,60,65,Blue);Ellipse(28,32,44,49,White);Line(Dark,5,50,38,50,56,63,61);break;
        case "tax":Receipt();break;
        case "police":Rect(20,70,60,13,"#253A70",4);Ellipse(26,28,48,65,Blue);Rect(26,49,48,26,Blue);Rect(26,58,24,17,Red);Line(Gold,5,50,11,50,18);Line(Gold,5,15,26,21,31);Line(Gold,5,85,26,79,31);break;
        case "thief":Poly(Dark,12,33,35,28,50,39,65,28,88,33,82,65,63,70,50,60,37,70,18,65);Ellipse(23,41,17,12,White);Ellipse(60,41,17,12,White);break;
        case "investment":Rect(15,62,17,23,Blue,2);Rect(38,45,17,40,Blue,2);Rect(61,28,17,57,Blue,2);Arrow(19,43,59,-28,Green,6);break;
        case "risk_chain":Ring(30,35,16,Red,8);Ring(69,66,16,Red,8);Line(Red,9,39,44,61,57);break;
        case "jackpot":Star(50,50,42,Gold,12);Ellipse(25,25,50,50,"#E5A521");Star(50,50,21,Pale,5);break;
        case "rewarded":Rect(11,22,78,57,Blue,6);Poly(White,39,34,65,50,39,68);break;
        case "safe":Poly(Green,17,20,50,12,83,20,80,56,66,76,50,88,34,76,20,56);Line(White,8,33,47,46,60,69,35);break;
        case "risk":Line(Red,9,50,86,50,51,24,23);Arrow(50,51,27,-29,Red,9);Line(Red,9,24,23,23,42);Line(Red,9,24,23,43,23);break;
        default:throw new ArgumentException("Unknown icon: "+id);
        }
    }
    static bool overwrite;
    void Save(string directory,string id,int size=256)
    {
        Directory.CreateDirectory(directory);string stem=Path.Combine(directory,id+"__v001");
        if(File.Exists(stem+".png") && !overwrite) throw new IOException("Refusing to overwrite "+stem);
        File.WriteAllText(stem+".svg",svg.ToString()+"</svg>\n",new UTF8Encoding(false));
        using(var resized=new Bitmap(size,size,PixelFormat.Format32bppArgb))using(var rg=Graphics.FromImage(resized)){
            rg.CompositingMode=CompositingMode.SourceCopy;rg.InterpolationMode=InterpolationMode.HighQualityBicubic;rg.PixelOffsetMode=PixelOffsetMode.HighQuality;
            rg.DrawImage(bitmap,new Rectangle(0,0,size,size));resized.Save(stem+".png",ImageFormat.Png);
        }
    }
    void SaveOnColor(string directory,string id)
    {
        // Exact white silhouette of the same native geometry for saturated buttons.
        for(int y=0;y<bitmap.Height;y++)for(int x=0;x<bitmap.Width;x++){
            int alpha=bitmap.GetPixel(x,y).A;bitmap.SetPixel(x,y,Color.FromArgb(alpha,255,255,255));
        }
        svg=new StringBuilder(System.Text.RegularExpressions.Regex.Replace(svg.ToString(),"#[0-9A-Fa-f]{6}","#FFFFFF"));
        Save(directory,id+"_on_color");
    }
    public void Dispose(){g.Dispose();bitmap.Dispose();}
    public static void Build(string root,bool replace)
    {
        overwrite=replace;
        string dir=Path.Combine(root,"04_Exports","UI");
        string[] names="cash key xp trophy crown level distance play shop worlds missions profile resume retry home lock info warning claim sound music haptics language endless bonus daily achievement collection magnet luck double_cash slow_motion tax police thief investment risk_chain jackpot rewarded safe risk".Split(' ');
        string onColor=" play shop worlds missions profile resume retry home info sound music haptics language endless level distance rewarded investment ";
        foreach(string name in names)using(var art=new ProductionUi()){art.Icon(name);art.Save(dir,"UI_"+name);if(onColor.Contains(" "+name+" "))art.SaveOnColor(dir,"UI_"+name);}
        for(int i=0;i<12;i++)using(var art=new ProductionUi()){
            string rim=i<3?"#7DD8FF":i<6?Green:Gold;
            if(i>=10)art.Star(50,50,47,Gold,16);
            art.Ellipse(10,10,80,80,rim);art.Ellipse(17,17,66,66,i<6?Blue:"#E5A521");
            if(i>=9)art.Crown(29);else {int n=1+i/3;for(int j=0;j<n;j++)art.Star(50+(j-(n-1)/2f)*22,50,n==1?24:13,White,5);}
            if(i%3>0)art.Ring(50,50,37,Pale,2);if(i%3==2){art.Ellipse(18,46,8,8,White);art.Ellipse(74,46,8,8,White);}
            art.Save(dir,"UI_rank_"+i.ToString("00"));
        }
        using(var art=new ProductionUi()){art.Rect(0,0,100,100,"#D9E1EA",3.125f);art.Rect(.8f,.8f,98.4f,98.4f,White,2.5f);art.Save(dir,"UI_panel");}
        using(var art=new ProductionUi()){art.Rect(0,0,100,100,White,3.125f);art.Save(dir,"UI_button_shape");}
        using(var art=new ProductionUi()){art.Rect(0,0,100,100,White,50);art.Save(dir,"UI_pill_shape",128);}
        Particles(Path.Combine(root,"04_Exports","Particles"));
        Console.WriteLine("Exported 41 original UI symbols, 12 rank badges, 3 component shapes, 7 particle textures. Eight existing GameArt symbols remain native; PT_coin awaits the model render.");
    }
    static void Particles(string directory)
    {
        Directory.CreateDirectory(directory);
        string[] kinds={"soft","spark","ring","streak","confetti","note","dust"};
        foreach(string kind in kinds){
            if(kind=="note"){using(var art=new ProductionUi()){art.Cash(12,25,76,44);art.Save(directory,"PT_note",128);}continue;}
            if(kind=="confetti"){using(var art=new ProductionUi()){art.Rect(22,15,56,70,White,1);art.Save(directory,"PT_confetti",64);}continue;}
            if(kind=="spark"){using(var art=new ProductionUi()){art.Star(50,50,44,White);art.Save(directory,"PT_spark",128);}continue;}
            string path=Path.Combine(directory,"PT_"+kind+"__v001.png");if(File.Exists(path) && !overwrite)throw new IOException("Refusing to overwrite "+path);
            using(var image=new Bitmap(128,128,PixelFormat.Format32bppArgb)){
                for(int y=0;y<128;y++)for(int x=0;x<128;x++){
                    double nx=(x+.5-64)/64,ny=(y+.5-64)/64,r=Math.Sqrt(nx*nx+ny*ny),a=0;
                    if(kind=="soft")a=Math.Pow(Math.Max(0,1-r),2.5);
                    if(kind=="ring")a=Math.Max(0,1-Math.Abs(r-.64)/.11);
                    if(kind=="streak")a=Math.Pow(Math.Max(0,1-Math.Abs(nx)/.14),1.5)*Math.Max(0,1-Math.Abs(ny)/.85);
                    if(kind=="dust"){double r1=Math.Sqrt(Math.Pow((nx+.2)/.7,2)+Math.Pow(ny/.53,2));double r2=Math.Sqrt(Math.Pow((nx-.27)/.58,2)+Math.Pow((ny+.08)/.67,2));a=Math.Min(.75,Math.Pow(Math.Max(0,1-r1),1.4)+Math.Pow(Math.Max(0,1-r2),1.4));}
                    image.SetPixel(x,y,Color.FromArgb((int)Math.Round(Math.Min(1,a)*255),255,255,255));
                }image.Save(path,ImageFormat.Png);
            }
        }
    }
    public static void Review(string root)
    {
        var paths=new List<string>();paths.AddRange(Directory.GetFiles(Path.Combine(root,"04_Exports","UI"),"*.png"));paths.AddRange(Directory.GetFiles(Path.Combine(root,"04_Exports","Particles"),"*.png"));paths.Sort();
        string review=Path.Combine(root,"Reviews");Directory.CreateDirectory(review);
        int cols=6,cellW=186,cellH=114,rows=(paths.Count+cols-1)/cols;
        var report=new StringBuilder("id,width,height,alpha_min,alpha_max,visible_pixels,edge_visible_pixels\n");
        using(var sheet=new Bitmap(cols*cellW,rows*cellH))using(var gg=Graphics.FromImage(sheet))using(var font=new Font("Arial",9)){
            gg.Clear(C("#EEF2F6"));
            for(int i=0;i<paths.Count;i++)using(var source=new Bitmap(paths[i])){
                string id=Path.GetFileNameWithoutExtension(paths[i]).Replace("__v001","");int min=255,max=0,visible=0,edge=0;
                for(int y=0;y<source.Height;y++)for(int x=0;x<source.Width;x++){int a=source.GetPixel(x,y).A;min=Math.Min(min,a);max=Math.Max(max,a);if(a>0){visible++;if(x==0||y==0||x==source.Width-1||y==source.Height-1)edge++;}}
                if(min!=0 || max==0)throw new Exception("Missing transparency or empty artwork: "+id);
                if(edge!=0 && !id.EndsWith("shape") && id!="UI_panel")throw new Exception("Clipped artwork: "+id);
                report.AppendFormat("{0},{1},{2},{3},{4},{5},{6}\n",id,source.Width,source.Height,min,max,visible,edge);
                int xx=(i%cols)*cellW,yy=(i/cols)*cellH;
                string[] backgrounds={White,Dark,Blue};for(int b=0;b<3;b++){using(var brush=new SolidBrush(C(backgrounds[b])))gg.FillRectangle(brush,xx+b*60+3,yy+5,58,65);gg.DrawImage(source,new Rectangle(xx+b*60+8,yy+13,48,48));}
                gg.DrawString(id,font,Brushes.Black,xx+4,yy+78);
            }
            sheet.Save(Path.Combine(review,"UI__v001__48px_backgrounds.png"),ImageFormat.Png);
        }
        File.WriteAllText(Path.Combine(review,"UI__v001__alpha_audit.csv"),report.ToString());
        Console.WriteLine("Verified alpha, dimensions and edge clearance for "+paths.Count+" PNG files; generated 48px review on white, dark and blue.");
    }
    public static void ReviewModels(string root)
    {
        string[] paths=Directory.GetFiles(Path.Combine(root,"04_Exports"),"*__preview.png",SearchOption.AllDirectories);Array.Sort(paths);
        int width=320,height=350,columns=3,rows=(paths.Length+columns-1)/columns;
        using(var sheet=new Bitmap(width*columns,height*rows))using(var gg=Graphics.FromImage(sheet))using(var font=new Font("Arial",12)){
            gg.Clear(C("#E8EFF6"));
            for(int i=0;i<paths.Length;i++)using(var source=new Bitmap(paths[i])){
                int x=i%columns*width,y=i/columns*height;
                gg.DrawImage(source,new Rectangle(x+8,y+4,304,304));
                gg.DrawString(Path.GetFileName(paths[i]).Replace("__v001__preview.png",""),font,Brushes.Black,x+8,y+314);
            }
            sheet.Save(Path.Combine(root,"Reviews","Models__v001__overview.png"),ImageFormat.Png);
        }
    }
}
