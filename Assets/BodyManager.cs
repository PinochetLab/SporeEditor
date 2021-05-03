using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyManager : MonoBehaviour
{
    public List<Transform> bones;
    public static BodyManager instance;
    public GameObject bonePrefab;

    void Start()
    {
        instance = this;
    }

    public static void RemoveLast()
    {
        instance.DeleteLast();
    }

    public void DeleteLast()
    {
        if (bones.Count < 3) return;
        Transform noLast = bones[bones.Count - 2];
        bones.Remove(noLast);
        Transform last = bones[bones.Count - 1];
        Transform next = bones[bones.Count - 2];
        

        Rigidbody rb = noLast.GetComponent<Rigidbody>();
        Rigidbody myRb = last.GetComponent<Rigidbody>();
        Rigidbody nextRb = next.GetComponent<Rigidbody>();
        CharacterJoint joint = noLast.GetComponent<CharacterJoint>();
        CharacterJoint myJoint = last.GetComponent<CharacterJoint>();
        CharacterJoint nextJoint = next.GetComponent<CharacterJoint>();
        if (nextJoint && nextJoint.connectedBody == rb)
        {
            Vector3 pos = noLast.position;
            Quaternion rot = noLast.localRotation;
            if (myJoint) Destroy(myJoint);
            Destroy(noLast.gameObject);
            last.position = pos;
            last.localRotation = rot;
            nextJoint.connectedBody = myRb;

        }
        else
        {
            if (joint && joint.connectedBody == nextRb)
            {
                Vector3 pos = noLast.position;
                Quaternion rot = noLast.localRotation;
                if (myJoint) Destroy(myJoint);
                Destroy(noLast.gameObject);
                last.position = pos;
                last.localRotation = rot;
                CharacterJoint newJoint = last.gameObject.AddComponent<CharacterJoint>();
                newJoint.connectedBody = nextRb;
            }
        }
    }

    public static void AddLast(float angle)
    {
        instance.AddNewLast(angle);
    }

    public void AddNewLast(float angle)
    {
      //  print("Angle is " + angle);
        if (angle > Mathf.PI / 8) angle = Mathf.PI / 6;
        else if (angle < -Mathf.PI / 8) angle = -Mathf.PI / 6;
        else angle = 0;
        
        if (bones.Count > 9) return;
        Transform last = bones[bones.Count - 1];
        Vector3 pos = last.position;
        Quaternion rot = last.localRotation;
        //  bones.Add(last);
        Vector3 dir = (Mathf.Sin(angle) * last.right + Mathf.Cos(angle) * last.up).normalized;
        GameObject newBone = Instantiate(bonePrefab, last.position + last.up * 0.43f + dir * 0.43f, rot);
        newBone.transform.up = dir;
        newBone.transform.parent = last.parent;
        Rigidbody lastRb = last.GetComponent<Rigidbody>();
        CharacterJoint joint = newBone.AddComponent<CharacterJoint>();
        joint.anchor = new Vector3(0, -1, 0);
        joint.connectedBody = lastRb;
        bones.Add(newBone.transform);
        foreach (Transform child in last)
        {
            Vector3 pos2 = child.localPosition;
            Quaternion rot2 = child.localRotation;
            child.parent = newBone.transform;
            child.localPosition = pos2;
            child.localRotation = rot2;
        }
    }
}
