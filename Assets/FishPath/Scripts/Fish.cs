using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Fish : MonoBehaviour {

    private float mCurrentLife = 0;

    private float mLastFrameLife;
    
    private float mStepTime = 0;
    
    private int mCurrentStep = 0;
    
    private int mLastFrameStep = 0;
	
    private float mSpeedScaleFactor = 1;

    [SerializeField]
    private float mSpeed = 20;
    
    private FinePoint oneFinePoint;

	private float SECOND_ONE_FRAME = 0.025f;

    private float mNotExecutedTime = 0;
    private float mLocalTime = 0;
    private int mFrameCnt = 0;

    [SerializeField]
    private FishPath mFishPath;

    FileStream fs1;

    public float Speed
    {
        get { return mSpeed; }
        set 
        { 
            mSpeed = value;
            if (mFishPath != null)
            {
                if (Application.isPlaying == false)
                {
                     mFishPath.baseSpeed = mSpeed;
                }
            }
        }
    }

    public FishPath FishPathData
    {
        get { return mFishPath; }
        set 
        { 
            mFishPath = value;
            if (mFishPath != null)
            {
                if (Application.isPlaying == false)
                {
                     mFishPath.baseSpeed = mSpeed;
                }
            }
        }
    }

    private float mUnActiveTime = 0;
    public float UnActiveTime
    {
        get { return mUnActiveTime; }
        set { mUnActiveTime = value; }
    }

    private int mFishKindId = 0;
    public int FishKindId
    {
        get { return mFishKindId; }
        set { mFishKindId = value; }
    }

    private Record_Fish mFishRecord;
    public Record_Fish FishRecord
    {
        get { return mFishRecord; }
        set { mFishRecord = value; }
    }

	// Use this for initialization
	void Start ()
    {
        oneFinePoint = new FinePoint();
        oneFinePoint.position = transform.localPosition;
        oneFinePoint.rotation = transform.eulerAngles.z;
        mFishPath.CaculateFinePoints(transform);
#if FishPath_3D
        Animation anim = GetComponent<Animation>();
        foreach (AnimationState state in anim)
        {
            if(FishKindId != 2)
                state.speed = mSpeed / 50.0f;
            state.time = Random.Range(0, 2f);
            if (state.name.Contains( "catch"))
            {
                AnimationEvent animevent = new AnimationEvent();
                animevent.functionName = "DeadCallback";
                animevent.intParameter = mFishRecord.id;
                animevent.time = state.length;
                state.clip.AddEvent(animevent);
            }
        }
#endif
	}

    public void OnDrawGizmos()
    {
        if (mFishPath && mFishPath.renderPath)
        {
            Vector2 vec2Pos = new Vector2(transform.localPosition.x, transform.localPosition.y);
            if (mFishPath.StartPosition != vec2Pos || mFishPath.FinePointList.Count == 0 || mFishPath.StartRotation != transform.eulerAngles.z)
            {
                if (Application.isPlaying == false)
                    mFishPath.CaculateFinePoints(transform);
            }
            for (int i = 0; i < mFishPath.FinePointList.Count - 1; i++)
            {
                try
                {
                    if (mFishPath.controlPoints[mFishPath.FinePointList[i].controlIndex].highLight)
                        Gizmos.color = mFishPath.controlPoints[mFishPath.FinePointList[i].controlIndex].color;
                    else
                        Gizmos.color = mFishPath.lineColour;
                    Gizmos.DrawLine(mFishPath.FinePointList[i].position * 0.002777778f, mFishPath.FinePointList[i + 1].position * 0.002777778f);
                }
                catch
                {
                    //Debug.Log("wzw");
                }

            }
        }
    }

    float GetElapsedTime(float dt)
    {
        //return dt;
        mNotExecutedTime += dt;
        return mNotExecutedTime;
    }

	// Update is called once per frame
	void Update () 
	{
        if (mFishPath == null) return;
        transform.localPosition = oneFinePoint.position;
        transform.eulerAngles = new Vector3(0, 0, oneFinePoint.rotation);
        if (mUnActiveTime > 0)
        {
            transform.Translate(Vector3.right * Time.deltaTime * mSpeed);
            mUnActiveTime -= Time.deltaTime;
            return;
        }
        float notExecutedTime = GetElapsedTime(Time.deltaTime) * 1.5f;
        if (notExecutedTime < SECOND_ONE_FRAME)
        {
            return;
        }
        
        float tempt = Mathf.FloorToInt(notExecutedTime / SECOND_ONE_FRAME) * SECOND_ONE_FRAME;
        mNotExecutedTime = notExecutedTime - tempt;
        float framedt = tempt;
		mLastFrameLife = mCurrentLife;
        mCurrentLife += tempt;
		mStepTime = 0;
        
        for (int i = 0; i < mFishPath.controlPoints.Length; i++)
		{
			mStepTime += mFishPath.controlPoints[i].mTime;
			if(mCurrentLife <= mStepTime)
			{
				mCurrentStep = i;
				break;
			}
			else
			{
				mCurrentStep = mFishPath.controlPoints.Length;
                if (i == mFishPath.controlPoints.Length - 1)
                {
                    //FishManager.GetInstance().RecycleFish(this);
                }
			}
		}

		if(mLastFrameStep != mCurrentStep)
		{
/*
            int tmpStep = mLastFrameStep;
            float t1 = mLastFrameLife;
            while (true)
            {
                tmpStep = tmpStep + 1;
                if (tmpStep > mCurrentStep)
                {
                    break;
                }
                float t2 = 0;
                for (int i = 0; i < tmpStep; i++)
                {
                    t2 += mFishPath.controlPoints[i].mTime;
                }
                float dt1 = t2 - t1;
                t1 = t2;
                int cnt1 = Mathf.FloorToInt(dt1 / SECOND_ONE_FRAME);
                for (int i = 0; i < cnt1; i++)
                {
                    CaculateTransform(tmpStep - 1, SECOND_ONE_FRAME);
                }
                CaculateTransform(tmpStep - 1, dt1 - SECOND_ONE_FRAME * cnt1);
            }
            float t3 = 0;
            for (int i = 0; i < mCurrentStep; i++)
            {
                t3 += mFishPath.controlPoints[i].mTime;
            }
            float dt2 = mCurrentLife - t3;
            int cnt2 = Mathf.FloorToInt(dt2 / SECOND_ONE_FRAME);
            for (int i = 0; i < cnt2; i++)
            {
                CaculateTransform(mCurrentStep, SECOND_ONE_FRAME);
            }
            CaculateTransform(mCurrentStep, dt2 - SECOND_ONE_FRAME * cnt2);
            mLastFrameStep = mCurrentStep;
 */
/*
            int tmpStep = mLastFrameStep;
            float t1 = mLastFrameLife;
            while (true)
            {
                tmpStep = tmpStep + 1;
                if (tmpStep > mCurrentStep)
                {
                    break;
                }
                float t2 = 0;
                for (int i = 0; i < tmpStep; i++)
                {
                    t2 += mFishPath.controlPoints[i].mTime;
                }
                float dt1 = t2 - t1;
                t1 = t2;
                int cnt1 = Mathf.FloorToInt(dt1 / SECOND_ONE_FRAME);
                for (int i = 0; i < cnt1; i++)
                {
                    CaculateTransform(tmpStep - 1, SECOND_ONE_FRAME);
                }
                CaculateTransform(tmpStep - 1, dt1 - SECOND_ONE_FRAME * cnt1);
            }
            float t3 = 0;
            for (int i = 0; i < mCurrentStep; i++)
            {
                t3 += mFishPath.controlPoints[i].mTime;
            }
            float dt2 = mCurrentLife - t3;
            int cnt2 = Mathf.FloorToInt(dt2 / SECOND_ONE_FRAME);
            for (int i = 0; i < cnt2; i++)
            {
                CaculateTransform(mCurrentStep, SECOND_ONE_FRAME);
            }
            CaculateTransform(mCurrentStep, dt2 - SECOND_ONE_FRAME * cnt2);
            mLastFrameStep = mCurrentStep;
*/

            int tmpStep = mLastFrameStep;
            float t1 = mLastFrameLife;
            while (true)
            {
                if (tmpStep > mCurrentStep)
                {
                    break;
                }
                float dt1 = 0;
                if (tmpStep == mCurrentStep)
                {
                    dt1 = mCurrentLife - t1;
                }
                else
                {
                    float t2 = 0;
                    for (int i = 0; i <= tmpStep; i++)
                    {
                        t2 += mFishPath.controlPoints[i].mTime;
                    }
                    dt1 = t2 - t1;
                    t1 = t2;
                }

                int cnt1 = Mathf.FloorToInt(dt1 / SECOND_ONE_FRAME);
                for (int i = 0; i < cnt1; i++)
                {
                    CaculateTransform(tmpStep, SECOND_ONE_FRAME);
                }
                CaculateTransform(tmpStep, dt1 - SECOND_ONE_FRAME * cnt1);
                tmpStep = tmpStep + 1;
            }
            mLastFrameStep = mCurrentStep;
 
		}
		else
		{
            int cnt1 = Mathf.FloorToInt(framedt / SECOND_ONE_FRAME);
			for(int i = 0; i < cnt1; i ++)
			{
                CaculateTransform(mCurrentStep, SECOND_ONE_FRAME);
			}
            CaculateTransform(mCurrentStep, framedt - SECOND_ONE_FRAME * cnt1);
		}        
	}

    public void CaculateTransform(int step, float dt)
    {
        if (dt <= 0)
        {
            return;
        }

        Vector2 vec2Pos = new Vector2(transform.localPosition.x, transform.localPosition.y);
        oneFinePoint = mFishPath.CaculateOneFinePoint1(vec2Pos, transform.eulerAngles.z, step, dt);
        transform.localPosition = oneFinePoint.position;
        transform.eulerAngles = new Vector3(0,0,oneFinePoint.rotation);
    }

    public void DeadCallback(int fishkindid)
    {
        this.GetComponent<Animation>().Play(mFishRecord.moveAnimationName);
    }
}
