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
using System;

public class Manager : MonoBehaviour
{
    public Canvas canvas;
    public GameObject repoPrefab;
    public GameObject scrollRoot;
    public GameObject starredScrollRoot;
    public GameObject retryPanel;
    public GameObject refreshUI;
    public RectTransform refreshUIRect;
    public Image refreshUIImage;
    public RectTransform refreshUIArrowRect;
    public Image refreshUIArrowImage;

    public GameObject optionsMenuRoot;
    public RectTransform optionsBtnRect;
    public TextMeshProUGUI headerText;
    public GameObject backButton;

    public List<GameObject> starredItems = new List<GameObject>();

    public RuntimeAnimatorController animControllerLoading;

    //private fields
    private RectTransform contentRect;
    private RectTransform starredContentRect;
    private byte[] texArray;
    private bool isLoaded = false;

    List<RepositoryData> repoDataList;
    private bool refreshInitiated = false;
    private Vector3 mousePrevPos;
    private Vector2 refreshStartPos;
    private bool isOptionsActive = false;
    private bool starredListActive = false;


    // Start is called before the first frame update
    void Start()
    {
        refreshUIRect = refreshUI.GetComponent<RectTransform>();
        refreshUIImage = refreshUI.GetComponent<Image>();
        refreshUIArrowRect = refreshUI.transform.GetChild(0).GetComponent<RectTransform>();
        refreshUIArrowImage = refreshUI.transform.GetChild(0).GetComponent<Image>();
        refreshStartPos = refreshUIRect.anchoredPosition;
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        canvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(Screen.currentResolution.height, Screen.currentResolution.width);

        contentRect = scrollRoot.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
        starredContentRect = starredScrollRoot.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();

        UpdateDataUI();

    }

    // Update is called once per frame
    void Update()
    {
        HandleRefresh();
    }

    public void BackBtn()
    {
        foreach (Transform child in starredContentRect.transform)
        {
            if (child.GetComponent<ItemFunctions>().isExpanded)
            {
                child.GetComponent<ItemFunctions>().OnClickExpandToggle();
            }
        }
        headerText.text = "Trending";
        starredListActive = false;
        starredScrollRoot.SetActive(false);
        scrollRoot.SetActive(true);

        backButton.SetActive(false);


    }

    public void ViewStarredRepos()
    {
        if (!starredListActive)
        {
            headerText.text = "Starred";
            starredListActive = true;
            starredScrollRoot.SetActive(true);
            scrollRoot.SetActive(false);

            optionsMenuRoot.SetActive(false);
            optionsBtnRect.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
            isOptionsActive = false;

            backButton.SetActive(true);
        }

    }

