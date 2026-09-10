Shader "TheRoom/GradientSky" {
Properties { _Top("Zenith",Color)=(0.14,0.19,0.37,1) _Horizon("Horizon",Color)=(0.93,0.59,0.4,1) _Bottom("Ground",Color)=(0.3,0.34,0.36,1) }
SubShader { Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"} Cull Off ZWrite Off
Pass { CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
struct appdata {float4 vertex:POSITION;}; struct v2f {float4 pos:SV_POSITION;float3 dir:TEXCOORD0;};
float4 _Top,_Horizon,_Bottom;
v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.dir=v.vertex.xyz;return o;}
fixed4 frag(v2f i):SV_Target{float3 d=normalize(i.dir);float t=saturate(d.y);float3 c=lerp(_Horizon.rgb,_Top.rgb,pow(t,.65));c=lerp(c,_Bottom.rgb,saturate(-d.y*4));return float4(c,1);}
ENDCG } } }
