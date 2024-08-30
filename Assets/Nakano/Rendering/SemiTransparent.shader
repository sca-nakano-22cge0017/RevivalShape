Shader "Custom/SemiTransparent"
{
    Properties
    {
        // �e�N�X�`���̒�`
        _MainTex("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        // �V�F�[�_�[�̏���

        // ���ǂ̂悤�ɂ��ă����_�����O�G���W���Ń����_�����O���邩���w��
        Tags { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalRenderPipeline"}

        Blend SrcAlpha OneMinusSrcAlpha

        // LOD(Level of Detail)�ɉ�����SubShader���g��������Ƃ��Ɏg�p����
        LOD 100

        // ��x�ڂ�Pass�@���ʂ�\�����Ȃ��悤�ɂ���
        Pass {
            ZWrite On
            ColorMask 0
        }

        // ���ۂ̃V�F�[�_�[�̋������L�q����
        // �����L�q���\�@Pass����DrawCall�����邽�ߕ`�敉�ׂ��オ��
        Pass
        {
            // ���ꂪ�����ƕ\������Ȃ�
            // �Q�l:https://note.com/matuba1335/n/n3191312fbeb5
            Tags { "LightMode" = "UniversalForward" }

            Cull Back

            ZWrite OFF
            Ztest LEqual

            // CFPROGRAM�`END���������L�q����
            CGPROGRAM

            // �֐��錾
            #pragma vertex vert   // ���_�V�F�[�_�[
            #pragma fragment frag // �t���O�����g�V�F�[�_�[
            
            // ��`�ς݊֐��̓ǂݍ���
            #include "UnityCG.cginc"

            // �\���̂̐錾
            // ����
            struct appdata
            {
                // �Z�}���e�B�N�X���u:�v�ȍ~�ŕϐ����ǂ����������̂����L�q
                // �ȉ��̏ꍇ�A�uPOSITION�v���Z�}���e�B�N�X��
                // ���ɂ��uNORMAL(�@���x�N�g��)�v�uCOLOR(���_�J���[)�v�Ȃǂ��ݒ�\
                // �u�\�v�ŏI���Ǝ��̍s���R�����g�A�E�g���Ă��邱�ƂɂȂ�̂Œ���

                float4 vertex : POSITION; // ���_���W
                float2 uv : TEXCOORD0;    // �e�N�X�`��UV���W
            };

            // �o��
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // �v���p�e�B�̎擾�@Properties�Ɠ������O�Ő錾���邱�ƂŃv���p�e�B���擾�\
            //

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            // ���_�V�F�[�_�[
            // �Ԃ�l�Fv2f�@�����Fappdata
            v2f vert (appdata v)
            {
                v2f o; // �Ԃ�l�̐錾

                // �I�u�W�F�N�g��Ԃ���J�����̃N���b�v��Ԃ֓_��ϊ�
                o.vertex = UnityObjectToClipPos(v.vertex);

                // �e�N�X�`���̃^�C�����O�A�I�t�Z�b�g�l��K�p������Ԃɕϊ���������
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            // �t���O�����g�V�F�[�_�[ �S�Ẵh�b�g���ɗl�X�ȏ������ɐF������
            // �Ԃ�l�Ffixed4�@�����Fv2f ���_�V�F�[�_�[�ŕϊ����ꂽ�l
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
