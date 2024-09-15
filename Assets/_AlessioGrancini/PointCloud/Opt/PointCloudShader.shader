Shader "Instanced/PointCloudShader"
{
    Properties
    {
        _Scale ("Point Scale", Float) = 0.01
    }
    SubShader
    {
        Tags {"RenderType"="Opaque"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert addshadow
        #pragma instancing_options procedural:setup

        struct Input
        {
            float3 color : COLOR;
        };

        float _Scale;
        
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            StructuredBuffer<float3> positionsBuffer;
            StructuredBuffer<float3> colorsBuffer;
        #endif

        void setup()
        {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                float3 position = positionsBuffer[unity_InstanceID];
                unity_ObjectToWorld._11_21_31_41 = float4(_Scale, 0, 0, 0);
                unity_ObjectToWorld._12_22_32_42 = float4(0, _Scale, 0, 0);
                unity_ObjectToWorld._13_23_33_43 = float4(0, 0, _Scale, 0);
                unity_ObjectToWorld._14_24_34_44 = float4(position, 1);
            #endif
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                o.Albedo = colorsBuffer[unity_InstanceID];
            #else
                o.Albedo = 1;
            #endif
        }
        ENDCG
    }
}
