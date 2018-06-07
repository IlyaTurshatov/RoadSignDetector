using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LargeImageScript : EventTrigger
{
    public Image content;
    public override void OnPointerClick(PointerEventData data)
    {
        GetComponent<Image>().enabled = false;
        content.enabled = false;
    }

    private void Start()
    {
        content = transform.GetChild(0).gameObject.GetComponent<Image>();
    }
}
