### Motion System (`Tarotro.Motion` namespace)

- **`Moveable.cs`**: Core class managing smooth 2D transform interpolation with physics-based damping
    
    - Handles position, rotation, and scale smoothing with configurable rates
    - Supports "juice" effects (brief scale/rotation bursts for visual feedback)
    - Tracks velocity and applies constraints (max speed, snap distances)
    - Manages size pinching and shadow parallax calculations
- **`MotionTuning.cs`**: ScriptableObject configuration for all motion parameters
    
    - Smoothing rates for position, scale, and rotation
    - Interaction zoom bonuses (hover/drag)
    - Juice effect parameters (frequency, amplitude, duration)
    - Accessibility option for reduced motion
- **`MotionSystem.cs`**: Central manager for all Moveable objects
    
    - Registers/unregisters Moveables and applies tuning
    - Ticks all objects each frame with consistent MotionFrame data
    - Configurable room width for parallax calculations
- **`MoveableView.cs`**: MonoBehaviour bridge connecting Moveable logic to Unity transforms
    
    - Applies visual transform, rotation, and scale from Moveable state
    - Handles shadow parallax offset
    - Supports Y-flip for coordinate system conversion
- **`HandLayout.cs`**: Utility for arranging cards in a fan layout
    
    - Applies position, rotation, and lift based on card index
    - Adds idle animation (bob and rotation sway)
    - Highlights selected cards with lift effect
    - Sorts cards by screen order

### Event Sequencing System (`Tarotro.Sequencing` namespace)

- **`GameEvent.cs`**: Immutable event class with multiple trigger types
    
    - `Immediate`: Execute instantly
    - `After`: Execute after delay
    - `Before`: Execute before delay expires
    - `Condition`: Execute until condition is true
    - `Ease`: Smooth interpolation between values with easing functions (Lerp, Quad, Elastic)
    - Fluent API for configuration (`.NonBlocking()`, `.OnRealClock()`, etc.)
- **`EventQueue.cs`**: Multi-lane event processor
    
    - Organizes events into named lanes (Base, Unlock, Achievement, Tutorial, Other)
    - Supports pause/resume with clock separation (Total vs Real time)
    - Blocking system prevents subsequent events until current completes
    - Persistent events survive queue clears
    - Fixed 60Hz processing step with frame skipping tolerance

### Runtime Integration

- **`GameLoopRunner.cs`**: MonoBehaviour orchestrating both systems
    - Injected with EventQueue and MotionSystem via VContainer
    - Ticks both systems each Update
    - Applies visual transforms in LateUpdate
    - Manages MoveableView registration for rendering

### DI Configuration

- Updated `GameLifetimeScope.cs` to register EventQueue and MotionSystem as singletons
- Added MotionTuning asset reference for dependency injection

## Notable Implementation Details

- Motion uses exponential decay (`exp(-rate * dt)`) for smooth, frame-rate-independent damping
- Velocity clamping prevents overshooting while maintaining responsiveness
- Juice effects use sine waves with cubic falloff for natural-feeling bursts
- Event system separates blocking (sequential) from non-blocking (parallel) execution
- Real vs Total time clocks allow pause-aware event scheduling

[https://claude.ai/code/session_011ZkGqU8BCqKSiCLX6ZJCqn](https://claude.ai/code/session_011ZkGqU8BCqKSiCLX6ZJCqn)