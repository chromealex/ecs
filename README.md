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
. Implement automatic states history with rollback system <b>(90% done)</b>
. Decrease initialization time and memory allocs <b>(30% done)</b>
. Random support to generate random numbers, store RandomState in game state <b>(100% done)</b>
. Rendering system <b>(0% done)</b>
