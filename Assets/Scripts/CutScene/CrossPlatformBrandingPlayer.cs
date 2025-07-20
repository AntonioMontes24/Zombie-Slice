using UnityEngine;
using UnityEngine.Video;

public class CrossPlatformBrandingPlayer : MonoBehaviour
{
    public string videoFileName = "MythicSyndicateIntro2.mp4"; // File must be in StreamingAssets

    void Start()
    {
        var videoPlayer = GetComponent<VideoPlayer>();

#if UNITY_WEBGL
        // Use URL for WebGL
        string videoURL = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
        videoPlayer.url = videoURL;
#else
        // Optional: use clip for Standalone if you prefer
        // videoPlayer.clip = someVideoClip;
        string videoURL = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
        videoPlayer.url = videoURL;
#endif

        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += vp => vp.Play();
    }
}
