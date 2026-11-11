using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour
{
    public GameObject winTextObj;
    public bool win = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log("偵測到進入：" + win);
    }
    void OnTriggerEnter(Collider other)
    {
    //    print("collision");
        // Debug.Log("偵測到進入：" + other.name);
        if(other.tag == "robot" && !win){
        //Animator animator = other.GetComponent<Animator>();
        //targetLight. = true; // 開燈
            win = true;
            Debug.Log("Win!!!");
            if (winTextObj != null)
            {
                winTextObj.SetActive(true);
            }
        }
       
    }
    void OnTriggerExit(Collider other)
    {
    //    print("collision");
    //    print(other.collider.tag);
        if(other.tag == "robot"){
            //Animator animator = other.GetComponent<Animator>();
            //targetLight. = true; // 開燈
            
            if (winTextObj != null)
            {
                winTextObj.SetActive(false);
            }
        }
       
    }
}
