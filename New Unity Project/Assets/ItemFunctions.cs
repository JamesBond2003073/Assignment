using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemFunctions : MonoBehaviour
{
    public Transform content;
    public RectTransform contentRect;

    public bool isExpanded = false;
    public RectTransform rect;
    public RectTransform imageRootRect;
    public RectTransform userNameRect;
    public RectTransform repoNameRect;
    public Vector2 refVel1 = Vector2.zero;
    public Vector2 refVel2 = Vector2.zero;
    public Vector2 refVel3 = Vector2.zero;
    public Vector2 refVel4 = Vector2.zero;
    public Vector2 refVel5 = Vector2.zero;
    public Vector2 refVel6 = Vector2.zero;
    private float smoothTime = 3f;

    public float targetRectY;
    public float targetRectHeight;
    private float targetImageRootRectHeight;
    private float targetRepoNameTop;

    // Start is called before the first frame update
    void Start()
    {
        content = transform.parent;
        contentRect = content.GetComponent<RectTransform>();
        rect = this.GetComponent<RectTransform>();
        imageRootRect = this.transform.GetChild(0).GetComponent<RectTransform>();
        userNameRect = this.transform.GetChild(1).GetComponent<RectTransform>();
        repoNameRect = this.transform.GetChild(2).GetComponent<RectTransform>();

        targetRectY = rect.sizeDelta.y;
        targetRectHeight = rect.anchoredPosition.y;
        targetImageRootRectHeight = imageRootRect.anchoredPosition.y;
        targetRepoNameTop = repoNameRect.offsetMax.y;
    }

    // Update is called once per frame
    void Update()
    {
        LerpToPos();
    }

    public void OnClickExpandToggle()
    {
        if (!isExpanded)
        {
            targetRectY = 400f;
            targetRectHeight -= 120f;
            targetImageRootRectHeight += 100f;
            targetRepoNameTop += 200f;

            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentRect.sizeDelta.y + 226f);

            foreach (Transform child in content)
            {
                if (child.GetSiblingIndex() > transform.GetSiblingIndex())
                {
                    child.GetComponent<ItemFunctions>().targetRectHeight -= 220f;
                }
            }
        }
        else
        {
            targetRectY = 200f;
            targetRectHeight += 120f;
            targetImageRootRectHeight -= 100f;
            targetRepoNameTop -= 200f;

            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentRect.sizeDelta.y - 226f);

            foreach (Transform child in content)
            {
                if (child.GetSiblingIndex() > transform.GetSiblingIndex())
                {
                    child.GetComponent<ItemFunctions>().targetRectHeight += 220f;
                }
            }
        }

        isExpanded = !isExpanded;
    }

    public void LerpToPos()
    {
        rect.sizeDelta = Vector2.SmoothDamp(rect.sizeDelta, new Vector2(rect.sizeDelta.x, targetRectY), ref refVel1, smoothTime * Time.deltaTime);
        rect.anchoredPosition = Vector2.SmoothDamp(rect.anchoredPosition, new Vector2(rect.anchoredPosition.x, targetRectHeight), ref refVel2, smoothTime * Time.deltaTime);
        imageRootRect.anchoredPosition = Vector2.SmoothDamp(imageRootRect.anchoredPosition, new Vector2(imageRootRect.anchoredPosition.x, targetImageRootRectHeight), ref refVel3, smoothTime * Time.deltaTime);
        repoNameRect.offsetMax = new Vector2(repoNameRect.offsetMax.x, targetRepoNameTop);
    }

    public void ShiftDown(int siblingIndex)
    {
        if (siblingIndex > this.transform.GetSiblingIndex())
            return;


    }


}
