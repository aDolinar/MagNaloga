using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;

public class UDPComms : MonoBehaviour
{
    public static UDPComms instance;
    bool simRunning;
    //data
    byte[] dataSent;
    float[] dataToBeSent;
    float[] dataReceived;
    byte[] dataBuffer;
    byte[] preallocSend;
    const int totalDataSent = 21;
    const int totalDataReceived = 3;
    const int bufferSize = 1024;
    int floatsize;
    //ports
    const int portSend = 25106;
    const int portReceive = 25008;
    //threads
    Thread threadSend;
    Thread threadReceive;
    //ip end points
    IPEndPoint IPEPsend;
    IPEndPoint IPEPreceive;
    IPEndPoint IPEPany;
    EndPoint EP;
    //sockets
    UdpClient clientSend;
    Socket socketReceive;

    private void Awake()
    {
        instance = this;
        simRunning = true;
    }
    private void Start()
    {
        floatsize = sizeof(float);
        dataSent = new byte[totalDataSent*floatsize];
        dataToBeSent = new float[totalDataSent];
        dataReceived = new float[totalDataReceived];
        dataBuffer = new byte[bufferSize];
        preallocSend = new byte[floatsize]; 
        IPEPsend = new IPEndPoint(IPAddress.Parse("127.0.0.1"), portSend);
        IPEPreceive = new IPEndPoint(IPAddress.Any, portReceive);
        IPEPany = new IPEndPoint(IPAddress.Any,0);
        EP = (EndPoint)(IPEPany);

        Debug.Log("Starting UDP");
        try
        {
            threadSend = new Thread(new ThreadStart(InitThreadSend));
            threadReceive = new Thread(new ThreadStart(InitThreadReceive));

            threadSend.IsBackground = true;
            threadReceive.IsBackground = true;

            threadSend.Start();
            threadReceive.Start();
        }
        catch (Exception exc)
        {
            Debug.LogError("UDP EXCEPTION: " + exc.Message);
        }
    }


    void InitThreadSend()
    {
        clientSend = new UdpClient();
        while (simRunning)
        {
            Thread.Sleep(20);
            for (int i = 0; i < totalDataSent; i++)
            {
                preallocSend = BitConverter.GetBytes(dataToBeSent[i]);
                for (int j = 0; j < floatsize; j++)
                {
                    dataSent[i * floatsize + j] = preallocSend[j];
                }
            }
            try
            {
                clientSend.Send(dataSent, dataSent.Length, IPEPsend);
            }
            catch (Exception exc)
            {
                Debug.LogError("UDP EXCEPTION: " + exc.Message);
            }
        }
        clientSend.Close();

    }
    void InitThreadReceive()
    {
        socketReceive = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        try
        {
            socketReceive.Bind(IPEPreceive);
            socketReceive.ReceiveFrom(dataBuffer, ref EP);
            while (simRunning)
            {
                try
                {
                    socketReceive.ReceiveFrom(dataBuffer, ref EP);
                    for (int i = 0; i < totalDataReceived; i++)
                    {
                        dataReceived[i] = BitConverter.ToSingle(dataBuffer, floatsize * i);
                    }
                }
                catch(Exception exc)
                {
                    Debug.LogError("UDP EXCEPTION in threadReceive: " + exc.Message);
                }
            }
            socketReceive.Close();
        }
        catch (Exception exc)
        {
            Debug.LogError("UDP EXCEPTION: " + exc.Message);
        }
    }
    public float GetDataIdx(int idx)
    {
        return dataReceived[idx];
    }
    public float[] GetData()
    {
        return dataReceived;
    }
    public void SendData(float[] data)
    {
        for (int i = 0; i < totalDataSent; i++)
        {
            dataToBeSent[i] = data[i];
        }
    }
    private void OnApplicationQuit()
    {
        simRunning = false;
    }
}