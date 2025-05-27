using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using TMPro;
using Oculus.Interaction.HandGrab;

public class ChangeLayerOnToggle : MonoBehaviour
{
    [SerializeField] private GameObject _worldDoll; // レイヤーを変更する対象のオブジェクト1
    [SerializeField] private GameObject _Hips; // レイヤーを変更する対象のオブジェクト2

    [SerializeField] private string newLayerName1 = "None"; // 変更後のレイヤー名1
    [SerializeField] private string newLayerName2 = "Both"; // 変更後のレイヤー名2

    [SerializeField]
    private TMP_Text targetText; // 色を変更する対象のTextMeshPro

    [SerializeField]
    private DistanceHandGrabInteractable _dhandGrab;

    [SerializeField]
    private HandGrabInteractable _handGrab;


    [SerializeField]
    private Color onColor = Color.green; // トグルがオンのときの色
    [SerializeField]
    private Color offColor = Color.red;  // トグルがオフのときの色


    private bool toggle=false;

    private void Start()
    {
        // トグルの初期状態に応じて色を設定
        targetText.color = offColor;

    }

    void Update()
    {
        if (Input.GetKeyDown("a")){
            ChangeLayer();
        }
    }

    // ボタンを押したときに呼び出されるメソッド
    public void ChangeLayer()
    {
        toggle = !toggle;
        if (toggle)
        {
            //ボタンの色を変更
            targetText.color = onColor;

            //Distancegrabを有効化
            _handGrab.enabled=true;
            //Distancegrabを有効化
            _dhandGrab.enabled = true;

            // レイヤー名をレイヤー番号に変換
            int newLayer1 = LayerMask.NameToLayer(newLayerName1);
            int newLayer2 = LayerMask.NameToLayer(newLayerName2);

            // オブジェクトとその子オブジェクトのレイヤーを変更
            SetLayerRecursively(_worldDoll, newLayer2);
            SetLayerRecursively(_Hips, newLayer1);
            Debug.Log("toggle:true");
        }
        else
        {
            //ボタンの色を変更
            targetText.color = offColor;

            //Distancegrabを無効化
            _handGrab.enabled = false;

            //Distancegrabを無効化
            _dhandGrab.enabled = false;

            // レイヤー名をレイヤー番号に変換
            int newLayer1 = LayerMask.NameToLayer(newLayerName1);
            int newLayer2 = LayerMask.NameToLayer(newLayerName2);

            // オブジェクトとその子オブジェクトのレイヤーを変更
            SetLayerRecursively(_worldDoll, newLayer1);
            SetLayerRecursively(_Hips, newLayer2);
            Debug.Log("toggle:false");

        }
        
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        // 子オブジェクトも再帰的に変更
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
