using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PLaye_Controller : MonoBehaviour
{
    cam_Controller cam_Controller;
    private void Awake()
    {
        cam_Controller = Camera.main.GetComponent<cam_Controller>();
    }



    [SerializeField] float movespeed = 5f;



    private void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        var moveinput = (new Vector3(h,0,v)).normalized;

        var movedir = cam_Controller.PlanarRotation * moveinput;


        transform.position += movedir * movespeed * Time.deltaTime;

    }





}