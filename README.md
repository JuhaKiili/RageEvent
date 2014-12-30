RageEvent
=========

Weakly typed event system for Unity.
___
### Caller
```C#
EventManager.Trigger("MyEvent", 1f);
```
### Listener
```C#
void Awake () {
	EventManager.Initialize(this);
}

[Listen("MyEvent")]
public void MyEvent(object[] parameters) {
	float f = (float) parameters[0];
	Debug.Log(f);
}
```
___
**Is this the most performant event system for Unity?**

No.

**Is this the most robust and safe event system for Unity?**

No.

**What good is it then?**

It's reasonably performant, automatically cleans up null references and doesn't require a lot of code.

