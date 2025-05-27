Shader "Custom/AlwaysVisibleWithShade"
{
    Properties
    {
        _Color ("Main Color", Color) = (0,0,1,1) // デフォルト色: 青 (R: 0, G: 0, B: 1, A: 1)
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" } // 常に最前面に描画
        Pass
        {
            ZTest Always         // 深度テスト無効化: 常に描画
            ZWrite Off           // 深度バッファ書き込みを無効化
            ColorMask RGB        // 色のみ描画（アルファは無視）

            // 色を適用
            Material
            {
                Diffuse [_Color]
            }

            Lighting On          // 照明を適用（必要に応じて調整）
        }
    }
}
