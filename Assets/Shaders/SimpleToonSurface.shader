Shader "Custom/SimpleToonSurfaceSteppedLerp"
{
    Properties
    {
        _Color ("Lit Color", Color) = (1,1,1,1)
        _Mid ("Mid Color", Color) = (0.8,0.8,0.8,1)
        _Shade ("Shadow Color", Color) = (0.5,0.5,0.5,1)
        _Threshold1 ("Shadow/Mid Threshold", Range(0,1)) = 0.4
        _Threshold2 ("Mid/Lit Threshold", Range(0,1)) = 0.7
        _BandSteps ("Steps Per Band", Range(1,20)) = 3
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf ToonSteppedLerp addshadow fullforwardshadows

        fixed4 _Color;
        fixed4 _Mid;
        fixed4 _Shade;
        float _Threshold1;
        float _Threshold2;
        float _BandSteps;

        struct Input {
            float3 worldNormal;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Albedo = _Color.rgb;
            o.Alpha = _Color.a;
        }

        // Custom lighting function for stepped+lerped toon shading
        inline half4 LightingToonSteppedLerp (SurfaceOutput s, half3 lightDir, half atten)
        {
            half NdotL = dot(s.Normal, lightDir) * 0.5 + 0.5; // [0,1]
            fixed3 toonCol;
            float bandLerp = 0;

            if (NdotL < _Threshold1)
            {
                // Shade to Mid
                float t = saturate(NdotL / max(_Threshold1, 0.0001));
                // Snap t to discrete steps
                t = floor(t * _BandSteps) / max(_BandSteps - 1, 1);
                toonCol = lerp(_Shade.rgb, _Mid.rgb, t);
            }
            else if (NdotL < _Threshold2)
            {
                // Mid to Lit
                float t = saturate((NdotL - _Threshold1) / max(_Threshold2 - _Threshold1, 0.0001));
                t = floor(t * _BandSteps) / max(_BandSteps - 1, 1);
                toonCol = lerp(_Mid.rgb, _Color.rgb, t);
            }
            else
            {
                toonCol = _Color.rgb;
            }

            toonCol *= _LightColor0.rgb * atten;
            return half4(toonCol * s.Albedo, s.Alpha);
        }
        ENDCG
    }
    FallBack "Diffuse"
}