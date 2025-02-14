using System.Collections.Generic;
using System.Linq;
using Activations;
using UnityEngine;

public partial struct EntityAPI
{
    public static void ConnectLimbs<J>(LimbBehaviour a, LimbBehaviour b) where J : Joint2D
    {
        J joint = a.gameObject.AddComponent<J>();
        joint.connectedBody = b.GetComponent<Rigidbody2D>();
        a.HasJoint = true;
        a.ConnectedLimbs.Add(b);
        a.NodeBehaviour.Connections.Append(b.NodeBehaviour);
        a.SkinMaterialHandler.adjacentLimbs.Append(b.SkinMaterialHandler);
        a.CirculationBehaviour.PushesTo.Append(b.CirculationBehaviour);
    }
    public static void ConnectLimbs(LimbBehaviour a, LimbBehaviour b, bool createJoint = false)
    {
        HingeJoint2D joint = createJoint ? a.gameObject.AddComponent<HingeJoint2D>() : null;
        if (createJoint)
        {
            joint.connectedBody = b.GetComponent<Rigidbody2D>();
            a.HasJoint = true;
        }
        
        a.ConnectedLimbs.Add(b);
        a.NodeBehaviour.Connections.Append(b.NodeBehaviour);
        a.SkinMaterialHandler.adjacentLimbs.Append(b.SkinMaterialHandler);
        a.CirculationBehaviour.PushesTo.Append(b.CirculationBehaviour);
    }
    public static void DisconnectLimbs(LimbBehaviour a, LimbBehaviour b, bool removeJoint = true)
    {
        if (removeJoint)
        {
            Object.Destroy(a.GetComponent<Joint2D>());
            a.HasJoint = false;
        }

        var connectedLimbs = new List<LimbBehaviour>();
        var connectedNodes = new List<ConnectedNodeBehaviour>();
        var adjacentLimbs = new List<SkinMaterialHandler>();
        var pushesTo = new List<CirculationBehaviour>();

        foreach (var i in a.ConnectedLimbs)
        {
            if (i != b)
                connectedLimbs.Add(i);
        }
        foreach (var i in a.NodeBehaviour.Connections)
        {
            if (i != b.NodeBehaviour)
                connectedNodes.Add(i);
        }
        foreach (var i in a.SkinMaterialHandler.adjacentLimbs)
        {
            if (i != b.SkinMaterialHandler)
                adjacentLimbs.Add(i);
        }
        foreach (var i in a.CirculationBehaviour.PushesTo)
        {
            if (i != b.CirculationBehaviour)
                pushesTo.Add(i);
        }
        
        a.ConnectedLimbs = connectedLimbs;
        a.NodeBehaviour.Connections = connectedNodes.ToArray();
        a.SkinMaterialHandler.adjacentLimbs = adjacentLimbs.ToArray();
        a.CirculationBehaviour.PushesTo = pushesTo.ToArray();

    }

    public static void CreateEntity(string key, GameObject entity)
    {
        PersonBehaviour behaviour = entity.GetComponent<PersonBehaviour>();
    }
    public static void ModifyEntity(string key, GameObject entity, float duration = 0)
    {
        PersonBehaviour behaviour = entity.GetComponent<PersonBehaviour>();
    }
    
    private static void Load_ECModule()
    {
        ModAPI.Register<EntityCreatorBehaviour>();
    }
}