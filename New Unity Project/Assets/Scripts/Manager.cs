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
using UnityEngine.EventSystems;

public class Manager : MonoBehaviour
{
    public Canvas canvas;
    public GameObject repoPrefab;
    public GameObject headerPrefab;
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

    public int itemCount = 0;
    public int headerCount = 0;

    public Dictionary<string, List<RepositoryData>> repoByLanguage = new Dictionary<string, List<RepositoryData>>();

    public RuntimeAnimatorController animControllerLoading;

    private Animator optionsAnim;

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
    private bool isRefreshOngoing = false;


    // Start is called before the first frame update
    void Start()
    {
        //Initialize important fields

        //DeviceChange.OnOrientationChange += HandleOrientationChange;

        optionsAnim = optionsMenuRoot.GetComponent<Animator>();
        refreshUIRect = refreshUI.GetComponent<RectTransform>();
        refreshUIImage = refreshUI.GetComponent<Image>();
        refreshUIArrowRect = refreshUI.transform.GetChild(0).GetComponent<RectTransform>();
        refreshUIArrowImage = refreshUI.transform.GetChild(0).GetComponent<Image>();
        refreshStartPos = refreshUIRect.anchoredPosition;
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        canvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(Screen.currentResolution.height, Screen.currentResolution.width);

        contentRect = scrollRoot.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
        starredContentRect = starredScrollRoot.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();

        //Call for fetching data and initialing UI
        UpdateDataUI();

    }

    // Update is called once per frame
    void Update()
    {
        //Handle refresh UI
        HandleRefresh();

        //Handle options menu UI
        HandleOptionsMenu();
    }

    //Method subscribed to addStarEvent to add a new starred item to list
    public void AddStarItem(RepositoryData data)
    {
        AddRepoItem(data, true);
    }

    //Method subscribed to addStarEvent to remove a starred item from list
    public void RemoveStarItem(GameObject starredItem)
    {
        RemoveRepoItem(starredItem);
    }

