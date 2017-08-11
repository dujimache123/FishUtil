using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

//通过控制点插值来的点，路径上的点
public class FinePoint
{
    public Vector2 position = Vector2.zero;
    public float rotation = 0;
    public int controlIndex = 0;
}

public class FishPath : ScriptableObject
{
	//基准速度
	[SerializeField]
	public float mBaseSpeed = 0;

	[SerializeField]
    public FishPathControlPoint[] mControlPoints = new FishPathControlPoint[0];

	[SerializeField]
    public bool mRenderPath = false;

    [SerializeField]
    public string mPathFileName = "Untitled";

    [SerializeField]
    public int mPathId = 0;

    public string FileName
    {
        get { return mPathFileName; }
        set { mPathFileName = value; }
    }

	public bool renderPath
	{
		get{return mRenderPath;}
		set{mRenderPath = value;}
	}

	private List<FinePoint> mFinePointsList = new List<FinePoint>();

	//路径的颜色
	public Color lineColour = Color.white;

	private float mCurrentLife;
	private float mLastFrameLife;
	private float mStepTime;
	private int mCurrentStep = 0;
	private int mLastFrameStep = 0;
	private float SECOND_ONE_FRAME = 0.025f;

	private Vector2 rotatedVec=Vector2.right;
	private Vector2 mStartPosition;
	private float mStartRotation;
	private FinePoint mCurrentFinePoint = new FinePoint();

    public Vector2 StartPosition
    {
        get { return mStartPosition; }
        set { 
            mStartPosition = value; 
        }
    }

    public float StartRotation
    {
        get { return mStartRotation; }
        set { mStartRotation = value; }
    }

    public List<FinePoint> FinePointList
    {
        get { return mFinePointsList; }
    }

	public int numberOfControlPoints
	{
		get
		{
			if (mControlPoints != null)
				return mControlPoints.Length;
			else
				return 0;
		}
	}

	//get the array of control points
	public FishPathControlPoint[] controlPoints
	{
		get { return mControlPoints; }
	}

	public float baseSpeed
	{
		set{mBaseSpeed = value;}
		get{return mBaseSpeed;}
	}

    public FishPath()
    { 
    }

	public void AddPoint(int index)
	{
		List<FishPathControlPoint> tempList = new List<FishPathControlPoint>(mControlPoints);
        FishPathControlPoint newpoint = ScriptableObject.CreateInstance<FishPathControlPoint>();
		tempList.Insert(index,newpoint);
		mControlPoints = tempList.ToArray();
		this.CaculateFinePoints();
	}

	public void AddPoint()
	{
		AddPoint(mControlPoints.Length);
	}

    public void AddPoint(FishPathControlPoint fpcp)
    {
        List<FishPathControlPoint> tempList = new List<FishPathControlPoint>(mControlPoints);
        tempList.Add(fpcp);
        mControlPoints = tempList.ToArray();
        this.CaculateFinePoints();
    }

	public void DeletePoint(int index)
	{
		List<FishPathControlPoint> tempList = new List<FishPathControlPoint>(mControlPoints);
		tempList.RemoveAt(index);
		mControlPoints = tempList.ToArray();
		this.CaculateFinePoints();
	}

	public void ResetPath()
	{
		mControlPoints = new FishPathControlPoint[0];
		mFinePointsList.Clear();
	}

	public float GetTotalTime()
	{
		float time = 0;
		for(int i = 0; i < numberOfControlPoints; i ++)
		{
			time += controlPoints[i].mTime;
		}
		return time;
	}

