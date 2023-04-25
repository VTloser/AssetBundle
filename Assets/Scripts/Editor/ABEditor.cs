using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System;
using System.Threading.Tasks;

public class ABEditor
{
    public static string path = Application.streamingAssetsPath + "/AssetBundle/";


    [MenuItem("AB������/�����Ա��ļ�")]
    public static void CreateABCompareFile()
    {
        //�ļ�����Ϣ��
        DirectoryInfo directory = Directory.CreateDirectory(path);
        //��Ŀ¼�µ������ļ���Ϣ
        FileInfo[] fileInfos = directory.GetFiles();

        //��ab���ļ�������С��MD5�������Դ�Ա��ļ�
        string abCompareInfo = null;
        foreach (var item in fileInfos)
        {
            if (item.Extension == "") //ab��û�к�׺
            {
                abCompareInfo += item.Name + "||" + item.Length + "||" + GetMD5(item.FullName);
                abCompareInfo += "\n";
            }
        }
        abCompareInfo = abCompareInfo.TrimEnd('\n');
        Debug.Log(abCompareInfo.ToString());
        //�洢AB���Ա��ļ�
        File.WriteAllText(path + "ABCompareInfo.txt", abCompareInfo.ToString());
        //�༭��ˢ��
        AssetDatabase.Refresh();
        Debug.Log("AB���Ա��ļ����ɳɹ�");
    }

    [MenuItem("AB������/�ϴ�AB�ļ��ͶԱ��ļ�")]
    public static void UpLoadALLAB()
    {
        //�ļ�����Ϣ��
        DirectoryInfo directory = Directory.CreateDirectory(path);
        //��Ŀ¼�µ������ļ���Ϣ
        FileInfo[] fileInfos = directory.GetFiles();

        foreach (var item in fileInfos)
        {
            if (item.Extension == ".txt" ||
                item.Extension == ".txt") //ab��û�к�׺
            {
                //�ϴ�
                UpLoad(item.FullName, item.Name);
            }
        }
    }

    private static void UpLoad(string filePath, string fileName)
    {
        //FTPUpLoad(filePath, fileName);
        IISIpLoad(filePath, fileName);
    }

    /// <summary>
    /// FTP�ϴ�
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="fileName"></param>
    private async static void FTPUpLoad(string filePath, string fileName)
    {
        await Task.Run(() =>
        {
            try
            {
                //1.����FTP���� �����ϴ�
                FtpWebRequest req = FtpWebRequest.Create(new Uri("ftp://127.0.0.1/AB/PC" + fileName)) as FtpWebRequest;
                //2. ����һ��ͨ��ƾ֤  ���������ϴ�
                NetworkCredential nc = new NetworkCredential("usetname", "password");
                req.Credentials = nc;
                //3.��������
                //���ô���Ϊ��
                req.Proxy = null;
                //������Ϻ��Ƿ�رտ�������
                req.KeepAlive = false;
                //����-�ϴ�
                req.Method = WebRequestMethods.Ftp.UploadFile;
                //ָ���������� 2��ֹ
                req.UseBinary = true;
                //4.�ϴ��ļ�
                //ftp���ļ�
                Stream uploadSteam = req.GetRequestStream();
                //��ȡ�ļ���Ϣ д��������
                using (FileStream file = File.OpenRead(filePath))
                {
                    //һ��һ����ϴ�����
                    byte[] bytes = new byte[2048];
                    //����ֵ�����ȡ�˶��ٸ��ֽ�
                    int contentLength = file.Read(bytes, 0, bytes.Length);
                    //ѭ���ϴ�
                    while (contentLength != 0)
                    {
                        //д�뵽�ϴ�����
                        uploadSteam.Write(bytes, 0, contentLength);
                        //д���ٶ�
                        contentLength = file.Read(bytes, 0, bytes.Length);
                    }

                    //ѭ����Ϻ� �ϴ�����
                    file.Close();
                    uploadSteam.Close();
                }
                Debug.Log($"{fileName}�ϴ��ɹ�");
            }
            catch (Exception e)
            {
                Debug.Log($"{fileName}�ϴ�ʧ�ܣ�[{ e }]");
            }
        });
    }


    private static void IISIpLoad(string filePath, string fileName)
    {
        Debug.Log(fileName);
        try
        {
            //����_webClient����
            WebClient _webClient = new WebClient();
            //ʹ��Windows��¼��ʽ
            _webClient.Credentials = new NetworkCredential("IUSR", "");
            //�ϴ������ӵ�ַ���ļ���������
            Uri _uri = new Uri(@$"http://192.168.10.104:5000/AssetBundle/");
            //�ϴ��ɹ��¼�ע��
            _webClient.UploadFileCompleted += (_, Y) => 
            {
                Debug.Log($"{fileName}||{Y.Result}");
            };
            //�첽��D���ϴ��ļ���������
            _webClient.UploadFileAsync(_uri, "PUT", filePath);
            Console.ReadKey();
        }
        catch (Exception e)
        {
            Debug.Log($"{fileName}�ϴ�ʧ�ܣ�[{ e }]");
        }

    }


    /// <summary>
    /// ��ȡ�ļ�MD5��
    /// </summary>
    /// <param name="filepath"></param>
    /// <returns></returns>
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
}
