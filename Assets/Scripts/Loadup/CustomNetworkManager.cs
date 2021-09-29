using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;

public class CustomNetworkManager : NetworkManager
{
    [Header("Audio")]
    public remixReplayEventRecord remixReplay;

    [Header("Xammp")]
    private bool first_server_start = false;
    private int xampp_pid = -1;
    public GameObject ovrAvatar, leftViveAvatar, rightViveAvatar;

    [Header("Cam Holder")]
    public GameObject cam_holder;

    public wwiseOptions soundOptions;

    private void replace_dir(string old_pwd, string new_pwd, string file_dir)
    {
        string whole_text = "";
        string line;
        // Read the file and display it line by line.  

        System.IO.StreamReader file = new System.IO.StreamReader(@new_pwd + @file_dir);
        
        while ((line = file.ReadLine()) != null)
        {
            if (line != "")
            {
                
                string corrected_npwd = new_pwd.Replace(@"\", @"/");
                string corrected_line = line.Replace(old_pwd, corrected_npwd);

                whole_text = whole_text + corrected_line + "\r\n";
            }
            else
            {
                whole_text = whole_text + "\r\n";
            }
        }

        file.Close();

        if (File.Exists(@new_pwd + @file_dir + "_OLD"))
            File.Delete(@new_pwd + @file_dir + "_OLD");

        System.IO.File.Move(@new_pwd + @file_dir, @new_pwd + @file_dir + "_OLD");

        File.WriteAllText(@new_pwd + @file_dir, whole_text);
    }
    /// <summary>
    /// FD: Boots up the database and shuts off offline controller avatars
    /// </summary>
    public override void OnStartServer()
    {
        ovrAvatar.SetActive(false);
        leftViveAvatar.SetActive(false);
        rightViveAvatar.SetActive(false);

        Config configt = Config.Instance;
        string serverStatus = configt.Data.serverStatus;

        if (serverStatus == "server") cam_holder.GetComponent<SetupManager>().setAudioAndCamFromConfig();

        if (!first_server_start)
        {
            

            // Get the current directory.
            string pwd = Directory.GetCurrentDirectory();

            Config config = Config.Instance;

            if (config.Data.initial_start)
            {
                Directory.SetCurrentDirectory(@pwd + @"\xampp");

                string setup_dir = @pwd + @"\xampp\setup_xampp.bat";



                Process sp = Process.Start(setup_dir);

                sp.WaitForExit();

                Directory.SetCurrentDirectory(@pwd);

                config.Data.initial_start = false;
                config.WriteToJson();
            }


            if (config.Data.xampp_dir != pwd.Replace(@"/", @"\"))
            {
                UnityEngine.Debug.Log("Updating XAMPP directories.");

                replace_dir(config.Data.xampp_dir, pwd, @"\xampp\apache\conf\httpd.conf");
                replace_dir(config.Data.xampp_dir, pwd, @"\xampp\apache\conf\extra\httpd-ssl.conf");
                replace_dir(config.Data.xampp_dir, pwd, @"\xampp\apache\conf\extra\httpd-xampp.conf");
                replace_dir(config.Data.xampp_dir, pwd, @"\xampp\mysql\bin\my.ini");
                replace_dir(config.Data.xampp_dir, pwd, @"\xampp\php\php.ini");

                config.Data.xampp_dir = pwd.Replace(@"\", @"/");
                config.WriteToJson();
            }


            //Build file path for xampp.
            string app_dir = @pwd + @"\xampp\xampp-control.exe";

            UnityEngine.Debug.Log("Attempting to open: " + app_dir);

            Process xampp_p = Process.Start(app_dir);

            xampp_pid = xampp_p.Id;

            first_server_start = true;


            print("starting remix replay");

            if (config.Data.remixReplay)
            {
                remixReplay = FindObjectOfType<remixReplayEventRecord>();

                remixReplay.startRemixReplay();
            }




        }

        Config config2 = Config.Instance;
        if (config2.Data.serverStatus == "server")
        {
            soundOptions = GameObject.FindObjectOfType<wwiseOptions>();
            soundOptions.setAudioToSpeakers();
        }

    }
    /// <summary>
    /// FD: Set's the offline player models to off once the experience starts
    /// </summary>
    public override void OnStartClient()
    {
        ovrAvatar.SetActive(false);
        leftViveAvatar.SetActive(false);
        rightViveAvatar.SetActive(false);
        soundOptions = GameObject.FindObjectOfType<wwiseOptions>();
        soundOptions.setAudioToHeadphones();
    }
    /// <summary>
    /// FD: Shut's xampp off when the program is shutdown
    /// </summary>
    public override void OnApplicationQuit()
    {
        if(xampp_pid != -1)
        {
            Process xampp_p = Process.GetProcessById(xampp_pid);
            xampp_p.Kill();
        }
    }
    /// <summary>
    /// FD: returns the avatar functionality for the appropriate controller on expereince stop
    /// </summary>
    public override void OnStopClient() 
    {
        if (OVRPlugin.GetSystemHeadsetType() != 0) ovrAvatar.SetActive(true);
        else
        {
            leftViveAvatar.SetActive(true);
            rightViveAvatar.SetActive(true);
        }
    }
    /// <summary>
    /// FD: returns the avatar functionality for the appropriate controller on expereince stop
    /// </summary>
    public override void OnStopServer()
    {
        //remixReplay.uploadServer.Kill();
        if (OVRPlugin.GetSystemHeadsetType() != 0) ovrAvatar.SetActive(true);
        else
        {
            leftViveAvatar.SetActive(true);
            rightViveAvatar.SetActive(true);
        }
    }
}