	public void CaculateFinePoints(Transform trans=null)
	{
        if (Application.isEditor == false) return;
        if (trans)
        {
            Vector2 vec2Pos = new Vector2(trans.localPosition.x, trans.localPosition.y);
            StartPosition = vec2Pos;
            StartRotation = trans.eulerAngles.z;
        }
        
        mFinePointsList.Clear();
		float time = 0;
		mCurrentFinePoint.position = mStartPosition;
		mCurrentFinePoint.rotation = mStartRotation;
        mCurrentFinePoint.controlIndex = 0;
        mFinePointsList.Add(mCurrentFinePoint);
		rotatedVec = Vector2.right;
		mCurrentLife = 0;
		mLastFrameLife = 0;
        mLastFrameStep = mCurrentStep = 0;
		float totaltime = GetTotalTime();
		

		while(time < totaltime)
		{
			time += SECOND_ONE_FRAME;
			mLastFrameLife = mCurrentLife;
			mCurrentLife += SECOND_ONE_FRAME;
			mStepTime = 0;
			for(int i = 0; i < mControlPoints.Length; i ++)
			{
				mStepTime += mControlPoints[i].mTime;
				if(mCurrentLife <= mStepTime)
				{
					mCurrentStep = i;
					break;
				}
				else
				{
					mCurrentStep = mControlPoints.Length;
				}
			}
			
			if(mLastFrameStep != mCurrentStep)
			{
				int tmpStep = mLastFrameStep;
				float t1 = mLastFrameLife;
				while(true)
				{
					tmpStep = tmpStep + 1;
					if(tmpStep > mCurrentStep)
					{
						break;
					}
					float t2 = 0;
					for(int i = 0; i < tmpStep; i ++)
					{
						t2 += mControlPoints[i].mTime;
					}
					float dt1 = t2 - t1;
					t1 = t2;
					int cnt1 = Mathf.FloorToInt(dt1 / SECOND_ONE_FRAME);
					for(int i = 0; i < cnt1; i ++)
					{
						mCurrentFinePoint = CaculateOneFinePoint(mCurrentFinePoint.position,mCurrentFinePoint.rotation,tmpStep-1,SECOND_ONE_FRAME,false);
						mFinePointsList.Add(mCurrentFinePoint);
					}
					mCurrentFinePoint = CaculateOneFinePoint(mCurrentFinePoint.position,mCurrentFinePoint.rotation,tmpStep-1,dt1-SECOND_ONE_FRAME*cnt1,false);
					mFinePointsList.Add(mCurrentFinePoint);

				}
				float t3 = 0;
				for(int i = 0; i < mCurrentStep; i ++)
				{
					t3 += mControlPoints[i].mTime;
				}
				float dt2 = mCurrentLife - t3;
				int cnt2 = Mathf.FloorToInt(dt2 / SECOND_ONE_FRAME);
				for(int i = 0; i < cnt2; i ++)
				{
					mCurrentFinePoint = CaculateOneFinePoint(mCurrentFinePoint.position,mCurrentFinePoint.rotation,mCurrentStep,SECOND_ONE_FRAME,false);
					mFinePointsList.Add(mCurrentFinePoint);
				}
				mCurrentFinePoint = CaculateOneFinePoint(mCurrentFinePoint.position,mCurrentFinePoint.rotation,mCurrentStep,dt2 - SECOND_ONE_FRAME*cnt2,false);
				mFinePointsList.Add(mCurrentFinePoint);
				mLastFrameStep = mCurrentStep;
			}
			else
			{
				mCurrentFinePoint = CaculateOneFinePoint(mCurrentFinePoint.position,mCurrentFinePoint.rotation,mCurrentStep,SECOND_ONE_FRAME,false);
				mFinePointsList.Add(mCurrentFinePoint);
			}
		}

        //string resultFile = Application.dataPath + "/FishPath/Resources/PathConfig/test.txt";
        //FileStream fs1 = new FileStream(resultFile, FileMode.Create, FileAccess.Write);
        //StreamWriter sw = new StreamWriter(fs1);
        //int index = 0;
        //foreach (FinePoint p in mFinePointsList)
        //{
        //    string str = index + "    " +  p.position.ToString() + "   " + p.rotation.ToString();
        //    sw.WriteLine(str);
        //    index++;
        //}

        //sw.Flush();
        //fs1.Close();
	}
	
	public FinePoint CaculateOneFinePoint(Vector2 startPosition,float startRotation,int step,float dt,bool log=true)
	{
		FinePoint point = new FinePoint();
        point.position = startPosition;
        point.rotation = startRotation;
        point.controlIndex = step;
		if(step < 0 || dt <= 0) return point;

		if(step >= 0 && step < mControlPoints.Length)
		{
            float rDelta = dt * mControlPoints[step].mRotationChange / mControlPoints[step].mTime;
            startRotation -= rDelta;
            
		}
		step = Mathf.Min(step,mControlPoints.Length-1);
		rotatedVec = MathUtil.GetInstance().Rotate(Vector2.right,startRotation);
		Vector2 dL = dt * mBaseSpeed * rotatedVec * mControlPoints[step].mSpeedScale;
		
		startPosition +=dL;
		point.position = startPosition;
		point.rotation = startRotation;
		point.controlIndex = step;
		return point;
	}

    public FinePoint CaculateOneFinePoint1(Vector2 startPosition, float startRotation, int step, float dt, bool log = true)
    {
        FinePoint point = new FinePoint();
        point.position = startPosition;
        point.rotation = startRotation;
        point.controlIndex = step;
        if (step < 0 || dt <= 0) return point;

        if (step >= 0 && step < mControlPoints.Length)
        {
            float rDelta = dt * mControlPoints[step].mRotationChange / mControlPoints[step].mTime;
            startRotation -= rDelta;

        }
        step = Mathf.Min(step, mControlPoints.Length - 1);
        rotatedVec = MathUtil.GetInstance().Rotate(Vector2.right, startRotation);
        Vector2 dL = dt * mBaseSpeed * rotatedVec * mControlPoints[step].mSpeedScale;

        startPosition += dL;
        point.position = startPosition;
        point.rotation = startRotation;
        point.controlIndex = step;
        //if (Application.isPlaying && log)
            //Debug.Log(startPosition.ToString() + "   " + string.Format("{0:.000}", (startRotation - 360)));
        return point;
    }
}
