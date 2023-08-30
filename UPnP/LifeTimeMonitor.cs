/*
Copyright 2006 - 2010 Intel Corporation

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Collections;
using OpenSource.Utilities;
using Intel.UPNP;

namespace OpenSource.UPnP
{
    /// <summary>
    /// This class keeps track of many timers as once. The users can setup one
    /// timer per object it passes in. The class will call back at the right
    /// moment with the reference to the object on which the timer was setup.
    /// </summary>
    public sealed class LifeTimeMonitor
    {
        private SortedList MonitorList = new SortedList();
        private object MonitorLock = new object();
        private SafeTimer_SINGLE SafeNotifyTimer = new SafeTimer_SINGLE();

        public delegate void LifeTimeHandler(LifeTimeMonitor sender, object obj);

        private WeakEvent OnExpiredEvent = new WeakEvent();

        /// <summary>
        /// Triggered when the lifetime duration passes
        /// </summary>
        public event LifeTimeHandler OnExpired
        {
            add
            {
                OnExpiredEvent.Register(value);
            }
            remove
            {
                OnExpiredEvent.UnRegister(value);
            }
        }

        ~LifeTimeMonitor()
        {
            //SafeNotifyTimer.dispose();
            SafeNotifyTimer = null;
        }

        /// <summary>
        /// Instantiates a new Monitor
        /// </summary>
        public LifeTimeMonitor()
        {
            //OpenSource.Utilities.InstanceTracker.Add(this);
        }

        private void OnTimedEvent(SafeTimer_SINGLE sender)
        {
            ArrayList eventList = new ArrayList();
            lock (MonitorLock)
            {
                while (MonitorList.Count > 0 && ((DateTime)MonitorList.GetKey(0)).CompareTo(DateTime.Now.AddSeconds(0.05)) < 0)
                {
                    eventList.Add(MonitorList.GetByIndex(0));
                    MonitorList.RemoveAt(0);
                }
            }
            foreach (object obj in eventList)
                OnExpiredEvent.Fire(this, obj);

        }

        public void Clear()
        {
            lock (MonitorLock)
            {
                MonitorList.Clear();
            }
        }

        /// <summary>
        /// Remove the pending callback associated with this object.
        /// </summary>
        /// <param name="obj">Object associated with the pending callback</param>
        /// <returns></returns>
        public bool Remove(object obj)
        {
            if (obj == null)
                return false;
            bool RetVal = false;
            lock (MonitorLock)
            {
                if (MonitorList.ContainsValue(obj) == true)
                {
                    RetVal = true;
                    MonitorList.RemoveAt(MonitorList.IndexOfValue(obj));
                }
            }
            return RetVal;
        }

        public void Add(object obj, int secondTimeout)
        {
            Add(obj, (double)secondTimeout);
        }

        /// <summary>
        /// Add a new callback event for a given object in a given number of
        /// seconds in the future. This class will call back at the appropriate
        /// time unless the event was removed, or was added with a new time.
        /// There can only be one pending event per object.
        /// </summary>
        /// <param name="obj">Object that will be returned to the user when the event is triggered</param>
        /// <param name="secondTimeout">Number of seconds to wait before calling the event</param>
        public void Add(object obj, double secondTimeout)
        {
            if (obj == null)
                return;
            if (secondTimeout <= 0)
            {
                secondTimeout = (double)0.01;
            }
            lock (MonitorLock)
            {
                //SafeNotifyTimer.Stop();
                //NotifyTimer.Stop();
                if (MonitorList.ContainsValue(obj) == true)
                {
                    MonitorList.RemoveAt(MonitorList.IndexOfValue(obj));
                }

                DateTime eventTriggerTime = DateTime.Now.AddSeconds(secondTimeout);
                while (MonitorList.ContainsKey(eventTriggerTime))
                {
                    eventTriggerTime = eventTriggerTime.AddMilliseconds(1);
                }
                MonitorList.Add(eventTriggerTime, obj);
            }
            OnTimedEvent(SafeNotifyTimer);
        }
    }
}
