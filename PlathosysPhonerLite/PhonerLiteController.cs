﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace PlathosysPhonerLite
{
    class PhonerLiteController
    {
        PhonerLite.CPhonerLite myPhonerLite;
        private int callID;

        public PhonerLiteController()
        {
            InstantiatePhonerLite();
            callID = 0;

            // Register PhonerLite event Handler
            myPhonerLite.OnCallState += new PhonerLite.ICPhonerLiteEvents_OnCallStateEventHandler(myPhonerLite_OnCallState);
        }

        /// <summary>
        /// Create Instance of myPhonerLite
        /// </summary>
        private void InstantiatePhonerLite()
        {
            int counter = 0;

            while (counter < 2)
            {
                try
                {
                    myPhonerLite = new PhonerLite.CPhonerLiteClass();
                    return;
                }
                catch
                {
                    counter++;
                    if (ActivatePhonerLiteCOM())
                        Thread.Sleep(1000);
                    else
                        counter = 2;
                }
            }

            MessageBox.Show(@"The COM interface of PhonerLite is not available. Please make sure PhonerLite is installed in the standard folder 'C:\Program Files(x86)\PhonerLite\PhonerLite.exe' and the COM interface is activated. To activate the COM interface of PhonerLite you need to run the command 'PhonerLite.exe -i' as administrator. This application will be closed.",
                "PlathosysPhonerLite - Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }

        /// <summary>
        /// Try to activate COM interface of PhonerLite
        /// </summary>
        /// <returns>Successful?</returns>
        private bool ActivatePhonerLiteCOM()
        {
            string fullPath = @"C:\Program Files (x86)\PhonerLite\PhonerLite.exe";
            if (File.Exists(fullPath) == false)
                return false;

            // Run PhonerLite.exe -i as admin
            var process = new Process();
            var startInfo = new ProcessStartInfo();
            //startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = Path.GetFileName(fullPath);
            startInfo.WorkingDirectory = Path.GetDirectoryName(fullPath);
            startInfo.Arguments = "-i";
            //process.StartInfo = startInfo;
            startInfo.UseShellExecute = true;
            startInfo.Verb = "runas";
            try
            {
                Process.Start(startInfo);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Message 4895: {ex.Message}", "PlathosysPhonerLite - Debugging Message");
                return false;
            }
        }

        /// <summary>
        /// PhonerLite event handler for changes on call state
        /// </summary>
        /// <param name="callID"></param>
        /// <param name="callState"></param>
        private void myPhonerLite_OnCallState(int callID, PhonerLite.TCallState callState)
        {
            this.callID = callID;
        }

        /// <summary>
        /// Control PhonerLite according to hookStatus 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public void OnHookChanged(object source, HookEventArgs e)
        {
            try
            {
                if (e.HookOff)
                    //myPhonerLite.HookOff(callID);
                    myPhonerLite.HookOff(0);
                else
                    //myPhonerLite.HookOn(callID);
                    myPhonerLite.HookOn(0);
            }
            catch
            {
                InstantiatePhonerLite();
            }
        }
    }
}
