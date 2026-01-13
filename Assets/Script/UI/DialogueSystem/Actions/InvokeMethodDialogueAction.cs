using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "InvokeMethodDialogueAction", menuName = "Dialogue System/Actions/Invoke Method")]
public class InvokeMethodDialogueAction : DialogueAction
{
    public enum Target
    {
        DialogueStarter,
        DialogueManager,
        Player,
        FindByTag,
        FindByName
    }

    [Header("Target")]
    public Target target = Target.DialogueStarter;

    [Tooltip("Used when Target = FindByTag")]
    public string targetTag;

    [Tooltip("Used when Target = FindByName")]
    public string targetName;

    [Header("Component (optional)")]
    [Tooltip("If set, invokes the method on this component type (e.g. 'QuestManager' or 'MyNamespace.QuestManager'). If empty, uses the first MonoBehaviour on the target GameObject.")]
    public string componentTypeName;

    [Header("Method")]
    [Tooltip("Name of the method to call. Must be parameterless.")]
    public string methodName;

    public override void Execute(DialogueContext context)
    {
        if (context == null)
        {
            return;
        }

        UnityEngine.Object targetObject = ResolveTargetObject(context);
        if (targetObject == null)
        {
            Debug.LogWarning($"[Dialogue] InvokeMethodDialogueAction could not resolve target '{target}'.");
            return;
        }

        object receiver = ResolveReceiver(targetObject);
        if (receiver == null)
        {
            Debug.LogWarning($"[Dialogue] InvokeMethodDialogueAction could not resolve receiver on target '{targetObject.name}'.");
            return;
        }

        if (string.IsNullOrWhiteSpace(methodName))
        {
            Debug.LogWarning("[Dialogue] InvokeMethodDialogueAction has no methodName set.");
            return;
        }

        MethodInfo method = receiver.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null)
        {
            Debug.LogWarning($"[Dialogue] Method '{methodName}' not found on '{receiver.GetType().Name}'.");
            return;
        }

        if (method.GetParameters().Length != 0)
        {
            Debug.LogWarning($"[Dialogue] Method '{receiver.GetType().Name}.{methodName}' has parameters. Only parameterless methods are supported.");
            return;
        }

        try
        {
            method.Invoke(receiver, null);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private UnityEngine.Object ResolveTargetObject(DialogueContext context)
    {
        switch (target)
        {
            case Target.DialogueStarter:
                return context.dialogueStarter;
            case Target.DialogueManager:
                return context.dialogueManager;
            case Target.Player:
                if (context.player != null) return context.player;
                return GameObject.FindGameObjectWithTag("Player");
            case Target.FindByTag:
                if (string.IsNullOrWhiteSpace(targetTag)) return null;
                return GameObject.FindGameObjectWithTag(targetTag);
            case Target.FindByName:
                if (string.IsNullOrWhiteSpace(targetName)) return null;
                return GameObject.Find(targetName);
            default:
                return null;
        }
    }

    private object ResolveReceiver(UnityEngine.Object targetObject)
    {
        // If target is already a component, and no specific component type requested, use it directly
        if (string.IsNullOrWhiteSpace(componentTypeName))
        {
            if (targetObject is Component component)
            {
                return component;
            }

            if (targetObject is GameObject go)
            {
                return go.GetComponent<MonoBehaviour>();
            }

            return null;
        }

        Type componentType = FindType(componentTypeName);
        if (componentType == null)
        {
            Debug.LogWarning($"[Dialogue] Component type '{componentTypeName}' could not be found.");
            return null;
        }

        if (targetObject is Component targetComponent)
        {
            return targetComponent.GetComponent(componentType);
        }

        if (targetObject is GameObject targetGo)
        {
            return targetGo.GetComponent(componentType);
        }

        return null;
    }

    private static Type FindType(string typeName)
    {
        // Try direct lookup first
        Type type = Type.GetType(typeName);
        if (type != null) return type;

        // Search all loaded assemblies (Unity-friendly)
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(a => a.GetType(typeName))
            .FirstOrDefault(t => t != null);
    }
}
