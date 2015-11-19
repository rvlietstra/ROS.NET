﻿// File: XmlRpcDispatch.cs
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
    public class XmlRpcDispatch : IDisposable
    {
        #region EventType enum

        [Flags]
        public enum EventType
        {
            ReadableEvent = 1,
            WritableEvent = 2,
            Exception = 4
        }

        #endregion

        #region P/Invoke

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcDispatch_Create", CallingConvention = CallingConvention.Cdecl)
        ]
        private static extern IntPtr create();

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcDispatch_Close", CallingConvention = CallingConvention.Cdecl)]
        private static extern void close(IntPtr target);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcDispatch_AddSource",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void addsource(IntPtr target, IntPtr source, uint eventMask);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcDispatch_RemoveSource",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void removesource(IntPtr target, IntPtr source);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcDispatch_SetSourceEvents",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void setsourceevents(IntPtr target, IntPtr source, uint eventMask);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcDispatch_Work", CallingConvention = CallingConvention.Cdecl)]
        private static extern void work(IntPtr target, double msTime);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcDispatch_Exit", CallingConvention = CallingConvention.Cdecl)]
        private static extern void exit(IntPtr target);

        [DllImport("XmlRpcWin32.dll", EntryPoint = "XmlRpcDispatch_Clear", CallingConvention = CallingConvention.Cdecl)]
        private static extern void clear(IntPtr target);

        #endregion

        #region Reference Tracking + unmanaged pointer management

        private IntPtr __instance;

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
        public static XmlRpcDispatch LookUp(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                AddRef(ptr);
                return new XmlRpcDispatch(ptr);
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
                        close(ptr);
                        XmlRpcUtil.Free(ptr);
                    }
                }
                ptr = IntPtr.Zero;
            }
        }

        public IntPtr instance
        {
#if !TRACE
            [DebuggerStepThrough]
#endif
                get
            {
                if (__instance == IntPtr.Zero)
                {
                    Console.WriteLine("UH OH MAKING A NEW INSTANCE IN instance.get!");
                    __instance = create();
                    AddRef(__instance);
                }
                return __instance;
            }
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
        public XmlRpcDispatch()
        {
            instance = create();
        }

#if !TRACE
        [DebuggerStepThrough]
#endif
        public XmlRpcDispatch(IntPtr otherref)
        {
            if (otherref != IntPtr.Zero)
                instance = otherref;
        }

        public void AddSource(XmlRpcClient source, int eventMask)
        {
            addsource(instance, source.instance, (uint) eventMask);
        }

        public void RemoveSource(XmlRpcClient source)
        {
            source.SegFault();
            removesource(instance, source.instance);
        }

        public void SetSourceEvents(XmlRpcClient source, int eventMask)
        {
            setsourceevents(instance, source.instance, (uint) eventMask);
        }

        public void Work(double msTime)
        {
            work(instance, msTime);
        }

        public void Exit()
        {
            try
            {
                exit(instance);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Clear()
        {
            try
            {
                //clear(instance);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}