Shader "Codel1417/GrabPassOverlay"
{
    SubShader
    {
        Tags { "RenderType" = "Transparent"  "Queue" = "Overlay" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" "IsEmissive" = "true"}
        Cull Off
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct v2f
            {
                float4 grabPos : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _FloofPass;

            v2f vert (float2 uv : TEXCOORD0)
            {
                v2f o;
                o.vertex = float4(float2(1,-1)*(uv*2-1),0,1);
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_FloofPass, i.grabPos.xy / i.grabPos.w);
            }
            ENDCG
        }
    }
}