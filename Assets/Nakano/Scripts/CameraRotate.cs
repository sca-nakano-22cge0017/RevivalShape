using UnityEngine;

/// <summary>
/// 確認フェーズ　カメラ制御
/// </summary>
public class CameraRotate : MonoBehaviour
{
    [SerializeField, Header("デバッグ用")] SampleCheck sampleCheck;

    [SerializeField] StageController stageController;
    [SerializeField] Camera _camera;
    Vector3 mapSize;

    // 注視位置
    Vector3 target;

    // 初期位置
    Vector3 defaultPos;
    Quaternion defaultRot;

    [SerializeField, Header("ajustAngle度毎に補正ポイントを置く")] float adjustAngle;
    Vector3[] point; // カメラ位置の補正 一定距離まで近付いたらこの座標に移動させる

    // 回転
    [SerializeField, Header("ドラッグの感度")] float sensitivity;
    Vector2 sPos;    //タッチした座標
    float wid, hei;  //スクリーンサイズ
    float tx, ty;

    // 拡縮
    float sDist = 0.0f, nDist = 0.0f;  //距離変数
    float vRatio = 1.0f;               //現在倍率
    [SerializeField, Header("Field of Viewの最大・最低値")] float vMax, vMin;
    [SerializeField, Header("Field of Viewの初期値")] float vDefault;
    [SerializeField, Header("基本の拡縮速度")] float vSpeed;

    [SerializeField, Header("スワイプ 手振れ補正値"), Tooltip("斜め方向へのスワイプをx軸/y軸に真っ直ぐな移動に補正する")]
    float dragAjust = 10;

    [SerializeField, Header("スワイプの範囲 最小")] Vector2 dragRangeMin;
    [SerializeField, Header("スワイプの範囲 最大")] Vector2 dragRangeMax;

    /// <summary>
    /// カメラを動かすかどうか
    /// falseのときは動かない
    /// </summary>
    public bool CanRotate { get; set; } = false;

    Vector3 movedPos = new Vector3(0, 0, 0); // debug

    void Awake()
    {
        wid = Screen.width;
        hei = Screen.height;

        point = new Vector3[(int)(360 / adjustAngle)];
    }

