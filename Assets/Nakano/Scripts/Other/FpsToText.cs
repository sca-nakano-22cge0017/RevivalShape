using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// FPS表示
// https://note.com/sowa_jp/n/n8aa9e845fd30
public class FpsToText : MonoBehaviour
{
	[SerializeField]
	Text text;

	[SerializeField, Header("何秒ごとにテキスト更新するか"), Range(0, 5)]
	int second = 1;

	int frameCount = 0;//Updateが呼ばれた回数カウント用
	float oldTime = 0.0f;//前回フレームレートを表示してからの経過時間計算用

	void Update()
	{
		//Updateが呼ばれた回数を加算
		frameCount++;

		//前フレームからの経過時間を計算：Time.realtimeSinceStartupはゲーム開始時からの経過時間（秒）
		float time = Time.realtimeSinceStartup - oldTime;

		//指定時間を超えたらテキスト更新
		if (time >= second)
		{
			//フレームレートを計算
			float fps = frameCount / time;

			//計算したフレームレートを小数点2桁まで丸めてテキスト表示：SetText()を使用してエディタ以外ではGCを発生させない
			text.text = fps.ToString();

			//カウントと経過時間をリセット
			frameCount = 0;
			oldTime = Time.realtimeSinceStartup;
		}
	}

}