    public void ToggleOptionsMenu()
    {
        if (!isLoaded)
            return;

        if (!isOptionsActive)
        {
            optionsMenuRoot.SetActive(true);
            optionsBtnRect.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));
            isOptionsActive = true;
        }
        else
        {
            optionsMenuRoot.SetActive(false);
            optionsBtnRect.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
            isOptionsActive = false;
        }
    }

    public void HandleRefresh()
    {
        if (!isLoaded)
            return;

        if (Input.GetMouseButton(0))
        {
            //float mouseXNormalized = Input.mousePosition.x / Screen.width;
            //float mouseYNormalized = Input.mousePosition.y / Screen.height;

            if (contentRect.anchoredPosition.y < 1063f && contentRect.anchoredPosition.y > 900f && !refreshInitiated)
            {
                refreshInitiated = true;
                mousePrevPos = Input.mousePosition;
                refreshUI.SetActive(true);
            }
            else if (refreshInitiated)
            {
                Vector2 deltaPos = Input.mousePosition - mousePrevPos;
                refreshUIRect.anchoredPosition = new Vector2(refreshUIRect.anchoredPosition.x, refreshUIRect.anchoredPosition.y + deltaPos.y * 0.6f);
                mousePrevPos = Input.mousePosition;

                refreshUIImage.color = new Color(refreshUIImage.color.r, refreshUIImage.color.g, refreshUIImage.color.b, 1f - (GetPercent(contentRect.anchoredPosition.y, 800f, 1063f)) / 100f);
                refreshUIArrowImage.color = new Color(refreshUIArrowImage.color.r, refreshUIArrowImage.color.g, refreshUIArrowImage.color.b, 1f - (GetPercent(contentRect.anchoredPosition.y, 800f, 1063f)) / 100f);

                refreshUIArrowRect.rotation = Quaternion.Euler(new Vector3(0f, 0f, (1f - ((GetPercent(contentRect.anchoredPosition.y, 800f, 1063f)) / 100f))) * -180f);

                if (contentRect.anchoredPosition.y <= 800f)
                    StartRefresh();
            }

        }
        else
        {
            refreshUIRect.anchoredPosition = refreshStartPos;
            refreshUIImage.color = new Color(refreshUIImage.color.r, refreshUIImage.color.g, refreshUIImage.color.b, 0f);
            refreshInitiated = false;
            refreshUI.SetActive(false);

        }

    }

    public void StartRefresh()
    {
        isLoaded = false;
        retryPanel.SetActive(false);
        refreshUIRect.anchoredPosition = refreshStartPos;
        refreshUIImage.color = new Color(refreshUIImage.color.r, refreshUIImage.color.g, refreshUIImage.color.b, 0f);
        refreshInitiated = false;
        refreshUI.SetActive(false);
        //scrollRoot.GetComponent<ScrollRect>().vertical = false;
        scrollRoot.GetComponent<CanvasGroup>().blocksRaycasts = false;

        foreach (Transform child in contentRect.transform)
        {
            //child.GetComponent<Button>().interactable = false;
            child.GetComponent<ItemFunctions>().enabled = false;
            ColorBlock block = child.GetComponent<Button>().colors;
            block.selectedColor = Color.white;
            block.highlightedColor = Color.white;
            child.GetComponent<Button>().colors = block;
            child.GetComponent<Animator>().runtimeAnimatorController = animControllerLoading;
            child.GetComponent<Animator>().Play("Loading");
            child.GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            child.GetChild(2).GetComponent<TextMeshProUGUI>().text = "";
            child.GetChild(1).GetChild(0).gameObject.SetActive(true);
            child.GetChild(2).GetChild(0).gameObject.SetActive(true);
            child.GetChild(0).GetChild(0).GetComponent<Image>().sprite = default;
            child.GetChild(0).GetChild(0).GetComponent<Image>().color = Color.grey;
            child.GetChild(3).gameObject.SetActive(false);
            child.GetChild(4).gameObject.SetActive(false);
            child.GetChild(5).gameObject.SetActive(false);
            child.GetComponent<RectTransform>().anchoredPosition = new Vector2(540f, child.GetComponent<RectTransform>().anchoredPosition.y);

            //child.GetComponent<Animator>().SetBool("isRefreshing", true);
        }

        UpdateDataUI();

    }

    public void DisplayErrorPanel()
    {
        retryPanel.SetActive(true);
    }

    public void AddRepoItem(RepositoryData repData, bool isStarredItem = false)
    {
        GameObject go;
        if (!isStarredItem)
            go = GameObject.Instantiate(repoPrefab, contentRect.transform);
        else
        {
            go = GameObject.Instantiate(repoPrefab, starredContentRect.transform);
            go.transform.GetChild(5).gameObject.SetActive(false);
            starredItems.Add(go);
        }
        //StartCoroutine(this.GetTextureRequest(repData.builtBy[0].url));
        //Task<Texture2D> task = Task.Run(() => GetRemoteTexture(repData.builtBy[0].url));
        //task.Start();
        StartCoroutine(GetText(repData.builtBy[0].avatar, go.transform.GetSiblingIndex(), isStarredItem));
        //icon.LoadImage(texArray);
        go.GetComponent<ItemFunctions>().repoData = repData;
        go.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = repData.username;
        go.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = repData.repositoryName;
        go.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = repData.description;
        go.transform.GetChild(4).GetChild(1).GetComponent<TextMeshProUGUI>().text = repData.language;
        go.transform.GetChild(4).GetChild(3).GetComponent<TextMeshProUGUI>().text = repData.totalStars;
        go.transform.GetChild(4).GetChild(5).GetComponent<TextMeshProUGUI>().text = repData.forks;
        go.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
        go.transform.GetChild(2).GetChild(0).gameObject.SetActive(false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(520f, -100f - (go.transform.GetSiblingIndex() * 220f));
        rect.sizeDelta = new Vector2(1040f, 200f);

        if (!isStarredItem)
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentRect.sizeDelta.y + 220f);
        else
            starredContentRect.sizeDelta = new Vector2(starredContentRect.sizeDelta.x, starredContentRect.sizeDelta.y + 220f);
    }

    public void RemoveRepoItem(GameObject item)
    {
        starredContentRect.sizeDelta = new Vector2(starredContentRect.sizeDelta.x, starredContentRect.sizeDelta.y - 220f);
        starredItems.Remove(item);

        foreach (Transform child in starredContentRect)
        {
            if (child.GetSiblingIndex() > item.transform.GetSiblingIndex())
            {
                child.GetComponent<ItemFunctions>().targetRectHeight += 220f;
            }
        }
        Destroy(item);


    }

    //public void LoadImages(RepositoryData[] repositoryDatas)
    //{
    //    foreach (RepositoryData data in repositoryDatas)
    //    {
    //        Texture2D tex = GetRemoteTexture(data.builtBy[0].url);
    //    }
    //}

    public async void UpdateDataUI()
    {
        try
        {
            Task<List<RepositoryData>> task = new Task<List<RepositoryData>>(GetRepoData);
            task.Start();

            Debug.Log("Loading data");
            repoDataList = await task;
            Debug.Log("Data Loaded");

            foreach (Transform child in contentRect.transform)
            {
                Destroy(child.gameObject);
            }
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, 0f);
            //scrollRoot.GetComponent<ScrollRect>().vertical = true;

            StartCoroutine(AddRepoItemsAfterDelay());


        }
        catch
        {
            DisplayErrorPanel();

        }
    }

    IEnumerator AddRepoItemsAfterDelay()
    {
        yield return new WaitForSeconds(0f);
        foreach (RepositoryData data in repoDataList)
        {
            AddRepoItem(data);
        }

        isLoaded = true;
        scrollRoot.GetComponent<ScrollRect>().vertical = true;
        scrollRoot.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    //public Texture2D GetRemoteTexture(string url)
    //{
    //    Texture2D textureOnline = new Texture2D(128, 128);
    //    using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
    //    {
    //        www.SendWebRequest();

    //        if (www.isNetworkError || www.isHttpError)
    //        {
    //            print(www.error);
    //            print("error while parsing url" + www.error);
    //        }

    //        else
    //        {
    //            if (www.isDone)
    //            {
    //                textureOnline = DownloadHandlerTexture.GetContent(www);
    //                print("getTexture done");
    //                //textureOnline.filterMode = FilterMode.Point;
    //                //texArray = textureOnline.EncodeToPNG();
    //            }
    //        }
    //    }
    //    return textureOnline;
    //}

    IEnumerator GetText(string url, int siblingIndex, bool isStarredItem = false)
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
                GameObject go;
                if (!isStarredItem)
                    go = scrollRoot.transform.GetChild(0).GetChild(0).GetChild(siblingIndex).gameObject;
                else
                    go = starredScrollRoot.transform.GetChild(0).GetChild(0).GetChild(siblingIndex).gameObject;
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

    public static float GetPercent(float input, float min, float max)
    {
        return ((input - min) * 100) / (max - min);
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
