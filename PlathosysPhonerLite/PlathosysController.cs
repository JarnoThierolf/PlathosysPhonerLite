using PlathosysApiWrapper;
using System;
using System.Text;
using System.Threading;

namespace PlathosysPhonerLite
{
    public class HookEventArgs : EventArgs
    {
        public bool HookOff { get; set; }
    }

    class PlathosysController
    {
        public event EventHandler<HookEventArgs> HookChanged;

        Timer timerHook;
        Timer timerInitPlathosys;
        bool hookOff;

        public PlathosysController()
        {
            timerHook = new Timer(new TimerCallback(timerHook_Tick), null, Timeout.Infinite, Timeout.Infinite);
            timerInitPlathosys = new Timer(new TimerCallback(timerInitPlathosys_Tick), null, 10, 1000);
            hookOff = false;
        }

        /// <summary>
        /// Open connection to Plathosys device
        /// </summary>
        /// <returns>successful?</returns>
        private bool OpenPlathosys()
        {
            // choose specific USB ID or 0 for first Device found
            int vendorID = 0;
            int productID = 0;
            // Variables to store found IDs
            int selectedVendorID;
            int selectedProductID;
            // Stringbuilder instances to store DeviceName and SerialNumber with max. 200 characters
            StringBuilder deviceName = new StringBuilder(200);
            StringBuilder serialNumber = new StringBuilder(200);

            try
            {
                if (Plathosys.Opendevice(vendorID, productID,
                    out selectedVendorID, out selectedProductID,
                    deviceName, serialNumber))
                    return true;
            }
            catch
            {
                Plathosys.Closedevice();
            }

            return false;
        }

        /// <summary>
        /// Initialize the Plathosys device to get correct hook status
        /// </summary>
        /// <returns>succesful?</returns>
        private bool InitPlathosys()
        {
            byte info1, info2, info3, info4, info5, info6, info7, info8, info9, info10;
            byte info11, info12, info13, info14, info15, info16;

            try
            {
                if (Plathosys.ReadCurrentInfodB(out info1, out info2, out info3, out info4,
                    out info5, out info6, out info7, out info8, out info9, out info10,
                    out info11, out info12, out info13, out info14, out info15, out info16))
                    return true;
            }
            catch
            {
                Plathosys.Closedevice();
            }

            hookOff = false;
            return false;
        }

        /// <summary>
        /// Read hook info
        /// </summary>
        /// <param name="hookOff"></param>
        /// <returns>succesful?</returns>
        private bool ReadHookPlathosys(out bool hookOff)
        {
            int hookAndPttInfo;

            try
            {
                // If reading the hook info succeeds
                if (Plathosys.ReadHookAndPTT(out hookAndPttInfo))
                {
                    hookOff = ((hookAndPttInfo & 1) == 1) ? true : false;
                    return true;
                }
            }
            catch
            {
                Plathosys.Closedevice();
            }

            hookOff = false;
            return false;
        }

        /// <summary>
        /// Opens connection to Plathosys device and
        /// initialize it to get correct hook status
        /// </summary>
        /// <param name="sender"></param>
        private void timerInitPlathosys_Tick(object sender)
        {
            // If opening the connection to the device succeeds
            if (OpenPlathosys())
                // If initializing the Plathosys device to get correct hook status succeeds
                if (InitPlathosys())
                {
                    // Stop this timmer
                    timerInitPlathosys.Change(Timeout.Infinite, Timeout.Infinite);
                    // Start timer to monitor hook status
                    timerHook.Change(10, 100);
                }
        }

        /// <summary>
        /// Read hook state and raise HoockChanged event
        /// </summary>
        /// <param name="sender"></param>
        private void timerHook_Tick(object sender)
        {
            bool tmpHookOff;

            // If reading Hook Info succeeds
            if (ReadHookPlathosys(out tmpHookOff))
            {
                // Raise HookChanged event according to hookInfo if hook status changed
                if (tmpHookOff != hookOff)
                {
                    hookOff = tmpHookOff;
                    OnHookChanged(hookOff);
                }
            }
            else
            {
                // Close connecion to Plathsoys device and retry
                Plathosys.Closedevice();
                timerHook.Change(Timeout.Infinite, Timeout.Infinite);
                timerInitPlathosys.Change(10, 1000);
            }
        }

        /// <summary>
        /// OnChangedHook Event
        /// </summary>
        /// <param name="hookOff"></param>
        protected virtual void OnHookChanged(bool hookOff)
        {
            // If there are subscribers to the event
            if (HookChanged != null)
                HookChanged(this, new HookEventArgs() { HookOff = hookOff });
        }
    }
}
