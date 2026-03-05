using System;
using System.Collections.Generic;
using UnityEngine;
using PoseTypes;
using Newtonsoft.Json.Linq;   // 建議使用 Newtonsoft

public class SkeletonJsonParser
{
    public event Action<FrameSample> OnFrameParsed;
    public event Action<string> OnParseError;

    public void Parse(string json)
    {
        try
        {
            var root = JObject.Parse(json);

            // 你的格式是 { "0": [ persons... ] }
            foreach (var property in root.Properties())
            {
                if (!int.TryParse(property.Name, out int frameIndex))
                {
                    OnParseError?.Invoke("Invalid frame index.");
                    return;
                }

                var frame = new FrameSample(frameIndex);
                frame.recvTime = Time.time;

                var personsArray = property.Value as JArray;
                if (personsArray == null)
                {
                    OnParseError?.Invoke("Persons array missing.");
                    return;
                }

                foreach (var personToken in personsArray)
                {
                    var jointsArray = personToken as JArray;
                    if (jointsArray == null || jointsArray.Count != PoseSchema.JointCount)
                    {
                        OnParseError?.Invoke("Joint count mismatch.");
                        continue;
                    }

                    var person = new PersonSkeleton();

                    for (int i = 0; i < PoseSchema.JointCount; i++)
                    {
                        var jointData = jointsArray[i] as JArray;
                        if (jointData == null || jointData.Count < 4)
                            continue;

                        float x = jointData[0].Value<float>();
                        float y = jointData[1].Value<float>();
                        float z = jointData[2].Value<float>();
                        float conf = jointData[3].Value<float>();

                        person.joints[i] = new PoseTypes.Joint(x, y, z, conf);
                    }

                    frame.persons.Add(person);
                }

                OnFrameParsed?.Invoke(frame);
            }
        }
        catch (Exception ex)
        {
            OnParseError?.Invoke($"Parse Exception: {ex.Message}");
        }
    }
}