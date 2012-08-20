//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
using Kinect.Toolbox;
using System;

namespace Microsoft.Samples.Kinect.SkeletonBasics
{
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using Microsoft.Kinect;
    

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        
        /// <summary>
        /// Width of output drawing
        /// </summary>
        private const float RenderWidth = 640.0f;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private const float RenderHeight = 480.0f;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of body center ellipse
        /// </summary>
        private const double BodyCenterThickness = 10;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Brush used to draw skeleton center point
        /// </summary>
        private readonly Brush centerPointBrush = Brushes.Blue;

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently tracked
        /// </summary>
        private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Drawing group for skeleton rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;

        

        //communicating over tcp
        private System.Net.Sockets.TcpClient tcp;

        private System.Net.Sockets.NetworkStream stream;

        private System.IO.StreamWriter easyWriter;

        private System.IO.StreamReader easyReader;

        private string ipAddress = "10.0.0.22";

        private int port = 8003;

        //////

        private int prevServo4 = 7500;
        private int prevServo2 = 7500;
        private int prevServo3 = 7500;
        private int prevServo5 = 7500;
        /// //////////////
        /// 
        float hipXR;
        float hipYR;
        float hipZR;
        float handXR;
        float handYR;
        float handZR;
        float shXR;
        float shYR;
        float shZR;

        float hipXL;
        float hipYL;
        float hipZL;
        float handXL;
        float handYL;
        float handZL;
        float shXL;
        float shYL;
        float shZL;
        /// 


       

        private System.Threading.Thread socketthread;

        private string mot = "lol";//keep track of which motion to play

