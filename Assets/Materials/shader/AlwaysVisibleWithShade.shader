Shader "Custom/AlwaysVisibleWithShade"
{
    Properties
    {
        _Color ("Main Color", Color) = (0,0,1,1) // �f�t�H���g�F: �� (R: 0, G: 0, B: 1, A: 1)
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" } // ��ɍőO�ʂɕ`��
        Pass
        {
            ZTest Always         // �[�x�e�X�g������: ��ɕ`��
            ZWrite Off           // �[�x�o�b�t�@�������݂𖳌���
            ColorMask RGB        // �F�̂ݕ`��i�A���t�@�͖����j

            // �F��K�p
            Material
            {
                Diffuse [_Color]
            }

            Lighting On          // �Ɩ���K�p�i�K�v�ɉ����Ē����j
        }
    }
}
