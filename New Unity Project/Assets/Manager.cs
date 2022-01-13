using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public GameObject repoPrefab;
    public GameObject scrollRoot;

    //private fields
    private RectTransform contentRect;

    // Start is called before the first frame update
    void Start()
    {
        contentRect = scrollRoot.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddRepoItem(RepositoryData repData)
    {
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentRect.sizeDelta.y + 64f);
    }

    public static RepositoryData[] GetRepoData()
    {
        return new RepositoryData[0];
    }
}

[System.Serializable]
public class RepositoryData
{
    string repoName;
}
