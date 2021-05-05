using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GenerateMesh : MonoBehaviour
{
    public int onSection = 5;
    public int onCircle = 6;
    public float radius = 1f;
    public Material material;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;
    public Transform sphere;

    void Start()
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
        meshRenderer.material = material;
     //   Draw();
    }

    float BeginPower(int i)
    {

        int n = onSection;
        return 1f / (n - 1) / (n - 1) * Mathf.Pow(((float)i - (float)n + 1), 2);
    }

    float EndPower(int i)
    {
        return BeginPower(onSection - i - 1);
    }

    float CirclePower(int i)
    {
        float res = 0;
        if (i <= (int)((float)onCircle / 2f)) return ((float)i - (float)onCircle / 4f) / ((float)onCircle / 4f);
        else return ((float)onCircle - (float)i - (float)onCircle / 4f) / ((float)onCircle / 4f);
    }


    void Update()
    {
        Draw();
    }

    void Draw()
    {
        mesh.Clear();
        List<Vector3> points = new List<Vector3>();
        List<Transform> bones = BodyManager.instance.bones;
        for (int i = 0; i < bones.Count; i++)
        {
            if (i == 0)
            {
                points.Add(bones[i].GetComponent<Bone>().point1.position);
                points.Add(bones[i].position);
            }else if (i == bones.Count - 1)
            {
                points.Add(bones[i].position);
                points.Add(bones[i].GetComponent<Bone>().point2.position);
            }
            else
            {
                points.Add(bones[i].position);
            }
        }
        int n = 0;
        for (int i = 0; i < points.Count - 1; i++)
        {
            n += onSection;
        }
        int s = (n - 1) * onCircle * 2 * 3;
        n *= onCircle;

      //  n += 2 * onCircle * onSection + 2;
      //  s += (2 * onCircle * (onSection - 1) + onCircle) * 3 * 2;

        Vector3[] vertices = new Vector3[n + 2 * onCircle * onSection + 2];
        Vector2[] uv = new Vector2[n + 2 * onCircle * onSection + 2];
        int[] triangles = new int[s + (2 * onCircle * (onSection - 1) + onCircle) * 3 * 2 + 2 * 2 * 3 * onCircle];

        List<int> tris = new List<int>();

        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < n; i++)
        {
            float angle = 2 * Mathf.PI * ((float)(i % onCircle) / (float)onCircle);
            //   print(Mathf.Sin(angle));
            int section = i / (onSection * onCircle);

            float prevSin = 0;
            Vector3 curveVecPrev = Vector3.zero;
            if (section > 0)
            {
                prevSin = Mathf.Cos(Mathf.Acos(Mathf.Abs(Vector3.Dot((points[section] - points[section - 1]).normalized,
                ((points[section] - points[section + 1]).normalized)))) / 2f);
                curveVecPrev = ((points[section] - points[section - 1]).normalized +
                    (points[section] - points[section + 1]).normalized).normalized;
            }

            float nextSin = 0;
            Vector3 curveVecNext = Vector3.zero;
            if (section < points.Count - 2)
            {
                nextSin = Mathf.Cos(Mathf.Acos(Mathf.Abs(Vector3.Dot((points[section + 1] - points[section]).normalized,
                ((points[section + 1] - points[section + 2]).normalized)))) / 2f);
                curveVecNext = ((points[section + 1] - points[section]).normalized +
                    (points[section + 1] - points[section + 2]).normalized).normalized;
            }

            

            int inSection = ((i % (onSection * onCircle)) / onCircle);
            Vector3 start = points[section] + (points[section + 1] - points[section]) * (float)(1 + inSection) / (1 + onSection);
            Vector3 snakeDir = (points[section + 1] - points[section]).normalized;
            Vector3 e1 = new Vector3(0, 0, 1);
            Vector3 e2 = Vector3.Cross(snakeDir, e1).normalized;

            Vector3 toPoint = (e1 * Mathf.Cos(angle) + e2 * Mathf.Sin(angle)).normalized;
            float circlePowerPrev = 0; 
            if (Vector3.Cross( snakeDir, curveVecPrev).magnitude != 0) circlePowerPrev = Vector3.Dot(snakeDir, curveVecPrev) * 
                    Vector3.Dot(toPoint, curveVecPrev);
            float circlePowerNext = 0;
            if (Vector3.Cross(-snakeDir, curveVecNext).magnitude != 0) circlePowerNext = Vector3.Dot(-snakeDir, curveVecNext) * 
                    Vector3.Dot(toPoint, curveVecNext);
            // print(e2.x);
            //   Vector3 pos = start + (toPoint * radius - (-snakeDir * Mathf.Sqrt(circlePowerPrev * BeginPower(inSection))
            //      + snakeDir * Mathf.Sqrt(circlePowerNext * EndPower(inSection))) * radius).normalized * radius;

            Vector3 axisPrev = Vector3.Cross(snakeDir, curveVecPrev).normalized;
            Vector3 axisNext = Vector3.Cross(-snakeDir, curveVecNext).normalized;
            float coefPrev = Vector3.Dot(snakeDir, curveVecPrev);
            float coefNext = Vector3.Dot(-snakeDir, curveVecNext);
            float bigValue = 0.6f;
            float times = 3f;
            if (Mathf.Abs(coefPrev) > bigValue) coefPrev = coefPrev * times - (bigValue * (times - 1)) * Mathf.Sign(coefPrev);
            if (Mathf.Abs(coefNext) > bigValue) coefNext = coefNext * times - (bigValue * (times - 1)) * Mathf.Sign(coefNext);

            toPoint = (toPoint + (Vector3.Cross(toPoint, axisPrev) * coefPrev * BeginPower(inSection) +
                Vector3.Cross(toPoint, axisNext) * coefNext * EndPower(inSection)) * 1f).normalized;

            // Vector3 pos = start + (toPoint * radius - snakeDir * circlePowerNext * EndPower(inSection) * 2
            //      + snakeDir * circlePowerPrev * BeginPower(inSection) * 2).normalized * radius;
            float prevCoef = (1 - Vector3.Dot(toPoint, curveVecPrev)) / 2;
            float nextCoef = (1 - Vector3.Dot(toPoint, curveVecNext)) / 2;
            float prevSin2 = Vector3.Cross(snakeDir, curveVecPrev).magnitude;
            float nextSin2 = Vector3.Cross(snakeDir, curveVecNext).magnitude;
            if (prevSin2 == 0) prevSin2 = 1; 
            if (nextSin2 == 0) nextSin2 = 1;
            float prevCos2 = Mathf.Abs(Vector3.Dot(snakeDir, curveVecPrev));
            float nextCos2 = Mathf.Abs(Vector3.Dot(-snakeDir, curveVecNext));

            float de = prevCoef * BeginPower(inSection) *circlePowerPrev +
                nextCoef * EndPower(inSection) * circlePowerNext;

            float rad = radius * (1 + de);
            
            Vector3 pos = start + toPoint * rad;
            vertices[i] = pos;
           // print(rad);
            //  positions.Add(pos);
        }

        for (int i = 0; i < onCircle * onSection; i++)
        {
            int index = i + n;
            int inCircle = i % onCircle;
            int inSection = ((i % (onSection * onCircle)) / onCircle);
            float angle = 2 * Mathf.PI * (float)inCircle / onCircle;
            Vector3 dir = (points[0] - points[1]).normalized;
            Vector3 e1 = new Vector3(0, 0, 1);
            Vector3 e2 = Vector3.Cross(e1, dir).normalized;
            Vector3 toPoint = e1 * Mathf.Cos(angle) + e2 * Mathf.Sin(angle);
            float dist = radius / (onSection - 1) * inSection;
            Vector3 start = points[0] + dir * dist;
            float rad = radius * Mathf.Sqrt(1 - dist * dist / 1.05f / 1.05f / radius / radius);
            Vector3 pos = start + toPoint * rad;
            vertices[index] = pos;
           // print("vertice " + index + " is " + pos * 100);
        }
        vertices[n + onCircle * onSection] = points[0] + (points[0] - points[1]).normalized * radius;

        

       // vertices[n + onCircle * onSection] = points[0];

        for (int i = 0; i < onCircle * onSection; i++)
        {
            int index = i + n + onCircle * onSection + 1;
            int inCircle = i % onCircle;
            int inSection = ((i % (onSection * onCircle)) / onCircle);
            float angle = -2 * Mathf.PI * (float)inCircle / onCircle;
            Vector3 dir = (points[points.Count - 1] - points[points.Count - 2]).normalized;
            Vector3 e1 = new Vector3(0, 0, 1);
            Vector3 e2 = Vector3.Cross(e1, dir).normalized;
            Vector3 toPoint = e1 * Mathf.Cos(angle) + e2 * Mathf.Sin(angle);
            float dist = radius / (onSection - 1) * inSection;
            Vector3 start = points[points.Count - 1] + dir * dist;
            float rad = radius * Mathf.Sqrt(1 - dist * dist / 1.05f / 1.05f / radius / radius);
            Vector3 pos = start + toPoint * rad;
            vertices[index] = pos;
        }
        vertices[n + 2 * onCircle * onSection + 1] = points[points.Count - 1] + (points[points.Count - 1] - points[points.Count - 2]).normalized * radius;


        for (int i = 0; i < n - onCircle; i++)
        {
            int intAngle = i % onCircle;
            if (intAngle < onCircle - 1)
            {
                tris.Add(i);
                tris.Add(i + 1);
                tris.Add(i + onCircle + 1);

                tris.Add(i);
                tris.Add(i + onCircle + 1);
                tris.Add(i + onCircle);
            }
            else
            {
                tris.Add(i);
                tris.Add(i - onCircle + 1);
                tris.Add(i + 1);

                tris.Add(i);
                tris.Add(i + 1);
                tris.Add(i + onCircle);
            }
        }

        for (int a = 0; a < onCircle * onSection - onCircle; a++)
        {
            int i = a + n;
            int intAngle = i % onCircle;
            if (intAngle < onCircle - 1)
            {
                tris.Add(i);
                
                tris.Add(i + onCircle + 1);
                tris.Add(i + 1);

                tris.Add(i);
                
                tris.Add(i + onCircle);
                tris.Add(i + onCircle + 1);
            }
            else
            {
                tris.Add(i);
                
                tris.Add(i + 1);
                tris.Add(i - onCircle + 1);

                tris.Add(i);
                
                tris.Add(i + onCircle);
                tris.Add(i + 1);
            }
        }

        for (int i = 0; i < onCircle; i++)
        {
            int index = n + onCircle * onSection - onCircle + i;
            int next = i + 1;
            if (next >= onCircle) next = 0;
            int nextInd = n + onCircle * onSection - onCircle + next;
           // print("Pos " + index + " is " + vertices[index]);
            tris.Add(index);

          //  if (i == 1) sphere.position = vertices[index];

            tris.Add(n + onCircle * onSection);
            tris.Add(nextInd);
        }


        for (int a = 0; a < onCircle * onSection - onCircle; a++)
        {
            int i = a + n + onCircle * onSection + 1;
            int intAngle = a % onCircle;
            if (intAngle < onCircle - 1)
            {
                tris.Add(i);
                tris.Add(i + 1);
                tris.Add(i + onCircle + 1);
                

                tris.Add(i);
                tris.Add(i + onCircle + 1);
                tris.Add(i + onCircle);
                
            }
            else
            {
                tris.Add(i);
                tris.Add(i - onCircle + 1);
                tris.Add(i + 1);
                

                tris.Add(i);
                tris.Add(i + 1);
                tris.Add(i + onCircle);
                
            }
        }

        for (int i = 0; i < onCircle; i++)
        {

            int index = n + 2 * onCircle * onSection + 1 - onCircle + i;
            int next = i + 1;
            if (next >= onCircle) next = 0;
            int nextInd = n + 2 * onCircle * onSection + 1 - onCircle + next;
            tris.Add(index);
            tris.Add(nextInd);
            tris.Add(n + 2 * onCircle * onSection + 1);
            
            print("Last is " + (tris.Count - 1));
        }

        for (int i = 0; i < onCircle; i++)
        {
            int next = i + 1;
            if (next >= onCircle - 1) next = 0;
            tris.Add(i);
            tris.Add(i + n);
            tris.Add(next);

            tris.Add(next);
            tris.Add(i + n);
            tris.Add(n + next);
        }

        for (int a = 0; a < onCircle; a++)
        {
            int i = n - onCircle + a;
            int nexta = a + 1;
            if (nexta >= onCircle - 1) nexta = 0;
            int next = n - onCircle + nexta;
            tris.Add(i);
            tris.Add(next);
            tris.Add(a + n + onCircle * onSection + 1);


            tris.Add(next);
            tris.Add(nexta + n + onCircle * onSection + 1);
            tris.Add(a + n + onCircle * onSection + 1);

        }

        //tris.Add(n - onCircle);
        //tris.Add(n - onCircle + 1);
        //tris.Add(0 + n + onCircle * onSection + 1);

        //tris.Add(n - onCircle + 1);
        //tris.Add(n - onCircle + 2);
        //tris.Add(0 + n + onCircle * onSection + 1 - 9);


        print("Count is " + tris.Count);

        for (int i = 0; i < tris.Count; i++)
        {
            triangles[i] = tris[i];
          //  print("Wrong!");
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

      //  if (meshFilter && mesh) meshFilter.mesh = mesh;
      //  if (meshFilter && mesh) meshRenderer.material = material;
    }
}
