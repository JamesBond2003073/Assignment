using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    public GameObject repoPrefab;
    public GameObject scrollRoot;
    public GameObject retryPanel;

    //private fields
    private RectTransform contentRect;
    private byte[] texArray;

    // Start is called before the first frame update
    void Start()
    {

        contentRect = scrollRoot.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
        UpdateDataUI();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DisplayErrorPanel()
    {
        retryPanel.SetActive(true);
    }

    public async void AddRepoItem(RepositoryData repData)
    {
        GameObject go = GameObject.Instantiate(repoPrefab, contentRect.transform);
        //StartCoroutine(this.GetTextureRequest(repData.builtBy[0].url));
        //Task<Texture2D> task = Task.Run(() => GetRemoteTexture(repData.builtBy[0].url));
        //task.Start();
        StartCoroutine(GetText(repData.builtBy[0].avatar, go.transform.GetSiblingIndex()));
        //icon.LoadImage(texArray);
        go.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = repData.username;
        go.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = repData.repositoryName;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentRect.sizeDelta.y + 226f);
    }

    public void LoadImages(RepositoryData[] repositoryDatas)
    {
        foreach (RepositoryData data in repositoryDatas)
        {
            Texture2D tex = GetRemoteTexture(data.builtBy[0].url);
        }
    }

    public async void UpdateDataUI()
    {
        Task<List<RepositoryData>> task = new Task<List<RepositoryData>>(GetRepoData);
        task.Start();

        Debug.Log("Loading data");
        List<RepositoryData> repoDataList = await task;
        Debug.Log("Data Loaded");

        foreach (RepositoryData data in repoDataList)
        {
            AddRepoItem(data);
        }

    }

    public Texture2D GetRemoteTexture(string url)
    {
        Texture2D textureOnline = new Texture2D(128, 128);
        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                print(www.error);
                print("error while parsing url" + www.error);
            }

            else
            {
                if (www.isDone)
                {
                    textureOnline = DownloadHandlerTexture.GetContent(www);
                    print("getTexture done");
                    //textureOnline.filterMode = FilterMode.Point;
                    //texArray = textureOnline.EncodeToPNG();
                }
            }
        }
        return textureOnline;
    }

    IEnumerator GetText(string url, int siblingIndex)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                var texture = DownloadHandlerTexture.GetContent(uwr);
                GameObject go = scrollRoot.transform.GetChild(0).GetChild(0).GetChild(siblingIndex).gameObject;
                Debug.Log(go.name);
                go.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = Color.white;
                go.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
            }
        }
    }

    public List<RepositoryData> GetRepoData()
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://gh-trending-api.herokuapp.com/repositories");
        HttpWebResponse response;
        response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());
        string json = reader.ReadToEnd();
        return JsonConvert.DeserializeObject<List<RepositoryData>>(json);
    }

}

[System.Serializable]
public class RepositoryData
{
    public string rank;
    public string username;
    public string repositoryName;
    public string url;
    public string description;
    public string language;
    public string languageColor;
    public string totalStars;
    public string forks;
    public string StarsSince;
    public string since;
    public List<BuiltBy> builtBy;

}

public class BuiltBy
{
    public string username;
    public string url;
    public string avatar;
}

public class Joke
{
    public string value;
}
