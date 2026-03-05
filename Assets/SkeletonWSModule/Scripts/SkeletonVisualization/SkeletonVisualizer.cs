using PoseTypes;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonVisualizer : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private GameObject jointPrefab;
    [SerializeField] private Transform skeletonRoot;

    [Header("Joint Rendering")]
    [SerializeField] private Vector3 jointScale = Vector3.one * 0.02f;

    [Header("Bone Rendering")]
    [SerializeField] private float boneWidth = 0.01f;
    [SerializeField] private Color boneColor = Color.green;

    [Header("Position Transform")]
    [SerializeField] private Vector3 positionScale = Vector3.one;
    [SerializeField] private Vector3 positionOffset = Vector3.zero;

    private class SkeletonInstance
    {
        public Transform root;
        public Transform[] joints = new Transform[PoseSchema.JointCount];
        public LineRenderer[] bones;
    }

    private readonly Dictionary<int, SkeletonInstance> persons = new();
    private readonly List<int> removeBuffer = new();

    private static Material boneMaterial;

    private void Awake()
    {
        if (boneMaterial == null)
            boneMaterial = new Material(Shader.Find("Sprites/Default"));
    }

    public void HandleFrame(FrameSample frame)
    {
        if (frame == null || frame.persons == null)
            return;

        var seen = new HashSet<int>();

        for (int p = 0; p < frame.persons.Count; p++)
        {
            var person = frame.persons[p];
            seen.Add(p);

            if (!persons.TryGetValue(p, out var instance))
            {
                instance = CreateInstance(p);
                persons.Add(p, instance);
            }

            UpdateJoints(instance, person);
            UpdateBones(instance);
        }

        RemoveMissingPersons(seen);
    }

    private SkeletonInstance CreateInstance(int id)
    {
        var inst = new SkeletonInstance();

        inst.root = new GameObject($"Person_{id}").transform;

        if (skeletonRoot != null)
            inst.root.SetParent(skeletonRoot, false);

        for (int i = 0; i < PoseSchema.JointCount; i++)
        {
            GameObject go = jointPrefab != null
                ? Instantiate(jointPrefab, inst.root)
                : GameObject.CreatePrimitive(PrimitiveType.Sphere);

            go.name = ((JointId)i).ToString();
            go.transform.localScale = jointScale;

            inst.joints[i] = go.transform;
        }

        inst.bones = CreateBones(inst.root);

        return inst;
    }

    private void UpdateJoints(SkeletonInstance inst, PersonSkeleton person)
    {
        for (int j = 0; j < PoseSchema.JointCount; j++)
        {
            var joint = person.joints[j];

            Vector3 pos = new Vector3(
                joint.x * positionScale.x,
                joint.z * positionScale.z,
                joint.y * positionScale.y
            ) + positionOffset;

            inst.joints[j].localPosition = pos;
        }
    }

    private LineRenderer[] CreateBones(Transform root)
    {
        var bones = new LineRenderer[SkeletonLayout.BonePairs.Length];

        for (int i = 0; i < SkeletonLayout.BonePairs.Length; i++)
        {
            var obj = new GameObject($"bone_{i}");
            obj.transform.SetParent(root);

            var lr = obj.AddComponent<LineRenderer>();

            lr.positionCount = 2;
            lr.startWidth = boneWidth;
            lr.endWidth = boneWidth;

            lr.material = boneMaterial;

            lr.startColor = boneColor;
            lr.endColor = boneColor;

            bones[i] = lr;
        }

        return bones;
    }

    private void UpdateBones(SkeletonInstance inst)
    {
        for (int i = 0; i < SkeletonLayout.BonePairs.Length; i++)
        {
            var (a, b) = SkeletonLayout.BonePairs[i];

            var bone = inst.bones[i];

            bone.SetPosition(0, inst.joints[(int)a].position);
            bone.SetPosition(1, inst.joints[(int)b].position);
        }
    }

    private void RemoveMissingPersons(HashSet<int> seen)
    {
        removeBuffer.Clear();

        foreach (var kv in persons)
            if (!seen.Contains(kv.Key))
                removeBuffer.Add(kv.Key);

        foreach (var id in removeBuffer)
        {
            Destroy(persons[id].root.gameObject);
            persons.Remove(id);
        }
    }
}