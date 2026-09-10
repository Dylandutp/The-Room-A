Shader "TheRoom/VertexLit" {
Properties { _BaseColor("Base Color",Color)=(0.35,0.48,0.19,1) }
SubShader { Tags {"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"} Pass { Tags {"LightMode"="UniversalForward"}
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
struct Attributes{float4 positionOS:POSITION;float3 normalOS:NORMAL;float4 color:COLOR;};
struct Varyings{float4 positionCS:SV_POSITION;float3 normalWS:TEXCOORD0;float4 color:COLOR;};
CBUFFER_START(UnityPerMaterial)
float4 _BaseColor;
CBUFFER_END
Varyings vert(Attributes a){Varyings o;o.positionCS=TransformObjectToHClip(a.positionOS.xyz);o.normalWS=TransformObjectToWorldNormal(a.normalOS);o.color=a.color;return o;}
half4 frag(Varyings i):SV_Target{float shade=.78+.22*saturate(dot(normalize(i.normalWS),normalize(float3(-.4,1,-.3))));return half4(_BaseColor.rgb*i.color.rgb*shade,1);}
ENDHLSL } } }
