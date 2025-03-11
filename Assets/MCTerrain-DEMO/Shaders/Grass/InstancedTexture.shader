Shader "Custom/InstancedTexture" {

    Properties
    {
        // There is no support for texture tiling/offset,
        // so make them not be displayed in material inspector
        [NoScaleOffset] _MainTex("Texture", 2D) = "white" {}
        _Color("Color (RGBA)", Color) = (1, 1, 1, 1) // add _Color property
    }

    SubShader {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull front
        LOD 100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma fullforwardshadows addshadow
            
            #include "UnityCG.cginc"
            
            struct appdata_t {
                float4 vertex   : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                float2 uv : TEXCOORD0;
            }; 

           // float4 _Colors[1023];

            v2f vert(appdata_t i, uint instanceID: SV_InstanceID) {
                // Allow instancing.
                UNITY_SETUP_INSTANCE_ID(i);

                v2f o;
                o.vertex = UnityObjectToClipPos(i.vertex);
                o.uv = i.uv;
                
                return o;
            }
            
            // texture we will sample
            sampler2D _MainTex;
            float4 _Color;


            fixed4 frag(v2f i) : SV_Target {

                // sample texture and return it
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                return col;
            }
            
            ENDCG
        }
    }
}
