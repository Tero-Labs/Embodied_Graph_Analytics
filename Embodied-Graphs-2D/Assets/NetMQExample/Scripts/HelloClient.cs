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

    public void Abstraction_conversion(string graph_as_string, string conversion_type)
    {
        transform.GetComponent<StartServer>().ExecuteCommand(conversion_type);
        _helloRequester = new HelloRequester();
        _helloRequester.graph_as_str = graph_as_string; //"{8,9,7,10}-{8,9}{9,7}{8,7}";
        _helloRequester.Start();
    }
}