Shader "Unlit/BlendingBiomes"
{
    Properties
    {
        _MainTex1 ("Texture 1", 2D) = "white" {}
        _MainTex2 ("Texture 2", 2D) = "white" {}
        _BiomeMap ("Biome Map", 2D) = "white" {}   
        _BlendAmount ("Blend Amount", Range(0, 1)) = 0.5
        _Brightness ("Brightness", Range(0, 10)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert
        
        struct Input
        {
            float2 uv_MainTex1;
            float2 uv_MainTex2;
            float2 uv_BiomeMap;
        };

        sampler2D _MainTex1;
        sampler2D _MainTex2;
        sampler2D _BiomeMap;
        float _BlendAmount;
        float _Brightness;

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Sample the biome map to determine the blend weight
            float blendWeight = tex2D(_BiomeMap, IN.uv_BiomeMap).r;
            
            // Blend between texture 1 and texture 2 based on the blend weight
            fixed4 c1 = tex2D(_MainTex1, IN.uv_MainTex1);
            fixed4 c2 = tex2D(_MainTex2, IN.uv_MainTex2);
            fixed4 finalColor = lerp(c1, c2, blendWeight * _BlendAmount);

            // Increase brightness
            finalColor.rgb *= _Brightness;

            // Output the final color
            o.Albedo = finalColor.rgb;
            o.Alpha = finalColor.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