    //Button to go back to trending repositories screen from starred repositories screen
    public void BackBtn()
    {
        //Set state for all starred items to collapsed
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

    //Switch to starred repos screen
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

    //Method to handle UI on screen orientation changes
    public void HandleOrientationChange(ScreenOrientation orientation)
    {

        Debug.Log("Orientation changed to- " + orientation.ToString());

        switch (orientation)
        {
            case ScreenOrientation.LandscapeLeft:
            case ScreenOrientation.LandscapeRight:
                contentRect.sizeDelta += new Vector2(0f, 50 * contentRect.transform.childCount);
                starredContentRect.sizeDelta += new Vector2(0f, 50 * starredContentRect.transform.childCount);
                break;

            case ScreenOrientation.Portrait:
            case ScreenOrientation.PortraitUpsideDown:
                contentRect.sizeDelta -= new Vector2(0f, 50 * contentRect.transform.childCount);
                starredContentRect.sizeDelta -= new Vector2(0f, 50 * starredContentRect.transform.childCount);
                break;

        }
    }

    //Handle options menu UI
    public void HandleOptionsMenu()
    {
        if (Input.GetMouseButton(0))
        {
            GameObject go = EventSystem.current.currentSelectedGameObject;

            if (isOptionsActive)
            {
                if (go == null)
                {
                    isOptionsActive = false;
                    optionsMenuRoot.GetComponent<Animator>().Play("fadeOut");
                    return;
                }

                if (go.name != "OptionsMenuRoot" && go.name != "StarredRepos" && go.name != "Exit")
                {
                    isOptionsActive = false;
                    optionsMenuRoot.GetComponent<Animator>().Play("fadeOut");
                    return;
                }
            }
        }

        if (!(optionsAnim.GetCurrentAnimatorStateInfo(0).length > optionsAnim.GetCurrentAnimatorStateInfo(0).normalizedTime) && !isOptionsActive)
            optionsMenuRoot.SetActive(false);
    }

    //Button method to activate options menu
    public void ToggleOptionsMenu()
    {
        if (!isLoaded)
            return;

        if (!isOptionsActive)
        {
            optionsMenuRoot.SetActive(true);
            isOptionsActive = true;
        }

    }

    //Handle refresh UI using user input (pull-down) for refresh
    public void HandleRefresh()
    {
        if (!isLoaded)
            if (!isRefreshOngoing)
            { return; }
            else
            {
                Debug.Log("Rotating");
                refreshUIArrowRect.rotation = Quaternion.Euler(0f, 0f, refreshUIArrowRect.transform.eulerAngles.z - 500f * Time.deltaTime);
                return;
            }


        if (Input.GetMouseButton(0))
        {
            //Check for condition to start refresh pull-down
            if (contentRect.anchoredPosition.y < 1063f && contentRect.anchoredPosition.y > 900f && !refreshInitiated)
            {
                refreshInitiated = true;
                mousePrevPos = Input.mousePosition;
                refreshUI.SetActive(true);
            }
            else if (refreshInitiated)
            {
                //handle pull-down and initiate refresh when threshold is reached

                Vector2 deltaPos = Input.mousePosition - mousePrevPos;
                refreshUIRect.anchoredPosition = new Vector2(refreshUIRect.anchoredPosition.x, refreshUIRect.anchoredPosition.y + deltaPos.y * 0.6f);
                mousePrevPos = Input.mousePosition;

                refreshUIImage.color = new Color(refreshUIImage.color.r, refreshUIImage.color.g, refreshUIImage.color.b, 1f - (Util.GetPercent(contentRect.anchoredPosition.y, 800f, 1063f)) / 100f);
                refreshUIArrowImage.color = new Color(refreshUIArrowImage.color.r, refreshUIArrowImage.color.g, refreshUIArrowImage.color.b, 1f - (Util.GetPercent(contentRect.anchoredPosition.y, 800f, 1063f)) / 100f);

                refreshUIArrowRect.rotation = Quaternion.Euler(new Vector3(0f, 0f, (1f - ((Util.GetPercent(contentRect.anchoredPosition.y, 800f, 1063f)) / 100f))) * -180f);

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

    //Initiate refresh action
    public void StartRefresh()
    {
        isRefreshOngoing = true;

        //clear existing data
        repoByLanguage.Clear();
        itemCount = 0;
        headerCount = 0;

        isLoaded = false;
        retryPanel.SetActive(false);
        refreshInitiated = false;
        scrollRoot.GetComponent<CanvasGroup>().blocksRaycasts = false;

        foreach (Transform child in contentRect.transform)
        {
            //Set items to loading state

            child.GetComponent<ItemFunctions>().enabled = false;

            if (!child.gameObject.CompareTag("Header"))
            {
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
            }

        }

        //Fetch and load data from remote and initialize UI again to complete refresh
        UpdateDataUI();

    }

    //Display error panel when data fetch fails
    public void DisplayErrorPanel()
    {
        retryPanel.SetActive(true);
    }

    //Add trending or starred repository item in scroll list
    public void AddRepoItem(RepositoryData repData, bool isStarredItem = false)
    {
        //Instantiate repo item
        GameObject go;
        if (!isStarredItem)
            go = GameObject.Instantiate(repoPrefab, contentRect.transform);
        else
        {
            go = GameObject.Instantiate(repoPrefab, starredContentRect.transform);
            go.transform.GetChild(5).gameObject.SetActive(false);
            Util.starredItems.Add(go);
        }

        //Assign image texture 
        StartCoroutine(GetText(repData.builtBy[0].avatar, go.transform.GetSiblingIndex(), isStarredItem));

        //Set data 
        go.GetComponent<ItemFunctions>().repoData = repData;
        go.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = repData.username;
        go.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = repData.repositoryName;
        go.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = repData.description;
        go.transform.GetChild(4).GetChild(1).GetComponent<TextMeshProUGUI>().text = repData.language;
        go.transform.GetChild(4).GetChild(3).GetComponent<TextMeshProUGUI>().text = repData.totalStars;
        go.transform.GetChild(4).GetChild(5).GetComponent<TextMeshProUGUI>().text = repData.forks;

        //Disable loading image
        go.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
        go.transform.GetChild(2).GetChild(0).gameObject.SetActive(false);

        //Handling item position in scroll list.
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        if (!isStarredItem)
            rect.anchoredPosition = new Vector2(520f, -100f - (itemCount * 220f) - (headerCount * 180f));
        else
            rect.anchoredPosition = new Vector2(520f, -100f - (go.transform.GetSiblingIndex() * 220f));
        rect.sizeDelta = new Vector2(1040f, 200f);

        //Increase contentRect height to accomodate new item
        if (!isStarredItem)
        {
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentRect.sizeDelta.y + 220f);
            itemCount++;
        }
        else
            starredContentRect.sizeDelta = new Vector2(starredContentRect.sizeDelta.x, starredContentRect.sizeDelta.y + 220f);

    }

    //Remove item from starred repository list
    public void RemoveRepoItem(GameObject item)
    {
        starredContentRect.sizeDelta = new Vector2(starredContentRect.sizeDelta.x, starredContentRect.sizeDelta.y - 220f);
        Util.starredItems.Remove(item);

        foreach (Transform child in starredContentRect)
        {
            if (child.GetSiblingIndex() > item.transform.GetSiblingIndex())
            {
                child.GetComponent<ItemFunctions>().targetRectHeight += 220f;
            }
        }
        Destroy(item);


    }

    ////Fetch data and initialize UI
    public async void UpdateDataUI()
    {
        try
        {
            // start new task to get data through web request
            Task<List<RepositoryData>> task = new Task<List<RepositoryData>>(GetRepoData);
            task.Start();

            Debug.Log("Loading data");

            //wait for response before executing further
            repoDataList = await task;
            Debug.Log("Data Loaded");


            repoByLanguage.Add("Unknown", new List<RepositoryData>());

            //initializing UI after receiving data

            foreach (Transform child in contentRect.transform)
            {
                Destroy(child.gameObject);
            }
            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, 0f);

            StartCoroutine(AddRepoItemsAfterDelay());


        }
        catch
        {
            DisplayErrorPanel();

        }
    }

    //Add Header Item 
    public void AddHeader(string language, string languageColor)
    {
        GameObject go;

        go = GameObject.Instantiate(headerPrefab, contentRect.transform);
        go.tag = "Header";

        go.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = language;
        Debug.Log("Language Color- " + languageColor);
        Color color;
        ColorUtility.TryParseHtmlString(languageColor, out color);
        go.GetComponent<Image>().color = color;

        if (language == "Unknown")
            go.GetComponent<Image>().color = Color.gray;

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(520f, -100f - (itemCount * 220f) - (headerCount * 180f));
        rect.sizeDelta = new Vector2(1100f, 160f);

        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentRect.sizeDelta.y + 180f);
        headerCount++;

    }

    //Add repository UI item in scroll list for every data in dataset
    IEnumerator AddRepoItemsAfterDelay()
    {
        yield return new WaitForSeconds(0f);

        //Categorize repos based on language
        foreach (RepositoryData data in repoDataList)
        {
            if (data.language != null)
                if (repoByLanguage.ContainsKey(data.language))
                    repoByLanguage[data.language].Add(data);
                else
                    repoByLanguage.Add(data.language, new List<RepositoryData> { data });
            else
                repoByLanguage["Unknown"].Add(data);

        }

        //Load the UI items with headers and proper grouping of repository items based on language
        foreach (KeyValuePair<string, List<RepositoryData>> pair in repoByLanguage)
        {
            AddHeader(pair.Key, pair.Value[0].languageColor);

            foreach (RepositoryData data in pair.Value)
            {
                AddRepoItem(data);
            }
        }

        //Checking for screen orientation to apply position modifications
        if (Screen.orientation == ScreenOrientation.LandscapeLeft || Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            Debug.Log("Landscape Detected");
            //contentRect.sizeDelta += new Vector2(0f, 50 * contentRect.transform.childCount);
            //starredContentRect.sizeDelta += new Vector2(0f, 50 * starredContentRect.transform.childCount);
        }

        foreach (Transform child in starredContentRect)
        {
            Destroy(child.gameObject);
        }

        isLoaded = true;
        isRefreshOngoing = false;
        refreshUIRect.anchoredPosition = refreshStartPos;
        refreshUIImage.color = new Color(refreshUIImage.color.r, refreshUIImage.color.g, refreshUIImage.color.b, 0f);
        refreshUI.SetActive(false);
        scrollRoot.GetComponent<ScrollRect>().vertical = true;
        scrollRoot.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    //Coroutine to fetch texture from url using webrequest
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

    //Method to fetch data from API using webrequest
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




