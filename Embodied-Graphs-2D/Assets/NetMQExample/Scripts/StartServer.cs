using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

public class StartServer : MonoBehaviour
{

    public void ExecuteCommand(string command)
    {
        string Path = Application.dataPath;
        var thread = new Thread(delegate () { Run_Command(Path, command); });
        thread.Start();
    }

    static void Run_Command(string Path, string command)
    {
        var processInfo = new ProcessStartInfo("python.exe", Path + "\\NetMQExample\\Scripts\\Graphserver.py --"+ command);
        //processInfo.Arguments = "--hypergraph_to_graph";
        processInfo.CreateNoWindow = true;
        processInfo.UseShellExecute = false;

        var process = Process.Start(processInfo);

        process.WaitForExit();
        //UnityEngine.Debug.Log("server_closed");
        process.Close();
    }

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
