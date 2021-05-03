using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpineArrowScript : MonoBehaviour
{
    public Material notHoverMat;
    public Material hoverMat;

    public MeshRenderer mesh;
    public Transform startArrow;
    private bool isDown = false;
    private Vector2 startPos;

    void OnMouseEnter()
    {
        mesh.material = hoverMat;
    }

    void OnMouseDown()
    {
        isDown = true;
        startPos = Input.mousePosition;
    }

    
    void OnMouseExit()
    {
        if (!isDown) mesh.material = notHoverMat;
    }
    

    void Update()
    {
        transform.position = startArrow.position;
        if (isDown)
        {
            Vector2 nowPos = Input.mousePosition;
            float dist = (nowPos - startPos).magnitude;

            if (!Input.GetKey(KeyCode.Mouse0) || dist > 200)
            {
                isDown = false;
                mesh.material = notHoverMat;
            }
            float x = Vector3.Dot(-startArrow.forward, (Vector3)(nowPos - startPos));
            float y = Vector3.Cross(-startArrow.forward, ((Vector3)(nowPos - startPos)).normalized).z;
           // print(x);
            if (Mathf.Abs(x) > 100)
            {
                

                if (x > 0)
                {
                    BodyManager.RemoveLast();
                }
                else
                {
                    float angle = Mathf.Atan2(x, y) / 5f;
                    BodyManager.AddLast(angle);
                }
                isDown = false;
                mesh.material = notHoverMat;
            }

        }
    }
}
