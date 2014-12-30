RageEvent
=========

Weakly typed event system for Unity.

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
