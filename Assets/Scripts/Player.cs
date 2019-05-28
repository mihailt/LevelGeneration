using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 8;
	float xInput = 0, yInput = 0;
    Rigidbody2D rb;

	void Start () {
        rb = GetComponent<Rigidbody2D>();        
	}
    void Update()
    {
        GetInput();
        Movement();
    }
	void GetInput(){
		xInput = Input.GetAxis("Horizontal"); 
		yInput = Input.GetAxis("Vertical");
	}

	void Movement(){
		Vector2 tempPos = transform.position;
		tempPos += new Vector2(xInput,yInput) * speed * Time.deltaTime;
        rb.MovePosition(tempPos);
	}    
}
