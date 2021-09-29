using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO.Compression;
using System.IO;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CD: c:UPDSender is a class that holds a simple socketing script to send messages via UDP
/// </summary>
public class UDPSender  {

    /// <summary>
    /// VD: sentToAddress: The ip address for the socket to be bound to
    /// </summary>
    string sentToAddress;
    /// <summary>
    /// VD: port: port the socket is bound to
    /// </summary>
    int port;

    /// <summary>
    /// VD: remoteIP: correct formatted for c:IPAddress
    ///</summary>
    IPAddress remoteIP = null;
    /// <summary>
    /// VD: remoteIPEP: Endpoint for messages that are sent
    /// </summary>
    IPEndPoint remoteIPEP = null;
    /// <summary>
    /// VD: mySocket: This classes instance of the standard Socket Class
    /// </summary>
    Socket mySocket = null;


    /// <summary>
    /// FD: UDPSender(String ipAddress, int port): Class initalizer for c:UDPSender, takes an v:ipAddress and v:port and binds those to a socket via f:bindToSocket
    /// </summary>
    /// <param name="ipAddress">String contatining a standard ip address</param>
    /// <param name="port">contains int for port number</param>
    public UDPSender(String ipAddress, int port) {
		sentToAddress = ipAddress;
		this.port = port;
		bindToSocket ();
	}


    /// <summary>
    /// FD: closeSocket(): Closes the socket via f:Close
    /// </summary>
	public void closeSocket() {
			mySocket.Close ();
	}

    /// <summary>
    /// FD: bindToSocket(): Binds the socket by setting up IP endpoints (v:remoteIPEP) and then creates a new UDP socket to v:mySocket.
    /// </summary>
    public void bindToSocket() {
		remoteIP = IPAddress.Parse(sentToAddress);
		remoteIPEP = new IPEndPoint(remoteIP, port);
		mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

	}

    /// <summary>
    /// FD: Send(string msg): sends a string v:msg over the established v:mySocket to the endpoint v:remoteIPEP
    /// </summary>
    /// <param name="msg">string contatining some message</param>
	public void Send(string msg) {
		mySocket.SendTo(Encoding.ASCII.GetBytes(msg), remoteIPEP);
	}
}
