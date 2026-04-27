using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ReturnToRicochetLauncher : MonoBehaviour
{
    public string launcherExe = "RicochetLauncher.exe";
    public Button quit;

    public void ReturnToLauncher()
    {
        StartCoroutine(LaunchLauncherOrExit());
    }

    private IEnumerator LaunchLauncherOrExit()
    {
        quit.interactable = false;

        string gameRoot = Directory.GetParent(Application.dataPath).FullName;
        string collectionRoot = Directory.GetParent(gameRoot).FullName;
        string launcherPath = Path.Combine(collectionRoot, launcherExe);

        Process process = null;
        bool launchSucceeded = false;

        // Try to start launcher
        try
        {
            if (File.Exists(launcherPath))
            {
                process = Process.Start(launcherPath);
                launchSucceeded = process != null;
            }
        }
        catch
        {
            launchSucceeded = false;
        }

        // If launcher started successfully → wait briefly then close game
        if (launchSucceeded)
        {
            Application.runInBackground = true;
            float timer = 0f;

            while (process != null && process.HasExited && timer < 5f)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(2f);
            Application.Quit();
        }
        else
        {
            // ❌ Launcher missing or failed → just exit game safely
            Application.Quit();
        }
    }
}