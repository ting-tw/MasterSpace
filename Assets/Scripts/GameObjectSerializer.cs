using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

[Serializable]
public class GameObjectData
{
    public AnimatorStateData animatorStateData;
    public TransformData transformData;
}

[Serializable]
public class TransformData
{
    public float[] position;
    public float[] rotation;
    public float[] scale;
}

[Serializable]
public class AnimatorStateData
{
    public List<BoolParameter> boolParameters = new List<BoolParameter>();
    public List<FloatParameter> floatParameters = new List<FloatParameter>();
    public List<IntParameter> intParameters = new List<IntParameter>();
    public List<TriggerParameter> triggerParameters = new List<TriggerParameter>();

    [Serializable]
    public class BoolParameter
    {
        public string name;
        public bool value;
    }

    [Serializable]
    public class FloatParameter
    {
        public string name;
        public float value;
    }

    [Serializable]
    public class IntParameter
    {
        public string name;
        public int value;
    }

    [Serializable]
    public class TriggerParameter
    {
        public string name;
        public bool set;
    }
}

public static class AnimatorSerializer
{
    public static AnimatorStateData Serialize(Animator animator)
    {
        var animatorStateData = new AnimatorStateData();

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            switch (parameter.type)
            {
                case AnimatorControllerParameterType.Bool:
                    animatorStateData.boolParameters.Add(new AnimatorStateData.BoolParameter { name = parameter.name, value = animator.GetBool(parameter.name) });
                    break;
                case AnimatorControllerParameterType.Float:
                    animatorStateData.floatParameters.Add(new AnimatorStateData.FloatParameter { name = parameter.name, value = animator.GetFloat(parameter.name) });
                    break;
                case AnimatorControllerParameterType.Int:
                    animatorStateData.intParameters.Add(new AnimatorStateData.IntParameter { name = parameter.name, value = animator.GetInteger(parameter.name) });
                    break;
                case AnimatorControllerParameterType.Trigger:
                    animatorStateData.triggerParameters.Add(new AnimatorStateData.TriggerParameter { name = parameter.name, set = animator.GetBool(parameter.name) });
                    break;
            }
        }

        return animatorStateData;
    }

    public static void Deserialize(Animator animator, AnimatorStateData animatorStateData)
    {
        if (animatorStateData == null) return;

        foreach (var param in animatorStateData.boolParameters)
        {
            animator.SetBool(param.name, param.value);
        }
        foreach (var param in animatorStateData.floatParameters)
        {
            animator.SetFloat(param.name, param.value);
        }
        foreach (var param in animatorStateData.intParameters)
        {
            animator.SetInteger(param.name, param.value);
        }
        foreach (var param in animatorStateData.triggerParameters)
        {
            if (param.set)
            {
                animator.SetTrigger(param.name);
            }
            else
            {
                animator.ResetTrigger(param.name);
            }
        }
    }
}

public class GameObjectSerializer : MonoBehaviour
{
    public static string SerializeGameObject(GameObject gameObject)
    {
        GameObjectData data = new GameObjectData();

        Animator animator = gameObject.GetComponent<Animator>();
        if (animator != null)
        {
            data.animatorStateData = AnimatorSerializer.Serialize(animator);
        }

        Transform transform = gameObject.transform;
        if (transform != null)
        {
            data.transformData = new TransformData();
            data.transformData.position = new float[3] { transform.position.x, transform.position.y, transform.position.z };
            data.transformData.rotation = new float[4] { transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w };
            data.transformData.scale = new float[3] { transform.localScale.x, transform.localScale.y, transform.localScale.z };
        }

        return JsonConvert.SerializeObject(data);
    }

    public static void DeserializeGameObject(GameObject gameObject, string jsonData)
    {
        GameObjectData data = JsonConvert.DeserializeObject<GameObjectData>(jsonData);

        if (data.animatorStateData != null)
        {
            Animator animator = gameObject.GetComponent<Animator>();
            if (animator == null)
            {
                animator = gameObject.AddComponent<Animator>();
            }
            AnimatorSerializer.Deserialize(animator, data.animatorStateData);
        }

        if (data.transformData != null)
        {
            Transform transform = gameObject.transform;
            transform.position = new Vector3(data.transformData.position[0], data.transformData.position[1], data.transformData.position[2]);
            transform.rotation = new Quaternion(data.transformData.rotation[0], data.transformData.rotation[1], data.transformData.rotation[2], data.transformData.rotation[3]);
            transform.localScale = new Vector3(data.transformData.scale[0], data.transformData.scale[1], data.transformData.scale[2]);
        }
    }
}
