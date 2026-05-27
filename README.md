# High-Performance Cellular Automata Engine (.NET 9)

A highly optimized, multi-threaded, and **zero-allocation** simulation engine for *Conway's Game of Life* and *HighLife*, built with **.NET 9**. This project leverages low-level optimization techniques, modern C# features, and hardware awareness to maximize execution speed and throughput on modern multi-core CPUs.

---

## Performance & Architecture Highlights

This engine was architected from the ground up to eliminate common managed-runtime overheads:

* **Zero Allocations (0 Bytes Heap Allocation):** Once the initialization phase is complete, the main simulation loop performs absolutely zero heap allocations. This completely eliminates Garbage Collection (GC) latency and pauses, ensuring rock-solid, fluid execution.
* **Compile-Time Polymorphism via Struct Constraints:** Instead of using traditional object-oriented interfaces (`ICellularRule` and `INeighbourhoodStrategy`) which incur virtual method invocation overhead (vtable lookups), rules and neighborhood typologies are implemented as **`structs`**. By passing them into the core engine via generic constraints (`where TRule : struct`), the .NET JIT compiler performs **Aggressive Inlining**, embedding the logic directly into the processing loop.
* **Branchless Logical Execution:** The state transitions for cells (`ConwayRule` and `HighLifeRule`) are computed using mathematical bitwise operations (`BornMask`, `SurviveMask`, `XorMask`) rather than conditional jumps (`if-else`). This eliminates CPU branch mispredictions, maintaining a highly deterministic execution pipeline regardless of population chaos.
* **Double Buffering via Pointer Swap:** The grid is represented as a contiguous, flat 1D array wrapped in a specialized `GridBuffer`. To compute the next generation, the engine reads from the current buffer and writes to the next. At the end of a cycle, a high-speed reference swap occurs without copying underlying array elements.
* **Cache Locality & Row Parallelism:** Computations are distributed across all available CPU cores using `Parallel.For` processing row-by-row. Data is arranged sequentially to maximize L1/L2 cache hits and eliminate false sharing.
* **Asynchronous Flicker-Free Rendering:** Rendering to the console happens on a dedicated background thread, fully decoupled from the simulation frequency. It utilizes a thread-safe `CopySnapshot` method, a local `StringBuilder` screen buffer, and raw ANSI escape codes for completely flicker-free visual updates.

---

## Features

* **Multiple Rulesets:** Supports classic *Conway's Game of Life* (B3/S23) and *HighLife* (B36/S23), which features native replicator patterns.
* **Multiple Neighborhoods:** Toggle between *Moore Neighborhood* (8 surrounding cells) and *Von Neumann Neighborhood* (4 orthogonal cells).
* **Flexible Topologies:** Configure the board as a bounded finite grid or a *Toroidal grid* (seamless wrapping around edges).
* **RLE Pattern Parser:** Built-in support to parse and load standard `.rle` (Run-Length Encoded) files directly onto the grid, handling positioning and resizing dynamically.
* **Real-time Performance HUD:** Tracks and displays Generations, Living Cells, Grids/sec (Updates Per Second), Threads/sec, and microscopic Cell Checks/sec.

---

## Configuration (`settings.json`)

The application automatically loads or generates a default configuration file on startup:

```json
{
  "Width": 160,
  "Height": 40,
  "Toroidal": true,
  "RuleType": "Conway",
  "NeighbourType": "Moore",
  "UseRandomPattern": true,
  "Density": 0.3,
  "RlePath": "patterns/gosper_glider_gun.rle",
  "FpsRate": 30,
  "StartupMode": "Fast",
  "ShowHelpScreen": true
}