using UnityEngine;

public class VelocityVerlet
{
    public float a { get; set; }
    public float aNext { get; set; }
    public float v { get; set; }
    public float p { get; set; }

    public VelocityVerlet(float a, float aNew, float v, float p)
    {
        this.a = a;
        this.aNext = aNew;
        this.v = v;
        this.p = p;
    }

    public float getDeltaP()
    {
        return Time.deltaTime * (v + Time.deltaTime * a * 0.5f);
    }

    public void addSpeed()
    {
        /*
        https://gamedev.stackexchange.com/questions/15708/how-can-i-implement-gravity
        acceleration = force(time, position, velocity) / mass;
        time += timestep;
        position += timestep * (velocity + timestep * acceleration / 2);
        velocity += timestep * acceleration;
        newAcceleration = force(time, position, velocity) / mass;
        velocity += timestep * (newAcceleration - acceleration) / 2;
        */
        //p += getDeltaP();
        v += Time.deltaTime * a;
        v += Time.deltaTime * (aNext - a) * 0.5f;
        a = aNext;
    }
}
