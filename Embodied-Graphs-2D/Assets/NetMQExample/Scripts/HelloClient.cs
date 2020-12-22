using UnityEngine;

public class HelloClient : MonoBehaviour
{
    private HelloRequester _helloRequester;

    private void Start()
    {
        
    }

    private void OnDestroy()
    {
        //_helloRequester.Stop();
    }

    public bool Abstraction_conversion(string graph_as_string, string conversion_type)
    {
        if (_helloRequester != null)
        {
            //bool flag = true;
            // housekeeping so that an already running thread does not throw netmq exception 
            if(_helloRequester.isalive())
            {
                return false;
            }
            //Debug.Log("checking is alive: " + _helloRequester.isalive().ToString() + " flag: " + flag.ToString());
        }

        transform.GetComponent<StartServer>().ExecuteCommand(conversion_type);
        _helloRequester = new HelloRequester();
        _helloRequester.graph_as_str = graph_as_string; //"{8,9,7,10}-{8,9}{9,7}{8,7}";
        _helloRequester.command = conversion_type;
        _helloRequester.Start();
        return true;
    }

    void Update()
    {
        if (_helloRequester != null)
        {
            if (_helloRequester.serverUpdateCame)
            {
                _helloRequester.serverUpdateCame = false;
                transform.GetComponent<GraphElementScript>().showconversion(_helloRequester.serverUpdate, _helloRequester.command);
            }
        }
        
    }
}