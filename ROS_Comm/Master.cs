﻿// File: Master.cs
// Project: ROS_C-Sharp
// 
// ROS.NET
// Eric McCann <emccann@cs.uml.edu>
// UMass Lowell Robotics Laboratory
// 
// Reimplementation of the ROS (ros.org) ros_cpp client in C#.
// 
// Created: 04/28/2015
// Updated: 10/07/2015

#region USINGZ

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using XmlRpc_Wrapper;

#endregion

namespace Ros_CSharp
{
#if !TRACE
    [DebuggerStepThrough]
#endif
    public static class master
    {
        public static int port;
        public static string host = "";
        public static string uri = "";
        public static TimeSpan retryTimeout = TimeSpan.FromSeconds(5);

        public static void init(IDictionary remapping_args)
        {
            if (remapping_args.Contains("__master"))
            {
                uri = (string) remapping_args["__master"];
                ROS.ROS_MASTER_URI = uri;
            }
            if (string.IsNullOrEmpty(uri))
                uri = ROS.ROS_MASTER_URI;
            if (!network.splitURI(uri, ref host, ref port))
            {
                port = 11311;
            }
        }

        /// <summary>
        ///     Checks if master is running? I think.
        /// </summary>
        /// <returns></returns>
        public static bool check()
        {
            XmlRpcValue args = new XmlRpcValue(), result = new XmlRpcValue(), payload = new XmlRpcValue();
            args.Set(0, this_node.Name);
            return execute("getPid", args, ref result, ref payload, false);
        }

        /// <summary>
        ///     Gets all currently published and subscribed topics and adds them to the topic list
        /// </summary>
        /// <param name="topics"> List to store topics</param>
        /// <returns></returns>
        public static bool getTopics(ref TopicInfo[] topics)
        {
            List<TopicInfo> topicss = new List<TopicInfo>();
            XmlRpcValue args = new XmlRpcValue(), result = new XmlRpcValue(), payload = new XmlRpcValue();
            args.Set(0, this_node.Name);
            args.Set(1, "");
            if (!execute("getPublishedTopics", args, ref result, ref payload, true))
                return false;
            topicss.Clear();
            for (int i = 0; i < payload.Size; i++)
                topicss.Add(new TopicInfo(payload[i][0].Get<string>(), payload[i][1].Get<string>()));
            topics = topicss.ToArray();
            return true;
        }

        /// <summary>
        ///     Gets all currently existing nodes and adds them to the nodes list
        /// </summary>
        /// <param name="nodes">List to store nodes</param>
        /// <returns></returns>
        public static bool getNodes(ref string[] nodes)
        {
            List<string> names = new List<string>();
            XmlRpcValue args = new XmlRpcValue(), result = new XmlRpcValue(), payload = new XmlRpcValue();
            args.Set(0, this_node.Name);

            if (!execute("getSystemState", args, ref result, ref payload, true))
            {
                return false;
            }
            for (int i = 0; i < payload.Size; i++)
            {
                for (int j = 0; j < payload[i].Size; j++)
                {
                    XmlRpcValue val = payload[i][j][1];
                    for (int k = 0; k < val.Size; k++)
                    {
                        string name = val[k].Get<string>();
                        names.Add(name);
                    }
                }
            }
            nodes = names.ToArray();
            return true;
        }

        internal static CachedXmlRpcClient clientForNode(string nodename)
        {
            XmlRpcValue args = new XmlRpcValue();
            args.Set(0, this_node.Name);
            args.Set(1, nodename);
            XmlRpcValue resp = new XmlRpcValue();
            XmlRpcValue payl = new XmlRpcValue();
            if (!execute("lookupNode", args, ref resp, ref payl, true))
                return null;
            if (!XmlRpcManager.Instance.validateXmlrpcResponse("lookupNode", resp, ref payl))
                return null;
            string nodeuri = payl.GetString();
            string nodehost = null;
            int nodeport = 0;
            if (!network.splitURI(nodeuri, ref nodehost, ref nodeport) || nodehost == null || nodeport <= 0)
                return null;
            return XmlRpcManager.Instance.getXMLRPCClient(nodehost, nodeport, nodeuri);
        }


        public static bool kill(string node)
        {
            CachedXmlRpcClient cl = clientForNode(node);
            if (cl == null)
                return false;
            XmlRpcValue req = new XmlRpcValue(), resp = new XmlRpcValue(), payl = new XmlRpcValue();
            req.Set(0, this_node.Name);
            req.Set(1, "Out of respect for Mrs. " + this_node.Name);
            if (!cl.Execute("shutdown", req, resp) || !XmlRpcManager.Instance.validateXmlrpcResponse("lookupNode", resp, ref payl))
                return false;
            payl.Dump();
            XmlRpcManager.Instance.releaseXMLRPCClient(cl);
            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="method"></param>
        /// <param name="request">Full request to send to the master </param>
        /// <param name="response">Full response including status code and status message. Initially empty.</param>
        /// <param name="payload">Location to store the actual data requested, if any.</param>
        /// <param name="wait_for_master">If you recieve an unseccessful status code, keep retrying.</param>
        /// <returns></returns>
        public static bool execute(string method, XmlRpcValue request, ref XmlRpcValue response, ref XmlRpcValue payload,
            bool wait_for_master)
        {
            try
            {
                DateTime startTime = DateTime.Now;
                string master_host = host;
                int master_port = port;

                //EDB.WriteLine("Trying to connect to master @ " + master_host + ":" + master_port);
                CachedXmlRpcClient client = XmlRpcManager.Instance.getXMLRPCClient(master_host, master_port, "/");
                bool printed = false;
                bool success = false;

                while (!success)
                {
                    // Check if we are shutting down
                    if (XmlRpcManager.Instance.shutting_down)
                        return false;

                    // if the client is connected, execute the RPC call
                    success = client.IsConnected && client.Execute(method, request, response);

                    // Set success to false when response validation fails
                    if (success && !XmlRpcManager.Instance.validateXmlrpcResponse(method, response, ref payload))
                        success = false;

                    if (success)
                    {
                        XmlRpcManager.Instance.releaseXMLRPCClient(client);
                        return true;
                    }
                    else
                    {
                        if (!wait_for_master)
                        {
                            XmlRpcManager.Instance.releaseXMLRPCClient(client);
                            return false;
                        }

                        if (!printed)
                        {
                            EDB.WriteLine("[{0}] FAILED TO CONTACT MASTER AT [{1}:{2}]. {3}", method, master_host,
                                master_port, (wait_for_master ? "Retrying..." : ""));
                            printed = true;
                        }
                        if (retryTimeout.TotalSeconds > 0 && DateTime.Now.Subtract(startTime) > retryTimeout)
                        {
                            EDB.WriteLine("[{0}] Timed out trying to connect to the master after [{1}] seconds", method,
                                retryTimeout.TotalSeconds);
                            XmlRpcManager.Instance.releaseXMLRPCClient(client);
                            return false;
                        }

                        //recreate the client, thereby causing it to reinitiate its connection (gross, but effective -- should really be done in xmlrpcwin32)
                        XmlRpcManager.Instance.releaseXMLRPCClient(client);
                        client = null;
                        Thread.Sleep(50);
                        client = XmlRpcManager.Instance.getXMLRPCClient(master_host, master_port, "/");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }
    }
}