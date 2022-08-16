Shader "Codel1417/PrePostProcess Capture"
{
    SubShader
    {
        Tags { "RenderType" = "Transparent"  "Queue" = "Transparent+1"  "IgnoreProjector"="True" "RenderType"="Opaque" "ForceNoShadowCasting" = "True"}
        LOD 100
        ZWrite off
        Cull Off    
        Lighting Off
        ZTest Always
        GrabPass
        {
            "_FloofPass"
        }
        Pass
        {
            ColorMask A
        }
    }
}