    void Update()
    {
        if (!CanRotate) return;

        // 回転
        if (Input.touchCount == 1)
        {
            Touch t1 = Input.GetTouch(0);

            if (t1.position.x <= dragRangeMin.x || t1.position.x > dragRangeMax.x ||
                t1.position.y <= dragRangeMin.y || t1.position.y > dragRangeMax.y)
                return; // 範囲外なら終了

            if (t1.phase == TouchPhase.Began)
            {
                sPos = t1.position;
            }
            else if (t1.phase == TouchPhase.Moved)
            {
                // スワイプ量
                tx = t1.position.x - sPos.x;
                ty = t1.position.y - sPos.y;

                // 移動量から回転角度を求める
                // dragAjustより移動量の絶対値が小さかったら0にし、水平/垂直の移動にする
                if (Mathf.Abs(tx) < dragAjust) tx = 0;
                float deltaAngleLR = tx / wid * sensitivity;

                if (Mathf.Abs(ty) < dragAjust) ty = 0;
                float deltaAngleTB = -ty / hei * sensitivity;

                // カメラから見て上・右向きのベクトルを回転軸として回転させる
                var angleAxisLR = Quaternion.AngleAxis(deltaAngleLR, transform.up); // 左右方向
                var angleAxisTB = Quaternion.AngleAxis(deltaAngleTB, transform.right); // 上下方向

                var pos = transform.position;
                pos -= target;
                pos = angleAxisLR * angleAxisTB * pos; // 回転移動
                pos += target;                          // 平行移動

                transform.position = pos;
                transform.LookAt(target, transform.up);

                // カメラがサンプルの下に回り込まないように調整
                if (pos.y <= 0.005f)
                {
                    CameraPosAdjust();
                }

                sPos = t1.position;
            }
        }

        // 拡縮
        else if (Input.touchCount == 2)
        {
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            if (t1.position.x <= dragRangeMin.x || t1.position.x > dragRangeMax.x ||
                t1.position.y <= dragRangeMin.y || t1.position.y > dragRangeMax.y)
                return; // 範囲外なら終了

            if (t2.position.x <= dragRangeMin.x || t2.position.x > dragRangeMax.x ||
                t2.position.y <= dragRangeMin.y || t2.position.y > dragRangeMax.y)
                return; // 範囲外なら終了

            if (t2.phase == TouchPhase.Began)
            {
                // タップした二点間の距離を取得
                sDist = Vector2.Distance(t1.position, t2.position);

                vRatio = _camera.fieldOfView;
            }
            else if (t1.phase == TouchPhase.Moved || t2.phase == TouchPhase.Moved)
            {
                nDist = Vector2.Distance(t1.position, t2.position);

                // 縮小
                if (sDist > nDist) vRatio += vSpeed * (1 - (vRatio - vMin) / (vMax - vMin)) * (sDist - nDist);
                // 拡大
                if (sDist < nDist) vRatio -= vSpeed * ((vRatio - vMin) / (vMax - vMin)) * (nDist - sDist);

                _camera.fieldOfView = vRatio;
                sDist = nDist;
            }
        }

#if false // PC上で動かす場合
        if (Input.GetMouseButtonDown(0))
        {
            movedPos = Input.mousePosition;
        }
        if (Input.GetMouseButton(0))
        {
            tx = (movedPos.x - Input.mousePosition.x) / wid;
            ty = (movedPos.y - Input.mousePosition.y) / hei;

            float deltaAngleLR = -tx * 5;
            float deltaAngleTB = ty * 5;

            var angleAxisLR = Quaternion.AngleAxis(deltaAngleLR, transform.up);
            var angleAxisTB = Quaternion.AngleAxis(deltaAngleTB, transform.right);

            var pos = transform.position;
            pos -= target;
            pos = angleAxisLR * angleAxisTB * pos;
            pos += target;

            if (pos.y > 0)
            {
                transform.position = pos;
                transform.rotation = angleAxisLR * angleAxisTB * transform.rotation;
            }
        }

        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.O))
        {
            vRatio = _camera.fieldOfView;
        }

        if (Input.GetKey(KeyCode.I)) // 拡大
        {
            vRatio -= vSpeed * ((vRatio - vMin) / (vMax - vMin));
            _camera.fieldOfView = vRatio;
        }
        if (Input.GetKey(KeyCode.O)) // 縮小
        {
            vRatio += vSpeed * (1 - (vRatio - vMin) / (vMax - vMin));
            _camera.fieldOfView = vRatio;
        }
#endif
    }

    /// <summary>
    /// カメラ位置を補正する
    /// </summary>
    void CameraPosAdjust()
    {
        Vector3 nearPos = new Vector3(100, 100, 100);

        var pos = transform.position;
        
        foreach(var p in point)
        {
            var nearDiff = (pos - nearPos).magnitude;
            var diff = (pos - p).magnitude;

            if(nearDiff > diff) nearPos = p;
        }

        transform.position = nearPos;
        transform.LookAt(target, transform.up);
    }

    /// <summary>
    /// カメラの位置・回転を初期状態に戻す
    /// </summary>
    public void RotateReset()
    {
        transform.position = defaultPos;
        transform.rotation = defaultRot;

        _camera.fieldOfView = vDefault;
    }

    /// <summary>
    /// カメラの注視位置を設定
    /// </summary>
    public void TargetSet()
    {
        // サイズ代入
        if (stageController)
            mapSize = stageController.MapSize;
        else if (sampleCheck)
            mapSize = sampleCheck.MapSize;

        // 注視位置設定
        target = mapSize / 2;
        target.x *= -1;
        target.y = 0;
        target += new Vector3(0.5f, 0, -0.5f);

        // サンプルのサイズに応じてカメラ位置を調整
        transform.position = new Vector3(-mapSize.x / 2 + 0.5f, mapSize.x + 10, mapSize.z + 2);
        defaultPos = transform.position;
        defaultRot = transform.rotation;

        //! Todo サイズに応じて、Field of Viewの初期値・最大値・最小値も変更

        // 補正ポイントの生成
        float r = (target - defaultPos).magnitude;
        Vector3 firstPos = new Vector3(r, 0, 0);
        
        for (int i = 0; i < point.Length; i++)
        {
            point[i] = Quaternion.Euler(0, i * adjustAngle, 0) * firstPos + target;
        }
    }
}
