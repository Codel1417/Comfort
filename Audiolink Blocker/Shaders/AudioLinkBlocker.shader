Shader "Codel1417/AudioLink Blocker"
{
    SubShader
    {
        Tags { "Queue"="Background-999" "IgnoreProjector"="True" "RenderType"="Opaque" "ForceNoShadowCasting" = "True"}
        LOD 100
        ZWrite off
        Cull Off    
        Lighting Off
        ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };
            
            v2f vert (float2 uv : TEXCOORD0)
            {
                v2f o;
                o.vertex = float4(float2(1,-1)*(uv*2-1),0,1);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return float4(0,0,0,0);
            }
            ENDCG
        }
        GrabPass
        {
            "_AudioTexture"
        }
    }
}
