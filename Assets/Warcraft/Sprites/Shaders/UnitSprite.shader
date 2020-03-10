Shader "ME.Warcraft/UnitSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _SwapTex("Color Data", 2D) = "transparent" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Target ("Target", Float) = 1
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }
    
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        
            CGPROGRAM
            #pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
            sampler2D _MainTex;
            sampler2D _AlphaTex;
            sampler2D _SwapTex;
			fixed4 _Color;
			half _Target;

            struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
			};

            v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

            fixed4 tableColor(half4 v) {
                
                //half4 v = half4(1, 1, 4, 8);
                half2 dotOffset = half2(1 / v.z * 0.5, 1 / v.w * 0.5);
                fixed4 swapCol = tex2D(_SwapTex, half2(v.x / v.z - dotOffset.x, v.y / v.w - dotOffset.y));
                //v = half4(2, 1, 4, 8);
                return swapCol;
                
            }
            
            bool isEquals(half3 col1, half3 col2) {
            
                half d = 0.15;
                return (col1.r >= col2.r - d && col1.r <= col2.r + d) &&
                        (col1.g >= col2.g - d && col1.g <= col2.g + d) &&
                        (col1.b >= col2.b - d && col1.b <= col2.b + d);
            
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                /*fixed4 c = tex2D(_MainTex, i.texcoord);
                c.rgb *= c.a;
                return c * i.color;*/
                
                fixed4 swapCol1 = tableColor(half4(1, 1, 4, 8));
                fixed4 swapCol2 = tableColor(half4(2, 1, 4, 8));
                fixed4 swapCol3 = tableColor(half4(3, 1, 4, 8));
                fixed4 swapCol4 = tableColor(half4(4, 1, 4, 8));
                
                fixed4 c = tex2D(_MainTex, i.texcoord);
                
                fixed a = c.a;
                if (isEquals(c, swapCol1)) {
                    c = tableColor(half4(1, _Target, 4, 8));
                } else if (isEquals(c, swapCol2)) {
                    c = tableColor(half4(2, _Target, 4, 8));
                } else if (isEquals(c, swapCol3)) {
                    c = tableColor(half4(3, _Target, 4, 8));
                } else if (isEquals(c, swapCol4)) {
                    c = tableColor(half4(4, _Target, 4, 8));
                }
                
                c.a = a;
                c.rgb *= c.a;
                return c * i.color;
                
            }
            ENDCG
        }
    }
}
