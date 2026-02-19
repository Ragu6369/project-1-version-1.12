using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    //----------------------------WayPoint Values-----------------------------------------
    [SerializeField] private WayPoint wayPoint;

    private int _targetWayPointIndex;

    private Transform _previousWayPoint;
    private Transform _targetWayPoint;

    private float _timeToWayPoint;
    private float _elapsedTime;
    // public float t = 0f;

    //---------------------------PLatform_Move_Values-------------------------------------
    [SerializeField] private float _speed;
    
    // Start is called before the first frame update
    void Start()
    {
        
        TargetNextWayPoint();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        _elapsedTime += Time.deltaTime;
        float distance = Vector3.Distance(transform.position, _targetWayPoint.position);
        float elapsedPercentage = _elapsedTime / _timeToWayPoint;
        elapsedPercentage = Mathf.SmoothStep(0,1,elapsedPercentage);
        /* Notes:
         * Lerp can get only 0 to 1 as 3rd argument npot speed so here 0 means start ,0.5 means mid, 1 means end
         * so we have to perform something like this below here we take a value with Time.fixedDelatTime or Time.deltaTime and divide by distance between current and target point for  platform 
         * and the value should be incremented else the lerp won't as 0 means start point if value is like 0.04 the lerp won't move so we have to increment 
         * and divide by distance is important without it still value will be more and if possible implement Clamp or SmoothStep to restrict within 0f to 1f
        */
        //t += (0.05f/Time.fixedDeltaTime)/distance ; 
        // transform.position = Vector3.Lerp(_previousWayPoint.position, _targetWayPoint.position,t);
        transform.position = Vector3.Lerp(_previousWayPoint.position, _targetWayPoint.position,elapsedPercentage * (_speed +1 ));
        transform.rotation = Quaternion.Lerp(_previousWayPoint.rotation, _targetWayPoint.rotation, elapsedPercentage);

        // here if the distance between the platform and targetpoint is ver close then call the platform changing
        // don't check if transform.position == targetWayPoint.position as it will have some .05 or lesser or more difference  it won't work 
        if(distance <= 0.05f)
        {
           TargetNextWayPoint();
            // t = 0f; Reset t to avoid overspeeding 
        }

        /* 
        // here it will call the function every 3 seconds to chnage platform but not good for longer distance 
        
        if (_elapsedTime >= 3)
        {
            TargetNextWayPoint();
        }

        */
    }

    private void TargetNextWayPoint()
    {
        _previousWayPoint = wayPoint.GetWayPoint(_targetWayPointIndex);
        _targetWayPointIndex = wayPoint.GetNextWayPointIndex(_targetWayPointIndex);
        _targetWayPoint = wayPoint.GetWayPoint(_targetWayPointIndex);

        _elapsedTime = 0;

        float distanceToWayPoint = Vector3.Distance(_previousWayPoint.position, _targetWayPoint.position);
        _timeToWayPoint = distanceToWayPoint / _speed;
    }
   
}
