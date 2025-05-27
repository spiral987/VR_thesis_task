Shader "Custom/AlwaysVisible"
{
    Properties
    {
        _Color ("Main Color", Color) = (0,0,1,0.5) // �f�t�H���g�F: �i�������j
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" } // �����I�u�W�F�N�g�p�L���[�i�����`��p�j
        Pass
        {
            ZTest Always         // �[�x�e�X�g�𖳌���
            ZWrite Off           // �[�x�o�b�t�@�ւ̏������݂𖳌���
            Cull Off             // ���ʕ`��
            Lighting Off         // ���C�e�B���O�𖳌���

            Blend SrcAlpha OneMinusSrcAlpha // �A���t�@�u�����f�B���O��L����

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            fixed4 _Color; // �v���p�e�B�Ƃ��Ă̐F

            struct appdata
            {
                float4 vertex : POSITION; // ���_�f�[�^
            };

            struct v2f
            {
                float4 pos : SV_POSITION; // �N���b�v��Ԃł̈ʒu
            };

            // ���_�V�F�[�_�[
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex); // �I�u�W�F�N�g��Ԃ���N���b�v��Ԃ֕ϊ�
                return o;
            }

            // �t���O�����g�V�F�[�_�[
            fixed4 frag(v2f i) : SV_Target
            {
                return _Color; // �w�肳�ꂽ�F�����̂܂܏o��
            }
            ENDCG
        }
    }
}
