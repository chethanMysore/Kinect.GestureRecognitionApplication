﻿
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using WindowsInput;
namespace Kinect.GestureRecognitionApplication
{
    /// <summary>
    /// Stores discrete gesture results for the GestureDetector.
    /// Properties are stored/updated for display in the UI.
    /// </summary>
    public sealed class GestureResultView : INotifyPropertyChanged
    {

        public float steerProgress = 0.0f;


        private float confidence = 0.0f;

        /// <summary> True, if the discrete gesture is currently being detected </summary>
        private bool detected = false;



        /// <summary> True, if the body is currently being tracked </summary>
        private bool isTracked = false;
        private bool right = false;
        private bool left = false;

        private bool close = false;

        /// <summary>
        /// Initializes a new instance of the GestureResultView class and sets initial property values
        /// </summary>

        /// <param name="isTracked">True, if the body is currently tracked</param>
        /// <param name="detected">True, if the gesture is currently detected for the associated body</param>
        /// <param name="confidence">Confidence value for detection of the 'Seated' gesture</param>
        public GestureResultView(bool isTracked, bool detected, float confidence, float progress, bool right, bool left, bool close)
        {

            this.IsTracked = isTracked;
            this.Detected = detected;
            this.Confidence = confidence;
            this.SteerProgress = progress;
            this.right = right;
            this.left = left;
            this.close = close;
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary> 
        /// Gets the body index associated with the current gesture detector result 
        /// </summary>

        /// <summary> 
        /// Gets a value indicating whether or not the body associated with the gesture detector is currently being tracked 
        /// </summary>
        public bool IsTracked
        {
            get
            {
                return this.isTracked;
            }

            private set
            {
                if (this.IsTracked != value)
                {
                    this.isTracked = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary> 
        /// Gets a value indicating whether or not the discrete gesture has been detected
        /// </summary>
        public bool Detected
        {
            get
            {
                return this.detected;
            }

            private set
            {
                if (this.detected != value)
                {
                    this.detected = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary> 
        /// Gets a float value which indicates the detector's confidence that the gesture is occurring for the associated body 
        /// </summary>
        public float Confidence
        {
            get
            {
                return this.confidence;
            }

            private set
            {
                if (this.confidence != value)
                {
                    this.confidence = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public bool Right
        {
            get
            {
                return this.right;
            }

            private set
            {
                if (this.right != value)
                {
                    this.right = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        public bool Close
        {
            get
            {
                return this.close;
            }

            private set
            {
                if (this.close != value)
                {
                    this.close = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        public bool Left
        {
            get
            {
                return this.left;
            }

            private set
            {
                if (this.left != value)
                {
                    this.left = value;
                    this.NotifyPropertyChanged();
                }
            }
        }


        public float SteerProgress
        {
            get
            {
                return this.steerProgress;
            }

            private set
            {
                if (this.SteerProgress != value)
                {
                    this.steerProgress = value;
                    this.NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Updates the values associated with the discrete gesture detection result
        /// </summary>
        /// <param name="isBodyTrackingIdValid">True, if the body associated with the GestureResultView object is still being tracked</param>
        /// <param name="isGestureDetected">True, if the discrete gesture is currently detected for the associated body</param>
        /// <param name="detectionConfidence">Confidence value for detection of the discrete gesture</param>
        public void UpdateGestureResult(bool isBodyTrackingIdValid, bool isGestureDetected, float detectionConfidence, float progress, bool right, bool left, bool close)
        {
            this.IsTracked = isBodyTrackingIdValid;
            this.Confidence = 0.0f;

            if (!this.IsTracked)
            {

                this.Detected = false;

                this.SteerProgress = -1.0f;
                this.Right = false;
                this.Left = false;

                this.Close = false;
            }

            else
            {
                this.Detected = isGestureDetected;
                this.SteerProgress = progress;


                if (this.Detected)
                {

                    this.Confidence = detectionConfidence;

                    this.Right = right;
                    this.Left = left;

                    this.Close = close;
                    if (this.SteerProgress >= 0.8 || this.Confidence >= 0.8)
                    {

                        if (this.Right)
                            System.Windows.Forms.SendKeys.SendWait("{RIGHT}");

                        else if (this.Left)
                            System.Windows.Forms.SendKeys.SendWait("{LEFT}");


                        System.Threading.Thread.Sleep(1500);
                    }


                }

            }
        }

        /// <summary>
        /// Notifies UI that a property has changed
        /// </summary>
        /// <param name="propertyName">Name of property that has changed</param> 
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
