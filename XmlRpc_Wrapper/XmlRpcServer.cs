﻿// File: XmlRpcServer.cs
// Project: XmlRpc_Wrapper
// 
// ROS.NET
// Eric McCann <emccann@cs.uml.edu>
// UMass Lowell Robotics Laboratory
// 
// Reimplementation of the ROS (ros.org) ros_cpp client in C#.
// 
// Created: 09/01/2015
// Updated: 10/07/2015

#region USINGZ

//#define REFDEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

#endregion

namespace XmlRpc_Wrapper
{
    public class XmlRpcServer : IDisposable
    {
        protected IntPtr __instance;

        public IntPtr instance
        {
#if !TRACE
            [DebuggerStepThrough]
#endif
                get { return __instance; }
#if !TRACE
            [DebuggerStepThrough]
#endif
                set
            {
                if (__instance != IntPtr.Zero)
                    RmRef(ref __instance);
                if (value != IntPtr.Zero)
                    AddRef(value);
                __instance = value;
            }
        }

        #region Reference Tracking + unmanaged pointer management

        public void Dispose()
        {
            Shutdown();
        }

        private static Dictionary<IntPtr, int> _refs = new Dictionary<IntPtr, int>();
        private static object reflock = new object();
#if REFDEBUG
        private static Thread refdumper;
        private static void dumprefs()
        {
            while (true)
            {
                Dictionary<IntPtr, int> dainbrammage = null;
                lock (reflock)
                {
                    dainbrammage = new Dictionary<IntPtr, int>(_refs);
                }
                Console.WriteLine("REF DUMP");
                foreach (KeyValuePair<IntPtr, int> reff in dainbrammage)
                {
                    Console.WriteLine("\t" + reff.Key + " = " + reff.Value);
                }
                Thread.Sleep(500);
            }
        }
#endif

#if !TRACE
        [DebuggerStepThrough]
#endif
        public static XmlRpcServer LookUp(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                AddRef(ptr);
                return new XmlRpcServer(ptr);
            }
            return null;
        }


#if !TRACE
        [DebuggerStepThrough]
#endif
        private static void AddRef(IntPtr ptr)
        {
#if REFDEBUG
            if (refdumper == null)
            {
                refdumper = new Thread(dumprefs);
                refdumper.IsBackground = true;
                refdumper.Start();
            }
#endif
            lock (reflock)
            {
                if (!_refs.ContainsKey(ptr))
                {
#if REFDEBUG
                    Console.WriteLine("Adding a new reference to: " + ptr + " (" + 0 + "==> " + 1 + ")");
#endif
                    _refs.Add(ptr, 1);
                }
                else
                {
#if REFDEBUG
                    Console.WriteLine("Adding a new reference to: " + ptr + " (" + _refs[ptr] + "==> " + (_refs[ptr] + 1) + ")");
#endif
                    _refs[ptr]++;
                }
            }
        }

#if !TRACE
        [DebuggerStepThrough]
#endif
        private static void RmRef(ref IntPtr ptr)
        {
            lock (reflock)
            {
                if (_refs.ContainsKey(ptr))
                {
#if REFDEBUG
                    Console.WriteLine("Removing a reference to: " + ptr + " (" + _refs[ptr] + "==> " + (_refs[ptr] - 1) + ")");
#endif
                    _refs[ptr]--;
                    if (_refs[ptr] <= 0)
                    {
#if REFDEBUG
                        Console.WriteLine("KILLING " + ptr + " BECAUSE IT'S A NOT VERY NICE!");
#endif
                        _refs.Remove(ptr);
                        shutdown(ptr);
                        XmlRpcUtil.Free(ptr);
                    }
                }
                ptr = IntPtr.Zero;
            }
        }

        public bool Shutdown()
        {
            return Shutdown(ref __instance);
        }

        public static bool Shutdown(ref IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                RmRef(ref ptr);
                return (ptr == IntPtr.Zero);
            }
            return true;
        }

        #endregion

#if !TRACE
        [DebuggerStepThrough]
#endif
        public XmlRpcServer()
        {
            instance = create();
        }

#if !TRACE
        [DebuggerStepThrough]
#endif
        public XmlRpcServer(IntPtr copy)
        {
            if (copy != IntPtr.Zero)
            {
                instance = copy;
            }
        }

        public int Port
        {
#if !TRACE
            [DebuggerStepThrough]
#endif
                get
            {
                SegFault();
                return getport(instance);
            }
        }

        public XmlRpcDispatch Dispatch
        {
#if !TRACE
            [DebuggerStepThrough]
#endif
                get
            {
                SegFault();
                if (_dispatch == null)
                {
                    IntPtr ret = getdispatch(instance);
                    if (ret == IntPtr.Zero)
                        return null;
                    _dispatch = XmlRpcDispatch.LookUp(ret);
                }
                return _dispatch;
            }
        }

        private XmlRpcDispatch _dispatch;

        #region P/Invoke

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcServer_Create", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr create();

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcServer_AddMethod", CallingConvention = CallingConvention.Cdecl
            )]
        private static extern void addmethod(IntPtr target, IntPtr method);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcServer_RemoveMethod",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void removemethod(IntPtr target, IntPtr method);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcServer_RemoveMethodByName",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void removemethodbyname(IntPtr target,
            [In] [Out] [MarshalAs(UnmanagedType.LPStr)] string method);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcServer_FindMethod",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr findmethod(IntPtr target, [In] [Out] [MarshalAs(UnmanagedType.LPStr)] string name);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcServer_BindAndListen",
            CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool bindandlisten(IntPtr target, int port, int backlog);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcServer_Work", CallingConvention = CallingConvention.Cdecl)]
        private static extern void work(IntPtr target, double msTime);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcServer_Exit", CallingConvention = CallingConvention.Cdecl)]
        private static extern void exit(IntPtr target);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcServer_Shutdown", CallingConvention = CallingConvention.Cdecl)
        ]
        private static extern void shutdown(IntPtr target);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcServer_GetPort", CallingConvention = CallingConvention.Cdecl)]
        private static extern int getport(IntPtr target);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcServer_GetDispatch",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr getdispatch(IntPtr target);

        #endregion

        public void AddMethod(XMLRPCCallWrapper method)
        {
            SegFault();
            addmethod(instance, method.instance);
        }

        public void RemoveMethod(XMLRPCCallWrapper method)
        {
            SegFault();
            removemethod(instance, method.instance);
        }

        public void RemoveMethod(string name)
        {
            SegFault();
            removemethodbyname(instance, name);
        }

        public void Work(double msTime)
        {
            SegFault();
            work(instance, msTime);
        }

        public void Exit()
        {
            SegFault();
            exit(instance);
        }

        public XMLRPCCallWrapper FindMethod(string name)
        {
            SegFault();
            IntPtr ret = findmethod(instance, name);
            if (ret == IntPtr.Zero) return null;
            return XMLRPCCallWrapper.LookUp(ret);
        }

        public bool BindAndListen(int port)
        {
            return BindAndListen(port, 5);
        }

        public bool BindAndListen(int port, int backlog)
        {
            SegFault();
            return bindandlisten(instance, port, backlog);
        }


#if !TRACE
        [DebuggerStepThrough]
#endif
        public void SegFault()
        {
            if (instance == IntPtr.Zero)
                throw new Exception("This isn't really a segfault, but your pointer is invalid, so it would have been!");
        }
    }
}