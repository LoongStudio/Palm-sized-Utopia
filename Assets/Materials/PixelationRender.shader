Shader "Custom/PostRendering/PixelationRender"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixelSize ("Pixel Size", Range(0.001, 0.1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
       
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
 
            sampler2D _MainTex;
            float _PixelSize;
 
            fixed4 frag(v2f_img i) : SV_Target
            {
                fixed4 col;
                float ratioX = (int)(i.uv.x / _PixelSize) * _PixelSize;
                float ratioY = (int)(i.uv.y / _PixelSize) * _PixelSize;
                col = tex2D(_MainTex, float2(ratioX, ratioY));
 
                // Convert to grey scale
                col = dot(col.rgb, float3(0.3, 0.59, 0.11));
               
                // Original Gameboy RGB Colors :
                // 15, 56, 15
                // 48, 98, 48
                // 139, 172, 15
                // 155, 188, 15
 
                if (col.r <= 0.25)
                {
                    col = fixed4(0.06, 0.22, 0.06, 1.0);
                }
                else if (col.r > 0.75)
                {
                    col = fixed4(0.6, 0.74, 0.06, 1.0);
                }
                else if (col.r > 0.25 && col.r <= 0.5)
                {
                    col = fixed4(0.19, 0.38, 0.19, 1.0);
                }
                else
                {
                    col = fixed4(0.54, 0.67, 0.06, 1.0);
                }
 
                return col;
            }
 
            ENDCG
        }
    }
}