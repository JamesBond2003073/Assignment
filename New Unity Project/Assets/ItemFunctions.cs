using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemFunctions : MonoBehaviour
{
    public Transform content;
    public RectTransform contentRect;

    public Animator anim;

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

    private float minRectHeight;
    private float targetRectY;
    private float targetRectHeight;
    private float targetImageRootRectHeight;
    private float targetRepoNameTop;
    private bool defSet = false;

    // Start is called before the first frame update
    void Start()
    {
        content = transform.parent;
        contentRect = content.GetComponent<RectTransform>();
        rect = this.GetComponent<RectTransform>();
        imageRootRect = this.transform.GetChild(0).GetComponent<RectTransform>();
        userNameRect = this.transform.GetChild(1).GetComponent<RectTransform>();
        repoNameRect = this.transform.GetChild(2).GetComponent<RectTransform>();

        anim = this.GetComponent<Animator>();


    }

    // Update is called once per frame
    void Update()
    {
        if (!(anim.GetCurrentAnimatorStateInfo(0).length > anim.GetCurrentAnimatorStateInfo(0).normalizedTime))
        {
            if (!defSet)
            {
                defSet = true;

                targetRectY = rect.sizeDelta.y;
                targetRectHeight = rect.anchoredPosition.y;
                targetImageRootRectHeight = imageRootRect.anchoredPosition.y;
                targetRepoNameTop = repoNameRect.offsetMax.y;
            }

            LerpToPos();
        }
    }

    public void OnClickExpandToggle()
    {
        if ((anim.GetCurrentAnimatorStateInfo(0).length > anim.GetCurrentAnimatorStateInfo(0).normalizedTime))
            return;

        if (!isExpanded)
        {
            minRectHeight = rect.anchoredPosition.y;
            targetRectY = 400f;
            targetRectHeight -= 120f;
            targetImageRootRectHeight += 100f;
            targetRepoNameTop += 200f;

            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentRect.sizeDelta.y + 226f);

            this.transform.GetChild(3).gameObject.SetActive(true);
            this.transform.GetChild(4).gameObject.SetActive(true);

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
            minRectHeight = rect.anchoredPosition.y;
            targetRectY = 200f;
            targetRectHeight += 120f;
            targetImageRootRectHeight -= 100f;
            targetRepoNameTop -= 200f;

            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentRect.sizeDelta.y - 226f);

            this.transform.GetChild(3).gameObject.SetActive(false);
            this.transform.GetChild(4).gameObject.SetActive(false);

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
        repoNameRect.offsetMax = new Vector2(repoNameRect.offsetMax.x, Manager.GetPercent(rect.anchoredPosition.y, minRectHeight, targetRectHeight) * (targetRepoNameTop / 100));
    }

    public void ShiftDown(int siblingIndex)
    {
        if (siblingIndex > this.transform.GetSiblingIndex())
            return;


    }


}
