using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using Newtonsoft.Json;
using Sacknet.KinectFacialRecognition;
using Sacknet.KinectFacialRecognition.KinectFaceModel;
using Sacknet.KinectFacialRecognition.ManagedEigenObject;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Kinect.GestureRecognitionApplication
{
    public partial class MainWindow : Window
    {
        
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        KinectSensor sensor;
        /// <summary>
        /// Reader for body frames
        /// </summary>
        BodyFrameReader bodyFrameReader;
        /// <summary>
        /// Array for the bodies
        /// </summary>
        /// <summary> List of gesture detectors, there will be one detector created for each potential body (max of 6) </summary>
        private List<GestureDetector> gestureDetectorList = null;

        private Body[] bodies = null;
        /// <summary>
        /// Screen width and height for determining the exact mouse sensitivity
        /// </summary>
        int screenWidth, screenHeight;

        /// <summary>
        /// timer for pause-to-click feature
        /// </summary>
        DispatcherTimer timer = new DispatcherTimer();

        /// <summary>
        /// How far the cursor move according to your hand's movement
        /// </summary>
        public float mouseSensitivity = MOUSE_SENSITIVITY;

        /// <summary>
        /// Time required as a pause-clicking
        /// </summary>
        public float timeRequired = TIME_REQUIRED;
        /// <summary>
        /// The radius range your hand move inside a circle for [timeRequired] seconds would be regarded as a pause-clicking
        /// </summary>
        public float pauseThresold = PAUSE_THRESOLD;
        /// <summary>
        /// Decide if the user need to do clicks or only move the cursor
        /// </summary>
        public static bool doClick = DO_CLICK;
        /// <summary>
        /// Use Grip gesture to click or not
        /// </summary>
       /// public bool useGripGesture = USE_GRIP_GESTURE;
        /// <summary>
        /// Value 0 - 0.95f, the larger it is, the smoother the cursor would move
        /// </summary>
        public float cursorSmoothing = CURSOR_SMOOTHING;

        // Default values
        public const float MOUSE_SENSITIVITY = 3.5f;
        public const float TIME_REQUIRED = 2f;
        public const float PAUSE_THRESOLD = 60f;
        public const bool DO_CLICK = true;
        public const float CURSOR_SMOOTHING = 0.2f;

        /// <summary>
        /// Determine if we have tracked the hand and used it to move the cursor,
        /// If false, meaning the user may not lift their hands, we don't get the last hand position and some actions like pause-to-click won't be executed.
        /// </summary>
        bool alreadyTrackedPos = false;

        /// <summary>
        /// for storing the time passed for pause-to-click
        /// </summary>
        float timeCount = 0;
        /// <summary>
        /// For storing last cursor position
        /// </summary>
        System.Windows.Point lastCurPos = new System.Windows.Point(0, 0);
    
        /// <summary>
        /// If true, user did a right hand Grip gesture
        /// </summary>
        bool wasRightGrip = false;
        private bool takeTrainingImage = false;
        private KinectFacialRecognitionEngine engine;
        private bool FaceDetected = false;
        private Sacknet.KinectFacialRecognition.IRecognitionProcessor activeProcessor;
        private MainWindowViewModel viewModel = new MainWindowViewModel();
        public MainWindow()
        {

            // get Active Kinect Sensor
            this.sensor = KinectSensor.GetDefault();

            // open the sensor
            this.sensor.Open();
            this.DataContext = this.viewModel;
            this.viewModel.TrainName = "Face 1";
            this.viewModel.ProcessorType = ProcessorTypes.PCA;
            this.viewModel.PropertyChanged += this.ViewModelPropertyChanged;
            this.viewModel.TrainButtonClicked = new ActionCommand(this.Train);
            this.viewModel.TrainNameEnabled = true;
            // open the reader for the body frames
            this.bodyFrameReader = sensor.BodyFrameSource.OpenReader();
            this.bodyFrameReader.FrameArrived += bodyFrameReader_FrameArrived;

            // initialize the gesture detection objects for our gestures
            this.gestureDetectorList = new List<GestureDetector>();

            //this.InitializeComponent();
            this.LoadProcessor();
            // get screen with and height
            screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            screenHeight = (int)SystemParameters.PrimaryScreenHeight;

            // set up timer, execute every 0.1s
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();

            int maxBodies = this.sensor.BodyFrameSource.BodyCount;
            for (int i = 0; i < maxBodies; ++i)
            {
                GestureResultView result = new GestureResultView( false, false, 0.0f,-1.0f,false,false,false);
                GestureDetector detector = new GestureDetector(this.sensor, result);
                this.gestureDetectorList.Add(detector);

            }

        }
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);
        /// <summary>
        /// Pause to click timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Timer_Tick(object sender, EventArgs e)
        {
            if (!doClick ) return;

            if (!alreadyTrackedPos)
            {
                timeCount = 0;
                return;
            }

            System.Windows.Point curPos = MouseControl.GetCursorPosition();
            foreach (Body body in this.bodies)
            {
                if ((lastCurPos - curPos).Length < pauseThresold)
                {
                    if ((timeCount += 0.1f) > timeRequired)
                    {
 
                        if (body.HandRightState == HandState.Lasso )
                        {


                            MouseControl.DoMouseClick1();


                        }
                       
                        else if (body.HandRightState == HandState.Open && body.HandLeftState == HandState.Open)
                        {
                            MouseControl.DoPause();
                        }
                      else if (body.HandRightState == HandState.Open && body.HandLeftState == HandState.Closed)
                        {
                            MouseControl.DoMouseClick();
                            MouseControl.DoMouseClick();
                        }
                        else if (body.HandRightState == HandState.Open && body.HandLeftState == HandState.Lasso)
                        {
                          
                            MouseControl.DoMouseClick();
                        }
                     
                        timeCount = 0;
                    }

                }

                else
                {
                    timeCount = 0;
                }

                lastCurPos = curPos;
            }
        }
        private static BitmapSource LoadBitmap(Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                bs.Freeze();
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }
        private void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ProcessorType":
                    this.LoadProcessor();
                    break;
            }
        }
        private void LoadProcessor()
        {
            if (this.viewModel.ProcessorType == ProcessorTypes.FaceModel)
                activeProcessor = new FaceModelRecognitionProcessor();
            else
                activeProcessor = new EigenObjectRecognitionProcessor();

            this.LoadAllTargetFaces();
            this.UpdateTargetFaces();

            if (this.engine == null)
            {
                this.engine = new KinectFacialRecognitionEngine(this.sensor, this.activeProcessor);
                this.engine.RecognitionComplete += this.Engine_RecognitionComplete;
            }

            this.engine.Processors = new List<IRecognitionProcessor> { this.activeProcessor };
        }
        private void Engine_RecognitionComplete(object sender, RecognitionResult e)
        {
            TrackedFace face = null;

            if (e.Faces != null)
                face = e.Faces.FirstOrDefault();

            using (var processedBitmap = (Bitmap)e.ColorSpaceBitmap.Clone())
            {
                if (face == null)
                {
                    this.viewModel.ReadyForTraining = false;
                }
                else
                {
                    using (var g = Graphics.FromImage(processedBitmap))
                    {
                        var isFmb = this.viewModel.ProcessorType == ProcessorTypes.FaceModel;
                        var rect = face.TrackingResult.FaceRect;
                        var faceOutlineColor = System.Drawing.Color.Green;

                        if (isFmb)
                        {
                            if (face.TrackingResult.ConstructedFaceModel == null)
                            {
                                faceOutlineColor = System.Drawing.Color.Red;

                                if (face.TrackingResult.BuilderStatus == FaceModelBuilderCollectionStatus.Complete)
                                    faceOutlineColor = System.Drawing.Color.Orange;
                            }

                            var scale = (rect.Width + rect.Height) / 6;
                            var midX = rect.X + (rect.Width / 2);
                            var midY = rect.Y + (rect.Height / 2);

                            if ((face.TrackingResult.BuilderStatus & FaceModelBuilderCollectionStatus.LeftViewsNeeded) == FaceModelBuilderCollectionStatus.LeftViewsNeeded)
                                g.FillRectangle(new SolidBrush(System.Drawing.Color.Red), rect.X - (scale * 2), midY, scale, scale);

                            if ((face.TrackingResult.BuilderStatus & FaceModelBuilderCollectionStatus.RightViewsNeeded) == FaceModelBuilderCollectionStatus.RightViewsNeeded)
                                g.FillRectangle(new SolidBrush(System.Drawing.Color.Red), rect.X + rect.Width + (scale * 2), midY, scale, scale);

                            if ((face.TrackingResult.BuilderStatus & FaceModelBuilderCollectionStatus.TiltedUpViewsNeeded) == FaceModelBuilderCollectionStatus.TiltedUpViewsNeeded)
                                g.FillRectangle(new SolidBrush(System.Drawing.Color.Red), midX, rect.Y - (scale * 2), scale, scale);

                            if ((face.TrackingResult.BuilderStatus & FaceModelBuilderCollectionStatus.FrontViewFramesNeeded) == FaceModelBuilderCollectionStatus.FrontViewFramesNeeded)
                                g.FillRectangle(new SolidBrush(System.Drawing.Color.Red), midX, midY, scale, scale);
                        }

                        this.viewModel.ReadyForTraining = faceOutlineColor == System.Drawing.Color.Green;

                        g.DrawPath(new System.Drawing.Pen(faceOutlineColor, 5), face.TrackingResult.GetFacePath());

                        if (!string.IsNullOrEmpty(face.Key))
                        {
                            
                            var score = Math.Round(face.ProcessorResults.First().Score, 2);

                            // Write the key on the image...
                            g.DrawString(face.Key + ": " + score, new Font("Arial", 100), System.Drawing.Brushes.Red, new System.Drawing.Point(rect.Left, rect.Top - 25));
                            if(face.Key=="Karunesh")
                            FaceDetected = true;
                            else
                                FaceDetected = false;
                        }
                    }

                    if (this.takeTrainingImage)
                    {
                        var eoResult = (EigenObjectRecognitionProcessorResult)face.ProcessorResults.SingleOrDefault(x => x is EigenObjectRecognitionProcessorResult);
                        var fmResult = (FaceModelRecognitionProcessorResult)face.ProcessorResults.SingleOrDefault(x => x is FaceModelRecognitionProcessorResult);

                        var bstf = new BitmapSourceTargetFace();
                        bstf.Key = this.viewModel.TrainName;

                        if (eoResult != null)
                        {
                            bstf.Image = (Bitmap)eoResult.Image.Clone();
                        }
                        else
                        {
                            bstf.Image = face.TrackingResult.GetCroppedFace(e.ColorSpaceBitmap);
                        }

                        if (fmResult != null)
                        {
                            bstf.Deformations = fmResult.Deformations;
                            bstf.HairColor = fmResult.HairColor;
                            bstf.SkinColor = fmResult.SkinColor;
                        }

                        this.viewModel.TargetFaces.Add(bstf);

                        this.SerializeBitmapSourceTargetFace(bstf);

                        this.takeTrainingImage = false;

                        this.UpdateTargetFaces();
                    }
                }

                this.viewModel.CurrentVideoFrame = LoadBitmap(processedBitmap);
            }

            // Without an explicit call to GC.Collect here, memory runs out of control :(
            GC.Collect();
        }
        private void SerializeBitmapSourceTargetFace(BitmapSourceTargetFace bstf)
        {
            var filenamePrefix = "TF_" + DateTime.Now.Ticks.ToString();
            var suffix = this.viewModel.ProcessorType == ProcessorTypes.FaceModel ? ".fmb" : ".pca";
            System.IO.File.WriteAllText(filenamePrefix + suffix, JsonConvert.SerializeObject(bstf));
            bstf.Image.Save(filenamePrefix + ".png");
        }
        private void LoadAllTargetFaces()
        {
            this.viewModel.TargetFaces.Clear();
            var result = new List<BitmapSourceTargetFace>();
            var suffix = this.viewModel.ProcessorType == ProcessorTypes.FaceModel ? ".fmb" : ".pca";

            foreach (var file in Directory.GetFiles(".", "TF_*" + suffix))
            {
                var bstf = JsonConvert.DeserializeObject<BitmapSourceTargetFace>(File.ReadAllText(file));
                bstf.Image = (Bitmap)Bitmap.FromFile(file.Replace(suffix, ".png"));
                this.viewModel.TargetFaces.Add(bstf);
            }
          
        }
        /// <summary>
        /// Updates the target faces
        /// </summary>
        private void UpdateTargetFaces()
        {
            if (this.viewModel.TargetFaces.Count > 1)
                this.activeProcessor.SetTargetFaces(this.viewModel.TargetFaces);

            this.viewModel.TrainName = this.viewModel.TrainName.Replace(this.viewModel.TargetFaces.Count.ToString(), (this.viewModel.TargetFaces.Count + 1).ToString());
        }

        /// <summary>
        /// Starts the training image countdown
        /// </summary>
        private void Train()
        {
            this.viewModel.TrainingInProcess = true;

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += (s2, e2) =>
            {
                timer.Stop();
                this.viewModel.TrainingInProcess = false;
                takeTrainingImage = true;
            };
            timer.Start();
        }
        
        /// <summary>
        /// Read body frames
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void bodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (!dataReceived)
            {
                alreadyTrackedPos = false;
                return;
            }

            if (this.bodies != null)
            {
                // loop through all bodies to see if any of the gesture detectors need to be updated
                int maxBodies = this.sensor.BodyFrameSource.BodyCount;
                for (int i = 0; i < maxBodies; ++i)
                {
                    Body body = this.bodies[i];
                    ulong trackingId = body.TrackingId;

                    // if the current body TrackingId changed, update the corresponding gesture detector with the new value
                    if (trackingId != this.gestureDetectorList[i].TrackingId)
                    {
                        this.gestureDetectorList[i].TrackingId = trackingId;

                        // if the current body is tracked, unpause its detector to get VisualGestureBuilderFrameArrived events
                        // if the current body is not tracked, pause its detector so we don't waste resources trying to get invalid gesture results
                        this.gestureDetectorList[i].IsPaused = trackingId == 0;
                    }
                }
            }
            foreach (Body body in this.bodies)
            {

                // get first tracked body only, notice there's a break below.
                if (body.IsTracked && FaceDetected)
                {
                    // get various skeletal positions
                    CameraSpacePoint handLeft = body.Joints[JointType.HandLeft].Position;
                    CameraSpacePoint handRight = body.Joints[JointType.HandRight].Position;
                    CameraSpacePoint spineBase = body.Joints[JointType.SpineBase].Position;
                    if (handRight.Z - spineBase.Z < -0.15f)
                    {
                         if ((handRight.Y - spineBase.Y >= 0.9) && (body.HandRightState == HandState.Lasso))
                        {
                           
                            MouseControl.Vol_Up();
                      
                        }
                          else if ((handRight.Y - spineBase.Y <= 0.3) && (body.HandRightState == HandState.Lasso))
                         {
                             MouseControl.Vol_Down();
                             
                         }
                         else if( (handRight.X - handLeft.X <= 0.25f) && (body.HandRightState == HandState.Open && body.HandLeftState == HandState.Open) )
                         {
                            
                             
                                 MouseControl.DoScroll();
                             
                         }
                         else if ((handRight.X - handLeft.X <= 0.25f) && (body.HandRightState == HandState.Lasso && body.HandLeftState == HandState.Lasso))
                         {


                             MouseControl.DoClose();

                         }
                         

                       if (handRight.Z - spineBase.Z < -0.15f) // if right hand lift up
                        {
                            /* hand x calculated by this. we don't use shoulder right as a reference cause the shoulder right
                             * is usually behind the lift right hand, and the position would be inferred and unstable.
                             * because the spine base is on the left of right hand, we plus 0.05f to make it closer to the right. */
                            float x = handRight.X - spineBase.X + 0.05f;
                            /* hand y calculated by this. ss spine base is way lower than right hand, we plus 0.51f to make it
                             * higer, the value 0.51f is worked out by testing for a several times, you can set it as another one you like. */
                            float y = spineBase.Y - handRight.Y + 0.51f;
                            // get current cursor position
                            System.Windows.Point curPos = MouseControl.GetCursorPosition();
                            // smoothing for using should be 0 - 0.95f. The way we smooth the cusor is: oldPos + (newPos - oldPos) * smoothValue
                            float smoothing = 1 - cursorSmoothing;
                            // set cursor position
                            MouseControl.SetCursorPos((int)(curPos.X + (x * mouseSensitivity * screenWidth - curPos.X) * smoothing), (int)(curPos.Y + ((y + 0.25f) * mouseSensitivity * screenHeight - curPos.Y) * smoothing));

                            alreadyTrackedPos = true;

                            // Grip gesture
                           if (doClick)
                            {
                                if (body.HandRightState == HandState.Closed)
                                {
                                    if (!wasRightGrip)
                                    {
                                        MouseControl.MouseLeftDown();
                                        wasRightGrip = true;
                                    }
                                }

                                else if (body.HandRightState == HandState.Open)
                                {
                                    if (wasRightGrip)
                                    {
                                        MouseControl.MouseLeftUp();
                                        wasRightGrip = false;
                                    }
                                }
                            }
                        }
                    }
                  
                    else
                    {
                       
                        wasRightGrip = true;
                        alreadyTrackedPos = false;
                    }

                    // get first tracked body only
                    break;
                }
            }
        }




        /*private void MouseSensitivity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MouseSensitivity.IsLoaded)
            {
                mouseSensitivity = (float)MouseSensitivity.Value;
                txtMouseSensitivity.Text = mouseSensitivity.ToString("f2");

                Properties.Settings.Default.MouseSensitivity = mouseSensitivity;
                Properties.Settings.Default.Save();
            }
        }

        private void PauseToClickTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PauseToClickTime.IsLoaded)
            {
                timeRequired = (float)PauseToClickTime.Value;
                txtTimeRequired.Text = timeRequired.ToString("f2");

                Properties.Settings.Default.PauseToClickTime = timeRequired;
                Properties.Settings.Default.Save();
            }
        }

        private void txtMouseSensitivity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                float v;
                if (float.TryParse(txtMouseSensitivity.Text, out v))
   {
                    MouseSensitivity.Value = v;
                    mouseSensitivity = (float)MouseSensitivity.Value;
                }
            }
        }

        private void txtTimeRequired_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                float v;
                if (float.TryParse(txtTimeRequired.Text, out v))
                {
                    PauseToClickTime.Value = v;
                    timeRequired = (float)PauseToClickTime.Value;
                }
            }
        }*/

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
/*MouseSensitivity.Value = Properties.Settings.Default.MouseSensitivity;
            PauseToClickTime.Value = Properties.Settings.Default.PauseToClickTime;
            PauseThresold.Value = Properties.Settings.Default.PauseThresold;
            chkNoClick.IsChecked = !Properties.Settings.Default.DoClick;
            CursorSmoothing.Value = Properties.Settings.Default.CursorSmoothing;
            
*/
        }

      /*  private void PauseThresold_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PauseThresold.IsLoaded)
            {
                pauseThresold = (float)PauseThresold.Value;
                txtPauseThresold.Text = pauseThresold.ToString("f2");

                Properties.Settings.Default.PauseThresold = pauseThresold;
                Properties.Settings.Default.Save();
            }
        }

        private void txtPauseThresold_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                float v;
                if (float.TryParse(txtPauseThresold.Text, out v))
                {
                    PauseThresold.Value = v;
                    timeRequired = (float)PauseThresold.Value;
                }
            }
        }

        private void btnDefault_Click(object sender, RoutedEventArgs e)
        {
            MouseSensitivity.Value = MainWindow.MOUSE_SENSITIVITY;
            PauseToClickTime.Value = MainWindow.TIME_REQUIRED;
            PauseThresold.Value = MainWindow.PAUSE_THRESOLD;
            CursorSmoothing.Value = MainWindow.CURSOR_SMOOTHING;

            chkNoClick.IsChecked = !MainWindow.DO_CLICK;
           
        }

        private void chkNoClick_Checked(object sender, RoutedEventArgs e)
        {
            chkNoClickChange();
        }


        public void chkNoClickChange()
        {
            doClick = !chkNoClick.IsChecked.Value;
            Properties.Settings.Default.DoClick = doClick;
            Properties.Settings.Default.Save();
        }

        private void chkNoClick_Unchecked(object sender, RoutedEventArgs e)
        {
            chkNoClickChange();
        }*/

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            if (timer != null)
            {
                timer.Stop();
                timer = null;
            }

            if (this.bodyFrameReader != null)
            {
                // BodyFrameReader is IDisposable
                this.bodyFrameReader.FrameArrived -= this.bodyFrameReader_FrameArrived;
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.gestureDetectorList != null)
            {
                // The GestureDetector contains disposable members (VisualGestureBuilderFrameSource and VisualGestureBuilderFrameReader)
                foreach (GestureDetector detector in this.gestureDetectorList)
                {
                    detector.Dispose();
                }

                this.gestureDetectorList.Clear();
                this.gestureDetectorList = null;
            }

            if (this.sensor != null)
            {
                this.sensor.Close();
                this.sensor = null;
            }
        }

        

        /*private void CursorSmoothing_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CursorSmoothing.IsLoaded)
            {
                cursorSmoothing = (float)CursorSmoothing.Value;
                txtCursorSmoothing.Text = cursorSmoothing.ToString("f2");

                Properties.Settings.Default.CursorSmoothing = cursorSmoothing;
                Properties.Settings.Default.Save();
            }
        }*/

        /*private void PCA_Checked(object sender, RoutedEventArgs e)
        {
            pca.Checked = true;
        }*/

        [JsonObject(MemberSerialization.OptIn)]
        public class BitmapSourceTargetFace : IEigenObjectTargetFace, IFaceModelTargetFace
        {
            private BitmapSource bitmapSource;

            /// <summary>
            /// Gets the BitmapSource version of the face
            /// </summary>
            public BitmapSource BitmapSource
            {
                get
                {
                    if (this.bitmapSource == null)
                        this.bitmapSource = MainWindow.LoadBitmap(this.Image);

                    return this.bitmapSource;
                }
            }

            /// <summary>
            /// Gets or sets the key returned when this face is found
            /// </summary>
            [JsonProperty]
            public string Key { get; set; }

            /// <summary>
            /// Gets or sets the grayscale, 100x100 target image
            /// </summary>
            public Bitmap Image { get; set; }

            /// <summary>
            /// Gets or sets the detected hair color of the face
            /// </summary>
            [JsonProperty]
            public System.Drawing.Color HairColor { get; set; }

            /// <summary>
            /// Gets or sets the detected skin color of the face
            /// </summary>
            [JsonProperty]
            public System.Drawing.Color SkinColor { get; set; }

            /// <summary>
            /// Gets or sets the detected deformations of the face
            /// </summary>
            [JsonProperty]
            public IReadOnlyDictionary<FaceShapeDeformations, float> Deformations { get; set; }
        }
    }


}
