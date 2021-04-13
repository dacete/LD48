Shader "Custom/2DShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags {"Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout"}
        LOD 100
    Lighting Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma target 2.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO

            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float2 linePos;
            float2 lineDir;
            float distanceMult;
            float DistToLine(float2 pt1, float2 lineDir, float2 testPt)
            {
                float2 perpDir = float2(lineDir.y, -lineDir.x);
                float2 dirToPt1 = pt1 - testPt;
                return abs(dot(normalize(perpDir), dirToPt1));
            }
            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float test = DistToLine(linePos, lineDir, i.vertex.xy);
            //float test = 5.0f*distance(linePos, i.vertex.xy);
                if (test < distanceMult) {
                    //col.xyz -= 0.2f*test / distanceMult;
                    col.w = 0;
                }
                else {
                    col.xyz /= clamp((test  + 25.0f*sin(_Time.w)) / (distanceMult * 5.0f),-100,10);
                }
            //col.w = 0;
                clip(col.a-0.5f);
                // apply fog
                return col;
            }
            ENDCG
        }
    }
}




