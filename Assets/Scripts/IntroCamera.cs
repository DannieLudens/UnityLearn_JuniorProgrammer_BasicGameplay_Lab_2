using System.Collections;                                // Needed for IEnumerator (coroutines)
using Unity.Cinemachine;                                 // Needed for CinemachineCamera and Brain
using UnityEngine;

public class IntroCamera : MonoBehaviour
{
    [SerializeField] private float showTime = 2f;        // Seconds to show the overview
    [SerializeField] private PlayerController player;    // Player to lock during the intro
    [SerializeField] private CinemachineBrain brain;     // Brain on Main Camera (to know when blend ends)

    private CinemachineCamera _overviewCam;              // This overview camera
    
    // Start the intro countdown
    private void Start()
    {
        _overviewCam = GetComponent<CinemachineCamera>(); // Cache our own camera
        StartCoroutine(PlayIntro());                     // Run the sequence
    }

    // Lock player, show overview, blend to player camera, unlock player
    private IEnumerator PlayIntro()
    {
        player.SetControlEnabled(false);                 // No input during the intro
        yield return new WaitForSeconds(showTime);       // Show the overview

        _overviewCam.enabled = false;                    // Brain blends to the next camera
        yield return null;                               // Wait exactly one frame so the blend starts
        yield return new WaitWhile(() => brain.IsBlending); // Wait until the blend finishes

        player.SetControlEnabled(true);                  // Give control back
    }
}