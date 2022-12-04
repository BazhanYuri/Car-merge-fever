## Homa Console Documentation

* [Settings](#settings)
* [Live Debugging](#live-debugging)
* [Performance](#performance)
* [Logs](#logs)
* [Advanced](#advanced)

# Overview
The Homa Console is a all-in-one debug tool that allows developers to :
- debug their game
- create tools for the QA team
- create tools for the Creative team

Open with a triple tap in the corner of your choice.

---

# Settings
Find settings for the Homa Console in **Project Settings/Homa Console** :
![](Documentation~/settings.png)

---

Pin and unpin the different modules using the lock icon :
![](Documentation~/lock.png)

When locked, the module will stay active even if you close the console, this will help you have a better lock at your game while debugging.

---

> :warning: ** For the console to work, you need the HOMA_DEVELOPMENT define symbol added to your Scripting Define Symbol in the Player settings. **

# Live Debugging
What it looks like :

![](Documentation~/live-debug.png)

How to integrate :

```csharp
// Don't forget to mark your class as debuggable !
[DebuggableClass]
public class MyAwesomecComponent
{
    // Use on properties
    [DebuggableField("General/Gameplay",Order = 10)]
    public bool AwesomeProperty
    {
        get => gameObject.activeSelf;
        set => gameObject.SetActive(value);
    }
    
    // Use on fields
    [DebuggableField]
    public bool someOtherProperty;
    
    // Use on functions
    [DebuggableAction]
    public void Test()
    {
        Debug.Log("hey");
    }
}
```

Once you tag a property, a field or a function, it will be displayed in the Live Debug section of the Homa Console.

You can add a Tag to group your debug item with other items, multiple groups can be used for a single debug item :

```csharp
// This property will be in "Cheats" group and will be displayed in a slider with range 0 to 1
[DebuggableField("Cheats", CustomName = "Name", LayoutOption = LayoutOption.Slider, Min = 0, Max = 1)]
public float XPosition
{
    get => transform.position.x;
    set => transform.position = new Vector3(value,transform.position.y,transform.position.z);
}
```

# Performance
A simple tool to display various in-game performance data :

![](Documentation~/performance.png)
# Logs
Get logs and search for them at runtime :

![](Documentation~/logs.png)

# Advanced

If you want a callback when the Console is shown/hidden (useful to hide and show the banner ads), you can listen to the following events :

```csharp
/// <summary>
/// Triggered when the main Console window opened / closed.
/// </summary>
public static event System.Action<bool> OnConsoleVisibilityChanged;
/// <summary>
/// Will send true if anything from the console becomes visible and false when everything from the console is hidden.
/// </summary>
public static event System.Action<bool> OnModuleVisibilityChanged;
```