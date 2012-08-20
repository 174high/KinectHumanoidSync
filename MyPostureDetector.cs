using System;
using Kinect.Toolbox;
public class MyPostureDetector : AlgorithmicPostureDetector
{
	public MyPostureDetector()
	{
        this = AlgorithmicPostureDetector();
	}

    public event Action<string> PostureDetected(){

    }

}
