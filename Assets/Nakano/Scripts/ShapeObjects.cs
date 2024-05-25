using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeObjects : MonoBehaviour
{
    PlayPhase playPhase;
    Vibration vibration;

    bool isVibrate = false;
    public bool IsVibrate { get{ return isVibrate; } set{ isVibrate = value; } }

    public float VibrateTime { get; set; } = 0.3f;

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
    }

    private void Update()
    {
        // 落下速度取得
        if(playPhase.IsFastForward)
        {
            fastForwardRatio = playPhase.FastForwardRatio;
        }
        else fastForwardRatio = 1;
    }

    private void FixedUpdate()
    {
        if (!IsFall) return;

        // 落下処理
        if (transform.position.y > TargetHeight)
        {
            transform.Translate(Vector3.down * FallSpeed * fastForwardRatio * Time.deltaTime);
        }
        else
        {
            var pos = transform.position;
            pos.y = TargetHeight;
            transform.position = pos;
            IsFall = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 衝突時に振動
        if (isVibrate)
        {
            vibration.PluralVibrate(1, (long)(VibrateTime * 1000));
        }
    }
}
