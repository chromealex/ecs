# Sending UI events to world [![](Logo-Tiny.png)](/../../#glossary)
It's the same as the method above.
```csharp
public class YourUIBehaviour : MonoBehaviour {
    
    public Button button;
    
    public void Start() {
    
        this.button.onClick.AddListener(this.AddMarker);
    
    }
    
    ...
    
    private void AddMarker() {
        
        // Send marker click to world
        Worlds.currentWorld.AddMarker(new OnYourUIBehaviourClick());
        
    }
    
}
```

[![](Footer.png)](/../../#glossary)
