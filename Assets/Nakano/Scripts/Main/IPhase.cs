using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 各フェーズの制御クラス用インターフェース
/// </summary>
public interface IPhase
{
    /// <summary>
    /// 初期化
    /// </summary>
    void Initialize();

    /// <summary>
    /// フェーズ開始時の処理
    /// </summary>
    void PhaseStart();

    /// <summary>
    /// フェーズ中の処理
    /// </summary>
    void PhaseUpdate();

    /// <summary>
    /// フェーズ終了時の処理
    /// </summary>
    void PhaseEnd();
}
