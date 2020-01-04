# ME.ECS
ME.ECS - it's ECS implementation for Unity Engine with full state automatic recover

# Performance
1.000.000 entities:
- for initialization: 155MB alloc, 525ms (Point objects, see Example directory)
- for one tick: 0b alloc, 25ms

1 entity:
- for initialization: 46.6KB alloc, 20ms (Point objects, see Example directory)
- for one tick: 0b alloc, 0.1ms

# Upcoming plans
1. Implement automatic states history with recovering system
2. Decrease initialization time and memory allocs
3. Rendering system
