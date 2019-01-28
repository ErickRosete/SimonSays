using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.CrossPlatformInput;

public class SimonSays : MonoBehaviour
{
    [SerializeField] int currentLevel;
    int life;

    bool inInstructions;
    bool inPlayerTurn;
    bool inTransition;
    bool inQuestion;
    bool acceptedSimpleBook;

    Queue<int> instructionsSequence;
    [SerializeField] Text currentText;

    [SerializeField] GameObject up;
    [SerializeField] GameObject left;
    [SerializeField] GameObject right;
    [SerializeField] GameObject down;
    [SerializeField] GameObject button_a;
    [SerializeField] GameObject button_b;

    [SerializeField] GameObject indicator;

    GameObject heartsContainer;
    GameObject btnsContainer;
    GameObject progressIndicator;

    void Start()
    {
        life = 3;
        inInstructions = false;
        inPlayerTurn = false;
        inTransition = false;
        inQuestion = false;
        acceptedSimpleBook = false;

        currentText.text = "";
        instructionsSequence = new Queue<int>();

        btnsContainer = GameObject.Find("ButtonsContainer");
        progressIndicator = GameObject.Find("ProgressIndicator");
        heartsContainer = GameObject.Find("HeartsContainer");
    }
    void OnGui()
    {

        var e = Event.current;

        if (e != null && e.isKey && Input.anyKeyDown && e.keyCode.ToString() != "None")
        {
            onKeyDown(e.keyCode);
        }
    }
    void Update()
    {
        if (life > 0)
        {
            if (currentLevel <= 5 && !acceptedSimpleBook)
            {
                if (!inInstructions && !inPlayerTurn && !inTransition && !inQuestion)
                    StartCoroutine("Instructions");

                if (inPlayerTurn)
                    playerTurn();

                if (inQuestion)
                    Question();
            }
            else
            {
                currentText.text = "CONGRATULATIONS!";
            }
        }
        else
        {
            currentText.text = "GAME OVER";
        }

    }

    IEnumerator Instructions()
    {
        inInstructions = true;
        if (currentLevel < 5)
            currentText.text = "LEVEL " + currentLevel;
        else
            currentText.text = "FINAL LEVEL";
        yield return new WaitForSeconds(2f);
        currentText.text = "REMEMBER THE FOLLOWING PATTERN";
        yield return new WaitForSeconds(2f);
        currentText.text = "";

        for (int i = 0; i < currentLevel + 3; i++)
        {
            int direction;
            if (currentLevel < 3)
                direction = Random.Range(0, 2);
            else if (currentLevel < 4)
            {
                direction = Random.Range(0, 3);
                //At least 1 arrow up
                if (i == 5 && !instructionsSequence.Contains(2))
                    direction = 2;
            }
            else if (currentLevel < 5)
                direction = Random.Range(0, 4);
            else
                direction = Random.Range(0, 6);

            instructionsSequence.Enqueue(direction);

            GameObject btn = button_a;
            switch (direction)
            {
                case 0:
                    btn = button_a;
                    break;
                case 1:
                    btn = button_b;
                    break;
                case 2:
                    btn = up;
                    break;
                case 3:
                    btn = left;
                    break;
                case 4:
                    btn = right;
                    break;
                case 5:
                    btn = down;
                    break;
            }

            Instantiate(btn,
                new Vector2(btnsContainer.transform.position.x + 1.2f * i,
                btnsContainer.transform.position.y),
                Quaternion.identity, btnsContainer.transform);

            Instantiate(indicator,
                new Vector2(progressIndicator.transform.position.x + 0.75f * i,
                progressIndicator.transform.position.y),
                Quaternion.identity, progressIndicator.transform);

            yield return new WaitForSeconds(1f);
        }
        yield return new WaitForSeconds(2f);

        //Destroy Instructions objects
        foreach (Transform child in btnsContainer.transform)
        {
            Destroy(child.gameObject);
        }
        currentText.text = "START";
        inInstructions = false;
        inPlayerTurn = true;
    }

    void playerTurn()
    {
        if (instructionsSequence.Count == 0)
        {
            inPlayerTurn = false;
            StartCoroutine("nextLevelMessage");
        }
        else
        {
            bool btna = CrossPlatformInputManager.GetButtonDown("A");
            bool btnb = CrossPlatformInputManager.GetButtonDown("B");
            bool up = CrossPlatformInputManager.GetButtonDown("Up");
            bool right = CrossPlatformInputManager.GetButtonDown("Right");
            bool left = CrossPlatformInputManager.GetButtonDown("Left");
            bool down = CrossPlatformInputManager.GetButtonDown("Down");

            if (btna || btnb || up || right || left || down)
            {
                int value = 0;
                if (btna)
                    value = 0;
                if (btnb)
                    value = 1;
                if (up)
                    value = 2;
                if (left)
                    value = 3;
                if (right)
                    value = 4;
                if (down)
                    value = 5;

                int currentStep = currentLevel + 3 - instructionsSequence.Count;
                var indicator = progressIndicator.transform.GetChild(currentStep);
                if (instructionsSequence.Dequeue() == value)
                {
                    indicator.GetComponent<Renderer>().material.color = Color.green;
                }
                else
                {
                    instructionsSequence.Clear();
                    indicator.GetComponent<Renderer>().material.color = Color.red;
                    Destroy(heartsContainer.transform.GetChild(life - 1).gameObject);
                    life--;
                    inPlayerTurn = false;
                    StartCoroutine("replayLevelMessage");
                }
            }
        }
    }

    IEnumerator replayLevelMessage()
    {
        inTransition = true;
        currentText.text = "BETTER LUCK NEXT TIME";
        yield return new WaitForSeconds(2f);

        foreach (Transform child in progressIndicator.transform)
        {
            Destroy(child.gameObject);
        }

        inTransition = false;
    }

    IEnumerator nextLevelMessage()
    {
        inTransition = true;
        currentText.text = "GREAT JOB";
        yield return new WaitForSeconds(2f);

        foreach (Transform child in progressIndicator.transform)
        {
            Destroy(child.gameObject);
        }

        progressIndicator.transform.position = new Vector2(
            progressIndicator.transform.position.x - 0.375f,
            progressIndicator.transform.position.y);

        currentLevel++;
        inTransition = false;

        if (currentLevel == 4)
            inQuestion = true;
    }

    void Question()
    {
        currentText.text = @"You won a book, but there is a BIGGER PRIZE.
Do you want to continue and win a greater prize?
Press A to accept the challenge.
Press B to keep the book.";

        if (CrossPlatformInputManager.GetButtonDown("A"))
        {
            inQuestion = false;
        }
        if (CrossPlatformInputManager.GetButtonDown("B"))
        {
            acceptedSimpleBook = true;
            inQuestion = false;
        }
    }

    void onKeyDown(KeyCode incomingKey)
    {
        var incomingKeyString = incomingKey.ToString();
        print("You pressed: " + incomingKeyString);
    }
}
