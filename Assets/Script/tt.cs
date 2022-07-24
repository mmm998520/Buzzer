using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tt : MonoBehaviour
{

    float currentV = 0;
    float newV = 0;
    float a = 10f;
    float usedV;
    float p = 0;
    float pp = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    float st = 0;
    float sp = 0;
    float sv0 = 0;
    float sa = 10;

    // Update is called once per frame
    float b;
    float bb;
    void Update()
    {
        st += Time.deltaTime;
        sp = sv0 * st + 0.5f * sa * st * st;


        float preV = currentV;
        currentV = currentV + a * Time.deltaTime;
        usedV = Mathf.Max((preV + currentV) *0.5f,-20.0f);
        p += usedV * Time.deltaTime;
        pp += Time.deltaTime * (usedV + Time.deltaTime * a * 0.5f);
        print(p - sp);
        print(pp - sp);

        /*
        Verlet

        for(i = 0..n)
        {
            x_prev = x
            x += v*dt + a(x)*dt/2
            v += (a(x_prev) + a(x))*dt/2
        }
        */

        /*
        Leapfrog
        
        for(i = 0..n)
        {
            x += v*dt
            v += a(x)*dt
        }
        */
        /*
        Leapfrog變體  Best
        
        for (i = 0..n)
        {
            v += a(x) * dt / 2
            x += v * dt
            v += a(x) * dt / 2
        }
        */
        /*
        Leapfrog變體2
        
        for (i = 0..n)
        {
            v_prev = v
            v += a(x + v * dt / 2) * dt
            x += (v_prev + v) * dt / 2
        }
        */
    }
}
