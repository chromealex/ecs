# ME.ECS
ME.ECS - it's ECS implementation for Unity Engine with full state automatic rollbacks

# Performance
1.000.000 entities:
- for initialization: 155MB alloc, 215ms (Point objects, see Example directory)
- for one tick: <b>0b alloc, 5ms</b>

1 entity:
- for initialization: 46.6KB alloc, 5ms (Point objects, see Example directory)
- for one tick: <b>0b alloc, 0.01ms</b>

# Upcoming plans
1. Implement automatic states history with rollback system <b>(90% done)</b>
2. Decrease initialization time and memory allocs <b>(30% done)</b>
3. Rendering system
