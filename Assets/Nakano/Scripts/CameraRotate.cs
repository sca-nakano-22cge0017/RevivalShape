using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 確認フェーズ　カメラ回転
/// </summary>
public class CameraRotate : MonoBehaviour
{
    [SerializeField] StageController stageController;
    [SerializeField] Camera camera;
    Vector3 mapSize;

    Vector3 target;

    // 回転
    [SerializeField, Header("ドラッグの感度")] float sensitivity;
    Vector2 sPos;   //タッチした座標
    float wid, hei;  //スクリーンサイズ
    float tx, ty;

    Vector3 defaultPos;
    Quaternion defaultRot;

    // 拡縮
    float sDist = 0.0f, nDist = 0.0f; //距離変数
    float vRatio = 1.0f; //現在倍率
    [SerializeField, Header("Field of Viewの最大・最低値")] float vMax, vMin;
    [SerializeField, Header("Field of Viewの初期値")] float vDefault;
    [SerializeField, Header("基本の拡縮速度")] float vSpeed;

    [SerializeField, Header("スワイプの範囲 最小")] Vector2 dragRangeMin;
    [SerializeField, Header("スワイプの範囲 最大")] Vector2 dragRangeMax;

    bool canRotate = false;
    public bool CanRotate { get { return canRotate; }  set { canRotate = value; } }

    void Start()
    {
        wid = Screen.width;
        hei = Screen.height;

        defaultPos = transform.position;
        defaultRot = transform.rotation;
    }

    void Update()
    {
        if(!canRotate) return;

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
                tx = (t1.position.x - sPos.x) / wid; //横移動量(-1<tx<1)
                ty = (t1.position.y - sPos.y) / hei; //縦移動量(-1<ty<1)

                // マウス移動量から求めた回転角度
                float deltaAngleLR = tx * sensitivity;
                float deltaAngleTB = -ty * sensitivity;

                var angleAxisLR = Quaternion.AngleAxis(deltaAngleLR, transform.up);
                var angleAxisTB = Quaternion.AngleAxis(deltaAngleTB, transform.right);

                var pos = transform.position;
                pos -= target;
                pos = angleAxisLR * angleAxisTB * pos;
                pos += target;

                if(pos.y > 0)
                {
                    transform.position = pos;
                    transform.rotation = angleAxisLR * angleAxisTB * transform.rotation;
                }
            }
        }
        else if(Input.touchCount == 2)
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
                sDist = Vector2.Distance(t1.position, t2.position);

                vRatio = camera.fieldOfView;
            }
            else if (t1.phase == TouchPhase.Moved || t2.phase == TouchPhase.Moved)
            {
                nDist = Vector2.Distance(t1.position, t2.position);
                if (sDist > nDist) vRatio += vSpeed * (1 - (vRatio - vMin) / (vMax - vMin)) * (sDist - nDist); // 縮小
                if (sDist < nDist) vRatio -= vSpeed * ((vRatio - vMin) / (vMax - vMin)) * (nDist - sDist); // 拡大
                camera.fieldOfView = vRatio;
                sDist = nDist;
            }
        }

        #if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.O))
        {
            vRatio = camera.fieldOfView;
        }

        if (Input.GetKey(KeyCode.I)) // 拡大
        {
            vRatio -= vSpeed * ((vRatio - vMin) / (vMax - vMin));
            camera.fieldOfView = vRatio;
        }
        if (Input.GetKey(KeyCode.O)) // 縮小
        {
            vRatio += vSpeed * (1 - (vRatio - vMin) / (vMax - vMin));
            camera.fieldOfView = vRatio;
        }
        #endif
    }

    public void RotateReset()
    {
        transform.position = defaultPos;
        transform.rotation = defaultRot;

        camera.fieldOfView = vDefault;
    }

    public void MapSizeInitialize()
    {
        mapSize = stageController.MapSize; // サイズ代入

        target = mapSize / 2;
        target.x *= -1;
    }
}
