Shader "Hidden/FrameBlend"
{
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_Tex0);
            TEXTURE2D(_Tex1);
            TEXTURE2D(_Tex2);

            half _SampleCount;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 main = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                half4 col0 = SAMPLE_TEXTURE2D(_Tex0, sampler_MainTex, IN.uv);
                half4 col1 = SAMPLE_TEXTURE2D(_Tex1, sampler_MainTex, IN.uv);
                half4 col2 = SAMPLE_TEXTURE2D(_Tex2, sampler_MainTex, IN.uv);

                return (main + col0 + col1 + col2) / (_SampleCount + 1);
            }

            ENDHLSL
        }
    }
}