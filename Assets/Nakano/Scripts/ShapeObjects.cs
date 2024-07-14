using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeObjects : MonoBehaviour
{
    private PlayPhase playPhase;
    private Vibration vibration;
    private StageController stageController;

    bool isVibrate = false;
    public bool IsVibrate { get{ return isVibrate; } set{ isVibrate = value; } }

    float vibrateTime = 0.3f;
    float vibrateTime_Normal = 0.3f;
    float vibrateTime_FastForward = 0.1f;

    /// <summary>
    /// �������x
    /// </summary>
    public float FallSpeed { get; set; } = 10;

    /// <summary>
    /// true�̂Ƃ�����
    /// </summary>
    public bool IsFall { get; set; } = false;

    /// <summary>
    /// �ڕW�̍����@�����I�����ɂ��̍����ŗ��܂�
    /// </summary>
    public int TargetHeight { get; set; } = 0;

    float fastForwardRatio = 1;

    private void Awake()
    {
        vibration = GameObject.FindObjectOfType<Vibration>();
        playPhase = GameObject.FindObjectOfType<PlayPhase>();
        stageController = GameObject.FindObjectOfType<StageController>();

        // �U�����Ԏ擾
        vibrateTime_Normal = playPhase.GetVibrateTime()[0];
        vibrateTime_FastForward = playPhase.GetVibrateTime()[1];
    }

    private void Update()
    {
        // �������x�擾�A�U�����ԕύX
        if(playPhase.IsFastForward)
        {
            vibrateTime = vibrateTime_FastForward;
            fastForwardRatio = playPhase.FastForwardRatio;
        }
        else
        {
            fastForwardRatio = 1;
            vibrateTime = vibrateTime_Normal;
        }

        if(stageController.phase != StageController.PHASE.PLAY) return;

        if(IsFall && !stageController.IsPause)
        {
            var tmpPos = this.transform;
            tmpPos.Translate(Vector3.down * FallSpeed * fastForwardRatio * Time.deltaTime);

            // ��������
            if (tmpPos.position.y >= TargetHeight)
            {
                transform.position = tmpPos.position;
            }
            else
            {
                vibration.PluralVibrate(1, (long)(vibrateTime * 1000));

                var pos = transform.position;
                pos.y = TargetHeight;
                transform.position = pos;
                IsFall = false;
            }
        }
    }
}
