using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DPFP;
using DPFP.Capture;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.IO;

using System.Threading;

namespace BiometricsNew
    {

    delegate void Function();
    public partial class frmBiometrics : Form, DPFP.Capture.EventHandler
        {
        int timetick;
        private DPFP.Capture.Capture Capturer;
        private DPFP.Verification.Verification Verificator = new DPFP.Verification.Verification ();
        private string empName, empID, dayNow;
        private int second = 0;
        private DateTime now = DateTime.Now;
        protected void Start ( )
            {
            if (null != Capturer)
                {
                try
                    {
                        Capturer.StartCapture ();
                        SetPrompt ( "Using the fingerprint reader, scan your fingerprint." );
                    }
                catch
                    {
                        SetPrompt ( "Can't initiate capture!" );
                    }
                }
            }
        protected void Stop ( )
            {
            if (null != Capturer)
                {
                try
                    {
                        Capturer.StopCapture ();
                    }
                catch
                    {
                       //MessageBox.Show ( "Capture Stop error" );
                    myMessage("Capture Stop error");

                }
                }
            }
        protected virtual void Init ( )
            {
            try
                {
                    Capturer = new DPFP.Capture.Capture ();

                if (null != Capturer)
                    Capturer.EventHandler = this;
                else
                    SetPrompt ( "Can't initiate capture operation!!" );
                }
            catch
                {
                //MessageBox.Show ( "Can't initiate capture operation", "Error", MessageBoxButtons.OK );
                myMessage("Can't initiate capture operation");
            }
            }

        public frmBiometrics ( )
            {
                InitializeComponent ();
            }

        protected virtual void Process(DPFP.Sample Sample)
            {
                DrawPicture ( ConvertSampleToBitmap ( Sample ) );
                DPFP.FeatureSet features = ExtractFeatures ( Sample, DPFP.Processing.DataPurpose.Verification );

            if (features != null)
                {

                string datenow = DateTime.Now.DayOfWeek.ToString();
                if (datenow == "Saturday" || datenow == "Sunday")
                {
                    myMessage("You cannot login during weekends");
                }
                else
                {
                    CheckOpen.cons();
                    
                    MySqlDataAdapter ada = new MySqlDataAdapter("SELECT B.FingerPrint, B.empID, E.FirstName FROM tblBiometric " +
                                                                  "AS B LEFT JOIN emp AS E ON E.empID = B.empID", msqlcon.con);
                    DataTable dt = new DataTable();
                    ada.Fill(dt);

                    foreach (DataRow dr in dt.Rows)
                    {
                        byte[] fp_byte = (byte[])dr["FingerPrint"];
                        empID = dr["empID"].ToString();
                        empName = dr["FirstName"].ToString();
                        Stream str = new MemoryStream(fp_byte);
                        DPFP.Template temp = new DPFP.Template(str);
                        DPFP.Verification.Verification.Result result = new DPFP.Verification.Verification.Result();
                        Verificator.Verify(features, temp, ref result);

                        if (result.Verified)
                        {
                            IsVerified("The fingerprint was VERIFIED.");

                            InsertAttendance(Convert.ToInt32(empID));

                            break;
                        }
                        else
                        {
                            IsNotVerified("The fingerprint was NOT VERIFIED.");
                        }
                    }
                }
            }
                    
            }

        protected void myMessage(string Mess)
        {
            this.Invoke(new Function(delegate ()
            {
                lblMessage.Visible = true;
                lblMessage.Text = Mess;
                timetick = 0;
            }
            ));
        }

        protected void disableMes()
        {
            this.Invoke(new Function(delegate ()
            {
                lblMessage.Visible = false;
                timetick = 0;
            }
            ));
        }

        public void InsertAttendance(int empID)
        {

            CheckOpen.cons();
            DataTable dt = new DataTable();
            MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM attendance WHERE empID=" + empID + " AND dateIn='" + DateTime.Now.ToString("yyyy-MM-dd") + "'", msqlcon.con);
            da.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                int goSave = 0;
                int getAttID = Convert.ToInt32(dt.Rows[0][0].ToString());
                DateTime d1;
                DateTime d2;
                string LasttimeLog = "";
                string getTapCount = dt.Rows[0][6].ToString();
                string SetTapCount = "";
                string SetAttendanceStat = "";
                string WhatLog="";
                if (getTapCount == "1")
                {
                    LasttimeLog = dt.Rows[0][3].ToString();
                    d1 = Convert.ToDateTime(LasttimeLog);
                    d2 = Convert.ToDateTime(DateTime.Now.ToString("hh:mm tt"));
                    TimeSpan ts = d1.Subtract(d2);
                    int totalMins = (int)ts.TotalMinutes;
                    if (totalMins < 0)
                    {
                        totalMins = totalMins * -1;
                    }
                    if (totalMins <= 5)
                    {
                        goSave = 2;
                    }
                    else
                    {
                        SetTapCount = "2";
                        WhatLog = "TimeOut1";
                        SetAttendanceStat = "Out";
                        goSave = 1;
                    }
                    
                }
                else if (getTapCount == "2")
                {

                    LasttimeLog = dt.Rows[0][4].ToString();
                    d1 = Convert.ToDateTime(LasttimeLog);
                    d2 = Convert.ToDateTime(DateTime.Now.ToString("hh:mm tt"));
                    TimeSpan ts = d1.Subtract(d2);
                    int totalMins = (int)ts.TotalMinutes;
                    if (totalMins < 0)
                    {
                        totalMins = totalMins * -1;
                    }
                    if (totalMins <= 5)
                    {
                        goSave = 2;
                    }
                    else
                    {
                        SetTapCount = "3";
                        WhatLog = "TimeIn2";
                        SetAttendanceStat = "In";
                        goSave = 1;
                    }
                    
                }
                else if (getTapCount == "3")
                {
                    LasttimeLog = dt.Rows[0][7].ToString();
                    d1 = Convert.ToDateTime(LasttimeLog);
                    d2 = Convert.ToDateTime(DateTime.Now.ToString("hh:mm tt"));
                    TimeSpan ts = d1.Subtract(d2);
                    int totalMins = (int)ts.TotalMinutes;
                    if (totalMins < 0)
                    {
                        totalMins = totalMins * -1;
                    }
                    if (totalMins <= 5)
                    {
                        goSave = 2;
                    }
                    else
                    {
                        SetTapCount = "4";
                        WhatLog = "TimeOut2";
                        SetAttendanceStat = "Out";
                        goSave = 1;
                    }
                }
                if (goSave == 1)
                {
                   
                    MySqlCommand cmd = new MySqlCommand("UPDATE attendance SET " + WhatLog + " = '" + DateTime.Now.ToString("hh:mm tt") + "', TapCount='" + SetTapCount + "', AttendanceStat='" + SetAttendanceStat + "' WHERE attendanceID=" + getAttID, msqlcon.con);
                    cmd.ExecuteNonQuery();
                   
                    myMessage("Successfully Logged " + SetAttendanceStat);
                  
                   
                }
                else if (goSave == 2)
                {
              
                    myMessage("Already Logged, Thank you!");
                }
                else
                {
                  
                    myMessage("Already did last Log");
               
                }
            }
            else
            {
                MySqlCommand cmd = new MySqlCommand("INSERT INTO attendance(Day, DateIn, TimeIn1, EmpID, TapCount, AttendanceStat) VALUES('" + DateTime.Now.DayOfWeek + "','" + DateTime.Now.ToString("yyyy-MM-dd") + "','" + DateTime.Now.ToString("hh:mm tt") + "'," + empID + ",'1','In')", msqlcon.con);
                cmd.ExecuteNonQuery();
                myMessage("Successfully Logged In, Thank you!");
                
            }
            
        }
        
        protected Bitmap ConvertSampleToBitmap (DPFP.Sample Sample)
            {
                DPFP.Capture.SampleConversion Convertor = new DPFP.Capture.SampleConversion ();
                Bitmap bitmap = null;
                Convertor.ConvertToPicture ( Sample, ref bitmap );
                return bitmap;
            }
        protected DPFP.FeatureSet ExtractFeatures (DPFP.Sample Sample, DPFP.Processing.DataPurpose Purpose)
            {
                DPFP.Processing.FeatureExtraction Extractor = new DPFP.Processing.FeatureExtraction ();
                DPFP.Capture.CaptureFeedback feedback = DPFP.Capture.CaptureFeedback.None;
                DPFP.FeatureSet features = new DPFP.FeatureSet ();
                Extractor.CreateFeatureSet ( Sample, Purpose, ref feedback, ref features );

                if (feedback == DPFP.Capture.CaptureFeedback.Good)
                    {
                        return features;
                    }
                else
                    {
                        return null;
                    }
            }
        public void OnComplete (object Capture, string ReaderSerialNumber, Sample Sample)
            {
            try
            {
                MakeReport("The fingerprint sample was captured.");
                SetPrompt("Scan the same fingerprint again.");
                Process(Sample);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void OnFingerGone (object Capture, string ReaderSerialNumber)
            {
                FingerGone_ ( "The fingerprint reader was gone." );
            }

        public void OnFingerTouch (object Capture, string ReaderSerialNumber)
            {
                MakeReport ( "The fingerprint reader was touched." );
            }

        public void OnReaderConnect (object Capture, string ReaderSerialNumber)
            {
                MakeReport ( "The fingerprint reader was connected." );
            }

        public void OnReaderDisconnect (object Capture, string ReaderSerialNumber)
            {
                MakeReport ( "The fingerprint reader was disconnected." );
            }

        public void OnSampleQuality (object Capture, string ReaderSerialNumber, CaptureFeedback CaptureFeedback)
            {
                if (CaptureFeedback == DPFP.Capture.CaptureFeedback.Good)
                    {
                        MakeReport ( "The quality of the fingerprint sample is good." );
                    }
                else
                    {
                        MakeReport ( "The quality of the fingerprint sample is poor." );
                    }
            }

        protected void MakeReport (string message)
            {
            this.Invoke ( new Function ( delegate ( )
                {
                    txtStatus.AppendText ( message + "\r\n" );
                }));
            }

        protected void IsVerified (string _ver)
            {
            this.Invoke ( new Function ( delegate ( )
                {
                    picFinger.Image = BiometricsNew.Properties.Resources.valid_fingerprint_512;
                    txtStatus.AppendText ( _ver + "\r\n" );
                    btnVer.Visible = true;
                    btnVer.Text = "FINGERPRINT VERIFIED";
                    btnVer.BackColor = Color.Green;
                    lblID.Text = empID;
                    lblName.Text = empName;
                    lblID.Visible = true;
                    lblName.Visible = true;
                }));
            }

        protected void IsNotVerified (string _ver)
            {
            this.Invoke ( new Function ( delegate ( )
                {
                    picFinger.Image = BiometricsNew.Properties.Resources.wrong_fingerprint_512;
                    txtStatus.AppendText ( _ver + "\r\n" );
                    lblName.Text = "";
                    btnVer.Visible = true;
                    btnVer.Text = "FINGERPRINT UNVERIFIED";
                    btnVer.BackColor = Color.DarkRed;
                }));
            }

        protected void FingerGone_ (string Fgone_)
            {
            this.Invoke ( new Function ( delegate ( )
                {
                    picFinger.Image = BiometricsNew.Properties.Resources.fingerprint_512;
                    txtStatus.AppendText ( Fgone_ + "\r\n" );
                    btnVer.Visible = false;
                    lblName.Visible = false;
                    lblID.Visible = false;
                }));
            }

        public void SetPrompt (string prompt)
            {
            this.Invoke ( new Function ( delegate ( )
                {
                    txtMakeReport.Text = prompt;
                }));
            }

        private void DrawPicture (Bitmap bitmap)
            {
            this.Invoke ( new Function ( delegate ( )
                {
                    picVer.Image = new Bitmap ( bitmap, picFinger.Size );
                }));
            }

        private void tmr_Tick(object sender, EventArgs e)
        {
            timetick = timetick + 1;
            if (timetick > 2)
            {
                //tmr.Enabled = false;
                disableMes();
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void TimerLoad_Tick (object sender, EventArgs e)
            {
                lblTime.Text = DateTime.Now.ToString ( "hh:mm tt" );
                second = second + 1;

                if (second >= 59)
                    {
                        TimerLoad.Stop ();
                    }
                TimerLoad.Start ();
            }

        protected virtual void frmBiometrics_Load (object sender, EventArgs e)
            {
            CheckOpen.cons();
            lblDate.Text = DateTime.Now.ToString ( "yyyy-MM-dd" );
                dayNow = now.DayOfWeek.ToString ();
                Init ();
                Start ();
                TimerLoad.Start ();
            tmr.Start();
            }
        }
    }
