using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpeningCutsceneDino : MonoBehaviour
{
    public float speed = 500;
    public float STEP_TIME = 0.75f;
    //GameObject dino;
    //SpriteRenderer dino = GameObject.Find("dino").GetComponent<SpriteRenderer>();
    //GameObject dialogue = GameObject.Find("Image");
    //SpriteRenderer dialogue = GameObject.Find("image").GetComponent<SpriteRenderer>();
    GameObject dialogue1;
    GameObject dialogue2;
    private Coroutine EndingScene;

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
        yield return new WaitForSeconds(3 * STEP_TIME);
        dialogue1.SetActive(false);

        flip();
        for(int i = 0; i < 6; i++) {
            moveBackward();
            yield return new WaitForSeconds(STEP_TIME);
        }

        yield return new WaitForSeconds(STEP_TIME);
        dialogue2.SetActive(true);
        yield return new WaitForSeconds(3 * STEP_TIME);
        dialogue2.SetActive(false);

        flip();
        speed = 82;
        for(int i = 0; i < 10; i++) {
            moveForward();
            yield return new WaitForSeconds(STEP_TIME);
        }

        yield return new WaitForSeconds(STEP_TIME);
        EndScene();
    }

    IEnumerator waiting() {
        yield return new WaitForSeconds(STEP_TIME);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // function for moving script forward a step
    void moveForward() {
        // Calculate the distance to move based on speed and time
        //float distanceToMove = (float) speed;
        float distanceToMove = speed * Time.deltaTime;

        // Move the sprite forward (in the direction of its local forward vector)
        transform.Translate(Vector2.right * distanceToMove);
    }

    void moveBackward() {
        // Calculate the distance to move based on speed and time
        float distanceToMove = speed * Time.deltaTime;
        //float distanceToMove = (float) speed;

        // Move the sprite forward (in the direction of its local forward vector)
        transform.Translate(Vector2.left * distanceToMove);
    }

    void flip() {
        //dino.transform.localScale = new Vector3(-dino.transform.localScale.x, dino.transform.localScale.y, dino.transform.localScale.z);
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    public void EndScene()
    {
        if (EndingScene == null) StartCoroutine(ContinueEndScene());
    }

    private IEnumerator ContinueEndScene()
    {
        AsyncOperation job = SceneManager.LoadSceneAsync(2);
        while (!job.isDone)
        {
            yield return null;
        }
        EndingScene = null;
        StateMachine.Instance.StateChange(State.Questing);
    }
}
