using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

/// <summary>
/// メインゲーム制御
/// </summary>
public class StageController : MonoBehaviour
{
    // フェーズ
    public enum PHASE { CHECK = 0, SELECT, PLAY, };
    public PHASE phase = 0;

    [SerializeField, Header("ステージ名")] string stageName;
    public string StageName { get { return stageName; } }

    [SerializeField] ShapeData shapeData;
    [SerializeField] StageDataLoader stageDataLoader;

    [SerializeField] CameraRotate cameraRotate;
    [SerializeField] SheatCreate sheatCreate;

    [SerializeField] CheckPhase checkPhase;
    [SerializeField] SelectPhase selectPhase;
    [SerializeField] PlayPhase playPhase;

    public Vector3 MapSize { get; private set; } = new Vector3(4, 4, 4);

    // データ取得完了したかどうか
    bool dataGot = false;

    /// <summary>
    /// ミス回数
    /// </summary>
    public int Miss { get; private set; } = 0;

    /// <summary>
    /// 再確認回数
    /// </summary>
    public int Reconfirmation { get; private set; } = 0;

    /// <summary>
    /// 使用図形の種類一覧
    /// </summary>
    public ShapeData.Shape[] ShapeType { get; private set; }

    /// <summary>
    /// 正しい答え
    /// </summary>
    public ShapeData.Shape[,,] CorrectAnswer { get; set; }

    /// <summary>
    /// プレイヤーの答え
    /// </summary>
    public ShapeData.Shape[,,] PlayerAnswer { get; set; }

    /// <summary>
    /// trueでステージクリア
    /// </summary>
    public bool IsClear { get; set; } = false;

    /// <summary>
    /// trueのときリトライ
    /// </summary>
    public bool IsRetry { get; set; } = false;

    [SerializeField, Header("スワイプの範囲 最小")] Vector2 dragRangeMin;
    [SerializeField, Header("スワイプの範囲 最大")] Vector2 dragRangeMax;

    [SerializeField, Header("フレームレート")] int fps = 120;

    // タップ/スワイプ可能範囲を描画
    [SerializeField] Texture _texture;
    [SerializeField] bool isDragRangeDraw = false;

