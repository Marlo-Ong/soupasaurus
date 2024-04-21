using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpeningCutsceneDino : MonoBehaviour
{
    public float speed = 120;
    //GameObject dino;
    //SpriteRenderer dino = GameObject.Find("dino").GetComponent<SpriteRenderer>();
    //GameObject dialogue = GameObject.Find("Image");
    //SpriteRenderer dialogue = GameObject.Find("image").GetComponent<SpriteRenderer>();
    GameObject dialogue1;
    GameObject dialogue2;

    // Start is called before the first frame update
    void Start()
    {
        dialogue1 = GameObject.Find("Dialogue1");
        dialogue2 = GameObject.Find("Dialogue2");
        dialogue2.SetActive(false);
        //GameObject dino = GameObject.Find("dino");
        StartCoroutine(something());
    }

    IEnumerator something()
    {
        //yield return new WaitForSeconds(1.0f);
        yield return new WaitForSeconds(3.0f);
        dialogue1.SetActive(false);

        flip();
        for(int i = 0; i < 5; i++) {
            moveBackward();
            yield return new WaitForSeconds(1.0f);
        }

        yield return new WaitForSeconds(1.0f);
        dialogue2.SetActive(true);
        yield return new WaitForSeconds(3.0f);
        dialogue2.SetActive(false);

        flip();
        for(int i = 0; i < 10; i++) {
            moveForward();
            speed = 200;
            yield return new WaitForSeconds(1.0f);
        }

        yield return new WaitForSeconds(1.0f);
        
    }

    IEnumerator waiting() {
        yield return new WaitForSeconds(1.0f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // function for moving script forward a step
    void moveForward() {
        // Calculate the distance to move based on speed and time
        float distanceToMove = speed * Time.deltaTime;

        // Move the sprite forward (in the direction of its local forward vector)
        transform.Translate(Vector2.right * distanceToMove);
    }

    void moveBackward() {
        // Calculate the distance to move based on speed and time
        float distanceToMove = speed * Time.deltaTime;

        // Move the sprite forward (in the direction of its local forward vector)
        transform.Translate(Vector2.left * distanceToMove);
    }

    void flip() {
        //dino.transform.localScale = new Vector3(-dino.transform.localScale.x, dino.transform.localScale.y, dino.transform.localScale.z);
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }
}
