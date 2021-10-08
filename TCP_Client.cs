using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class TCP_Client : MonoBehaviour
{
    [Serializable]
    public class FrameObject
    {
        public List<float> data;
    }

    public static TCP_Client current;
    public string host = "10.5.177.178";
    public Int32 port = 50000;

    public float temperature = 25.4f;

    internal Boolean socketReady = false;
    internal String inputBuffer = "";
    internal int pixelCount = 0;

    TcpClient tcpSocket;
    NetworkStream netStream;

    StreamWriter socketWriter;
    StreamReader socketReader;

    void Awake()
    {
        current = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        setupSocket();
    }

    public List<Tuple<int, int, float>> GetFrame()
    {
        string request = Convert.ToString(temperature);
        writeSocket(request + "\n");
        // Debug.Log("Sent ping");
        String jsonString = readSocket();
        // Debug.Log(aws);
        // Make this more robust... sometimes parts of the Json string is missing!
        try
        {
            FrameObject deserialized = JsonUtility.FromJson<FrameObject>(jsonString);
            List<Tuple<int, int, float>> frameData = new List<Tuple<int, int, float>>();
            Debug.Log("deserialized Count");
            Debug.Log(deserialized.data.Count);
            for (int i = 0; i < pixelCount; i++)
            {
                int index = i * 3;
                Tuple<int, int, float> tmpData = new Tuple<int, int, float>((int)deserialized.data[index], (int)deserialized.data[index + 1], deserialized.data[index + 2]);
                frameData.Add(tmpData);
            }
            Debug.Log(frameData[0].Item3);
            pixelCount = 0;

            return frameData;
        }
        catch (ArgumentException) 
        {
            Debug.Log("JSON ERROR...");
            pixelCount = 0;

            return null;
        }

    }

    void OnApplicationQuit()
    {
        closeSocket();
    }

    // Helper methodes for:
    // ...setting up the communication
    public void setupSocket()
    {
        try
        {
            tcpSocket = new TcpClient(host, port);
            netStream = tcpSocket.GetStream();
            socketWriter = new StreamWriter(netStream);
            socketReader = new StreamReader(netStream);
            socketReady = true;
        }
        catch (Exception e)
        {
            // Something went wrong
            Debug.Log("Socket error: " + e);
        }
    }

    //...writing to a socket...
    public void writeSocket(string line)
    {
        if (!socketReady)
        {
            return;
        }
        // This might be completly useless...
        // byte[] bytes = Encoding.Default.GetBytes(line);
        // line = Encoding.UTF8.GetString(bytes);
        Debug.Log(line);
        socketWriter.Write(line);
        socketWriter.Flush();
    }

    //...reading from a socket...
    public String readSocket()
    {
        int total = 0;
        string lengthStr = "";
        string lengthPix = "";
        char[] data = null;

        pixelCount = 0;
        if (!socketReady)
        {
            return "";
        }

        if (netStream.DataAvailable)
        {
            char nextChar = (char)socketReader.Read();

            while (nextChar != '\n') 
            {
                lengthStr += nextChar;
                nextChar = (char)socketReader.Read();
            }
            Debug.Log("Total is " + lengthStr);
            total = Int32.Parse(lengthStr);
            Debug.Log(total);

            nextChar = (char)socketReader.Read();

            while (nextChar != '\n')
            {
                lengthPix += nextChar;
                nextChar = (char)socketReader.Read();
            }
            Debug.Log("Pixel count is " + lengthPix);
            pixelCount = Int32.Parse(lengthPix);
            Debug.Log(pixelCount);

            data = new char[total];
            int n = socketReader.Read(data, 0, total);
            if (n < total) 
            {
                Debug.Log("Number of characters read did not match total characters expected. Looking for more...");
                while (n != total) 
                {
                    char additionalChar = (char)socketReader.Read();
                    data.SetValue(additionalChar, n - 1);
                    n++;
                }
            }
            Debug.Log($"{n} characters read");
            string jsonString = new string(data);
            Debug.Log("Data is " + jsonString);

            total = 0;
            lengthStr = "";
            lengthPix = "";
            return jsonString;
        }

        return "";
    }

    //...closing a socket...
    public void closeSocket()
    {
        if (!socketReady)
        {
            return;
        }

        socketWriter.Close();
        socketReader.Close();
        tcpSocket.Close();
        socketReady = false;
    }
}
