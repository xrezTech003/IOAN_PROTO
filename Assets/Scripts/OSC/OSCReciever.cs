//
//	  UnityOSC - Open Sound Control interface for the Unity3d game engine
//
//	  Copyright (c) 2012 Jorge Garcia Martin
//	  Last edit: Gerard Llorach 2nd August 2017
//
// 	  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// 	  documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// 	  the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,
// 	  and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// 	  The above copyright notice and this permission notice shall be included in all copies or substantial portions
// 	  of the Software.
//
// 	  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// 	  TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// 	  THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// 	  CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// 	  IN THE SOFTWARE.
//


using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityOSC
{
    /// <summary>
    ///     CD : OSCReciever
    ///     Reciever for holding OSC messages and supplying them
    /// </summary>
    public class OSCReciever
    {
        #region PRIVATE_VAR
        /// <summary>
        ///     VD : _queue
        ///     Queue of OSCMessages
        /// </summary>
        Queue<OSCMessage> _queue = new Queue<OSCMessage>();

        /// <summary>
        ///     VD : _server
        ///     instance of OSCServer class
        /// </summary>
        OSCServer _server;
        #endregion

        #region PUBLIC_FUNC
        /**
        <summary>
            FD : Open(int)
            Log if PlayerSettings isn't running in the background
            Close v:_server if not null
            Set v:_server to new OSCServer with port number
            <param name="port"></param>
        </summary>
        **/
        public void Open(int port)
        {
#if UNITY_EDITOR
            if(PlayerSettings.runInBackground == false)
            {
                Debug.LogWarning("Recommend PlayerSettings > runInBackground = true");
            }
#endif
            if (_server != null)
            {
                _server.Close();
            }

            _server = new OSCServer(port);
            _server.SleepMilliseconds = 0;
            _server.PacketReceivedEvent += didRecievedEvent;
        }

        /**
        <summary>
            FD : Close()
            Close v:_server if not null
        </summary>
        **/
        public void Close()
        {
            if (_server != null)
            {
                _server.Close();
                _server = null;
            }
        }

        /**
        <summary>
            FD : hasWaitingMessages()
            Lock v:_queue and return if v:_queue has any elements
        </summary>
        **/
        public bool hasWaitingMessages()
        {
            lock (_queue)
            {
                return 0 < _queue.Count;
            }
        }

        /**
        <summary>
            FD : getNextMessage()
            Lock v:_queue and return the next message in v:_queue
        </summary>
        **/
        public OSCMessage getNextMessage()
        {
            lock (_queue)
            {
                return _queue.Dequeue();
            }
        }
        #endregion

        #region PRIVATE_FUNC
        /**
        <summary>
            FD : didRecievedEvent(OSCSender, OSCPacket)
            Lock v:_queue
                If packet is a bundle
                    Add all messages in packet to v:_queue
                Else
                    add packet to v:_queue
            <param name="sender"></param>
            <param name="packet"></param>
        </summary>
        **/
        void didRecievedEvent(OSCServer sender, OSCPacket packet)
        {
            lock (_queue)
            {
                if (packet.IsBundle())
                {
                    var bundle = packet as OSCBundle;

                    foreach (object obj in bundle.Data)
                    {
                        OSCMessage msg = obj as OSCMessage;
                        _queue.Enqueue(msg);
                    }
                }
                else
                {
                    _queue.Enqueue(packet as OSCMessage);
                }
            }
        }
        #endregion
    }
}
