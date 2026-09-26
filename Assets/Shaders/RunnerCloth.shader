Shader "Runner/Cloth Tint" {
 Properties {_MainTex("Original artwork",2D)="white"{} _ClothTint("Cloth tint",Color)=(.24,.47,1,1) _TintStrength("Tint strength",Range(0,1))=0 _Glossiness("Smoothness",Range(0,1))=.15}
 SubShader {Tags {"RenderType"="Opaque"} LOD 200
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 sampler2D _MainTex;fixed4 _ClothTint;half _TintStrength,_Glossiness;
 struct Input{float2 uv_MainTex;};
 void surf(Input IN,inout SurfaceOutputStandard o){fixed3 c=tex2D(_MainTex,IN.uv_MainTex).rgb;
   half value=max(c.r,max(c.g,c.b));half mask=smoothstep(.03,.16,c.b-max(c.r,c.g))*smoothstep(.27,.50,value)*_TintStrength;
   o.Albedo=lerp(c,_ClothTint.rgb*lerp(.55,1.25,value),mask);o.Metallic=0;o.Smoothness=_Glossiness;o.Alpha=1;
 }
 ENDCG
 }
 FallBack "Standard"
}
