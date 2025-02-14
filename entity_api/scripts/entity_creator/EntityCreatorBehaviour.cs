using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class EntityCreatorBehaviour : MonoBehaviour
{
    private static List<Instance> Instances = new List<Instance>();

    public static void addInstance(float duration, PersonBehaviour behaviour)
    {
        Instances.Add(new Instance(duration, behaviour));
    }
    
    private void Update()
    {
        foreach (Instance i in Instances)
        {
            i.duration -= .1f * Time.deltaTime;

            i.update(Time.deltaTime);
            
            if (i.duration <= 0)
                Instances.Remove(i);
        }
    }

    protected class Instance
    {
        public float duration;
        public float starting_duration { get; private set;}

        public void update(float delta)
        {
            
        }
        
        public Instance(float duration, PersonBehaviour behaviour)
        {
            this.duration = duration;
            starting_duration = duration;
        }
    }
}