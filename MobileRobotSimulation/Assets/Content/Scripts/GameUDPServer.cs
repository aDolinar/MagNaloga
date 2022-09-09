// -------------------------------------------------------------------------- //
// 					Bimeo Game UDP Server						 	  		  //
// -------------------------------------------------------------------------- //
//	File:			GameUDPServer.js							  		  	  //
//	Description:	Game UDP Server script for BiMeo Games		  	  		  //
// -------------------------------------------------------------------------- //
// 	Date created:	May 2013												  //	
//	Date modified:	July 2013												  //
// -------------------------------------------------------------------------- //
// 	Created by:		Matjaz Mihelj											  //	
//	Extended by:	Ales Hribar								 				  //
// -------------------------------------------------------------------------- //
//					All rights reserved Kinestica							  //
// -------------------------------------------------------------------------- //

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;

public class GameUDPServer : MonoBehaviour
{
	public static GameUDPServer instance;
	// RECEIVE THREAD PARAMETERS
	const int PORT_RECEIVE = 25008;
	const int NUM_RECEIVE_FLOATS = 3;
	private Thread UDPThreadReceive;
	float[] dataReceive = new float[NUM_RECEIVE_FLOATS];
	bool isRunningReceive = true;

	// SEND THREAD PARAMETERS
	const int PORT_SEND = 25106;
	const int NUM_SEND_FLOATS = 19;
	private Thread UDPThreadSend;
	float[] dataSend = new float[NUM_SEND_FLOATS];
	bool isRunningSend = true;

	public bool showInput = true;
	//	private string IPSendAddress;	

	// DEBUG MODE PARAMETERS
	bool showDebug = false;
	private string udpStatus = "";

    //////////////////////////////////////////////////////
    // 	UPD SEND AND RECEIVE INITIALIZATION
    //////////////////////////////////////////////////////	
    private void Awake()
    {
		instance = this;
    }
    void Start()
	{

		print("Start");
		try
		{
			//Starting the UDP Server thread.
			UDPThreadReceive = new Thread(new ThreadStart(StartReceive));
			UDPThreadSend = new Thread(new ThreadStart(StartSend));

			isRunningReceive = true;
			isRunningSend = true;

			UDPThreadReceive.IsBackground = true;
			UDPThreadSend.IsBackground = true;

			UDPThreadReceive.Start();
			UDPThreadSend.Start();

			udpStatus = "Started UDP Receiver and Send Threads!";
		}
		catch (Exception e)
		{
			udpStatus = "An UDP Exception has occurred: " + e.ToString();
		}
	}

	//////////////////////////////////////////////////////
	// 	STOP RECEIVING AND SENDING DATA WHEN APPLICATION QUIT
	//////////////////////////////////////////////////////	
	void OnApplicationQuit()
	{
		isRunningReceive = false;
		isRunningSend = false;
		udpStatus = "UDP Stopped.";
	}


	//////////////////////////////////////////////////////
	// 	RECEIVING DATA THROUGH UDP PORT
	//////////////////////////////////////////////////////		
	void StartReceive()
	{
		byte[] data = new byte[NUM_RECEIVE_FLOATS * 4];

		IPEndPoint ipep = new IPEndPoint(IPAddress.Any, PORT_RECEIVE);
		Socket newsock = new Socket(AddressFamily.InterNetwork,
					  SocketType.Dgram, ProtocolType.Udp);

		try
		{
			newsock.Bind(ipep);
			udpStatus = "Waiting for a client...";
			IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
			EndPoint Remote = (EndPoint)(sender);

			try
			{
				newsock.ReceiveFrom(data, ref Remote);
				udpStatus = "Message received";

				while (isRunningReceive)
				{
					data = new byte[1024];

					try
					{

						newsock.ReceiveFrom(data, ref Remote);
						// GET VALUES
						for (int i = 0; i < NUM_RECEIVE_FLOATS; i++)
						{
							dataReceive[i] = BitConverter.ToSingle(data, sizeof(float) * i);
							Console.WriteLine(dataReceive[i]);
						}

					}
					catch (Exception e)
					{
						udpStatus = "An UDP Exception has occurred. Problem Receiving:  " + e.ToString();
					}
				}

				newsock.Close();
			}
			catch (Exception e)
			{
				udpStatus = "An UDP Exception has occurred. Problem Receiving First:  " + e.ToString();
			}
		}
		catch (Exception e)
		{
			udpStatus = "An UDP Exception has occurred. Problem Connecting: " + e.ToString();
		}
	}

	//////////////////////////////////////////////////////
	// 	SENDING DATA THROUGH UDP PORT
	//////////////////////////////////////////////////////	

	void StartSend()
	{

		UdpClient sock = new UdpClient();
		IPEndPoint iep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT_SEND);

		byte[] dataB;
		dataB = new byte[NUM_SEND_FLOATS * sizeof(float)];

		while (isRunningSend)
		{
			Thread.Sleep(20);

			for (int i = 0; i < NUM_SEND_FLOATS; i++)
			{
				byte[] temp = BitConverter.GetBytes(dataSend[i]);

				for (int j = 0; j < sizeof(float); j++)
				{
					dataB[i * sizeof(float) + j] = temp[j];
				}

			}

			try
			{
				sock.Send(dataB, dataB.Length, iep);

			}
			catch (Exception e)
			{
				udpStatus = "An UDP Exception has occurred. Problem Connecting: " + e.ToString();
			}
		}

		sock.Close();

	}

	//////////////////////////////////////////////////////
	// 	SET STATUS FOR RECEIVING AND SENDING DATA
	//////////////////////////////////////////////////////

	void SetIsRunningReceive(bool isRunReceive)
	{
		isRunningReceive = isRunReceive;
	}

	void SetIsRunningSend(bool isRunSend)
	{
		isRunningSend = isRunSend;
	}


	//////////////////////////////////////////////////////
	// 	CLOSE UDP CONNECTION
	//////////////////////////////////////////////////////	
	public void closeConnection()
	{
		isRunningReceive = false;
		isRunningSend = false;
	}


	//////////////////////////////////////////////////////
	// 	PARSE DATA FROM RECEIVE UDP PORT
	//////////////////////////////////////////////////////

	public float getDataX(int idx)
	{
		return dataReceive[idx];
	}

	public float[] getData()
	{
		float[] data = new float[NUM_RECEIVE_FLOATS];
		for (int i = 0; i < NUM_RECEIVE_FLOATS; i++)
		{
			data[i] = dataReceive[i];
		}
		return data;
	}



	public void sendData(float[] output)
	{
		for (int i = 0; i < NUM_SEND_FLOATS; i++)
		{
			dataSend[i] = output[i];
		}
	}

	public int getFloatCount(bool send)
	{
		int r = 0;
		if (send)
			r = NUM_SEND_FLOATS;
		else
			r = NUM_RECEIVE_FLOATS;
		return r;
	}


	void OnGUI()
	{
		if (showDebug)
		{
			Rect rectObj = new Rect(20, Screen.height - 60, Screen.width - 40, 22);

			GUIStyle style = new GUIStyle();
			style.alignment = TextAnchor.UpperLeft;

			GUI.Box(rectObj, "UDP Status: " + udpStatus, style);
		}

		// TOGGLE DISPLAY
		if (Input.GetKey(KeyCode.F4))
		{
			showDebug = !showDebug;
		}
	}
}