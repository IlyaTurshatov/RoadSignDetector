using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OnImageClick : EventTrigger
{
    GameObject largeImage;

    public override void OnPointerClick(PointerEventData data)
    {

        largeImage.GetComponent<Image>().enabled = true;
        var content = largeImage.GetComponent<LargeImageScript>().content;
        content.overrideSprite = GetComponent<Image>().overrideSprite;
        content.enabled = true;
    }

    public void Start()
    {
        largeImage = GameObject.Find("LargeImage");
    }
}
