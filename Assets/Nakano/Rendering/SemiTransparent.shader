Shader "Custom/SemiTransparent"
{
    Properties
    {
        // テクスチャの定義
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        // シェーダーの処理

        // いつどのようにしてレンダリングエンジンでレンダリングするかを指定
        Tags { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalRenderPipeline"}

        Blend SrcAlpha OneMinusSrcAlpha

        // LOD(Level of Detail)に応じてSubShaderを使い分けるときに使用する
        LOD 100

        // 一度目のPass　裏面を表示しないようにする
        Pass {
            ZWrite On
            ColorMask 0
        }

        // 実際のシェーダーの挙動を記述する
        // 複数記述が可能　Pass毎にDrawCallが走るため描画負荷が上がる
        Pass
        {
            // これが無いと表示されない
            // 参考:https://note.com/matuba1335/n/n3191312fbeb5
            Tags { "LightMode" = "UniversalForward" }

            Cull Back

            ZWrite OFF
            Ztest LEqual

            // CFPROGRAM～END内が挙動記述部分
            CGPROGRAM

            // 関数宣言
            #pragma vertex vert   // 頂点シェーダー
            #pragma fragment frag // フラグメントシェーダー
            
            // 定義済み関数の読み込み
            #include "UnityCG.cginc"

            // 構造体の宣言
            // 入力
            struct appdata
            {
                // セマンティクス→「:」以降で変数がどういったものかを記述
                // 以下の場合、「POSITION」がセマンティクス名
                // 他にも「NORMAL(法線ベクトル)」「COLOR(頂点カラー)」なども設定可能
                // 「能」で終わると次の行もコメントアウトしていることになるので注意

                float4 vertex : POSITION; // 頂点座標
                float2 uv : TEXCOORD0;    // テクスチャUV座標
            };

            // 出力
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // プロパティの取得　Propertiesと同じ名前で宣言することでプロパティを取得可能
            //

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            // 頂点シェーダー
            // 返り値：v2f　引数：appdata
            v2f vert (appdata v)
            {
                v2f o; // 返り値の宣言

                // オブジェクト空間からカメラのクリップ空間へ点を変換
                o.vertex = UnityObjectToClipPos(v.vertex);

                // テクスチャのタイリング、オフセット値を適用した状態に変換をかける
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            // フラグメントシェーダー 全てのドット毎に様々な情報を元に色を決定
            // 返り値：fixed4　引数：v2f 頂点シェーダーで変換された値
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                col.a = _Color.a;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
