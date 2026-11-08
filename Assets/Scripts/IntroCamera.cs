using System.Collections;                                // Needed for IEnumerator (coroutines)
using UnityEngine;

public class IntroCamera : MonoBehaviour
{
    [SerializeField] private float showTime = 2f;        // Seconds to show the overview

    // Start the intro countdown
    private void Start()
    {
        StartCoroutine(EndIntroAfterDelay());            // Run the coroutine
    }

    // Wait, then hand control to the player camera
    private IEnumerator EndIntroAfterDelay()
    {
        yield return new WaitForSeconds(showTime);       // Pause here without freezing the game
        gameObject.SetActive(false);                     // Brain blends to the next camera
    }
}