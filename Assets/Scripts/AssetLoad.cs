using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetLoad : MonoBehaviour
{
    private async void Awake()
    {
        //AssetBundle ab  = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/mcx");
        //GameObject T = ab.LoadAsset<GameObject>("MCX");
        //Instantiate(T);

        StartCoroutine(LoadABAysnc("mcx", "MCx"));
    }

    AssetBundleCreateRequest abcr;
    /// <summary>
    /// �첽����AB��
    /// </summary>
    /// <param name="Abname"></param>
    /// <param name="ResName"></param>
    /// <returns></returns>
    IEnumerator LoadABAysnc(string Abname, string ResName)
    {
        //��һ������ab��
        abcr = AssetBundle.LoadFromFileAsync($"{Application.streamingAssetsPath}/{Abname}");
        yield return abcr;
        //������Դ
        AssetBundleRequest abr = abcr.assetBundle.LoadAssetAsync<GameObject>(Abname);
        yield return abr;

        RelyOn();
        RelyOn();
        Instantiate(abr.asset as GameObject);
    }


    public void RelyOn()
    {
        //��������
        AssetBundle abMain = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + "StandaloneWindows");

        //���������ڵĹ̶��ļ�
        AssetBundleManifest abm = abMain.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

        //�ӹ̶��ļ��еõ�mcx��������
        string[] strs = abm.GetAllDependencies("mcx");

        //��������������
        for (int i = 0; i < strs.Length; i++)
        {
            //AssetBundle ab = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + strs[i]);
//Debug.Log(ab.name);
        }

    }


    private void OnGUI()
    {
        if (GUI.Button(new Rect(25,25,100,100),"ж��ab��(ȫ)"))
        {
            abcr.assetBundle.Unload(true);
        }

        if (GUI.Button(new Rect(25, 125, 100, 100), "ж��ab��(����)"))
        {
            abcr.assetBundle.Unload(false);
        }
    }
}