    private void Awake()
    {
        if(SelectButton.SelectStage != null)
            stageName = SelectButton.SelectStage; // 選択ステージ名を取得

        stageDataLoader.StageDataGet(stageName);  // ステージの配置データをロード開始

        Application.targetFrameRate = fps;

        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.red);
        texture.Apply();
        _texture = texture;
    }

    void Update()
    {
        // ロードが終わっていなければ次の処理に進ませない
        if (!stageDataLoader.stageDataLoadComlete) return;

        // データを変数として取得していなければ
        if(!dataGot)
        {
            dataGot = true;

            // マップサイズ取得
            MapSize = stageDataLoader.LoadStageSize();
            
            // 配列 要素数指定
            ShapeType = new ShapeData.Shape[System.Enum.GetValues(typeof(ShapeData.Shape)).Length];
            CorrectAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];
            PlayerAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];

            for (int z = 0; z < MapSize.z; z++)
            {
                for (int y = 0; y < MapSize.y; y++)
                {
                    for (int x = 0; x < MapSize.x; x++)
                    {
                        CorrectAnswer[x, y, z] = ShapeData.Shape.Empty;
                        PlayerAnswer[x, y, z] = ShapeData.Shape.Empty;
                    }
                }
            }

            cameraRotate.TargetSet();

            // 正答取得
            CorrectAnswer = stageDataLoader.LoadStageMap(MapSize);

            // 使用している図形の種類を取得
            ShapeType = shapeData.ShapeTypes(CorrectAnswer);

            checkPhase.Initialize();
            selectPhase.Initialize();
            playPhase.Initialize();

            // シート作成
            sheatCreate.Sheat();

            // 確認フェーズに移行
            ToCheckPhase();
        }

        // クリア時の遷移処理
        if (IsClear && Input.GetMouseButton(0))
        {
            if (!TapOrDragRange(Input.mousePosition)) return;

            // ステージ選択画面に戻る
            if (!playPhase.IsDebug)
                SceneManager.LoadScene("SelectScene");
            IsClear = false;
        }

        // 再挑戦時の処理
        if (IsRetry && Input.GetMouseButton(0))
        {
            if(!TapOrDragRange(Input.mousePosition)) return;

            // 確認フェーズに戻る
            if (!playPhase.IsDebug)
            {
                ToCheckPhase();
                TestButton testButton = GameObject.FindObjectOfType<TestButton>();
                testButton.BackToggle();
            }
                
            IsRetry = false;
        }
    }

    /// <summary>
    /// 確認フェーズに移行
    /// フェーズを管理するenum型変数の変更、他フェーズ用のUI非表示等
    /// </summary>
    public void ToCheckPhase()
    {
        switch (phase)
        {
            case PHASE.SELECT:
                Reconfirmation++;
                break;
            case PHASE.PLAY:
                Miss++;
                cameraRotate.Restore();
                break;
        }

        phase = PHASE.CHECK;

        playPhase.PlayPhaseEnd();
        selectPhase.SelectPhaseEnd();

        // カメラの回転ができるようにする
        cameraRotate.CanRotate = true;

        // シートの表示設定
        sheatCreate.SheatDisp(true, true);

        // 確認フェーズ開始処理
        checkPhase.CheckPhaseStart();
    }

    /// <summary>
    /// 選択フェーズに移行
    /// フェーズを管理するenum型変数の変更、他フェーズ用のUI非表示等
    /// </summary>
    public void ToSelectPhase()
    {
        phase = PHASE.SELECT;

        checkPhase.CheckPhaseEnd();
        playPhase.PlayPhaseEnd();

        // カメラ
        cameraRotate.CanRotate = false;
        cameraRotate.RotateReset();

        // シート
        sheatCreate.SheatDisp(false, false);

        // 選択フェーズ開始処理
        selectPhase.SelectPhaseStart();
    }

    /// <summary>
    /// 実行フェーズに移行
    /// フェーズを管理するenum型変数の変更、他フェーズ用のUI非表示等
    /// </summary>
    public void ToPlayPhase()
    {
        phase = PHASE.PLAY;

        checkPhase.CheckPhaseEnd();
        selectPhase.SelectPhaseEnd();

        // シート
        sheatCreate.SheatDisp(true, false);

        cameraRotate.PlayPhaseCamera();

        // 実行フェーズ開始処理
        playPhase.PlayPhaseStart();
    }

    /// <summary>
    /// タップ/ドラッグできるかを判定する　範囲外をタップしているならfalse
    /// </summary>
    /// <param name="_pos">タップ位置</param>
    /// <returns></returns>
    public bool TapOrDragRange(Vector3 _pos)
    {
        bool canTap = false;

        var p = _pos;
        if (p.x <= dragRangeMin.x || p.x > dragRangeMax.x || p.y <= dragRangeMin.y || p.y > dragRangeMax.y)
            canTap = false;
        else canTap = true;

        return canTap;
    }

    /// <summary>
    /// タップ/ドラッグできるかを判定する　範囲外をタップしているならfalse
    /// </summary>
    /// <param name="_pos">タップ位置</param>
    /// <param name="_minPos">範囲　最小値</param>
    /// <param name="_maxPos">範囲　最大値</param>
    /// <returns></returns>
    public bool TapOrDragRange(Vector3 _pos, Vector3 _minPos, Vector3 _maxPos)
    {
        bool canTap = false;

        if (_pos.x <= _minPos.x || _pos.x > _maxPos.x || _pos.y <= _minPos.y || _pos.y > _maxPos.y)
            canTap = false;
        else canTap = true;

        return canTap;
    }

    /// <summary>
    /// タップ/ドラッグできる範囲を取得
    /// </summary>
    /// <returns>範囲の最小/最大の座標が返ってくる</returns>
    public Vector2[] GetTapOrDragRange()
    {
        Vector2[] r = {dragRangeMin, dragRangeMax};
        return r;
    }

    private void OnGUI()
    {
        if(isDragRangeDraw)
        {
            var rect = new Rect(dragRangeMin.x, dragRangeMin.y, dragRangeMax.x - dragRangeMin.x, dragRangeMax.y - dragRangeMin.y);
            GUI.DrawTexture(rect, _texture);
        }
    }
}
