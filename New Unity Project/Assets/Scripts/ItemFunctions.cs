using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Totality.GameTemplate;

public class ItemFunctions : MonoBehaviour
{

    public Transform content;
    public RectTransform contentRect;

    public Animator anim;
    public RepositoryData repoData;

    public bool isExpanded = false;
    public RectTransform rect;
    public RectTransform imageRootRect;
    public RectTransform userNameRect;
    public RectTransform repoNameRect;
    public RectTransform starRect;
    public RectTransform buttonRect;
    public Vector2 refVel1 = Vector2.zero;
    public Vector2 refVel2 = Vector2.zero;
    public Vector2 refVel3 = Vector2.zero;
    private float smoothTime = 3f;

    private float minRectY;
    private float targetRectY;
    private float targetBtnY;
    public float targetRectHeight;
    private float targetImageRootRectHeight;
    private float targetRepoNameTop;
    private float targetStarY;
    private bool defSet = false;

    public bool isStarred = false;
    public GameObject starredItem;

    public RepositoryDataEvent addStarItemEvent;
    public GameObjectEvent removeStarItemEvent;
    public IntEvent shiftDownEvent;
    public IntEvent shiftUpEvent;

    // Start is called before the first frame update
    void Start()
    {

        content = transform.parent;
        contentRect = content.GetComponent<RectTransform>();

        rect = this.GetComponent<RectTransform>();
        imageRootRect = this.transform.GetChild(0).GetComponent<RectTransform>();
        anim = this.GetComponent<Animator>();

        if (transform.gameObject.CompareTag("Header"))
            return;

        buttonRect = this.transform.GetChild(0).GetComponent<RectTransform>();
        userNameRect = this.transform.GetChild(1).GetChild(1).GetComponent<RectTransform>();
        repoNameRect = this.transform.GetChild(1).GetChild(2).GetComponent<RectTransform>();
        starRect = this.transform.GetChild(1).GetChild(5).GetComponent<RectTransform>();




    }

    //Method to add repository item to starred list that can be accessed in a separate screen
    public void StarItem()
    {
        if (!isStarred)
        {
            this.transform.GetChild(1).GetChild(5).GetChild(0).GetChild(0).gameObject.SetActive(true);

            //raise addStarItemEvent to add item to starred list
            addStarItemEvent.Raise(repoData);
            isStarred = true;
            starredItem = Util.starredItems[Util.starredItems.Count - 1];
        }
        else
        {
            this.transform.GetChild(1).GetChild(5).GetChild(0).GetChild(0).gameObject.SetActive(false);

            ////raise removeStarItemEvent to remove item from starred list
            removeStarItemEvent.Raise(starredItem);
            isStarred = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Initialize default values for position targets
        if (!(anim.GetCurrentAnimatorStateInfo(0).length > anim.GetCurrentAnimatorStateInfo(0).normalizedTime))
        {
            if (!defSet)
            {
                defSet = true;

                targetRectHeight = rect.anchoredPosition.y;

                if (transform.gameObject.CompareTag("Header"))
                    return;

                targetRectY = rect.sizeDelta.y;
                targetImageRootRectHeight = imageRootRect.anchoredPosition.y;
                targetRepoNameTop = repoNameRect.offsetMax.y;
                targetStarY = starRect.anchoredPosition.y;
            }

            //Control position of item every frame
            LerpToPos();
        }
    }

    //Manage expand and collapse for item 
    public void OnClickExpandToggle()
    {
        if ((anim.GetCurrentAnimatorStateInfo(0).length > anim.GetCurrentAnimatorStateInfo(0).normalizedTime))
            return;

        if (!isExpanded)
        {
            minRectY = rect.sizeDelta.y;
            targetRectY = 400f;
            targetBtnY = -100f;
            //targetRectHeight -= 220f;
            targetImageRootRectHeight += 100f;
            targetRepoNameTop += 200f;
            targetStarY += 100f;

            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentRect.sizeDelta.y + 226f);

            this.transform.GetChild(1).GetChild(3).gameObject.SetActive(true);
            this.transform.GetChild(1).GetChild(4).gameObject.SetActive(true);

            //Raise shiftDown event to adjust position of other items below the target
            shiftDownEvent.Raise(transform.GetSiblingIndex());
        }
        else
        {
            minRectY = rect.sizeDelta.y;
            targetRectY = 200f;
            targetBtnY = 0f;
            //targetRectHeight += 220f;
            targetImageRootRectHeight -= 100f;
            targetRepoNameTop -= 200f;
            targetStarY -= 100f;

            contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentRect.sizeDelta.y - 226f);

            this.transform.GetChild(1).GetChild(3).gameObject.SetActive(false);
            this.transform.GetChild(1).GetChild(4).gameObject.SetActive(false);

            //Raise shiftUp event to adjust position of other items below the target
            shiftUpEvent.Raise(transform.GetSiblingIndex());
        }

        isExpanded = !isExpanded;
    }

    //Control position of item every frame
    public void LerpToPos()
    {
        rect.anchoredPosition = Vector2.SmoothDamp(rect.anchoredPosition, new Vector2(rect.anchoredPosition.x, targetRectHeight), ref refVel2, smoothTime * Time.deltaTime);
        if (this.gameObject.CompareTag("Header"))
            return;
        buttonRect.sizeDelta = Vector2.SmoothDamp(buttonRect.sizeDelta, new Vector2(buttonRect.sizeDelta.x, targetRectY), ref refVel1, smoothTime * Time.deltaTime);
        buttonRect.anchoredPosition = Vector2.SmoothDamp(buttonRect.anchoredPosition, new Vector2(buttonRect.anchoredPosition.x, targetBtnY), ref refVel3, smoothTime * Time.deltaTime);
        //imageRootRect.anchoredPosition = Vector2.SmoothDamp(imageRootRect.anchoredPosition, new Vector2(imageRootRect.anchoredPosition.x, targetImageRootRectHeight), ref refVel3, smoothTime * Time.deltaTime);
        //repoNameRect.offsetMax = new Vector2(repoNameRect.offsetMax.x, Util.GetPercent(rect.sizeDelta.y, minRectY, targetRectY) * (targetRepoNameTop / 100));
        //starRect.anchoredPosition = new Vector2(starRect.anchoredPosition.x, Util.GetPercent(rect.sizeDelta.y, minRectY, targetRectY) * (targetStarY / 100));
    }

    //Method subscribed to shiftDownEvent
    public void ShiftDown(int siblingIndex)
    {
        if (siblingIndex < this.transform.GetSiblingIndex())
            targetRectHeight -= 210f;

    }

    //Method subscribed to shiftUpEvent
    public void ShiftUp(int siblingIndex)
    {
        if (siblingIndex < this.transform.GetSiblingIndex())
            targetRectHeight += 210f;
    }


}
