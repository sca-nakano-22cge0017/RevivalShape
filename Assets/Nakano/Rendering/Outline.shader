Shader "Custom/Outline"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 0)
        _OutlineWidth("Outline Width", Float) = 1
    }

        SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            Cull Front//�`��ʂ𗠑��ɂ���

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _OutlineColor;
            float _OutlineWidth;

            struct v2f
            {
                float4 position : SV_POSITION;
            };

            v2f vert(float4 position : POSITION, float3 normal : NORMAL, float4 color : COLOR)
            {
                v2f o;
                float3 normalWS = UnityObjectToWorldNormal(color.rgb);//���_�J���[���g��
                float3 positionWS = mul(unity_ObjectToWorld, position);
                o.position = UnityWorldToClipPos(positionWS + normalWS * _OutlineWidth);//�@�������ɉ����o��
                return o;
            }

            float4 frag(v2f IN) : SV_Target
            {
                return _OutlineColor;
            }

            ENDCG
        }

        Pass
        {
                //���ۂ̕`��p�X
        }
    }
}