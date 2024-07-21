using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpeningCutsceneDino : MonoBehaviour
{
    public float speed = 500;
    public float STEP_TIME = 0.75f;
    GameObject dialogue1;
    GameObject dialogue2;
    private bool isSceneEnding;

    // Start is called before the first frame update
    void Start()
    {
        dialogue1 = GameObject.Find("Dialogue1");
        dialogue2 = GameObject.Find("Dialogue2");
        dialogue2.SetActive(false);
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
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    public void EndScene()
    {
        if (isSceneEnding)
            return;
        
        isSceneEnding = true;
        AudioManager.Instance.IsCutscenePlaying = false;
        AsyncOperation job = SceneManager.LoadSceneAsync(2);
    }
}
