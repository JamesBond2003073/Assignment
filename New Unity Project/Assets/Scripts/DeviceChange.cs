using System;
using System.Collections;
using UnityEngine;

public class DeviceChange : MonoBehaviour
{
    public static event Action<Vector2> OnResolutionChange;
    public static event Action<ScreenOrientation> OnOrientationChange;
    public static float CheckDelay = 0.5f;        // How long to wait until we check again.

    static Vector2 resolution;                    // Current Resolution
    static ScreenOrientation orientation;        // Current Device Orientation
    static bool isAlive = true;                    // Keep this script running?

    void Start()
    {
        StartCoroutine(CheckForChange());
    }

    IEnumerator CheckForChange()
    {
        resolution = new Vector2(Screen.width, Screen.height);
        orientation = Screen.orientation;

        while (isAlive)
        {

            // Check for a Resolution Change
            if (resolution.x != Screen.width || resolution.y != Screen.height)
            {
                resolution = new Vector2(Screen.width, Screen.height);
                if (OnResolutionChange != null) OnResolutionChange(resolution);
            }

            // Check for an Orientation Change
            switch (Screen.orientation)
            {
                case ScreenOrientation.Unknown:            // Ignore
                case ScreenOrientation.AutoRotation:            // Ignore
                                                                // Ignore
                    break;
                default:
                    if (orientation != Screen.orientation)
                    {
                        orientation = Screen.orientation;
                        if (OnOrientationChange != null) OnOrientationChange(orientation);
                    }
                    break;
            }

            yield return new WaitForSeconds(CheckDelay);
        }
    }

    void OnDestroy()
    {
        isAlive = false;
    }

}
