Shader "Hidden/KernelBlur"
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

            half _BlurStrength;

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

            static const int BLUR_SAMPLE_COUNT = 8;
            static const float2 BLUR_KERNEL[BLUR_SAMPLE_COUNT] = {
                float2(-1.0, -1.0),
                float2(-1.0, 1.0),
                float2(1.0, -1.0),
                float2(1.0, 1.0),
                float2(-0.70711, 0),
                float2(0, 0.70711),
                float2(0.70711, 0),
                float2(0, -0.70711),
            };

            half4 frag(Varyings IN) : SV_Target
            {
                float2 scale = _BlurStrength / 100;

                half4 color = 0;
                for (int j = 0; j < BLUR_SAMPLE_COUNT; j++) {
                    color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + BLUR_KERNEL[j] * scale);
                }
                
                color.rgb /= BLUR_SAMPLE_COUNT;
                color.a = 1;

                return color;
            }

            ENDHLSL
        }
    }
}