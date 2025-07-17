Shader "Toon/Grass"
{
    Properties
    {
        [Header(Season Colors)]
        _TopColorSpring("春季顶部颜色", Color) = (0.6, 1, 0.6, 1)
        _BottomColorSpring("春季底部颜色", Color) = (0.4, 0.9, 0.4, 1)
        _TopColorSummer("夏季顶部颜色", Color) = (0.3, 1, 0.3, 1)
        _BottomColorSummer("夏季底部颜色", Color) = (0.2, 0.8, 0.2, 1)
        _TopColorAutumn("秋季顶部颜色", Color) = (1, 0.7, 0.3, 1)
        _BottomColorAutumn("秋季底部颜色", Color) = (0.8, 0.5, 0.2, 1)
        _TopColorWinter("冬季顶部颜色", Color) = (0.8, 0.9, 1, 1)
        _BottomColorWinter("冬季底部颜色", Color) = (0.7, 0.8, 1, 1)
        _SeasonFloat("季节权重（0=春，0.25=夏，0.5=秋，0.75=冬）", Range(0, 1)) = 0.0
        _SeasonOffset("季节起点偏移", Range(0, 1)) = 0.0
        
        [Header(Shading)]
        _TranslucentGain("半透明增益", Range(0,1)) = 0.5
        
        [Header(Blades)]
        _BendRotationRandom("随机弯曲程度", Range(0, 1)) = 0.2
        _BladeWidth("草叶宽度", Float) = 0.05
        _BladeWidthRandom("草叶宽度随机", Float) = 0.02
        _BladeHeight("草叶高度", Float) = 0.5
        _BladeHeightRandom("草叶高度随机", Float) = 0.3
        
        [Header(Tessellation)]
        _TessellationUniform1("草坪密度", Range(1, 64)) = 1
        
        [Header(Wind)]
        _WindDistortionMap("风力噪声图", 2D) = "white" {}
        _WindFrequency("摆动频率", Vector) = (0.05, 0.05, 0, 0)
        _WindStrength("风力强度", Float) = 1
    }
    
    SubShader
    {
        Cull Off

        Pass
        {
            Tags
            {
                "RenderType" = "Opaque"
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry grassGeo
            #pragma hull hull
            #pragma domain domain
            #pragma target 4.6
            #pragma multi_compile_fwdbase
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            
            #include "./lib/Grass.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // 声明属性变量（通过CBUFFER统一管理）
            CBUFFER_START(UnityPerMaterial)
                float4 _TopColorSpring;
                float4 _BottomColorSpring;
                float4 _TopColorSummer;
                float4 _BottomColorSummer;
                float4 _TopColorAutumn;
                float4 _BottomColorAutumn;
                float4 _TopColorWinter;
                float4 _BottomColorWinter;
                float _SeasonFloat;
                float _SeasonOffset;
            CBUFFER_END
            // 采样器声明
            // TEXTURE2D(_WindDistortionMap);
            SAMPLER(sampler_WindDistortionMap);

            float4 frag (grassGeometryOutput i, half facing : VFACE) : SV_Target
            {
                float3 normal = facing > 0 ? i.normal : -i.normal;

                // 获取主光源和阴影（确保light参数完整）
                Light mainLight = GetMainLight(i._ShadowCoord);
                half shadow = mainLight.shadowAttenuation;
                float NdotL = saturate(dot(normal, mainLight.direction) + _TranslucentGain) * shadow;

                // 计算光照强度（显式声明为float4，Alpha=1.0）
                float3 ambient = SampleSH(normal);
                float4 lightIntensity = float4(
                    NdotL * mainLight.color + ambient,  // RGB通道：光照+环境光
                    1.0                                 // Alpha通道：不透明
                );

                // 四季颜色插值
                float season = frac(_SeasonFloat + _SeasonOffset);
                float4 topColors[4] = {_TopColorSpring, _TopColorSummer, _TopColorAutumn, _TopColorWinter};
                float4 bottomColors[4] = {_BottomColorSpring, _BottomColorSummer, _BottomColorAutumn, _BottomColorWinter};
                float seasonStep = season * 4.0;
                int seasonIndex = (int)seasonStep;
                int nextSeasonIndex = (seasonIndex + 1) % 4;
                float t = seasonStep - seasonIndex;
                float4 currentTopColor = lerp(topColors[seasonIndex], topColors[nextSeasonIndex], t);
                float4 currentBottomColor = lerp(bottomColors[seasonIndex], bottomColors[nextSeasonIndex], t);

                float4 bottomColor = currentBottomColor * lightIntensity; // 底部颜色（float4）
                float4 topColor = currentTopColor * lightIntensity;       // 顶部颜色（float4）

                // 最终插值（所有参数均为float4）
                float4 col = lerp(bottomColor, topColor, i.uv.y);

                return col; // 返回正确的float4
            }
            ENDHLSL
        }

        // 阴影投射Pass（保持不变）
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}