        private string prevMot = "M01";
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Draws indicators to show which edges are clipping skeleton data
        /// </summary>
        /// <param name="skeleton">skeleton to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private static void RenderClippedEdges(Skeleton skeleton, DrawingContext drawingContext)
        {
            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, RenderHeight - ClipBoundsThickness, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, RenderWidth, ClipBoundsThickness));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, RenderHeight));
            }

            if (skeleton.ClippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(RenderWidth - ClipBoundsThickness, 0, ClipBoundsThickness, RenderHeight));
            }
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            Image.Source = this.imageSource;

            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                TransformSmoothParameters parameters = new TransformSmoothParameters();
                parameters.Smoothing = 0.2f;
                parameters.Correction = 0.8f;
                parameters.Prediction = 0.0f;
                parameters.JitterRadius = 0.5f;
                parameters.MaxDeviationRadius = 0.5f;
                // Turn on the skeleton stream to receive skeleton frames
                this.sensor.SkeletonStream.Enable(parameters);

                // Add an event handler to be called whenever there is new color frame data
                this.sensor.SkeletonFrameReady += this.SensorSkeletonFrameReady;

                

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                                   }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }

            if (null == this.sensor)
            {
                this.statusBarText.Text = Properties.Resources.NoKinectReady;
            }
            
            

            try
            {
                tcp = new System.Net.Sockets.TcpClient(ipAddress, port);
                tcp.NoDelay = false;
                tcp.ReceiveTimeout = 7000;
                //tcp.NoDelay = false;
                stream = tcp.GetStream();
                easyWriter = new System.IO.StreamWriter(stream);
                easyWriter.AutoFlush = true;
               // easyWriter.Write("M00\n");
                
                easyReader = new System.IO.StreamReader(stream);
                
            }
            catch (System.ArgumentNullException ee)
            {
                System.Console.WriteLine("ArgumentNullException: {0}", ee);
            }
            catch (System.Net.Sockets.SocketException ee)
            {
                System.Console.WriteLine("SocketException: {0}", ee);
            }

            
    
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            {
                this.sensor.Stop();
            }
            try
            {
                tcp.Close();
            }
            catch (System.Exception ee)
            {
                System.Console.WriteLine("Error stopping the socket ", ee);
            }

        }
        /*
         * My Own Functions Below
         * */
     

        private int servo2(bool writeToSocket = false)
        {
            //Calculate Side 1
            float temp1 = System.Math.Abs(handZR - hipZR);
            float temp2 = System.Math.Abs(handYR - hipYR);
            double side1 = System.Math.Sqrt(temp1 * temp1 + temp2 * temp2);
            //Calculate Side 2
            temp1 = System.Math.Abs(shYR - hipYR);
            temp2 = System.Math.Abs(shZR - hipZR);
            double side2 = System.Math.Sqrt(temp1 * temp1 + temp2 * temp2);
            //Calculate Side 3
            temp1 = System.Math.Abs(shYR - handYR);
            temp2 = System.Math.Abs(shZR - handZR);
            double side3 = System.Math.Sqrt(temp1 * temp1 + temp2 * temp2);
            //Calculate the angle we want
            double angleRightArm = System.Math.Acos((side2 * side2 + side3 * side3 - side1 * side1) / (2 * side2 * side3)) * 180;
            //Determine if the right had is in front of the body. If so, we need to change the scaling function to get correct front-to-back motion
            bool rightHandInFront = (handZR < hipZR) || (handYR > shYR);//if hands closer to camera than hip OR hand above shoulder
            //if the hand is "out front" scale it into the front range, otherwise scale it into the back range. 
            //note: if hands above head, scale it as in front!!!
            /* scaling from [min-max] range to the [a-b] range
                     (b-a)(x - min)
               f(x) = --------------  + a
                       max - min            */
            int scaledAngleRightArm;
            //The scaling function is split to handle the hands in front of the body and behind, since the hand-shoulder-hip angle does not determine this
            if (rightHandInFront)
            {
                scaledAngleRightArm = (int)((((12500 - 7500) * (angleRightArm - 0)) /
                                                 (450 - 0)) + 7500);
            }
            else
            {
                scaledAngleRightArm = (int)((((5500 - 7500) * (angleRightArm - 0)) /
                                                      (450 - 0)) + 7500);
            }
            //Check to prevent moving more than N clicks away from the previous(current) servo position
           /* if (Math.Abs(scaledAngleRightArm - prevServo2) > 300)
            {
                if (scaledAngleRightArm > prevServo2) { scaledAngleRightArm = prevServo2 + 300; }
                else { scaledAngleRightArm = prevServo2 - 300; }
            }*/
            //Check if the position to move to is within range
            if (scaledAngleRightArm > 5500 && scaledAngleRightArm < 12500)
            {   
                prevServo2 = scaledAngleRightArm;
                if (writeToSocket) { easyWriter.WriteLine("S02 " + scaledAngleRightArm); }
                System.Console.WriteLine(scaledAngleRightArm);
                return scaledAngleRightArm;
            }
            return 0;//return 0 if out of range
        }

        private int servo3(bool writeToSocket = false)
        {
            //Calculate Side 1
            float temp1 = System.Math.Abs(handZL - hipZL);
            float temp2 = System.Math.Abs(handYL - hipYL);
            double side1 = System.Math.Sqrt(temp1 * temp1 + temp2 * temp2);
            //Calculate Side 2
            temp1 = System.Math.Abs(shYL - hipYL);
            temp2 = System.Math.Abs(shZL - hipZL);
            double side2 = System.Math.Sqrt(temp1 * temp1 + temp2 * temp2);
            //Calculate Side 3
            temp1 = System.Math.Abs(shYL - handYL);
            temp2 = System.Math.Abs(shZL - handZL);
            double side3 = System.Math.Sqrt(temp1 * temp1 + temp2 * temp2);
            //Calculate the angle we want
            double angleLeftArm = System.Math.Acos((side2 * side2 + side3 * side3 - side1 * side1) / (2 * side2 * side3)) * 180;
            //check if the hand is in front you the body
            bool leftHandInfront = (handZL < hipZL) || (handYL > shYL);//if hands closer to camera than hip OR hand above shoulder
            /*   The scaling function to get from what the kinect spits out to the servo's safe range
             * scaling from [min-max] range to the [a-b] range
                     (b-a)(x - min)
               f(x) = --------------  + a
                       max - min
             */
            int scaledAngleLeftArm;
            //The scaling function is split in half, one for if the hand is in front and another if the hand is behind you.
            if (leftHandInfront)
            {
                scaledAngleLeftArm = (int)((((2500 - 7500) * (angleLeftArm - 0)) /
                                              (450 - 0)) + 7500);
            }
            else
            {
                scaledAngleLeftArm = (int)((((9500 - 7500) * (angleLeftArm - 0)) /
                                            (450 - 0)) + 7500);
            }
            if (scaledAngleLeftArm > 2500 && scaledAngleLeftArm < 9500)//if it is within the safe range of motion for the servo
            {
                if (writeToSocket) { easyWriter.WriteLine("S03 " + scaledAngleLeftArm); }
                System.Console.WriteLine(scaledAngleLeftArm);
                return scaledAngleLeftArm;
            }
            return 0; 
        }

        private int servo4(bool writeToSocket = false)
        {
            //Calculate Side 1
            float temp1 = System.Math.Abs(shXR - hipXR);
            float temp2 = System.Math.Abs(shYR - hipYR);
            double side1 = System.Math.Sqrt(temp1 * temp1 + temp2 * temp2);
            //Calculate Side 2
            temp1 = System.Math.Abs(handXR - hipXR);
            temp2 = System.Math.Abs(handYR - hipYR);
            double side2 = System.Math.Sqrt(temp1 * temp1 + temp2 * temp2);
            //Calculate Side 3
            temp1 = System.Math.Abs(shXR - handXR);
            temp2 = System.Math.Abs(shYR - handYR);
            double side3 = System.Math.Sqrt(temp1 * temp1 + temp2 * temp2);
            //Calculate the angle we want
            double angle2 = System.Math.Acos((side1 * side1 + side3 * side3 - side2 * side2) / (2 * side1 * side3)) * 180;
            //The full range of motion is 7500 to 12300. 
            //Right now I'm going to HALVE that.
            int scaledAngle4 = (int)((((9900 - 7500) * (angle2 - 60)) /
                                       (450 - 60)) + 7500);
            //Restriction to keep the servo from jumping more than N clicks away from its current position
             /* if (Math.Abs(scaledAngle4 - prevServo4) > 500)//I just picked this number to be grater than - Can be played with
               {
                   if (scaledAngle4 > prevServo4) { scaledAngle4 = prevServo4 + 500; }
                   else { scaledAngle4 = prevServo4 - 500; }
               }*/
            //Protections to ensure motor stays within SAFE RANGE -- THIS IS THE ONLY SAFTY 
            //Then Write the safe value to the socket IF flagged to
            if (scaledAngle4 < 9000 && scaledAngle4 > 7500)
            {
                prevServo4 = scaledAngle4;
                if (writeToSocket) { easyWriter.WriteLine("S04 " + scaledAngle4); }
                //System.Console.WriteLine(scaledAngle4); 
                return scaledAngle4;
            }
            return 0;
        }

        private int servo5(bool writeToSocket = false)
        {
           //Calculate Side 1
           float temp1 = System.Math.Abs(shXL - hipXL);
           float temp2 = System.Math.Abs(shYL - hipYL);
           double side1 = System.Math.Sqrt(temp1 * temp1 + temp2 * temp2);
           //Calculate Side 2
           temp1 = System.Math.Abs(handXL - hipXL);
           temp2 = System.Math.Abs(handYL - hipYL);
           double side2 = System.Math.Sqrt(temp1 * temp1 + temp2 * temp2);
           //Calculate Side 3
           temp1 = System.Math.Abs(shXL - handXL);
           temp2 = System.Math.Abs(shYL - handYL);
           double side3 = System.Math.Sqrt(temp1 * temp1 + temp2 * temp2);
           //Calculate the angle we're interested in
           double angle2 = System.Math.Acos((side1 * side1 + side3 * side3 - side2 * side2) / (2 * side1 * side3)) * 180;
           //Scale the angle to the range of the servo motion
            //The full range is from 7500 to 2700. 
            //Right not I'm going HALVE that. 
           int scaledAngle5 = (int)((((5100 - 7500) * (angle2 - 60)) /
                                        (450 - 60)) + 7500);
            if (scaledAngle5 > 6000 && scaledAngle5 < 7500)
            {
                if (writeToSocket) { easyWriter.WriteLine("S05 " + scaledAngle5); }
                System.Console.WriteLine(scaledAngle5);
                return scaledAngle5;
                }
            return 0;//return 0 if the value is out of range
        }
        private void returnRobotHome() { easyWriter.WriteLine("M01"); }


        /// <summary>
        /// Event handler for Kinect sensor's SkeletonFrameReady event
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {   
            Skeleton[] skeletons = new Skeleton[0];

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                // Draw a transparent background to set the render size
                dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

                if (skeletons.Length != 0)
                {
                    foreach (Skeleton skel in skeletons)
                    {
                        //lets do something here

                        hipXR = skel.Joints[JointType.HipRight].Position.X;
                        hipYR = skel.Joints[JointType.HipRight].Position.Y;
                        hipZR = skel.Joints[JointType.HipRight].Position.Z;
                        handXR = skel.Joints[JointType.HandRight].Position.X;
                        handYR = skel.Joints[JointType.HandRight].Position.Y;
                        handZR = skel.Joints[JointType.HandRight].Position.Z;
                        shXR = skel.Joints[JointType.ShoulderRight].Position.X;
                        shYR = skel.Joints[JointType.ShoulderRight].Position.Y;
                        shZR = skel.Joints[JointType.ShoulderRight].Position.Z;

                        hipXL = skel.Joints[JointType.HipLeft].Position.X;
                        hipYL = skel.Joints[JointType.HipLeft].Position.Y;
                        hipZL = skel.Joints[JointType.HipLeft].Position.Z;
                        handXL = skel.Joints[JointType.HandLeft].Position.X;
                        handYL = skel.Joints[JointType.HandLeft].Position.Y;
                        handZL = skel.Joints[JointType.HandLeft].Position.Z;
                        shXL = skel.Joints[JointType.ShoulderLeft].Position.X;
                        shYL = skel.Joints[JointType.ShoulderLeft].Position.Y;
                        shZL = skel.Joints[JointType.ShoulderLeft].Position.Z;
                        ///////////////////////////////////

                       // servo4();
                       // servo5();//bool "true" to write to socket

                        servo3(true);
                        servo2(true);
                        double smallDistance = Math.Abs(hipXL-hipXR);
                        if (Math.Abs(handYL - shYL) > smallDistance) { servo5(true); }
                        if (Math.Abs(handYR - shYR) > smallDistance) { servo4(true); }




                        if (skel.Joints[JointType.FootRight].Position.Y > skel.Joints[JointType.KneeLeft].Position.Y)
                        {
                            returnRobotHome();
                            this.Close();
                        }















                        ////////////////end user code//////////////////////////
                        RenderClippedEdges(skel, dc);

                        if (skel.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.DrawBonesAndJoints(skel, dc);
                        }
                        else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                        {
                            dc.DrawEllipse(
                            this.centerPointBrush,
                            null,
                            this.SkeletonPointToScreen(skel.Position),
                            BodyCenterThickness,
                            BodyCenterThickness);
                        }
                    }
                }

                // prevent drawing outside of our render area
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
            }
        }

        /// <summary>
        /// Draws a skeleton's bones and joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
            // Render Torso
            this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

            // Left Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
            this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

            // Right Arm
            this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
            this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
            this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

            // Left Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
            this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

            // Right Leg
            this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
            this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
            this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);
 
            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;

                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;                    
                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;                    
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
                }
            }
        }

        /// <summary>
        /// Maps a SkeletonPoint to lie within our render space and converts to Point
        /// </summary>
        /// <param name="skelpoint">point to map</param>
        /// <returns>mapped point</returns>
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            // We are not using depth directly, but we do want the points in our 640x480 output resolution.
            DepthImagePoint depthPoint = this.sensor.MapSkeletonPointToDepth(
                                                                             skelpoint,
                                                                             DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
        }

        /// <summary>
        /// Draws a bone line between two joints
        /// </summary>
        /// <param name="skeleton">skeleton to draw bones from</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="jointType0">joint to start drawing from</param>
        /// <param name="jointType1">joint to end drawing at</param>
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {
            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            {
                drawPen = this.trackedBonePen;
            }

            drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
        }

        /// <summary>
        /// Handles the checking or unchecking of the seated mode combo box
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void CheckBoxSeatedModeChanged(object sender, RoutedEventArgs e)
        {
            if (null != this.sensor)
            {
                if (this.checkBoxSeatedMode.IsChecked.GetValueOrDefault())
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                }
                else
                {
                    this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
                }
            }
        }
    }
}