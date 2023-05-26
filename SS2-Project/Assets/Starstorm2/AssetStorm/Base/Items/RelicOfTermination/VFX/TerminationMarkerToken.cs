using Moonstorm.Starstorm2;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static Moonstorm.Starstorm2.Items.RelicOfTermination;

public class TerminationMarkerToken : MonoBehaviour
{
    TerminationToken token;
    float timer = 0;
    float maxTime = 30;
    ObjectScaleCurve curve;
    AnimationCurve sizeCurve;
    GameObject ring;
    // Start is called before the first frame update
    void Start() 
    {
        
        //sizeCurve = new AnimationCurve();
        //sizeCurve.keys = new Keyframe[] {
        //        new Keyframe(0, .235f, .235f, .235f),
        //        new Keyframe(1, .07f, .07f, .07f),
        //};
        sizeCurve = AnimationCurve.Linear(0, .235f, 1, .066f);
    
        token = this.GetComponentInParent<TerminationToken>();
        if (token)
        {
            maxTime = token.timeLimit;
            timer = 0;
            var posind = this.GetComponent<PositionIndicator>();
            if (posind)
            {
                var insobj = posind.insideViewObject;
                //insobj.GetComponent
                //insobj
                //curve = insobj.GetComponent<ObjectScaleCurve>();
                if (insobj)
                {
                    ring = insobj.transform.Find("Ring").gameObject;
                    //SS2Log.Info("Found ring ");
                    if (NetworkServer.active)
                    {
                        SS2Log.Info("Found ring Server");
                    }
                    else
                    {
                        SS2Log.Info("Found ring Client");
                    }
                    //var curve = ring.GetComponent<ObjectScaleCurve>();
                    //curve.timeMax = timeLimit;
                    //timeLimit += Time.fixedDeltaTime; //cushion?
                    //SS2Log.Info("successfully setup stuff " + maxTime);
                }
                //ring = insobj.transform.Find("Ring").gameObject;
                //var curve = ring.GetComponent<ObjectScaleCurve>();
                //curve.timeMax = timeLimit;
                //timeLimit += Time.fixedDeltaTime; //cushion?
                //SS2Log.Info("successfully setup stuff " + timer);
                //sizeCurve = new AnimationCurve();
                //sizeCurve.keys = new Keyframe[] {
                //new Keyframe(0, .225f, .225f, .225f),
                //new Keyframe(1, .065f, .065f, .065f),
                ////new Keyframe(1, 1, 0.33f, 0.33f)
                //};
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //SS2Log.Info("ss2");
        if (!ring)
        {
            sizeCurve = AnimationCurve.Linear(0, .235f, 1, .066f);

                token = GetComponentInParent<TerminationToken>();
                if (token)
                {
                    maxTime = token.timeLimit;
                    timer = 0;
                var posind = GetComponent<PositionIndicator>();
                if (posind)
                {
                    var insobj = posind.insideViewObject;
                    //insobj.GetComponent
                    //insobj
                    //curve = insobj.GetComponent<ObjectScaleCurve>();
                    if (insobj)
                    {
                        ring = insobj.transform.Find("Ring").gameObject;
                        //SS2Log.Info("Found ring ");
                        if (NetworkServer.active)
                        {
                            SS2Log.Info("Found ring Server");
                        }
                        else
                        {
                            SS2Log.Info("Found ring Client");
                        }
                    }
                }
            }
        }

        //if (NetworkServer.active)
        //{
        //    SS2Log.Info("Is Active");
        //}
        //else
        //{
        //    SS2Log.Info("Isn't Active");
        //}

        float ratio = timer / maxTime;
        if(ratio < 1 && ring)
        {
            SS2Log.Info("updating ring " + ratio);
            float amount = sizeCurve.Evaluate(ratio);
            Vector3 vec = new Vector3(amount, amount, amount);
            ring.transform.localScale = vec;
            //SS2Log.Info("timer " + ratio + " | " + amount);
            timer += Time.fixedDeltaTime;
        }
        //float amount = sizeCurve.Evaluate(timer / maxTime);
        //Vector3 vec = new Vector3(amount, amount);
        //ring.transform.localScale = vec;
        //
        //timer += Time.fixedDeltaTime;
    }
}
