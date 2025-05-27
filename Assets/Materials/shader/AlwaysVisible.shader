Shader "Custom/AlwaysVisible"
{
    Properties
    {
        _Color ("Main Color", Color) = (0,0,1,0.5) // デフォルト色: 青（半透明）
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" } // 透明オブジェクト用キュー（透明描画用）
        Pass
        {
            ZTest Always         // 深度テストを無効化
            ZWrite Off           // 深度バッファへの書き込みを無効化
            Cull Off             // 両面描画
            Lighting Off         // ライティングを無効化

            Blend SrcAlpha OneMinusSrcAlpha // アルファブレンディングを有効化

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            fixed4 _Color; // プロパティとしての色

            struct appdata
            {
                float4 vertex : POSITION; // 頂点データ
            };

            struct v2f
            {
                float4 pos : SV_POSITION; // クリップ空間での位置
            };

            // 頂点シェーダー
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex); // オブジェクト空間からクリップ空間へ変換
                return o;
            }

            // フラグメントシェーダー
            fixed4 frag(v2f i) : SV_Target
            {
                return _Color; // 指定された色をそのまま出力
            }
            ENDCG
        }
    }
}
