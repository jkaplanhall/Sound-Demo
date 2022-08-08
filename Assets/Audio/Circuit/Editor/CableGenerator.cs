using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CableGenerator : EditorWindow
{

    GameObject SegmentPrefab;
    GameObject PlugPrefab;
    GameObject RendererPrefab;
    Vector3 SpawnLocation = new Vector3(0,10,0);
    float Spacing = .15f;
    float Scale = .1f;

    

    int NumberOfSegments = 40;

    [MenuItem("Custom Tools/ Cable Generator")]
    public static void ShowWindow()
    {
        GetWindow(typeof(CableGenerator));
    }

    private void Init()
    {
        Debug.Log("Test");
    }
    private void OnGUI()
    {
        //controls
        SegmentPrefab = EditorGUILayout.ObjectField("Segment Prefab", SegmentPrefab, typeof(GameObject), false) as GameObject;
        PlugPrefab = EditorGUILayout.ObjectField("Plug Prefab", PlugPrefab, typeof(GameObject), false) as GameObject;
        RendererPrefab = EditorGUILayout.ObjectField("Renderer Prefab", RendererPrefab, typeof(GameObject), false) as GameObject;
        SpawnLocation = EditorGUILayout.Vector3Field("Spawn Location", SpawnLocation);
        Spacing = EditorGUILayout.FloatField("Segment Spacing", Spacing);
        Scale = EditorGUILayout.FloatField("Segment Scale", Scale);
        NumberOfSegments = EditorGUILayout.IntField("Number of Segments", NumberOfSegments);



        if (GUILayout.Button("SpawnCable"))
        {

            GameObject cable = new GameObject("Cable");

            cable.transform.position = SpawnLocation;
            Vector3 Pos = cable.transform.position;
            Rigidbody LastBody = null;

            //spawn first plug
            GameObject FirstPlug = Instantiate(PlugPrefab);
            FirstPlug.transform.position = Pos - Vector3.forward * .1f;
            FirstPlug.transform.parent = cable.transform;
            //flip the plug to the right angle
            FirstPlug.transform.Rotate(Vector3.up * 180);



            List<Transform> linkTransforms = new List<Transform>();

            //set last body
            LastBody = FirstPlug.GetComponent<Rigidbody>();

            Rigidbody FirstBody = null;
            //make and attach each of the links together
            for (int i = 0; i < NumberOfSegments; i++)
            {
                //move to the next position
                Pos += Vector3.forward * Spacing;

                //create the link
                GameObject SegmentObj = Instantiate(SegmentPrefab);
                //set transform variables
                SegmentObj.transform.position = Pos;
                SegmentObj.transform.parent = cable.transform;
                SegmentObj.GetComponent<SphereCollider>().radius = Scale;

                //save link
                linkTransforms.Add(SegmentObj.transform);


                //link joint to the previous rigidbody
                SegmentObj.GetComponent<ConfigurableJoint>().connectedBody = LastBody;



                //set the last body to the last link
                LastBody = SegmentObj.GetComponent<Rigidbody>();

                if (FirstBody == null)
                    FirstBody = LastBody;


            }


            //create cable renderer
            GameObject rendererObj = Instantiate(RendererPrefab);

            CableRenderer renderer = rendererObj.GetComponent<CableRenderer>();
            linkTransforms.Reverse();
            renderer.SetPath(linkTransforms.ToArray());

            //move to the next position (oh gosh the same line of code)
            Pos += Vector3.forward * (Spacing + .1f);

            //spawn second plug
            GameObject SecondPlug = Instantiate(PlugPrefab);
            SecondPlug.transform.position = Pos;
            SecondPlug.transform.parent = cable.transform;

            //link the plugs together
            CPlug Plug1 = FirstPlug.GetComponent<CPlug>();
            CPlug Plug2 = SecondPlug.GetComponent<CPlug>();
            Plug1.SetPartner(Plug2);
            Plug2.SetPartner(Plug1);

            float MaxDist = (NumberOfSegments * Spacing) * 1.25f;
            Plug1.m_MaxPartnerDist = MaxDist;
            Plug2.m_MaxPartnerDist = MaxDist;

            //connect the plugs to the cable with fixed joints
            FixedJoint FirstJoint = FirstPlug.AddComponent<FixedJoint>();
            FixedJoint SecondJoint = SecondPlug.AddComponent<FixedJoint>();

            FirstJoint.connectedBody = FirstBody;
            SecondJoint.connectedBody = LastBody;

            //shift spawn for next cable
            SpawnLocation += Vector3.right * 1f;
        }
    }

    
}
