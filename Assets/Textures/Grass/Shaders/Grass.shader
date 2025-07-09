Shader "Toon/Grass"
{
    Properties
    {
        [Header(Season Colors)]
        _TopColorS("夏季顶部颜色（Summer Top）", Color) = (0.3, 1, 0.3, 1) // 夏季绿草顶部
        _BottomColorS("夏季底部颜色（Summer Bottom）", Color) = (0.2, 0.8, 0.2, 1) // 夏季绿草底部
        _TopColorE("秋季顶部颜色（Autumn Top）", Color) = (1, 0.7, 0.3, 1) // 秋季枯草顶部
        _BottomColorE("秋季底部颜色（Autumn Bottom）", Color) = (0.8, 0.5, 0.2, 1) // 秋季枯草底部
        _SeasonFloat("季节权重（0=夏季，1=秋季）", Range(0, 1)) = 0.5 // 控制季节过渡
        
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
                float4 _TopColorS;
                float4 _BottomColorS;
                float4 _TopColorE;
                float4 _BottomColorE;
                float _SeasonFloat; // 季节权重（0=夏季，1=秋季）
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

                // 插值季节颜色（确保是float4）
                float4 currentTopColor = lerp(_TopColorS, _TopColorE, _SeasonFloat);
                float4 currentBottomColor = lerp(_BottomColorS, _BottomColorE, _SeasonFloat);

                // 确保所有参与lerp的参数都是float4
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