using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyManager : MonoBehaviour
{
    public List<Transform> bones;
    List<Rigidbody> rbs = new List<Rigidbody>();
    public static BodyManager instance;
    public GameObject bonePrefab;

    void Start()
    {
        SetKinematic(true);
        instance = this;
        for (int i = 0; i < bones.Count; i++)
        {
            Rigidbody rb = bones[i].GetComponent<Rigidbody>();
            if (rb) rbs.Add(rb);
        }
    }

    private void Update()
    {
      //  ZeroSpeeds();
    }

    public void SetKinematic(bool isKin)
    {
        foreach (Transform child in transform)
        {
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = isKin;
        }
    }
    public static void RemoveLast()
    {
        instance.DeleteLast();
    }

    public static void RemoveFirst()
    {
        instance.DeleteFirst();
    }

    public void DeleteLast()
    {
        SetKinematic(false);
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
        SetKinematic(true);
    }

    public void DeleteFirst()
    {
        SetKinematic(false);
        if (bones.Count < 3) return;
        Transform first = bones[0];
        Transform second = bones[1];
        Vector3 pos = second.position;
        Quaternion rot = second.rotation;

        CharacterJoint[] thirdJoints = bones[2].GetComponents<CharacterJoint>();
        foreach(CharacterJoint joint in thirdJoints)
        {
            if (joint.connectedBody == second.GetComponent<Rigidbody>())
            {
                Destroy(joint);
            }
        }

        CharacterJoint[] secondJoints = second.GetComponents<CharacterJoint>();
        foreach (CharacterJoint joint in secondJoints)
        {
            if (joint.connectedBody == bones[2].GetComponent<Rigidbody>() || joint.connectedBody == first.GetComponent<Rigidbody>())
            {
                Destroy(joint);
            }
        }

        CharacterJoint[] firstJoints = first.GetComponents<CharacterJoint>();
        foreach (CharacterJoint joint in firstJoints)
        {
            if (joint.connectedBody == second.GetComponent<Rigidbody>())
            {
                Destroy(joint);
            }
        }

        bones.Remove(second);
        Destroy(second.gameObject);
        first.position = pos;
        first.rotation = rot;
        CharacterJoint newJoint = first.gameObject.AddComponent<CharacterJoint>();
        newJoint.connectedBody = bones[1].GetComponent<Rigidbody>();
        SetKinematic(true);
    }

    public static void AddLast(float angle)
    {
        instance.AddNewLast(angle);
    }

    public static void AddFirst(float angle)
    {
        instance.AddNewFirst(angle);
    }

    public void AddNewLast(float angle)
    {
        SetKinematic(false);
        //  print("Angle is " + angle);
        if (angle > Mathf.PI / 8) angle = Mathf.PI / 6;
        else if (angle < -Mathf.PI / 8) angle = -Mathf.PI / 6;
        else angle = 0;
        
        if (bones.Count > 5) return;
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
        SetKinematic(true);
    }

    public void AddNewFirst(float angle)
    {
        SetKinematic(false);
        //  print("Angle is " + angle);
        if (angle > Mathf.PI / 8) angle = Mathf.PI / 6;
        else if (angle < -Mathf.PI / 8) angle = -Mathf.PI / 6;
        else angle = 0;

        if (bones.Count > 5) return;
        Transform first = bones[0];
        Vector3 pos = first.position;
        Quaternion rot = first.localRotation;
        //  bones.Add(last);
        Vector3 dir = (Mathf.Sin(angle) * first.right + Mathf.Cos(angle) * first.up).normalized;
        GameObject newBone = Instantiate(bonePrefab, first.position - first.up * 0.43f - dir * 0.43f, rot, first.parent);
        newBone.transform.up = dir;
        Rigidbody firstRb = first.GetComponent<Rigidbody>();
        CharacterJoint joint = newBone.AddComponent<CharacterJoint>();
        joint.anchor = new Vector3(0, 1, 0);
        joint.connectedBody = firstRb;
        bones.Add(newBone.transform);

        for (int i = bones.Count - 1; i >= 1; i--)
        {
            bones[i] = bones[i - 1];
        }

        bones[0] = newBone.transform;

        for (int i = 0; i < bones.Count; i++)
        {
            print("i " + i + " ; bone is " + bones[i].gameObject.name);
        }

        foreach (Transform child in first)
        {
            Vector3 pos2 = child.localPosition;
            Quaternion rot2 = child.localRotation;
            child.parent = newBone.transform;
            child.localPosition = pos2;
            child.localRotation = rot2;
        }
        SetKinematic(true);
    }
}
