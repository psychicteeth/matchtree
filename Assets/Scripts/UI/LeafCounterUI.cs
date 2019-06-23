using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeafCounterUI : MonoBehaviour
{
    // set in editor
    public GameObject circleIndicator;
    public LeafData leafData;
    public PlayerState playerState;
    public Transform upperContainer;
    public Transform lowerContainer;

    // stash circle indicator classes
    List<CircleIndicator> circleUIs = new List<CircleIndicator>();

    void Start()
    {
        // create a number of circle indicators
        for (int i = 0; i < leafData.leaves.Count; i++)
        {
            GameObject go = GameObject.Instantiate(circleIndicator);
            if (i < 5)
            {
                go.transform.SetParent(upperContainer);
            }
            else
            {
                go.transform.SetParent(lowerContainer);
            }
            go.transform.Find("Leaf").gameObject.GetComponent<Image>().sprite = leafData.leaves[i];
            CircleIndicator ci = go.GetComponent<CircleIndicator>();
            Debug.Assert(ci != null);
            circleUIs.Add(ci);
        }
    }

    void Update()
    {
        // obtain count values from player state and update ui
        for (int i = 0; i < leafData.leaves.Count; i++)
        {
            int leafCount = playerState.GetLeafCount(i);
            circleUIs[i].value = leafCount;
        }
    }
}
