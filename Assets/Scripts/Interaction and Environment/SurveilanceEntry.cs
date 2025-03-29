using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Video;

public class SurveilanceEntry : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoPlayer videoPlayer;
    public Canvas worldSpaceCanvas;
    public bool playOnce = true;
    public bool loopVideo = false;

    private bool hasPlayed = false;
    public CinemachineCamera surveilanceCam;
    public MeshCollider colliderToNextDoor;
    private void Awake()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("No VideoPlayer assigned to SurveilanceEntry!");
            return;
        }

        // Configure video player
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = loopVideo;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player
        if (other.CompareTag("Player"))
        {
            if (!playOnce || (playOnce && !hasPlayed))
            {
                surveilanceCam.gameObject.SetActive(true);
                PlayVideo();
            }
        }
    }

    private void PlayVideo()
    {
        if (videoPlayer != null && worldSpaceCanvas != null)
        {
            worldSpaceCanvas.enabled = true;
            videoPlayer.Play();
            hasPlayed = true;
            StartCoroutine(DisableColliderAfterFewSeconds());
        }
    }

    IEnumerator DisableColliderAfterFewSeconds() {
        yield return new WaitForSeconds(35f);
        surveilanceCam.gameObject.SetActive(false);
        colliderToNextDoor.enabled = false;
    }
}
