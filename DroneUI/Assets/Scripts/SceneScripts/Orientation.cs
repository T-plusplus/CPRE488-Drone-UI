using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

using System;
using UnityEngine.UI;
using System.Threading;
using System.Text;

public class Orientation : MonoBehaviour
{

    bool socketReady = false;
    //Test
    private String Host = "127.0.0.1";
    //Real
    //private String Host = "192.168.1.1";
    public Int32 Port = 8080;
    /// <summary>
    /// Get LoopTime Button.(The quad manages itself by entering a fail loop if the main execution loop takes to long, 
    /// so its useful for a developer to get this data.)
    /// </summary>
    private Button LTB;
    /// <summary>
    /// Sensor Data Button. Although the scene will audit the quad's data "quietly" in Update(), This button will print to console.
    /// </summary>
    private Button SDB;
    private Button SendDBM;
    //May remove the Retry connection button(Code not written yet, but it's in the scene)


    private Text Status;
    private InputField InputField;
    private Text LogText;
    private string Buffer="";
    private readonly string x = "Connection Status: ";

    Thread thread;
    #region private members 	
    private TcpClient socketConnection;
    private Thread clientReceiveThread;
    private bool isDebug = false;
    private bool isSensorPress = false;

    private string SensorBuf = "";
    #endregion
    // Start connection with Quad and Initialize objects
    void Start()
    {
        LTB = GameObject.Find("LoopTime").GetComponent<Button>();
        LTB.onClick.AddListener(LoopTime);
        SDB = GameObject.Find("SensorData").GetComponent<Button>();
        SDB.onClick.AddListener(SensorData);
        InputField = GameObject.Find("InputField").GetComponent<InputField>();
        LogText = GameObject.Find("LogView/Text").GetComponent<Text>();
        LogText.text = "Session Started";
        SendDBM = GameObject.Find("DebugSend").GetComponent<Button>();
        SendDBM.onClick.AddListener(delegate { SendDebug(InputField.text); });

        Status = GameObject.Find("Status").GetComponent<Text>();

        ConnectToTcpServer();
    }

    // Send the char requesting the sensor data from drone. Drone responds, use data.
    void Update()
    {
        //Debug.Log("Update");
        if (Buffer!="")
        {
            //print to debug
            Debug.Log(Buffer);
            //add to our console
            LogText.text += ("\n" + Buffer);
            //clear buffer
            Buffer = "";
        }
        if(SensorBuf!="")
        {
            //print to debug
            Debug.Log(SensorBuf);
            //add to our console
            LogText.text += ("\n" + SensorBuf);
            //clear buffer
            SensorBuf = "";
            Debug.Log(ParseData(" "+ SensorBuf));
        }
    }
    void RetryConnect()
    {
        ConnectToTcpServer();
    }
    void LoopTime()
    {
        SendMSG("l");
        if (!socketReady)
        {
            
        }
        else
        {
            //WriteToDrone("l");
            //Debug.Log(ReadSocket());
            
        }
    }
    void SensorData()
    {
        isSensorPress = true;
        SendMSG("s");
        if (!socketReady)
        {

        }
        else
        {
            
        }
    }
    void SendDebug(string msg)
    {

        char[] tmp = msg.ToCharArray();
        SendMSG("d");
        for(int dex=0; dex<tmp.Length; dex++)
        {
            SendMSG("" + tmp[dex]);
        }
        SendMSG("\n");
    }
    /// <summary>
    /// Setup socket connection.
    /// </summary>
    private void ConnectToTcpServer()
    {
        try
        {
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
            Status.text = x + "Connected";
            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
            Status.text = x + "Negative";
            socketReady = false;
        }

    }
    /// <summary> 	
    /// Runs in background clientReceiveThread; Listens for incomming data.
    /// </summary>
    private void ListenForData()
    {
        try
        {
            socketConnection = new TcpClient(Host, Port);
            Byte[] bytes = new Byte[1024];
            while (true)
            {
                // Get a stream object for reading 				
                using (NetworkStream stream = socketConnection.GetStream())
                {
                    int length;
                    // Read incomming stream into byte arrary. 					
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incomingData = new byte[length];
                        Array.Copy(bytes, 0, incomingData, 0, length);
                        // Convert byte array to string message.
                        string serverMessage = Encoding.ASCII.GetString(incomingData);
                        Debug.Log("server message received as: " + serverMessage);
                        if(isSensorPress)
                        {
                            SensorBuf = serverMessage;
                            isSensorPress = false;
                            Buffer=string.Copy(SensorBuf);
                        }
                        else
                        {
                            if(serverMessage.ToCharArray()[0]=='A' || serverMessage.ToCharArray()[0]=='G')
                            {
                                SensorBuf = serverMessage;
                                //SensorBuf = Buffer;
                            }
                            Buffer = serverMessage;
                            //SensorBuf = serverMessage;
                        }
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
            socketReady = false;
        }

    }

    /// <summary>
    /// Send message to server using socket connection.
    /// </summary>
    private void SendMSG(string toSend)
    {
        if (socketConnection == null)
        {
            Debug.Log("Not Connected");
            return;
        }
        try
        {
            // Get a stream object for writing.
            NetworkStream stream = socketConnection.GetStream();
            if (stream.CanWrite)
            {
                string clientMessage = toSend;
                // Convert string message to byte array.
                byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
                // Write byte array to socketConnection stream.
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                Debug.Log("Client sent his message - should be received by server");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }

    }
    private float[] ParseData(string dataLine)
    {
        float[] ret = new float[3];
        string[] temp = dataLine.Split(' ');
        for (int dex = 0; dex < temp.Length; dex++)
        {
            ret[dex] = float.Parse(temp[dex]);
            Debug.Log(dex + " " + ret[dex]);
        }
        return ret;
    }
}
