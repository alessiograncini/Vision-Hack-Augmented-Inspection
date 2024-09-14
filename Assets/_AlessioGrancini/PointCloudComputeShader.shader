Shader "Custom/PointCloudComputeShader"
{
    Properties
    {
        _PointSize ("Point Size", Float) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 5.0
            #include "UnityCG.cginc"

            struct PointData
            {
                float3 position;
                float3 color;
            };

            StructuredBuffer<PointData> pointBuffer;
            float _PointSize;
            float4x4 _LocalToWorld;

            struct appdata
            {
                uint vertexID : SV_VertexID;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float pointSize : PSIZE;
            };

            v2f Vert(appdata v)
            {
                PointData pd = pointBuffer[v.vertexID];
                v2f o;
                // Apply the GameObject's transform to the point position
                float3 worldPosition = mul(_LocalToWorld, float4(pd.position, 1.0)).xyz;
                o.vertex = UnityObjectToClipPos(float4(worldPosition, 1.0));
                o.color = float4(pd.color, 1.0);
                o.pointSize = _PointSize * 1000; // Adjust scaling as needed
                return o;
            }

            fixed4 Frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
