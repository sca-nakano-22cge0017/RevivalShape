using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// FPS�\��
// https://note.com/sowa_jp/n/n8aa9e845fd30
public class FpsToText : MonoBehaviour
{
	[SerializeField]
	Text text;

	[SerializeField, Header("���b���ƂɃe�L�X�g�X�V���邩"), Range(0, 5)]
	int second = 1;

	int frameCount = 0;//Update���Ă΂ꂽ�񐔃J�E���g�p
	float oldTime = 0.0f;//�O��t���[�����[�g��\�����Ă���̌o�ߎ��Ԍv�Z�p

	void Update()
	{
		//Update���Ă΂ꂽ�񐔂����Z
		frameCount++;

		//�O�t���[������̌o�ߎ��Ԃ��v�Z�FTime.realtimeSinceStartup�̓Q�[���J�n������̌o�ߎ��ԁi�b�j
		float time = Time.realtimeSinceStartup - oldTime;

		//�w�莞�Ԃ𒴂�����e�L�X�g�X�V
		if (time >= second)
		{
			//�t���[�����[�g���v�Z
			float fps = frameCount / time;

			//�v�Z�����t���[�����[�g�������_2���܂Ŋۂ߂ăe�L�X�g�\���FSetText()���g�p���ăG�f�B�^�ȊO�ł�GC�𔭐������Ȃ�
			text.text = fps.ToString();

			//�J�E���g�ƌo�ߎ��Ԃ����Z�b�g
			frameCount = 0;
			oldTime = Time.realtimeSinceStartup;
		}
	}

}