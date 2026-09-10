Shader "TheRoom/ButtonVertexOutline"
{
    Properties 
    { 
        _OutlineColor("Outline Color",Color)=(1,1,1,1) 
        _OutlineWidth("Outline Width (canvas units)",Float)=4 
    }

    SubShader 
    {
        Tags { "RenderType"="Opaque"  "RenderPipeline"="UniversalPipeline" }
        Pass 
        {
            Cull Front ZWrite On ZTest LEqual
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes 
            {
                float4 positionOS:POSITION; 
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings 
            {
                float4 positionCS:SV_POSITION; 
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
            half4 _OutlineColor; 
            float _OutlineWidth;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                // Expand the rectangular mesh at its corners to keep hard edges joined.
                float3 expanded = IN.positionOS.xyz + normalize(sign(IN.positionOS.xyz)) * _OutlineWidth;
                OUT.positionCS = TransformObjectToHClip(expanded);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}
