using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;

public class Orientation : MonoBehaviour
{

    private bool socketReady = false;                
    private TcpClient mySocket;
    private NetworkStream NetStream;
    StreamWriter sw;
    StreamReader sr;
    public string Host = "192.88.1.1";
    public int Port = 8080;
    //Start connection with Quad here
    private void Awake()
    {
        Connect();
    }
    // Initialize objects
    void Start()
    {
        
    }

    // Send the char requesting the sensor data from drone. Drone responds, use data.
    void Update()
    {if(socketReady)
        while(NetStream.DataAvailable)
        {
            string data = ReadSocket();
            Debug.Log(data);
        }
    }
    private void Connect()
    {
        Debug.Log("Trying to connect...");
        try
        {
            mySocket = new TcpClient(Host, Port);
            NetStream = mySocket.GetStream();
            sr = new StreamReader(NetStream);
            sw = new StreamWriter(NetStream);
            
            socketReady = true;
        }
        catch (System.Exception e)
        {
            Debug.Log("Socket error:" + e);           
        }

    }
    public string ReadSocket()
    {                        
        if (!socketReady)
            return "";
        if (NetStream.DataAvailable)
            return sr.ReadLine();
        return "No Data";
    }
}
