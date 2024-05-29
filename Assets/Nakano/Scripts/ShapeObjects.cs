using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeObjects : MonoBehaviour
{
    PlayPhase playPhase;
    Vibration vibration;

    bool isVibrate = false;
    public bool IsVibrate { get{ return isVibrate; } set{ isVibrate = value; } }

    float vibrateTime = 0.3f;
    float vibrateTime_Normal = 0.3f;
    float vibrateTime_FastForward = 0.1f;

    /// <summary>
    /// 落下速度
    /// </summary>
    public float FallSpeed { get; set; } = 10;

    /// <summary>
    /// trueのとき落下
    /// </summary>
    public bool IsFall { get; set; } = false;

    /// <summary>
    /// 目標の高さ　落下終了時にこの高さで留まる
    /// </summary>
    public int TargetHeight { get; set; } = 0;

    float fastForwardRatio = 1;

    private void Awake()
    {
        vibration = GameObject.FindObjectOfType<Vibration>();
        playPhase = GameObject.FindObjectOfType<PlayPhase>();

        // 振動時間取得
        vibrateTime_Normal = playPhase.GetVibrateTime()[0];
        vibrateTime_FastForward = playPhase.GetVibrateTime()[1];
    }

    private void Update()
    {
        // 落下速度取得、振動時間変更
        if(playPhase.IsFastForward)
        {
            fastForwardRatio = playPhase.FastForwardRatio;
            vibrateTime = vibrateTime_FastForward;
        }
        else
        {
            fastForwardRatio = 1;
            vibrateTime = vibrateTime_Normal;
        }

        if(IsFall)
        {
            var tmpPos = this.transform;
            tmpPos.Translate(Vector3.down * FallSpeed * fastForwardRatio * Time.deltaTime);

            // 落下処理
            if (tmpPos.position.y >= TargetHeight)
            {
                transform.position = tmpPos.position;
            }
            else
            {
                var pos = transform.position;
                pos.y = TargetHeight;
                transform.position = pos;
                IsFall = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 衝突時 かつ 目標位置まで来ていたら振動
        if (Mathf.Abs(transform.position.y - TargetHeight) <= 0.1f && isVibrate)
        {
            vibration.PluralVibrate(1, (long)(vibrateTime * 1000));
        }
    }
}
