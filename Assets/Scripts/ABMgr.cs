using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;


public class ABMgr : MonoBehaviour
{

    /// <summary>     ������    </summary>
    public string ABMainName
    {
        get
        {
#if UNITY_DEITOR || UNITY_STANDALONE
            return "StandaloneWindows";
#elif UNITY_IOS
                return "Ios";
#elif UNITY_ANDROID
                return "Android"
#endif
        }
    }

    /// <summary>    ·��     </summary>
    public string Path
    {
        get
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return Application.streamingAssetsPath + "/AssetBundle/";
#endif
        }
    }


    /// <summary>    �ļ�����·��     </summary>
    private string LoadSavePath
    {
        get
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return Application.streamingAssetsPath + "/AssetBundle/";
#endif
        }
    }

    private string ServerPath = "http://192.168.10.104:5000/AssetBundle/";

    private string CompareFile = "ABCompareInfo.txt";


    #region ����AssetBundle

    private void Awake()
    {
        Load();
    }

    async public void Load()
    {
        await System.Threading.Tasks.Task.Delay(3000);

        ////�첽����
        //LoadSceneAsync("scene 1", (x) =>
        //{
        //    //SceneManager.LoadScene(x, LoadSceneMode.Additive);
        //    Manager_Scene.LoadSceneAsync(x, LoadSceneMode.Additive, (_) => 
        //    {
        //        for (int i = 0; i < 100; i++)
        //        {
        //            //Instantiate(LoadRes<GameObject>("gun", "MCX"));
        //            LoadResAsync<GameObject>("zombie1", "zombie1", (_) => { Instantiate(_); });
        //            //LoadScnen("scene 1", (_) => { Manager_Scene.LoadSceneAsync(_, LoadSceneMode.Additive); });
        //        }
        //    });
        //});

        //ͬ�����ط���
        //LoadScnen("scene 1", (x) =>
        //{
        //    Manager_Scene.LoadSence(x, LoadSceneMode.Additive);

        //    for (int i = 0; i < 10; i++)
        //    {
        //        Instantiate(LoadRes<GameObject>("zombie1", "zombie1"));
        //    }
        //});


        //for (int i = 0; i < 10; i++)
        //{
        //    Instantiate(LoadRes<GameObject>("zombie1", "zombie1"));
        //}

        //LocalLoad();


        NetLoadAsset();
    }

    AssetBundleManifest abmf;
    /// <summary> AssetBundle����ű� ���ڼ���Ƿ����ظ�����AssetBundle����ű�   </summary>
    private Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();

    /// <summary> �������ݼ�¼�����ظ�����Ч��   </summary>
    private Dictionary<string, dynamic> LoadedAss = new Dictionary<string, dynamic>();

    #region ͬ������

    /// <summary>
    /// ͬ������AB��
    /// </summary>
    /// <param name="abName"></param>
    private void LoadAB(string abName)
    {
        //��ȡ����
        if (!abDic.ContainsKey("StandaloneWindows"))
        {
            AssetBundle abmain = AssetBundle.LoadFromFile(Path + ABMainName);
            abDic.Add("StandaloneWindows", abmain);

            //��ȡ��������
            abmf = abDic["StandaloneWindows"].LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
        //��ȡ��������
        string[] request = abmf.GetAllDependencies(abName);
        //������������
        for (int i = 0; i < request.Length; i++)
        {
            if (!abDic.ContainsKey(request[i]))
            {
                var Depent = AssetBundle.LoadFromFile(Path + request[i]);
                abDic.Add(request[i], Depent);
            }
        }
        //����Ŀ���
        if (!abDic.ContainsKey(abName))
        {
            AssetBundle ab = AssetBundle.LoadFromFile(Path + abName);
            abDic.Add(abName, ab);
        }
    }

    /// <summary>
    /// ͬ�����ط���
    /// </summary>
    /// <param name="abName">ab����</param>
    /// <param name="ResName">��Դ����</param>
    /// <returns></returns>
    public object LoadRes(string abName, string ResName)
    {
        LoadAB(abName);
        if (!LoadedAss.ContainsKey(abName))
        {
            LoadedAss.Add(abName, abDic[abName].LoadAsset(ResName));
        }
        return LoadedAss[abName];
    }

    public T LoadRes<T>(string abName, string ResName) where T : Object
    {
        LoadAB(abName);
        if (!LoadedAss.ContainsKey(abName))
        {
            LoadedAss.Add(abName, abDic[abName].LoadAsset<T>(ResName));
        }
        return LoadedAss[abName];
    }

    public Object LoadRes(string abName, string ResName, System.Type types)
    {
        LoadAB(abName);
        if (!LoadedAss.ContainsKey(abName))
        {
            LoadedAss.Add(abName, abDic[abName].LoadAsset(ResName, types));
        }
        return LoadedAss[abName];
    }

    public void LoadScnen(string abName, UnityAction<string> CallBack)
    {
        LoadAB(abName);
        if (!LoadedAss.ContainsKey(abName))
        {
            LoadedAss.Add(abName, abDic[abName].GetAllScenePaths());
        }
        CallBack?.Invoke(LoadedAss[abName][0]);
    }


    #endregion


    #region �첽����

    List<IEnumerator> ieList = new List<IEnumerator>();
    //private AssetBundle mainPage;
    private AssetBundleManifest abManifest;
    public float ProcessValue;
    public float ResVale;
    private AssetBundleCreateRequest CueentProcess;
    private AssetBundleRequest CueentRes;

    /// <summary>
    /// �첽����AB��
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="isMainPage"></param>
    public void LoadABAsync(string abName, bool isMainPage = false)
    {
        StartCoroutine(IE_LoadABAsync(abName, isMainPage));
    }


    IEnumerator IE_LoadABAsync(string abName, bool isMainPage = false)
    {
        abDic.Add(abName, null);
        AssetBundleCreateRequest abcr = AssetBundle.LoadFromFileAsync(Path + abName);
        CueentProcess = abcr;
        yield return abcr;
        abDic[abName] = abcr.assetBundle;
        if (isMainPage)
        {
            abManifest = abDic[abName].LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
    }

    /// <summary>
    /// �첽����AB���Լ�����������������
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    private void LoadABPageAsync(string pageName, UnityAction callback)
    {
        IEnumerator ie = Ie_LoadABPage(pageName, callback);
        ie.MoveNext();
        ieList.Add(ie);
    }

    bool isAnsycLoadMainPage;
    IEnumerator Ie_LoadABPage(string abName, UnityAction callback)
    {
        //��ȡ����
        if ((!abDic.ContainsKey(ABMainName) || abDic[ABMainName] == null) && !isAnsycLoadMainPage)
        {
            LoadABAsync(ABMainName, true);
            isAnsycLoadMainPage = true;
            yield return ABMainName;
        }
        else
        {
            yield return ABMainName;
        }

        string[] pagesName = abManifest.GetAllDependencies(abName);//�õ�����������������

        for (int i = 0; i < pagesName.Length; i++)
        {
            if (!abDic.ContainsKey(pagesName[i]))
            {
                LoadABAsync(pagesName[i]);
                yield return pagesName[i];
            }
            else
            {
                yield return pagesName[i];
            }
        }

        //����Ŀ���
        if (!abDic.ContainsKey(abName))
        {
            LoadABAsync(abName);
            yield return abName;
        }
        else
        {
            yield return abName;
        }

        callback?.Invoke();
    }


    /// <summary>
    /// ����첽�����Ƿ���ɣ������ɣ�IE_LoadABPageЭ�̼���ִ��
    /// </summary>
    private void DetectionLoadingCompleted()
    {
        for (int i = 0; i < ieList.Count; i++)
        {
            if (abDic.ContainsKey((string)ieList[i].Current)
                && abDic[(string)ieList[i].Current] != null
                || (string)ieList[i].Current == ABMainName && abDic.ContainsKey(ABMainName) && abDic[ABMainName] != null)
            {
                if (!ieList[i].MoveNext())
                {
                    ieList.Remove(ieList[i]);
                }
            }
        }
    }
    private void Update()
    {
        DetectionLoadingCompleted();

        ProcessValue = CueentProcess?.progress ?? 0;

        ResVale = CueentRes?.progress ?? 0;
    }

    /// <summary>
    /// �첽����
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="ResName"></param>
    /// <param name="CallBack"></param>
    public void LoadResAsync(string abName, string ResName, UnityAction<Object> CallBack)
    {
        LoadABPageAsync(abName, () =>
        {
            StartCoroutine(ResAsync(abName, ResName, CallBack));
        });
    }

    public void LoadResAsync(string abName, string ResName, System.Type type, UnityAction<Object> CallBack)
    {
        LoadABPageAsync(abName, () =>
        {
            StartCoroutine(ResAsync(abName, ResName, type, CallBack));
        });
    }

    public void LoadResAsync<T>(string abName, string ResName, UnityAction<T> CallBack) where T : Object
    {
        LoadABPageAsync(abName, () =>
        {
            StartCoroutine(ResAsync<T>(abName, ResName, CallBack));
        });
    }

    public void LoadSceneAsync(string abName, UnityAction<string> CallBack)
    {
        LoadABPageAsync(abName, () =>
        {
            StartCoroutine(ResAsync(abName, CallBack));
        });
    }



    private IEnumerator ResAsync(string abName, string ResName, UnityAction<Object> CallBack)
    {

        AssetBundleRequest abr = abDic[abName].LoadAssetAsync(ResName);
        CueentRes = abr;
        yield return abr;
        CallBack?.Invoke(abr.asset);
    }

    private IEnumerator ResAsync(string abName, string ResName, System.Type type, UnityAction<Object> CallBack)
    {

        AssetBundleRequest abr = abDic[abName].LoadAssetAsync(ResName, type);
        CueentRes = abr;
        yield return abr;
        CallBack?.Invoke(abr.asset);
    }

    private IEnumerator ResAsync<T>(string abName, string ResName, UnityAction<T> CallBack) where T : Object
    {

        AssetBundleRequest abr = abDic[abName].LoadAssetAsync<T>(ResName);
        CueentRes = abr;
        yield return abr;
        CallBack?.Invoke(abr.asset as T);
    }

    private IEnumerator ResAsync(string abName, UnityAction<string> CallBack)
    {

        string[] scenePaths = abDic[abName].GetAllScenePaths();
        yield return scenePaths;
        CallBack?.Invoke(scenePaths[0]);
    }

    #endregion
    #endregion


    #region ж��AssetBundle

    /// <summary>
    /// ������ж��
    /// </summary>
    /// <param name="ab"></param>
    /// <param name="WithLoad">�Ƿ������Ѽ�������</param>
    public void UnAB(string abName, bool WithLoad)
    {
        if (abDic.ContainsKey(abName))
        {
            abDic[abName].Unload(WithLoad);
            abDic.Remove(abName);
        }
    }

    /// <summary>
    /// ���а�ж��
    /// </summary>
    /// <param name="WithLoad">�Ƿ������Ѽ�������</param>
    public void UnAllAB(bool WithLoad)
    {
        AssetBundle.UnloadAllAssetBundles(WithLoad);
        abDic.Clear();
    }

    #endregion

    /// <summary>
    /// ���ؼ��أ�����Ҫ�����Ϳ��Լ��ء��������
    /// </summary>
    public void LocalLoad()
    {
#if UNITY_EDITOR
        var t = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName("mcx", "mcx");
        var gob = AssetDatabase.LoadAssetAtPath<GameObject>(t[0]);
        Instantiate(gob);
#endif 
    }

    #region ����AssetBundle


    /// <summary>
    /// ����������AssetBundle
    /// </summary>
    /// <param name="path"></param>
    /// <param name="filename"></param>
    /// <param name="callBack"></param>
    public void NetLoadAsset()
    {

        remoteABInfo.Clear();
        localABInfo.Clear();
        NeedLoadAB.Clear();

        ReadLoaclAsset();


        List<Task> task = new List<Task>();
        //���رȽ��ļ�
        StartCoroutine(LoadAssetBundle(ServerPath, CompareFile, () =>
        {
            //TODO-���Զ�˵������ļ��ͱ��ص������ļ�MD5����ͬ����Ϊû���κθ���


            //���ݱȽ��ļ�����Ab�ļ�
            string info = File.ReadAllText(LoadSavePath + CompareFile);
            string[] abs = info.Split("\n");
            string[] infos = null;
            foreach (var item in abs)
            {
                infos = item.Split("||");
                //Զ��AB����Ϣ
                remoteABInfo.Add(infos[0], new ABInfo(infos[0], infos[1], infos[2]));
                Debug.Log(infos[0]);
            }
            Debug.Log("�����ļ��������");


            foreach (var item in remoteABInfo.Keys)//����Զ��AB�ļ���Ϊ�ο�
            {
                if (!localABInfo.ContainsKey(item) || localABInfo[item] != remoteABInfo[item]) //����û���ļ����� �����ļ����������ͬ
                {
                    NeedLoadAB.Add(item);
                }
                else if (localABInfo[item] == remoteABInfo[item]) //��Դ��ͬ
                {
                    localABInfo.Remove(item);
                }
                else
                {
                    //  localABInfo ʣ�µĶ��Ǳ��ض�����Ϣ ѡ��ɾ��
                }
            }

            if (NeedLoadAB.Count == 0)
            {
                Debug.Log("��ǰ��ԴΪ���°汾��������");
            }
            else
            {
                for (int i = 0; i < NeedLoadAB.Count; i++)
                {
                    string loadname = NeedLoadAB[i];
                    Debug.Log($"��Ҫ���صİ���{loadname}�����ش�СΪ{ remoteABInfo[loadname].size / 1024f / 1024f} MB");
                    StartCoroutine(LoadAssetBundle(ServerPath, loadname, () =>
                    {
                        Debug.Log("Load Success");
                        NeedLoadAB.Remove(loadname);//���سɹ�֮���NeedLoadAB �Ƴ�
                    }));
                }
            }
            //ToDo �ļ�����ʧ�� �ٴ�NeedLoadAB����û������ɵĲ��� ����������ش���


        }));

        //LoadIISCompareFile(path, filename, callBack);

    }

    private void ReadLoaclAsset()
    {
        DirectoryInfo directory = Directory.CreateDirectory(LoadSavePath);
        //��Ŀ¼�µ������ļ���Ϣ
        FileInfo[] fileInfos = directory.GetFiles();
        foreach (var item in fileInfos)
        {
            if (item.Extension == "") //ab��û�к�׺
            {
                localABInfo.Add(item.Name, new ABInfo(item.Name, item.Length, GetMD5(item.FullName)));
            }
        }
    }

    private static string GetMD5(string filepath)
    {
        //�����ļ�·������ȡ�ļ�����Ϣ
        //���ļ�������ʽ��
        using (FileStream file = new FileStream(filepath, FileMode.Open))
        {
            //����һ��MD5���� ����MD5��
            MD5 mD5 = new MD5CryptoServiceProvider();
            var md5Info = mD5.ComputeHash(file);

            //�ر��ļ���
            file.Close();

            //��16�ֽ�ת����16����ƴ���ַ���������MD5�볤��
            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < md5Info.Length; i++)
            {
                sb.Append(md5Info[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }


    /// <summary>  Զ��AB��   </summary>
    private Dictionary<string, ABInfo> remoteABInfo = new Dictionary<string, ABInfo>();

    /// <summary>  ����AB��   </summary>
    private Dictionary<string, ABInfo> localABInfo = new Dictionary<string, ABInfo>();

    // <summary>  ��Ҫ���ص�AB��   </summary>
    private List<string> NeedLoadAB = new List<string>();


    private class ABInfo
    {
        public string name;
        public long size;
        public string md5;

        public ABInfo(string name, string size, string md5)
        {
            this.name = name;
            this.size = long.Parse(size);
            this.md5 = md5;
        }
        public ABInfo(string name, long size, string md5)
        {
            this.name = name;
            this.size = size;
            this.md5 = md5;
        }

        public static bool operator ==(ABInfo a, ABInfo b)
        {
            if (!(a is null) && !(b is null) && a.md5 == b.md5) return true;
            return false;
        }

        public static bool operator !=(ABInfo a, ABInfo b)
        {
            if (!(a is null) && !(b is null) && a.md5 == b.md5) return false;
            return true;
        }

    }

    /// <summary>
    /// ��IIS���������ضԱ��ļ�
    /// </summary>
    public void LoadIISCompareFile(string serverPath, string filenName, UnityAction callBack = null)
    {
        WebClient _webClient = new WebClient();
        //ʹ��Ĭ�ϵ�ƾ�ݡ�����ȡ��ʱ��ֻ��Ĭ��ƾ�ݾͿ���
        _webClient.Credentials = CredentialCache.DefaultCredentials;
        string uri = serverPath + filenName;
        //���ص����ӵ�ַ���ļ���������
        System.Uri _uri = new System.Uri(uri);
        //ע�����ؽ����¼�֪ͨ
        //_webClient.DownloadProgressChanged += _webClient_DownloadProgressChanged;
        //ע����������¼�֪ͨ
        _webClient.DownloadFileCompleted += (_, Y) =>
        {
            Debug.Log($"{filenName}||{Y.Error}");
            AssetDatabase.Refresh();
            callBack?.Invoke();
        };
        //�첽����
        _webClient.DownloadFileAsync(_uri, LoadSavePath + filenName);
    }

    /// <summary>
    /// ʹ��webRequest����
    /// </summary>
    /// <param name="Serverpath"></param>
    /// <param name="filename"></param>
    /// <param name="CallBack"></param>
    /// <returns></returns>
    IEnumerator LoadAssetBundle(string serverPath, string fileName, UnityAction callBack = null)
    {
        //�������ϵ��ļ�·��
        string uri = serverPath + fileName;

        using (var webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log("Download Error:" + webRequest.error);
            }
            else
            {
                //������ɺ�ִ�еĻص�
                if (webRequest.isDone)
                {
                    byte[] results = webRequest.downloadHandler.data;
                    if (!Directory.Exists(LoadSavePath))
                    {
                        Directory.CreateDirectory(LoadSavePath);
                    }
                    using (FileStream fs = File.Create(LoadSavePath + fileName))
                    {
                        fs.Write(results, 0, results.Length);
                        fs.Close();
                    }

                    //using (FileStream fs = File.Create(fileName))  //Task�̲߳��ܷ���applocation
                    //{
                    //    fs.Write(results, 0, results.Length);
                    //    fs.Close();
                    //}

                    //FileInfo fileInfo = new FileInfo(LoadSavePath + filename);
                    //FileStream fs = fileInfo.Create();
                    ////fs.Write(�ֽ�����, ��ʼλ��, ���ݳ���);
                    //fs.Write(results, 0, results.Length);
                    //fs.Flush(); //�ļ�д��洢��Ӳ��
                    //fs.Close(); //�ر��ļ�������
                    //fs.Dispose(); //�����ļ�����

                    callBack?.Invoke();
                }
            }
        }
    }

    #endregion

}
